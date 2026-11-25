[English](HANDOFF.md) · **简体中文**

# 协作者接力说明

更新于 2026-11-25。首次接力先读本页，再读[工程入口](../RememberMeUnity/README.zh-CN.md)和[交互约定](../RememberMeUnity/Docs/Interaction.zh-CN.md)。

下一次上传先运行 node tools/check-repo.cjs，后续操作见[固定上传流程](UPLOAD.zh-CN.md)。release.json 记录当前正式版本及对应源码。

项目文档现提供 27 组中英对照，包括历史第一幕完整阅读稿和厨房工作稿，默认入口使用英文。上传预检通过 tools/check-languages.cjs 检查语言配对及切换链接。

## 已上传内容

| 内容 | 仓库位置或提交 | 状态 |
| --- | --- | --- |
| 11 月 24 日厨房、配方和家庭记忆更新 | 63ffbf0、563cb85、ab11d82 | 已进入 main，可从历史提交追溯 |
| 11 月 25 日剧情、美术和交互修复 | 98c6c90 | 源码、素材及 .meta、场景、网页成品已进入 main |
| 中英文 README、协作流程与早期截图整理 | b81c9a7、608e6c8 | 已进入 main |
| 当前 Unity 源工程 | RememberMeUnity/Assets、Packages、ProjectSettings | 可克隆后通过 Unity Hub 打开 |
| 当前网页成品 | unity-v10/ | 已公开发布，入口为 unity/ |
| 第二幕厨房原始工作稿 v0.4 | [scene1_draft_2.md](../script/drafts/scene1_draft_2.zh-CN.md) | 从本机提供的原稿补入，供接力核对 |
| 待评审侧围栏预览 | [预览图](handoff/Cemetery-Side-Fence-Preview.png) | 已补入，替换方案等待用户确认 |
| 本轮浏览器与公开网页验证记录 | [完整流程日志](handoff/2026-11-25-browser-verification.log)、[公开入口日志](handoff/2026-11-25-public-verification.log) | 已补入可复核的简短记录 |
| 本轮目录归档验证 | [八个历史入口验证](handoff/2026-11-25-archive-verification.log) | 保留查询参数与页面片段的浏览器跳转已通过 |

[下载 v10 Windows x64](https://jeddakholmes-byte.github.io/RememberGame/windows-v10/)。完整解压后运行 RememberMe.exe。

## 昨天和今天完成的内容

- 11 月 24 日加入第二幕厨房、食材与配方、爸爸的饼干、家庭记忆线索，以及走廊和客厅流程，发布到 v9。
- 11 月 25 日调整墓园家属和四座特殊墓碑的引导，第一段八音盒回忆改为空屋，增加雨中回家过场及卧室再次开盒的衔接。
- 补充卧室家具、雨天室内与窗景、食材插画及三张可见的配方卡，修正物件碰撞与旧存档落入家具内部的恢复。
- 恢复暂停物品栏和厨房存读档，移除收集托盘悬浮，修复水门遮罩盖住对话框的问题。
- 统一菜单和对话的确认入口，补齐 WASD 菜单导航、卡片撤回和进度保留，修复剧情行走的站姿覆盖与脚底落点，发布 v10。

## 当前验证证据

公开试玩为 [v10](https://jeddakholmes-byte.github.io/RememberGame/unity-v10/)。网页右上角 Feedback 可提交反馈。

浏览器实测包含 A/D 横向选择、W/S 纵向选择，两幕自动流程、卡片撤回、物品栏和存读档、水门返回、牵奶奶与结尾。自动流程检查了剧情行走存在连续动画帧，动画的美术观感仍由真人试玩评审。

## 下一位协作者的任务

### 1. 接手验收并处理试玩反馈

- 从 main 创建任务分支，按[协作流程](../CONTRIBUTING.zh-CN.md)打开 Unity 6000.6.1f1。
- 分别从 New Game 和 Kitchen 试玩。重点记录壁炉脚底位置、厨房门槛、卡片撤回与重开、暂停后继续、奶奶跟随及进客厅的观感。
- 对反馈先记录版本、复现步骤、预期和实际行为，再修改对应脚本，补验同一路径。验收结果放进 PR。

### 2. Windows 实机验收

本次桌面包对应 v10 游戏提交 98c6c90，使用 Unity 2022.3.20f1c1 兼容构建环境与 Direct3D 11。已在 macOS 的 CrossOver 中运行成品，第一幕、换章和厨房结尾均通过自动通关检查。下一步在 Windows 真机检查显示、声音、WASD 和食谱撤回。

### 3. 核对第二幕剧本并评审围栏

- 将厨房工作稿 v0.4 与 KitchenPrototype.cs 的触发、对白和分支逐项核对，先列出差异与拟改内容。
- 原稿含早期像素画描述，当前美术基准采用已接入的绘本风资源。
- 侧围栏预览已放入本页，交由用户确定替换方案。

## 实现约束与文件入口

- 第一幕剧情在 Act1Game.cs／Act1Story.cs，布局在 Act1Builder.cs。构建会重新装配第一幕，场景手改应同步到装配脚本。
- 第二幕主流程在 KitchenPrototype.cs，界面在 KitchenCanvas.cs，物品栏和存档在 KitchenInventory.cs。
- 菜单输入通过 Act1HUD.HandleMenuInput 和 StoryInput；鼠标和键盘调用同一个动作入口。
- 剧情行走使用 GraceMotor.MoveScripted，Place 用于换场景和恢复位置。
- 第一幕与第二幕分别使用 96 PPU 和 134 PPU，坐标原点也不同，按交互约定转换。
- 新资源与 .meta 一起提交。历史网页位于 archive/web/，旧版入口兼容逻辑位于根目录 404.html。
