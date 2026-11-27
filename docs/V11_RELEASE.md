**English** · [简体中文](V11_RELEASE.zh-CN.md)

# v11 release candidate

Prepared November 27, 2026 for `jeddakholmes-byte/RememberGame`. The team page title is **Remember Me — v11**, feedback targets the team repository, and the candidate directory is `unity-v11/`. The intended public URL is `https://jeddakholmes-byte.github.io/RememberGame/unity-v11/`.

## Included work

- Act I: selected dialogue segmentation and pauses, Dad's response, and clearer transitions through the music-box memory and journey home.
- Act II: photograph call cue and fades, two seconds to observe the kitchen after fade-in, a reunion leading to Grace's decision to help, and a short approach toward Grandma.
- First ingredient: find flour, deliver it at the recipe book, receive Grandma's acknowledgement and visible flour beside the book. Other ingredients remain collectible early. A saved `flourSetOut` flag preserves this introduction; older later-stage saves bypass it.
- Earlier trial work: Grandma waits while Grace checks the exit, and the player returns manually from the parlor to fetch her.
- Web startup: versioned resource URLs and underlying startup error display. No new art or lid-performance changes are included.

## Source and build provenance

The six runtime files under `RememberMeUnity/Assets/Scripts/` match the six SHA-256 hashes recorded by the user-built second trial, committed as `1da4081`. `unity-v11/Build/` reuses that build byte-for-byte; its HTML is generated from the updated team template. No new Unity compilation or player packaging is claimed for the formal project. `unity-v11/build-info.json` records source and artifact hashes and the outstanding checks.

The local cached `upstream/main` is `e28a87d978c5d5d9f0c994a11c8057ddbee23ad7`. Before integration, the runtime scripts match that cached team source; only two Unity README files differ. A live remote check failed with a network/DNS error, so freshness is unverified. Fetch the current team branch and compare overlapping changes before merging.

## Files for a focused team submission

| Path | Purpose |
| --- | --- |
| `RememberMeUnity/Assets/Scripts/Act1Game.cs` | Act I pacing, transitions and WebGL compile compatibility |
| `RememberMeUnity/Assets/Scripts/KitchenPrototype.cs` | Kitchen narrative, first ingredient and manual return flow |
| `RememberMeUnity/Assets/Scripts/KitchenPresentation.cs` | Guidance visibility and memory fade |
| `RememberMeUnity/Assets/Scripts/KitchenCanvas.cs` | First ingredient checklist and flour image |
| `RememberMeUnity/Assets/Scripts/KitchenInventory.cs` | Introduction save persistence |
| `RememberMeUnity/Assets/Scripts/KitchenRouteCheck.cs` | Updated route assertions |
| `RememberMeUnity/Assets/WebGLTemplates/RememberMePages/index.html` | Team title, feedback and startup resource revision |
| `unity-v11/` | Complete candidate browser build, font license and provenance |
| `docs/V11_RELEASE.md`, `docs/V11_RELEASE.zh-CN.md` | Scope, evidence and release steps |
| `docs/UPLOAD.md`, `docs/UPLOAD.zh-CN.md` | Align future release paths with the deployment guide |

Keep personal `LaiDoc/`, `Trials/`, `narrative-trial-01/`, local environment configuration and personal build/preview helpers outside the focused team submission. The current fork's main branch contains earlier personal build commits; opening a PR directly from that branch includes those commits. Prepare a clean release branch from current team main and transfer only the listed changes. Review the complete PR file list.

## Verification and activation

Laia reports that Act I pacing/guidance is satisfactory and the revised Act II opening feels more comfortable. The startup failure reproduces in a normal browser window but the same public trial starts in an incognito window, implicating stale browser state. The earlier lid-stall report was attributed to the user's computer; performance work is deferred at their request.

Static checks cover integrated source hashes, identical binary artifacts, gzip integrity, Wasm validity, template expansion, team branding, resource references and version consistency. Updated automated Unity/browser route checks have not been run. This candidate is not yet published to the team repository.

Before activation, use `RememberMeUnity/QA/Verify-Web.cjs` with `REMEMBER_WEB_ROOT` pointing to `unity-v11/`, then manually check cemetery-to-kitchen transitions, first flour delivery, early ingredient collection, saves and manual return for Grandma. To rebuild from formal source, use the existing builder in `docs/DEPLOY.md`; its output directory name must match the filenames referenced by the resulting HTML.

After the checks pass, update `release.json` to v11 with the actual source commit and verification evidence, change the root play link and `unity/index.html` to `unity-v11/`, and update current release links in both READMEs. Those published-entry files intentionally still describe v10 in this preparation step. Record concise team release evidence in both handoff files.

Run `node tools/check-repo.cjs` before uploading and inspect its report. The existing six personal LaiDoc originals lack bilingual counterparts; exclude them from this submission rather than rewriting them as part of this release. Submit the focused branch to team `main`, merge through team review, inspect Pages deployment and verify the team v11 URL.

Suggested PR title: **Prepare v11 narrative pacing and kitchen onboarding**. Explain the included behavior above and distinguish manual trial feedback, static validation, and pending runtime/public verification.


Preparation evidence: integrated formal sources compiled successfully against Windows and WebGL references (existing serialized-field warnings remain). Candidate static integrity checks passed. Repository preflight was run and its report inspected: the six pre-existing personal LaiDoc originals lack Chinese pairs, and the checker requires a fresh game build/full browser QA for the formal source changes. Runtime QA and public deployment are still pending.


## Guided import with GitHub Desktop

Clone the team repository into a separate local folder, create and select `release/v11-narrative` from its current main, then run `Import-V11-To-Team.cmd` from the original personal checkout. Select the new team clone in the folder dialog. The helper requires the team origin, the named branch and a clean working tree. It compares all existing target file blobs with the prepared baseline and verifies source SHA-256 values before copying the 19-file allowlist. If team files changed, it stops before copying and lists them. It copies neither personal materials nor the importer itself, and does not commit, push or switch the default play entry. Syntax and manifest checks passed; actual import into a new team clone is pending.

After successful import, review the 19 changes, commit with `Prepare v11 narrative pacing and kitchen onboarding`, publish the branch and create a PR targeting team main. If publishing requires write access or a fork, resolve that prompt before proceeding. The direct v11 URL becomes available after merge and successful Pages deployment; the default play entry still follows release.json until activation.
