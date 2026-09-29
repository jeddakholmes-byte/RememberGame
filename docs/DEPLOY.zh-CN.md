[English](DEPLOY.md) · **简体中文**

# 发布并更新站点

仓库是 <https://github.com/jeddakholmes-byte/RememberGame>，已发布的站点是 <https://jeddakholmes-byte.github.io/RememberGame/>。

没有任何需要构建的东西。RPG Maker MV 工程就是静态 HTML5，GitHub Pages 提供的也是静态 HTML5，所以发布就意味着*把文件放进仓库*。

---

## 1. 已经就位的部分

| | |
|---|---|
| 仓库 | `jeddakholmes-byte/RememberGame`，**公开**，默认分支 `main` |
| 内容 | `README.md`、`index.html`、`art/`、`script/`、`docs/`、`game/`（MV 工程） |
| Pages | **已启用**，来源 = 分支 `main`，文件夹 `/ (root)` |
| 线上地址 | <https://jeddakholmes-byte.github.io/RememberGame/> |
| 游戏地址 | <https://jeddakholmes-byte.github.io/RememberGame/game/> |
| `.nojekyll` | 存在，因此 Pages 原样提供这些文件夹 |

**已于 2026-09-29 验证：** 已发布的站点对落地页、`/game/`、`game/index.html`、`game/data/Map001.json` 和 `game/img/titles1/RM_Title.png` 都返回 HTTP 200；在浏览器里加载 `/game/` 能到达标题画面，开始新游戏后进入地图 1 的 (12,12)，并且没有脚本错误、没有失败请求。

初始设置之后，每次推送到 `main` 站点都会自动重新部署——通常一分钟内完成。

---

## 2. 当时用到的命令

```bash
cd /Users/leon/Downloads/Game_Your_Psychology/final_project/RememberGame

git init -b main
git add -A
git commit -m "Remember Me — Act I: RPG Maker MV 1.6.1 build, team documentation, GitHub Pages setup"

git remote add origin https://github.com/jeddakholmes-byte/RememberGame.git
git push -u origin main
```

首次推送约 440 MB，几乎全是 `game/`。网速慢的话要做好等几分钟的准备。这是一次性成本——之后的提交只发送变化的部分。

---

## 3. 在另一台机器上从零做一遍

如果你需要重新创建这个仓库，完整流程如下。

### 3.1 一次性设置

