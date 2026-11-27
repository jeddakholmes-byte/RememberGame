**English** · [简体中文](UPLOAD.zh-CN.md)

# Repeatable uploads

Review the changes and verification summary for each upload. Reuse the project's build, test and release entry points.

## Run the preflight

From the repository root, run node tools/check-repo.cjs. It prints a one-line summary and writes the detailed file list and errors to the local RememberMeUnity/Build/Review/upload-report.json.

The script checks document links, changed CJS syntax, published web resources, the GitHub per-file size threshold and recorded browser verification. It compares game files with the published source commit in release.json and identifies changes that need a fresh build and playtest.

Start with the summary, then read the listed files as needed. Use the [handoff](HANDOFF.md) and [interaction rules](../RememberMeUnity/Docs/Interaction.md) for implementation context.

## Select verification by change

| Change | Verification |
| --- | --- |
| README or contributor documentation | Preflight and document diff |
| Archived web builds, 404 or redirect script | Preflight and node tools/check-archive-links.cjs |
| Published build, play entry or release.json | Preflight and public web-entry check |
| Game code, assets, dependencies or project settings | Preflight, build, full browser route and manual checks of the affected actions |

staticChecks reports static verification. requiredChecks lists browser or gameplay verification to complete separately.

## Release layout

- v10 is stored in unity-v10/. Earlier v2–v9 builds are in archive/web/.
- Place future releases in unity-v11/, unity-v12/ and subsequent version directories.
- release.json records the published version, build directory, source commit, URL and full verification record. Update it and the play entries after the new build passes verification.
- Add each handoff's changes, verification and remaining tasks to HANDOFF.md and HANDOFF.zh-CN.md, with concise evidence.

## Commit and upload

Run git fetch origin and check for remote changes. Review the reported diff, stage the task's files, commit and push the task branch, then follow the contributor review workflow.

For releases, use the [deployment guide](DEPLOY.md) to check the output, entries and version record. After pushing, inspect the Pages run for that commit and open the public link.

A verified source revision can retain its verification record through later documentation edits. Rebuild and run the relevant verification when game files change.

## Bilingual documents

Maintain an English default file and its Chinese counterpart together. Run node tools/check-languages.cjs to check pairs, reciprocal links and English default titles. The upload preflight includes this check. Review project facts, procedural conditions and direct, evidence-based prose in both languages.
