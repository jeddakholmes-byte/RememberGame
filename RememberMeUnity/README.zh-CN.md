# Remember Me · Unity 工程

[English](README.md) | 简体中文

[仓库首页](../README.zh-CN.md) · [在线试玩 v10](https://jeddakholmes-byte.github.io/RememberGame/unity-v10/) · [协作流程](../CONTRIBUTING.zh-CN.md)

## 打开与试玩

1. 安装 Unity **6000.6.1f1**，从 Unity Hub 添加本目录。
2. 打开 `Assets/Scenes/RememberMeAct1.unity`，点击 Play。
3. 第一幕从 New Game 开始。标题菜单的 Kitchen 可直接进入第二幕，第二幕场景为 `Assets/Scenes/Kitchen.unity`。
4. 修改交互前阅读[交互约定](Docs/Interaction.zh-CN.md)。

网页构建需要 Web Build Support，Windows 构建需要 Windows Build Support。本地 `Play-RememberMe.cmd` 打开当前公开网页版。

## 修改位置

| 内容 | 文件或目录 |
| --- | --- |
| 第一幕剧情与状态 | `Assets/Scripts/Act1Game.cs`、`Act1Story.cs` |
| 第一幕房间、物件、碰撞与布局 | `Assets/Editor/Act1Builder.cs`、`Assets/Scripts/Act1Room.cs` |
| 第二幕厨房、走廊、客厅与剧情 | `Assets/Scripts/KitchenPrototype.cs` |
| 厨房界面、配方和物品栏 | `Assets/Scripts/KitchenCanvas.cs`、`KitchenPresentation.cs`、`KitchenInventory.cs` |
| 共用输入、对话和菜单 | `Assets/Scripts/StoryInput.cs`、`Act1HUD.cs` |
| 角色移动和动画 | `Assets/Scripts/GraceMotor.cs`、`ActorView.cs`、`GrandmaFigure.cs` |
| 第一幕美术和音频 | `Assets/Art/`、`Assets/Audio/` |
| 第二幕美术、音频及房间数据 | `Assets/Resources/KitchenChapter/` |
| 编辑器检查与网页构建 | `Assets/Editor/Act1Verification.cs`、`WebBuild.cs` |
| 完整流程及浏览器验证 | `Assets/Scripts/KitchenRouteCheck.cs`、`QA/Verify-Web.cjs` |
| 历史截图 | [QA/Reference](QA/Reference/README.zh-CN.md) |

## 构建与验证

Unity 菜单 **Remember Me → Build for GitHub Pages** 输出 `Build/GitHubPages/docs/`，包含两幕。命令行入口为 `RememberMe.Editor.WebBuild.Build`，并传入 `-customBuildPath`。Windows 命令行入口为 `RememberMe.Editor.Act1Builder.Build`，输出 `Build/Windows/`。

从仓库根目录运行 `node tools/build-players.cjs "<Unity editor executable>"`，依次生成 Windows 和 WebGL。完整压缩 Build/Windows 文件夹。玩家将其全部解压后运行 RememberMe.exe。

构建会调用 `Act1Builder.Assemble` 重新生成第一幕场景。布局改动同时维护装配脚本，提交场景与新增资源的 `.meta`。

Windows 流程验证参数为 `-remember-smoke -with-kitchen`，可用 `-kitchen-capture` 加截图目录。网页验证使用 `QA/Verify-Web.cjs`，依赖 Node.js、Playwright 和 Chrome／Chromium。具体命令见[发布说明](../docs/DEPLOY.zh-CN.md)。

## 本地文件与历史资料

`Library/`、`Temp/`、`Logs/`、`UserSettings/`、`Build/` 为本机生成目录，已由 `.gitignore` 管理。`QA/Reference/` 保存早期截图，包含旧剧情画面。

`Assemble-Art.ps1` 是早期素材导入工具，依赖原始工作区和外部原图。常规协作直接使用 `Assets/` 下的现有资源。

[LegacyDeployment](Docs/Archive/LegacyDeployment/README.zh-CN.md) 保存早期 GitHub Actions 部署样例。当前网页通过仓库 `main` 分支的根目录发布，流程见仓库发布说明。
