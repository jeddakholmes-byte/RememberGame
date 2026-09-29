[English](GAME_REFERENCE.md) · **简体中文**

# 游戏参考手册

游戏内部的线路图。改动任何事件之前，先读这份文档。

以下内容全部来自 `game/data/Map001.json`、`Map002.json`、`System.json` 和 `Tilesets.json`。改动游戏后，也要改这份文档——否则它就不再成立。

---

## 1. 一览

| | |
|---|---|
| 标题 | `RememberMe` |
| 引擎 | RPG Maker MV 1.6.1 |
| 画面 | 816 × 624（`Community_Basic` 插件） |
| 主角 | Grace，角色 1，行走图 `$RM_Grace`，脸图 `RM_Face_Grace` 第 3 帧 |
| 地图 1 | `Act1_Cemetery`——24 × 18，远景图 `!RM_Cemetery`，BGM `RM_Act1_Cemetery`，环境音 `RM_Rain` |
| 地图 2 | `Act1_House_Flashback`——24 × 18，远景图 `!RM_House`，BGM `RM_Flashback`，环境音 `RM_Room` |
| 图块组 | `Act1_TransparentCollision`——一层不可见的碰撞层；画面由远景图提供 |
| 起始位置 | 地图 1，(12,12)，朝下 |
| 开关 | 8 个，均已命名 |
| 变量 | 1 个（`0001`，旧坟计数器） |
| 事件指令总数 | 两张地图合计 508 条 |
| 战斗 | 无 |

---

## 2. 地图 1——`Act1_Cemetery`（`data/Map001.json`）

20 个事件。位置为图块坐标。

| # | 事件 | 位置 | 触发 | 作用 | 事件页 |
|---|---|---|---|---|---|
| 1 | `OpeningController` | (0,0) | **自动执行** | 冷开场。淡出画面，开启玩家队伍跟随，禁止存档，启动雨声环境音，然后播放 Grace 的三句开场台词，打开雨声，淡入画面，以音量 32 播放墓地 BGM，让 Mom 说话，让 Mom 走两步，显示 CONTROLS 卡片，把开关 1 设为 ON，禁止菜单。 | p1 自动执行（整段场景），p2 确定键，开关 1 为 ON 后为空 |
| 2 | `Mom` | (13,12) | 确定键 | 首次对话：四句来回（"Mom?" → "Are we going home soon?" → "Where is Grandma now?"），然后 "You can look at Grandma's stone. I'll be right here."。设置自身开关 A，所以此后她只说那句短台词。事件页 2 是一个自动执行页，在开关 6（`MomCall`）为 ON 且 Grace 尚未看过墓碑时触发——Mom 把她叫过去。 | p1 对话，p2 自动执行 |
| 3 | `Dad` | (14,7) | 确定键 | 事件页 1：Grace 说 "Dad?"，Dad 说 "…"，Grace 旁白 "He's looking at Grandma's stone."。事件页 2（开关 2 之后）：真正的对话——Dad 转身，"Hey, Gracie."，"Are you crying?"，"A little."，"Why?"，"Because I miss Grandma."——接着门吱呀作响，菜单恢复，淡出，开关 3 设为 ON，场所移动到地图 2 的 (12,13)，朝右。事件页 3 是闪回之后的空白页。 | p1, p2, p3 |
| 4 | `Gravestone_Main` | (11,6) | 确定键 | 墓碑特写。在 30 帧内淡入 `RM_Gravestone`，播放 `RM_Chime`，然后是 Grace 关于名字和照片的五句台词。擦除图片之前，先在 24 帧内把图片淡出。把开关 2 设为 ON（`VisitedGrave`）。Dad 转身擦脸。 | p1 首次细读，p2 简短重读 |
| 5 | `WhiteFlowers` | (10,6) | 确定键 | 主墓碑周围的花。首次（开关 8 为 OFF）是长版本，之后是一句话版本。把开关 8 设为 ON。 | p1 |
| 6 | `OldGrave_WitheredFlowers` | (5,10) | 确定键 | "I don't know this person. The flowers here seem withered." 计入旧坟。 | p1 |
| 7 | `UncleJames` | (8,9) | 确定键 | 一句台词："Take your time, Grace." | p1 |
| 8 | `Aunt` | (17,10) | 确定键 | 一句台词，无名字："She holds a tissue in both hands." | p1 |
| 9 | `Bench_RelicBox` | (5,13) | 确定键 | "Mom's bag is by the bench. There is a little box, too." | p1 |
| 10 | `AfterFlashback_EndAct` | (1,0) | **自动执行** | 终章，也是第一幕最后的内容。见 §4。 | p1 空，p2 结局，p3 开关 5 为 ON 后为空 |
| 11 | `ExitLeft` | (11,16) | 玩家接触 | 挡住大门：Mom 说 "Stay inside the gate, Grace."，并把 Grace 往回推。 | p1 |
| 12 | `ExitRight` | (12,16) | 玩家接触 | 与 11 相同。 | p1 |
| 13 | `Gravestone_Right` | (12,6) | 确定键 | 同一块墓碑的第二个靠近格。文字和图片淡入淡出与事件 4 完全一致——**要么两处都改，要么都不改**。 | p1, p2 |
| 14 | `Ambience` | (0,0) | **并行处理** | 声音底噪：等待约 15 秒，播放 `RM_WindGust`；等待约 20 秒，`RM_CrowCaw`；等待约 25 秒，`RM_WindGust`。开关 1 为 ON 后循环。 | p1 空，p2 并行循环 |
| 15 | `Bench_RelicBox` | (4,13) | 确定键 | 长椅的第二个靠近格。文字与事件 9 相同。 | p1 |
| 16 | `OldGrave_3` | (5,4) | 确定键 | "There is nothing here." 计入旧坟。 | p1 |
| 17 | `OldGrave_4` | (18,4) | 确定键 | "This grave… It seems to be abondoned a long time ago." 计入旧坟。**拼写错误 `abondoned` 已经随游戏发布**——计划中的润色会修掉它，但它目前仍在 `game/` 里。 | p1 |
| 18 | `WhiteFlowers` | (13,6) | 确定键 | 第二簇花。与事件 5 完全相同，共用开关 8。 | p1 |
| 19 | `OldGrave_2` | (18,10) | 确定键 | "I can barely read the name on it." 计入旧坟。 | p1 |
| 20 | `Grandpa` | (16,7) | 确定键 | 在角色梳理阶段加入。行走图 `$RM_Grandpa`，朝左，脸图 `RM_Face_Grandpa` 索引 6，一句旁白："Grandpa is standing very still." 无开关。 | p1 |

