"""Check documented title IDs and flag game/camera source changes for review."""
import argparse
import hashlib
import json
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
SNAPSHOT = ROOT / "docs/_data/compatibility_source_hashes.json"


def source_hashes():
    paths = [ROOT / "KAMI.Core/GameManager.cs"]
    for folder in ("Games", "Cameras"):
        paths.extend((ROOT / "KAMI.Core" / folder).rglob("*.cs"))
    return {p.relative_to(ROOT).as_posix(): hashlib.sha256(
        p.read_text(encoding="utf-8-sig").replace("\r\n", "\n").encode()).hexdigest()
        for p in sorted(paths)}


def registry():
    source = (ROOT / "KAMI.Core/GameManager.cs").read_text(encoding="utf-8-sig")
    result, pending = {}, []
    for token in re.finditer(r'case\s+"(?P<serial>[^"]+)"\s*:|return\s+new\s+(?P<handler>[\w.]+)\s*\(', source):
        if token["serial"]:
            pending.append(token["serial"])
        elif pending:
            for serial in pending:
                if serial in result:
                    raise ValueError("Duplicate registered title ID: " + serial)
                result[serial] = token["handler"].split(".")[-1]
            pending = []
    if pending or not result:
        raise ValueError("GameManager format changed; review the registry reader.")
    return result


def check(data_path):
    data = json.loads(data_path.read_text(encoding="utf-8"))
    documented = {}
    for game in data["games"]:
        source = ROOT / "KAMI.Core/Games" / game["source"]
        if not source.is_file():
            raise ValueError("Missing documented handler: " + game["source"])
        for variant in game["variants"]:
            for serial in variant["serials"]:
                if serial in documented:
                    raise ValueError("Duplicate documented title ID: " + serial)
                documented[serial] = source.stem
    registered = registry()
    mismatched = sorted(key for key in set(registered) | set(documented)
                        if registered.get(key) != documented.get(key))
    if mismatched:
        raise ValueError("Title ID / handler mismatch: " + ", ".join(mismatched))
    recorded, current = json.loads(SNAPSHOT.read_text()), source_hashes()
    changed = sorted(p for p in set(recorded) | set(current) if recorded.get(p) != current.get(p))
    if changed:
        raise ValueError("Review compatibility notes for changed source:\n" + "\n".join(changed)
                         + "\nAfter reviewing, run: python tools/check-compatibility.py --record-reviewed-source")
    print(f"Passed: {len(documented)} title IDs, {len(data['games'])} handlers, {len(current)} reviewed source files.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("data", nargs="?", type=Path, help="Rendered compatibility.json from Jekyll")
    parser.add_argument("--record-reviewed-source", action="store_true",
                        help="Refresh hashes only after reviewing affected compatibility notes")
    args = parser.parse_args()
    if args.record_reviewed_source:
        SNAPSHOT.write_text(json.dumps(source_hashes(), indent=2) + "\n", encoding="utf-8")
        print("Recorded reviewed source hashes; this does not change compatibility claims.")
    elif args.data:
        try:
            check(args.data)
        except (ValueError, KeyError) as error:
            raise SystemExit(str(error))
    else:
        parser.error("Provide the rendered data path or --record-reviewed-source.")