1. **安装 Git。** macOS：在终端里运行 `git --version`；如果它报错，接受 Xcode Command Line Tools 的安装。Windows：安装 [Git for Windows](https://git-scm.com/download/win)。
2. **告诉 Git 你是谁：**
   ```bash
   git config --global user.name "Your Name"
   git config --global user.email "your@email.com"
   ```
3. **认证。** GitHub 不再接受账号密码用于 Git。在 <https://github.com/settings/tokens> 创建一个**细粒度个人访问令牌**，权限设为 *Repository access → RememberGame* 和 *Contents: Read and write*。当 `git push` 索要密码时，粘贴该令牌。macOS 会把它记在钥匙串里。
4. **拿到文件。** `git clone https://github.com/jeddakholmes-byte/RememberGame.git`。MV 工程在 `game/` 里；其余都是文档。
5. **提交前先删掉 macOS 垃圾文件：**
   ```bash
   find . -name ".DS_Store" -delete
   ```

### 3.2 提交并推送

```bash
git add -A
git status              # 检查一下：你应该看到 game/, docs/, README.md, index.html
git commit -m "Initial import of Act I and documentation"
git branch -M main
git remote add origin https://github.com/jeddakholmes-byte/RememberGame.git
git push -u origin main
```

### 3.3 如果仓库在 GitHub 上还不存在，就创建它

1. <https://github.com/new>
2. 名称：`RememberGame`。可见性：**Public**（免费账号的 GitHub Pages 只能服务公开仓库）。
3. **不要**添加 README、`.gitignore` 或许可证——你已经有了文件，一个初始提交会让你的首次推送产生冲突。
4. 创建，然后运行 §3.2 里的推送。

---

## 4. 打开 GitHub Pages（只需一次）

1. 打开 <https://github.com/jeddakholmes-byte/RememberGame/settings/pages>。
2. **Source**：*Deploy from a branch*。
3. **Branch**：`main`，文件夹 **`/ (root)`**。保存。
4. 等待 1–2 分钟。页面会显示线上地址：<https://jeddakholmes-byte.github.io/RememberGame/>。
5. 部署之后的第一次访问可能 404 一分钟以内。用 `Cmd + Shift + R` 强制刷新。

用 API 做同样的事：

```bash
curl -X POST \
  -H "Authorization: Bearer <TOKEN>" \
  -H "Accept: application/vnd.github+json" \
  https://api.github.com/repos/jeddakholmes-byte/RememberGame/pages \
  -d '{"source":{"branch":"main","path":"/"}}'
```

---

## 5. 添加文件，以及更新站点

### 新文件放在哪里

| 你手上的东西 | 放进 | 说明 |
|---|---|---|
| 新美术资源 | `art/incoming/` | 不要直接放进 `game/`。命名规则在 `art/incoming/README.md` |
| 新剧本，或对现有台词的一处改动 | `script/drafts/` | 一个文件一个场景。`script/drafts/README.md` 展示了如何提议修改现有台词 |
| 已完工、可以直接进游戏的素材 | `game/img/…` 或 `game/audio/…` | 只有在对照 `art/README.md` 检查过之后才能放。**绝不要重命名或删除已有文件**——游戏是按文件名字符串查找素材的 |
| 已完工、可以直接进游戏的剧本 | RPG Maker MV 编辑器里对应地图的事件 | 然后重新导出 `script/ACT1_SCRIPT.md`，让仓库里的文本仍与游戏一致 |

### 提交一次改动

任何改动之后：

```bash
cd path/to/RememberGame
git add -A
git commit -m "Act I: <what changed>"
git push
```

GitHub Pages 会自动重新部署，通常一分钟内完成。想看进度：<https://github.com/jeddakholmes-byte/RememberGame/actions>。

**如果你是在 RPG Maker MV 编辑器里改的**，先保存工程并关闭编辑器，再提交。MV 每次保存都会重写整个 JSON 文件，所以一处小改动可能表现为很大的 diff——这很正常，不是文件损坏。

**改动任何美术或剧本之后，提交前先以新游戏跑一遍。** 从章节中途的存档开始会恢复旧的开关状态，让正确的改动看起来是坏的。

### 不要提交这些

`.DS_Store` 和 `._*` 已经由 `.gitignore` 忽略。也不要提交自己试玩产生的 `save/*.rpgsave` 文件，更不要提交个人访问令牌。

---

## 6. 出问题的时候

| 现象 | 原因 | 处理 |
|---|---|---|
| `remote: Support for password authentication was removed` | 你输入了 GitHub 密码 | 使用个人访问令牌（§3.1） |
| `error: File ... is 116 MB; this exceeds GitHub's file size limit of 100 MB` | 单个文件太大。本工程最大的文件是 7.7 MB，所以不该触发这个限制 | 用 `find . -type f -size +50M` 找到它，删掉或压缩 |
| 推送卡住，然后 `RPC failed; HTTP 408` | 440 MB 的推送在慢速链路上超时 | 重试——Git 会续传。如果一直失败，提高缓冲区：`git config http.postBuffer 524288000` |
| 推送被拒：`non-fast-forward` | 别人向仓库提交过 | 先 `git pull --rebase origin main`，再推送 |
| Pages 地址返回 404 | Pages 未启用，或者还没过 60 秒 | §4 |
| 站点能加载但游戏是黑屏 | 通常是浏览器缓存了旧的 `index.html`，或者你打开的是 `file:///` | 强制刷新；在本地用 HTTP 提供服务 |
| 只有落地页能用，`/game/` 返回 404 | `.nojekyll` 被删了，或 `game/index.html` 被改名 | 恢复 `.nojekyll` 和该文件名 |
| Pages 构建因 Jekyll 错误失败 | 某个文件的文件名是 Jekyll 不接受的（含 `#`、`%` 或 `:`），或者 `.nojekyll` 缺失 | 恢复 `.nojekyll` |
| 队友克隆下来的仓库巨大 | 仓库有 440 MB，因为它带着完整的 RPG Maker MV 运行库 | 属正常。浅克隆快得多：`git clone --depth 1 <url>` |

---

## 7. 关于体积

`game/` 有 400 MB，其中约 330 MB 是 **RPG Maker MV 运行库**——战斗背景、敌人图、战斗动画和音乐，这个游戏一个都用不到。它是有意留在仓库里的：那是 MV 编辑器在每个下拉列表里都会提供的公共素材库，删掉它，就等于从下一位编辑者手里拿走可选项。

如果你需要的是一个精简的分发版本，而不是这个工作仓库，就用 RPG Maker MV 自带的 **File → Deployment → Web Browsers**，并勾选 **Exclude unused files**。这样产出大约 20–40 MB，并安全地移除未使用的素材。请在一个用完就丢的副本里做，不要在本仓库里做。