有两对是刻意重复的，好让 Grace 从任意一侧都能交互：事件 4/13（墓碑）和 9/15（长椅）。改动其中一个，就同步改另一个。

---

## 3. 地图 2——`Act1_House_Flashback`（`data/Map002.json`）

3 个事件。这张地图是一段脚本化场景，不是可以探索的地方。

| # | 事件 | 位置 | 触发 | 作用 |
|---|---|---|---|---|
| 1 | `FlashbackController` | (0,0) | **自动执行** | 整段闪回以及返回地图的过程。在开关 3（`FlashbackActive`）为 ON 时运行。见 §4。 |
| 2 | `Grandma` | (12,6) | 确定键 | 行走图 `RM_Family` 第 2 帧，朝上。没有任何指令——由控制器移动她。 |
| 3 | `FlashbackDad` | (11,7) | 确定键 | 行走图 `RM_Family` 第 0 帧，朝上。没有任何指令——由控制器移动。 |

房屋内部是 `!RM_House` 远景图；镜头位置在脚本里用 `$gameMap.setDisplayPos(3.5, 3)` 设置。

---

## 4. 两段脚本化的重头戏

### 4.1 `FlashbackController`（地图 2，事件 1）

作为一段连续的自动执行运行：

1. 把画面压暗为冷色调，`setDisplayPos(3.5, 3)`，启动 `RM_Room`（BGS，音量 16）和 `RM_Flashback`（BGM，音量 22），淡入画面。
2. Dad 走一步，等待 24 帧。
3. Grandma：*"I need to go home."* Dad：*"Mom. You are home."*
4. Dad 向左走一步。门把手音效。Grandma：*"No."* Grandma 移动；Dad：*"We talked about this."*
5. 两次门把手撞击，Grandma：*"Grace?"*
6. Dad：*"Mom, please—"* 手掌拍门，等待，**震动画面**，`RM_KnockHard` 以音量 62 播放。
7. Grandma：*"Let me go home!"* **再次震动画面**，再次 `RM_KnockHard`，等待。
8. Dad：*"I CAN'T DO THIS AGAIN."*
9. 淡出 BGM 和 BGS，停止 SE，等待，Dad 用 `RM_Step1` 走开，淡出画面。
10. 开关 3 设为 OFF，开关 4 设为 ON，返回地图 1 的 (13,7)。

