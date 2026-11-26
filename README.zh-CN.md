[English](README.md) · **简体中文**

# Remember Me

围绕 Grace、奶奶和家庭记忆展开的叙事游戏，香港中文大学（深圳）CSS5190 Game Your Psychology 课程项目。

[在线试玩 v10 · 第一、二幕](https://jeddakholmes-byte.github.io/RememberGame/unity-v10/) · [Windows x64 离线版下载](https://github.com/jeddakholmes-byte/RememberGame/releases/download/v10-windows-x64/RememberMe-v10-Windows-x64.zip) · [提交试玩反馈](https://github.com/jeddakholmes-byte/RememberGame/issues/new) · [参与协作](CONTRIBUTING.zh-CN.md)

**下一位协作者先读[接力说明](docs/HANDOFF.zh-CN.md)**，其中列出 11 月 24 日、25 日更新的上传状态、验证记录和下一步任务。

## 从这里开始

| 要做什么 | 阅读入口 |
| --- | --- |
| 打开工程、修改游戏 | [Unity 工程说明](RememberMeUnity/README.zh-CN.md) |
| 修改交互、物品栏、移动 | [交互约定](RememberMeUnity/Docs/Interaction.zh-CN.md) |
| 查找代码、美术、旧版资料 | [文档目录](docs/README.zh-CN.md) |
| 构建和发布网页 | [发布说明](docs/DEPLOY.zh-CN.md) |
| 提交剧本或美术 | [协作流程](CONTRIBUTING.zh-CN.md) |

## 当前内容

Unity 工程包含第一、二幕。Grace 在墓园看望奶奶，经历家庭闪回，短暂进入空屋记忆，再经过雨中回家的过场回到卧室。再次接触纪念物后进入奶奶的厨房，通过食材、图片卡与烘焙推进走廊和客厅的剧情。

v10 包含 WASD 菜单导航、配方卡撤回、原画物品栏、厨房存读档、水门遮罩修复和剧情行走动画修复。

## 仓库目录

~~~text
RememberGame/
├── RememberMeUnity/             当前游戏开发
│   ├── Assets/Scripts/          剧情、输入、界面、移动与存档
│   ├── Assets/Editor/           第一幕场景装配、构建和检查
│   ├── Assets/Art、Audio/       第一幕资源
│   ├── Assets/Resources/        第二幕资源与房间数据
│   ├── Packages、ProjectSettings/ 工程依赖和设置
│   ├── Docs/                   Unity 实现与交互约定
│   └── QA/                     验证脚本，Reference/ 保存历史截图
├── art/incoming/               新美术交付
├── script/drafts/              剧本草稿，包含第二幕 v0.4
├── docs/                       接力说明、更新记录、发布指南
│   └── handoff/                验证证据和待评审围栏图
├── unity-v10/                  当前公开网页成品
├── unity/、index.html          固定试玩入口和首页
├── archive/web/                集中保存 v2 至 v9 成品
├── game/                       原 RPG Maker MV 工程
└── tools/                      MV 工具及旧网页入口验证
~~~

[开发入口](RememberMeUnity/README.zh-CN.md) · [交付与分支流程](CONTRIBUTING.zh-CN.md) · [文档目录](docs/README.zh-CN.md) · [历史成品说明](archive/README.zh-CN.md)

## 游玩和编辑

使用电脑浏览器打开[在线试玩](https://jeddakholmes-byte.github.io/RememberGame/unity-v10/)。WASD／方向键移动，Shift 跑步，C 蹲下，E／Enter／空格确认，I 打开物品栏，Esc 打开菜单。横向菜单使用 A/D，纵向菜单使用 W/S。配方卡可用 Backspace 或点击已放卡片撤回。

安装 Unity **6000.6.1f1**，通过 Unity Hub 添加 RememberMeUnity，打开 Assets/Scenes/RememberMeAct1.unity。标题菜单的 Kitchen 可以直接进入第二幕。构建网页需要 Web Build Support，构建 Windows 需要 Windows Build Support。

## 协作方式

同步 main 后为每项任务创建分支，完成改动后试玩受影响的流程，再提交 Pull Request。Unity 素材与对应的 .meta 一起提交。

构建会调用 Assets/Editor/Act1Builder.cs 重新装配第一幕。需要保留到下次构建的布局改动应同步到该装配脚本。

[分支操作、交付内容和检查清单](CONTRIBUTING.zh-CN.md)

## 素材与署名

仓库包含项目制作的画作、动画、音频以及原 MV 资源。新增素材时记录来源与许可证。网页成品附带 Tinos 字体许可证，MV 资源遵循其原有授权。
