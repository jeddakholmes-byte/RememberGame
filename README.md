# Remember Me — Act I

**CSS5190 Game Your Psychology: AI, Creativity & Mental Health**（香港中文大学（深圳），2026 秋季）期末项目的回合制叙事游戏，RPG Maker MV 1.6.1 制作。

这个仓库同时是**可玩的游戏**和**小组的工作台**：素材规范、剧本规范、文件清单、最新工程，都在这里。

---

## 你该看哪个文件

| 你是 | 看这个 | 里面有什么 |
|---|---|---|
| **美工** | [`art/README.md`](art/README.md) | 每类素材放在哪个文件夹、文件尺寸和格子怎么算、命名规则、现有素材清单、交付流程、投稿区 |
| **剧本** | [`script/README.md`](script/README.md) | 一句台词在游戏里怎么写、每行多少字、颜色码和表情索引、角色语气、可直接抄的模板、草稿放哪 |
| 想知道某个文件是干嘛的 | [`docs/FILE_GUIDE.md`](docs/FILE_GUIDE.md) | 仓库里每个文件夹、每个关键文件的用途，以及"能不能改" |
| 想知道某句话、某个开关在哪 | [`docs/GAME_REFERENCE.md`](docs/GAME_REFERENCE.md) | 两张地图的 23 个事件、8 个开关、通关流程 |
| 想读完整的剧本 | [`script/ACT1_SCRIPT.md`](script/ACT1_SCRIPT.md) | 第一幕全部游戏内文本，按出现顺序 |
| 要往仓库里加文件 | [`docs/DEPLOY.md`](docs/DEPLOY.md) | 怎么放、怎么提交、怎么发布上线 |
| 想直接玩 | <https://jeddakholmes-byte.github.io/RememberGame/> | 浏览器里直接玩，不用装任何东西 |

---

## 玩 / 打开

| | |
|---|---|
| **在线玩** | <https://jeddakholmes-byte.github.io/RememberGame/> |
| **游戏直链** | <https://jeddakholmes-byte.github.io/RememberGame/game/> |
| **用 MV 打开改** | 用 RPG Maker MV 1.6.1 打开 `game/Game.rpgproject` |
| **在自己电脑上跑** | `cd game && python3 -m http.server 8080`，然后开 <http://127.0.0.1:8080/>。**双击 `index.html` 打不开**，必须走 HTTP |

操作：**方向键**走，**Enter / 空格**看或说话，**Esc / X** 开菜单。画面 816 × 624，只用键盘。

---

## 仓库里有什么

```
RememberGame/
├── README.md              ← 你在这里
├── index.html             ← 落地页（GitHub Pages 的"开始游戏"入口）
├── art/                   ← 美工
│   ├── README.md              素材规范、尺寸、命名、清单、交付流程
│   ├── specs/                 自动生成的对照图（每个角色的 8 个表情、行走图格子）
│   └── incoming/              新素材先放这里，别直接扔进 game/
├── script/                ← 剧本
│   ├── README.md              写作规范、格式、语气、模板
│   ├── ACT1_SCRIPT.md         第一幕全文（从工程导出，只读）
│   └── drafts/                新剧本草稿
├── docs/                  ← 技术与运维
│   ├── FILE_GUIDE.md          每个文件是干嘛的
│   ├── GAME_REFERENCE.md      地图 / 事件 / 开关 / 流程
│   ├── DEPLOY.md              怎么加文件、怎么发布
│   ├── VERIFICATION.md        上线前的浏览器实跑记录
│   └── screenshots/           游戏截图
└── game/                  ← 游戏本体。1169 个文件，400 MB，RPG Maker MV 1.6.1 工程
```

**`game/` 就是完整的工程，没有删改过任何东西。** 它同时是两样东西：

- 用 RPG Maker MV 打开的工程，和
- GitHub Pages 正在托管的那份网页版。

这不是巧合。RPG Maker MV 的游戏本身就是 HTML5 + JavaScript，没有"转换"这一步——所以网页版是原版 1:1 的拷贝，而不是重写。

---

## 游戏是什么

**第一幕，单章。** Grace 七岁，在奶奶的葬礼上。她在墓园里走一圈，看墓碑，跟妈妈、爸爸、舅舅和爷爷说话，然后跟着爸爸进了一段闪回——奶奶走之前，爸爸和她最后那一次争吵。这一章结束在妈妈把奶奶留给 Grace 的盒子递给她。

| | |
|---|---|
| 主角 | Grace（`$RM_Grace`） |
| 地图 | `Act1_Cemetery`（Map001）和 `Act1_House_Flashback`（Map002），都是 24 × 18 |
| 事件指令 | 508 条 |
| 对白 | 81 个对话框、187 行、513 个词 |
| 时长 | 大约十分钟 |
| 开关 | 8 个，全部已命名 — 见 [`docs/GAME_REFERENCE.md`](docs/GAME_REFERENCE.md) |
| 结局 | `ACT I — END / The things Grandma left behind.`，然后回到标题画面 |

没有分支，不会失败。可选内容只有四段支线对话和花朵、旧墓碑的调查。

---

## 当前完成度 —— 动手改之前先看这段

这一版**不是最终版**。计划里的打磨做了一部分：

**已经做进去的。** 四个人物都有自己独立的行走图和脸图；爷爷的事件已经在 (16,7)；开场有 CONTROLS 操作说明卡；菜单和存档权限规则已经就位；妈妈那段长对话只会播一次；两处花共用一个开关；爸爸第 2 页的传送改到了 Map002 (12,13) 朝右。

**还没做进去的。** 闪回里还留着两次屏幕震动和两次 `RM_KnockHard` 音效，"后退一步"的修正也没有。事件上还没有 `<RMHint:>` 备注，`RM_Act1_Experience` 插件在 `js/plugins/` 里但没在 `js/plugins.js` 里打开——所以没有互动提示、也没有脚步声。`data/System.json` 目前还是 `locale: "zh_CN"` 配默认中文界面用语、`optFollowers: true`、标题音乐音量 90。017 号事件里那个拼错的 `abondoned` 还在。

**没有为了掩盖这些改过任何东西。** 仓库发布时，`game/` 里一个文件都没动过。

不过它是能跑通的。上线前在浏览器里从 New Game 一路打到 `ACT I — END` 再回到标题画面，没有脚本报错，也没有缺素材——记录在 [`docs/VERIFICATION.md`](docs/VERIFICATION.md)。

---

## 加文件之前

- **新素材先进 `art/incoming/`**，新剧本先进 `script/drafts/`。`game/` 里永远是能跑的干净版本，别直接往里扔半成品。
- **`game/img/` 和 `game/audio/` 里已有文件不要改名、不要覆盖。** 游戏是按文件名的字符串去找图的，改名不会报错，只会静默地不显示。
- **加完东西一定要跑一遍，并且从 New Game 开始。** 读旧存档会把旧的开关状态一起载进来，让正确的改动看起来是坏的。
- 提交和发布的具体步骤见 [`docs/DEPLOY.md`](docs/DEPLOY.md)。

---

## 版权与素材来源

用 **RPG Maker MV 1.6.1**（Kadokawa / Degica）制作。`game/img/` 和 `game/audio/` 里大部分文件是 RPG Maker MV 自带的 Run Time Package 素材，随工程一起分发是 RTP 本身的用途。

**这个游戏自己做的素材，文件名都以 `RM_` 开头**——标题画、两张远景底图、墓碑和遗物箱特写、七张脸图、四张行走图、碰撞图块、标题音乐，以及大约三十个音效。