第 6 步和第 7 步——两次震动画面和两次重击——正是计划中的润色要删掉的部分。**它们目前还在**，音量分别为 50 和 70。

### 4.2 `AfterFlashback_EndAct`（地图 1，事件 10，事件页 2）

开关 4（`FlashbackComplete`）为 ON 后自动执行一次：

1. 菜单恢复，画面色调还原，雨声和 `RM_Rain` 重启，镜头滚动到 Dad，淡入画面，墓地 BGM 以音量 45 播放。
2. Dad：*"Grace?"* … *"You okay?"* Grace：*"… Yeah."* Dad 走开。
3. 章节的转折——Grace：*"Dad got angry with Grandma sometimes."* / *"Grandma isn't here anymore. And Dad is crying."* / *"I don't understand."*
4. Mom 出现在 Grace 身旁，说 *"Grace. We're going home now."*
5. **交接的切换镜头。** 淡出画面，在 1 秒内把墓地 BGM 淡出，把 Mom 移到 (8,14) 朝右，把 Grace 场所移动到 (9,14) 朝左且不带淡入淡出，再淡入画面。正是这段黑场让 Mom 读起来像是自己走了过来，而不是瞬移——它为什么不能删，见 §8。
6. `RM_Box` 启动，然后 Mom：*"I brought this from Grandma's house. Your name is on it."*
7. `RM_Box` 在 30 帧内淡入，`RM_Cardboard` + `RM_Swell`，Mom：*"For Grace."* / *"We found it with her things. Maybe she wanted you to have it."*
8. Grace：*"Can I open it?"* Mom：*"When we get home. Let's keep it dry."*
9. 淡出画面，擦除图片，两句收尾旁白，然后是卡片：**`ACT I — END` / `The things Grandma left behind.`**
10. 全部淡出，`code 354`（返回标题画面）。**注意：** 这里的开关操作指令被设为范围 **0001–0005 = ON**；原本只打开了 0005。放宽范围会重新启动环境音循环，并点亮三个结局根本用不到的开关。

---

## 5. 开关

八个开关都在 `System.json` 里命名。标为*"需要 ON"*的事件页，只有在该开关为 ON 时才会成为活动事件页；如果有多个事件页满足条件，**编号最大的事件页优先**。

| # | 名称 | 由谁设为 ON | 由谁读取 | 含义 |
|---|---|---|---|---|
| 1 | `OpeningComplete` | 末尾的 `OpeningController` p1；`AfterFlashback_EndAct` p2 | `OpeningController` p2，`Ambience` p2 | 冷开场已经结束。让环境音循环在整个章节持续运行，并在结尾停掉它。 |
| 2 | `VisitedGrave` | `Gravestone_Main` p1，`Gravestone_Right` p1 | `Dad` p2（事件页条件），`Mom` p2，两个墓碑的 p2 事件页 | Grace 已读过 Grandma 的墓碑。**这是解锁闪回的闸门**——在它为 ON 之前，Dad 不会敞开心扉。 |
| 3 | `FlashbackActive` | `Dad` p2，紧接在场所移动之前 | `FlashbackController` p1（自动执行）；由该事件清除 | "我们正在闪回里。" |
| 4 | `FlashbackComplete` | 末尾的 `FlashbackController` p1 | `AfterFlashback_EndAct` p2（自动执行），`Dad` p3，`FlashbackController` p2 | 闪回已经播放过。 |
| 5 | `Act1Complete` | `AfterFlashback_EndAct` p2 | `AfterFlashback_EndAct` p3 | 第一幕结束。阻止结局重播。 |
| 6 | `MomCall` | `OldGrave_WitheredFlowers`、`OldGrave_2`、`OldGrave_3`、`OldGrave_4`——四个中任意一个 | `Mom` p2（自动执行），`Gravestone_Main` p2，`Gravestone_Right` p2，`Mom` p2 清除它 | 玩家已经看过足够多的支线旧坟，所以 Mom 把 Grace 叫到真正的墓碑前。 |
| 7 | `MomHintDelivered` | `Mom` p2 | `Mom` p2 | 一次性防护，让 Mom 那句 "Grandma's stone is near Dad" 只播放一次。 |
| 8 | `FlowerSeen` | 两个 `WhiteFlowers` 事件 | 两个 `WhiteFlowers` 事件 | 两簇花共用，使长版本播放一次，之后播放短版本。 |

### 变量

| # | 名称 | 含义 |
|---|---|---|
| 1 | `0001` | 已查看的旧坟计数器。由 `OldGrave_WitheredFlowers`、`OldGrave_2`、`OldGrave_3`、`OldGrave_4` 递增。达到 4 后，这些事件不再设置 `MomCall`。 |

