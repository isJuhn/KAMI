# KAMI website

A Jekyll site for GitHub Pages: two layouts, one stylesheet, and one small theme script. No external fonts, analytics, remote theme, or frontend package manager. The landing page, compatibility list, quickstart, developer guide and about page are all rendered at build time. Navigation, content, native game disclosures and system color preference work without JavaScript.

## Publish and migrate

1. Merge the website changes into KAMI's default branch.
2. In Settings → Pages, select **Deploy from a branch**, select the default branch, and choose **/docs**. Wait for the Pages build to succeed.
3. Verify `https://isjuhn.github.io/KAMI/` and its compatibility, quickstart, developers, about and 404 pages.
4. Only then merge the companion redirect in `isJuhn/isJuhn.github.io` and the retirement notice in `isJuhn/KAMI-Docs`. The legacy root must keep a working link to `/KAMI/compatibility/`; do not redirect it before the new page exists. Stop publishing MkDocs output over that redirect.

Preview on a fork before moving upstream. `baseurl: /KAMI` works for both the upstream project and a fork with the same repository name. GitHub Pages supplies the origin. Change `baseurl` if the repository is renamed; use an empty value for a root user site or custom root domain. All internal links use Jekyll URL filters. Do not add `.nojekyll`.

## Edit content

- `index.html`: landing page and work-in-progress screenshot.
- `compatibility.html` and `_data/compatibility.yml`: structured support information and historical reports. Keep title IDs, version restrictions, source evidence and release/development distinctions together.
- `quickstart.md`, `developers.md`, `about.md`: user instructions, contribution guide, history and license.
- `_config.yml`: navigation, metadata, project links and base path.
- `_layouts/`: shared shell and article layout.
- `assets/css/site.css`: both color themes and responsive layouts.
- `assets/js/theme.js`: optional System / Light / Dark selection. System follows live OS changes; explicit choices persist in local storage. Selecting System removes the override. Storage failure does not prevent selection on the current page.

The download button points to the latest GitHub release, not a hard-coded asset. Keep runtime requirements in sync with that release. Development support must not be presented as released or gameplay-tested without evidence. The full MIT notice is reproduced on the about page; preserve it when editing.

The screenshot is labeled as work in progress. CSS clips its 30 px OS title bar; replace the image and remove that rule when a clean capture of the released interface is available.

## What is generated, and what needs review?

No AI service is involved in building or maintaining the site. All published output comes from the checked-in source through Jekyll.

| Content | Source of truth | Maintenance |
| --- | --- | --- |
| Navigation, page shell, metadata and internal URLs | `_config.yml` and `_layouts/` | Generated for every page at build time. |
| Compatibility entries, title counts and machine-readable `compatibility.json` | `_data/compatibility.yml` | Rendered automatically; edit the data once. |
| Supported title IDs and their handlers | Application `GameManager.cs` | CI compares the documented IDs and handlers against the code. |
| Version restrictions, release distinctions, limitations and historical ratings | Reviewed source, release notes and testing reports | Curated. CI flags changes to game/camera code for review; it cannot infer playability. |
| Quickstart, build instructions, runtime requirements and project history | Markdown pages, current release and build files | Review when controls, requirements or releases change. These are not scraped automatically. |
| Screenshot and social preview | `assets/images/` | Replace deliberately; the screenshot remains labeled as work in progress. |
| Download destination | GitHub's `/releases/latest` | Follows the latest release without a site edit. |

When game or camera code changes, the Website check asks for a compatibility review. Update affected notes, source links and release information, then run `python tools/check-compatibility.py --record-reviewed-source` and commit the resulting hash file. Hashes normalize line endings and contain repository-relative paths only. Recording hashes acknowledges a review; it does not generate or certify gameplay claims. Even source-only changes that leave title IDs unchanged are flagged.

When making a release, revisit the quickstart's release tag, runtime requirement, compatibility release metadata, and any source-only notes that have become released. Keep the history selective rather than duplicating every release note.

## Optional local preview

With an existing Ruby/Bundler installation, review `Gemfile`, then run `bundle install` and `bundle exec jekyll serve` from this directory. These are optional maintainer installation steps; GitHub provides the hosted Jekyll build. Commit `Gemfile.lock` if the team chooses to freeze local dependencies.

## Validation

The **Website** GitHub Actions check builds with GitHub's official Jekyll action and runs all three checks on relevant pushes and pull requests. It has read-only permissions and does not deploy. GitHub Pages remains responsible for deployment. Action revisions are pinned; the build toolchain is GitHub's Pages toolchain, not a separate frontend dependency tree.

After building locally, run these from the repository root:

```console
python tools/check-site.py docs/_site --base /KAMI
python tools/check-compatibility.py docs/_site/compatibility.json
node tools/check-theme.cjs
```

These check rendered HTML, internal links and fragments, assets, required pages, title ID/handler coverage, reviewed game/camera source, and theme behavior (system changes, saved choices, unavailable storage and reloads). They use only Python/Node standard libraries. They do not run emulator gameplay tests or validate external websites.

Check 320 px, 390 px and desktop layouts, both themes, keyboard focus, and all content with scripts blocked. No menu, game list or download action should require JavaScript. Verify the published base-path URLs and custom 404. The social preview intentionally retains the light brand presentation in both themes.

Before committing, inspect the diff for personal paths, private names or email addresses, credentials and image metadata. Use a public GitHub handle and noreply address for attribution. Keep local servers, generated builds and logs out of the source directory.
