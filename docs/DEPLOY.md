# Publish and update the site

The repository is <https://github.com/jeddakholmes-byte/RememberGame> and the published site is <https://jeddakholmes-byte.github.io/RememberGame/>.

There is nothing to build. RPG Maker MV projects are static HTML5, GitHub Pages serves static HTML5, so publishing means *put the files in the repository*.

---

## 1. What is already in place

| | |
|---|---|
| Repository | `jeddakholmes-byte/RememberGame`, **public**, default branch `main` |
| Contents | `README.md`, `index.html`, `docs/`, `game/` (the MV project), `enhancement/` |
| Pages entry point | repository root; `/` serves the landing page, `/game/` serves the game |
| `.nojekyll` | present, so Pages serves the folders as-is |

If the site is not live yet, do §4 once. After that it redeploys automatically on every push.

---

## 2. The commands that were used

```bash
cd /Users/leon/Downloads/Game_Your_Psychology/final_project/RememberGame

git init -b main
git add -A
git commit -m "Remember Me — Act I: RPG Maker MV 1.6.1 build, team documentation, GitHub Pages setup"

git remote add origin https://github.com/jeddakholmes-byte/RememberGame.git
git push -u origin main
```

The push is roughly 440 MB (1169 files in `game/`, 53 in `enhancement/`). Expect a few minutes on a slow connection. It is a one-time cost — later commits only send what changed.

---

## 3. Doing it from scratch on another machine

If you ever need to re-create this repository, this is the whole procedure.

### 3.1 One-time setup

1. **Install Git.** macOS: run `git --version` in Terminal; if it complains, accept the Xcode Command Line Tools install. Windows: install [Git for Windows](https://git-scm.com/download/win).
2. **Tell Git who you are:**
   ```bash
   git config --global user.name "Your Name"
   git config --global user.email "your@email.com"
   ```
3. **Authenticate.** GitHub stopped accepting account passwords for Git. Create a **fine-grained personal access token** at <https://github.com/settings/tokens> with *Repository access → RememberGame* and *Contents: Read and write*. When `git push` asks for a password, paste the token. macOS will remember it in the Keychain.
4. **Get the files.** Either `git clone https://github.com/jeddakholmes-byte/RememberGame.git`, or build the folder yourself: copy the RPG Maker MV project to `game/`, the enhancement package to `enhancement/`, and the documents to `docs/`.
5. **Delete macOS junk before you commit:**
   ```bash
   find . -name ".DS_Store" -delete
   ```

### 3.2 Commit and push

```bash
git add -A
git status              # sanity check: you should see game/, docs/, README.md, index.html
git commit -m "Initial import of Act I and documentation"
git branch -M main
git remote add origin https://github.com/jeddakholmes-byte/RememberGame.git
git push -u origin main
```

### 3.3 Create the repository on GitHub, if it does not exist

1. <https://github.com/new>
2. Name: `RememberGame`. Visibility: **Public** (GitHub Pages on a free account only serves public repositories).
3. **Do not** add a README, `.gitignore` or licence — you already have files, and an initial commit would make your first push conflict.
4. Create, then run the push from §3.2.

---

## 4. Turn on GitHub Pages (once)

1. Open <https://github.com/jeddakholmes-byte/RememberGame/settings/pages>.
2. **Source**: *Deploy from a branch*.
3. **Branch**: `main`, folder **`/ (root)`**. Save.
4. Wait 1–2 minutes. The page will show the live URL: <https://jeddakholmes-byte.github.io/RememberGame/>.
5. The first visit after a deploy can 404 for up to a minute. Hard-refresh with `Cmd + Shift + R`.

The same thing over the API:

```bash
curl -X POST \
  -H "Authorization: Bearer <TOKEN>" \
  -H "Accept: application/vnd.github+json" \
  https://api.github.com/repos/jeddakholmes-byte/RememberGame/pages \
  -d '{"source":{"branch":"main","path":"/"}}'
```

---

## 5. Updating the site

Edits go in `game/`, documentation goes in `docs/`. After any change:

```bash
cd path/to/RememberGame
git add -A
git commit -m "Act I: <what changed>"
git push
```

GitHub Pages redeploys automatically, usually within a minute. To watch it: <https://github.com/jeddakholmes-byte/RememberGame/actions>.

**If you edited in the RPG Maker MV editor**, save the project first and close the editor, then commit. MV rewrites the whole JSON files on every save, so one small change can show up as a large diff — that is normal, not corruption.

### Do not commit these

`.DS_Store` and `._*` are already ignored by `.gitignore`. Also avoid committing `save/*.rpgsave` files from your own playtesting, and never commit a personal access token.

---

## 6. When something goes wrong

| Symptom | Cause | Fix |
|---|---|---|
| `remote: Support for password authentication was removed` | You typed your GitHub password | Use a personal access token (§3.1) |
| `error: File ... is 116 MB; this exceeds GitHub's file size limit of 100 MB` | A single file is too big. This project's largest file is 7.7 MB, so the limit should never trigger | Find it with `find . -type f -size +50M`, remove or compress it |
| Push hangs, then `RPC failed; HTTP 408` | The 440 MB push timed out on a slow link | Retry — Git resumes. If it keeps failing, raise the buffer: `git config http.postBuffer 524288000` |
| Push rejected: `non-fast-forward` | Someone else committed to the repository | `git pull --rebase origin main` then push |
| Pages URL gives 404 | Pages not enabled, or fewer than 60 seconds have passed | §4 |
| Site loads but the game is a black screen | Usually the browser cached an old `index.html`, or you are opening `file:///` | Hard-refresh; serve over HTTP locally |
| Only the landing page works, `/game/` 404s | `.nojekyll` was deleted, or `game/index.html` was renamed | Restore `.nojekyll` and the filename |
| The Pages build fails with a Jekyll error | A file has a name Jekyll dislikes (a `#`, `%` or `:`), or `.nojekyll` is missing | Restore `.nojekyll` |
| A teammate's clone is enormous | The repository is 440 MB because it carries the full RPG Maker MV run-time library | Expected. Shallow clone is much faster: `git clone --depth 1 <url>` |

---

## 7. Notes on size

`game/` is 400 MB, and about 330 MB of that is the **RPG Maker MV run-time package** — battle backgrounds, enemy art, battle animations and music that this game never uses. It stays in the repository on purpose: it is the pooled library the MV editor offers in every dropdown, so removing it removes choices from whoever edits next.

If you ever need a small distribution build instead of the working repository, use RPG Maker MV's own **File → Deployment → Web Browsers** with **Exclude unused files** ticked. That produces roughly 20–40 MB and removes the unused assets safely. Do that in a throwaway copy, not in this repository.