计数器与开关 6 的职责有重叠：开关 6 让 Mom 在*第一座*旧坟之后就叫走 Grace；计数器把这一行为封顶在四次。

---

## 6. 通关流程

1. 章节以淡入开始。Grace 的旁白，然后是 Mom 的警告，然后是一张 CONTROLS 卡片。
2. **和 Mom 对话**（13,12）——可选，但它立起 "Where is Grandma now?" 这条线。
3. **读 Grandma 的墓碑**（11,6）或（12,6）。这会设置 `VisitedGrave`。也可以先查看两簇花和四座旧坟；看过任意一座旧坟后，Mom 会把 Grace 叫过去。
4. **和 Dad 对话**（14,7）。完整对话只会在第 3 步之后播放。它以一声门的吱呀和一次场所移动结束。
5. **闪回**自行播放。无需输入。
6. **回到墓地**后是结局自动执行：Dad 问 Grace 是否还好，Grace 的领悟，然后一段黑场把 Grace 移到长椅旁的 Mom 身边，接着是盒子交接。`ACT I — END`。
7. Grace 随时可以和 Uncle James（8,9）、Aunt（17,10）、Grandpa（16,7）对话，并查看长椅（4,13 / 5,13）。

最短路径：墓碑 → Dad。其余都只是附加内容。

---

## 7. 台词在哪里

| 说话者 | 脸图 | 文字框数 |
|---|---|---|
| Grace | `RM_Face_Grace` | 46 |
| Mom | `RM_Face_Mom` | 14 |
| Dad | `RM_Face_Dad` | 10 |
| Grandma | `RM_Face_Grandma` | 4 |
| Uncle James | `RM_Face_UncleJames` | 1 |
| Aunt | `RM_Face_Aunt` | 1 |
| Grandpa | `RM_Face_Grandpa` | 1 |
| 旁白（无脸图） | — | 4 |

游戏中完整的文本，按顺序列在 [../script/ACT1_SCRIPT.md](../script/ACT1_SCRIPT.md)。那个文件由地图数据生成，所以它才是玩家实际读到的内容的准绳。

Grace 的台词使用颜色代码 `\C[4]`，Mom `\C[6]`，Dad `\C[1]`，Grandma `\C[5]`，Uncle James `\C[3]`，旁白 `\C[7]`。名字标签是单独一行，写作 `\C[4]GRACE\C[0]`。

---

## 8. 常见操作

**改一句台词。** MV 编辑器 → 地图 1 → 双击该事件 → 选择事件页 → 在显示文字框里改文本。编辑器打开时，绝不要手动编辑 `Map001.json`；保存时编辑器会覆盖你的改动。

**淡入或淡出一张全屏图片。** MV 的显示图片**没有时长字段**，所以淡入淡出需要两条指令：以**不透明度 0**显示图片，再用移动图片把它移到**不透明度 255**，时长 30 帧，并勾选*等待完成*。淡出则是用移动图片在 24 帧内把不透明度移到 0，然后清除图片。墓碑（事件 4 和 13）和盒子（事件 10）都用这个套路——照抄它，不要另发明一套。

**加一个交互提示。** 还没接上。`RM_Act1_Experience.js` 会从事件的**备注栏**读取 `<RMHint: your text>`（事件编辑器右上角的框）——不是从注释指令读。然后启用该插件。两步都还没做。

**往地图上加一个新角色。** 把行走图放进 `game/img/characters/`（以 `$` 开头的单角色为 144 × 192），把脸图放进 `game/img/faces/`（576 × 288），新建一个事件，设置它的图像，并在每个显示文字框里设置脸图名称和 0 到 7 的索引。

**移动玩家的起始点。** MV 编辑器 → 右键地图 1 → *编辑地图* → 设置玩家起始位置。当前起始点是 (12,12)。

**测试开关改动。** 一律从**新游戏**开始。从章节中途的存档开始，会恢复旧的开关状态，让你的改动看起来是坏的。

**不要删掉盒子交接里的黑场。** Mom 那句 *"Grace. We're going home now."* 之后的五条指令——淡出画面、淡出 BGM、设置 Mom 的位置、场所移动 Grace、淡入画面——正是它们让这段交接读起来像一次切换镜头。`RM_Box.png` 是全屏的，没有这五条指令，Mom 就是直接瞬移进画面，图片硬拍在上面。看起来就像 bug，而且以前已经被误删过一次。
