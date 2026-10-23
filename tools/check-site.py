"""Validate a rendered Jekyll site with only the Python standard library."""
import argparse
from html.parser import HTMLParser
from pathlib import Path
from urllib.parse import unquote, urlsplit


class Page(HTMLParser):
    def __init__(self, text):
        super().__init__(convert_charrefs=True)
        self.ids, self.links, self.errors = set(), [], []
        self.h1 = 0
        self.feed(text)
        if self.h1 != 1:
            self.errors.append(f"expected one h1, found {self.h1}")
        if "{{" in text or "{%" in text:
            self.errors.append("unrendered Liquid")

    def handle_starttag(self, tag, attrs):
        attrs = dict(attrs)
        if tag == "h1":
            self.h1 += 1
        if "id" in attrs:
            if attrs["id"] in self.ids:
                self.errors.append("duplicate id: " + attrs["id"])
            self.ids.add(attrs["id"])
        if tag == "img" and "alt" not in attrs:
            self.errors.append("image missing alt")
        for key in ("href", "src"):
            if key in attrs:
                self.links.append(attrs[key])


def check(root, base):
    root = root.resolve()
    pages = {p.relative_to(root).as_posix(): Page(p.read_text(encoding="utf-8"))
             for p in root.rglob("*.html")}
    errors = []
    required = ["index.html", "404.html"] + [name + "/index.html" for name in
                ("compatibility", "quickstart", "developers", "about")]
    for name in required:
        if name not in pages:
            errors.append("missing page: " + name)
    for name, page in pages.items():
        errors.extend(name + ": " + error for error in page.errors)
        for link in page.links:
            url = urlsplit(link)
            if url.scheme or url.netloc:
                continue
            path = unquote(url.path)
            if path.startswith("/"):
                if base and not (path == base or path.startswith(base + "/")):
                    errors.append(f"{name}: link escapes base path: {link}")
                    continue
                target = root / path[len(base):].lstrip("/")
            else:
                target = root / name if not path else root / Path(name).parent / path
            if target.is_dir():
                target /= "index.html"
            target = target.resolve()
            if not target.is_relative_to(root):
                errors.append(f"{name}: target escapes output directory: {link}")
                continue
            if not target.is_file():
                errors.append(f"{name}: missing target: {link}")
                continue
            key = target.relative_to(root).as_posix()
            if url.fragment and key in pages and unquote(url.fragment) not in pages[key].ids:
                errors.append(f"{name}: missing fragment: {link}")
    if errors:
        raise SystemExit("\n".join(errors))
    print(f"Passed: {len(pages)} pages; internal links, fragments, images and rendered content.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("directory", type=Path)
    parser.add_argument("--base", default="/KAMI")
    args = parser.parse_args()
    check(args.directory.resolve(), args.base.rstrip("/"))
