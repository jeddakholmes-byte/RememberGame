[English](V11_RELEASE.md) · **简体中文**

# v11 发布候选版

2026 年 11 月 27 日整理，目标仓库为 `jeddakholmes-byte/RememberGame`。页面标题为 **Remember Me — v11**，反馈链接指向团队仓库，候选网页目录为 `unity-v11/`。计划地址为 `https://jeddakholmes-byte.github.io/RememberGame/unity-v11/`。

## 本次内容

- 第一幕：部分文字分框与停顿、爸爸的回应，以及音乐盒记忆和回家过程的衔接。
- 第二幕：照片上的呼唤字幕与淡入淡出，厨房淡入后两秒观察，通过重逢对话形成 Grace 帮忙的决定，并走近奶奶。
- 第一份材料：找面粉、到食谱旁放下、得到奶奶回应，并显示台面面粉图像。其他材料可以提前收集；新增 `flourSetOut` 存档字段记录交付，旧的后续阶段存档直接跳过引导。
- 前轮试验：Grace 查看出口时让奶奶等候，进入客厅后由玩家亲自折返回去接她。
- 网页启动：资源地址携带统一版本标识，启动失败时显示实际错误。本轮没有更换美术，也没有修改盒盖卡顿。

## 源码与构建来源

正式工程 `RememberMeUnity/Assets/Scripts/` 内六份运行时源码，与用户成功构建并提交为 `1da4081` 的第二轮试验记录的六份 SHA-256 完全一致。`unity-v11/Build/` 逐字节复用该构建，HTML 由更新后的团队模板生成。本次不宣称已从正式工程重新编译打包。`unity-v11/build-info.json` 记录源码、构建哈希和待验证项。

本地缓存的 `upstream/main` 为 `e28a87d978c5d5d9f0c994a11c8057ddbee23ad7`。整合前，运行时脚本与该团队版本一致，Unity 工程下仅两份 README 有差异。在线远端检查因网络/DNS 错误失败，无法确认缓存是否最新。合并前应先获取团队最新分支，检查重叠修改。

## 团队提交范围

| 路径 | 用途 |
| --- | --- |
| `RememberMeUnity/Assets/Scripts/Act1Game.cs` | 第一幕节奏、转场及 WebGL 编译兼容处理 |
| `RememberMeUnity/Assets/Scripts/KitchenPrototype.cs` | 厨房叙事、首份材料和折返流程 |
| `RememberMeUnity/Assets/Scripts/KitchenPresentation.cs` | 引导显示与记忆淡入淡出 |
| `RememberMeUnity/Assets/Scripts/KitchenCanvas.cs` | 首份材料清单与面粉图像 |
| `RememberMeUnity/Assets/Scripts/KitchenInventory.cs` | 引导进度存档 |
| `RememberMeUnity/Assets/Scripts/KitchenRouteCheck.cs` | 更新后的路线断言 |
| `RememberMeUnity/Assets/WebGLTemplates/RememberMePages/index.html` | 团队名称、反馈与启动资源版本 |
| `unity-v11/` | 完整候选网页包、字体许可与来源记录 |
| `docs/V11_RELEASE.md`、`docs/V11_RELEASE.zh-CN.md` | 范围、证据及发布步骤 |
| `docs/UPLOAD.md`、`docs/UPLOAD.zh-CN.md` | 与部署指南统一后续版本路径 |

个人 `LaiDoc/`、`Trials/`、`narrative-trial-01/`、本地环境配置和个人构建/预览工具不纳入本次团队提交。当前 fork 的 main 已含之前的个人网页提交，直接从该分支提 PR 会包含这些历史。应从最新团队 main 建立干净的发布分支，只迁入上述修改，并检查 PR 的完整文件清单。

## 验证与正式启用

Laia 已反馈第一幕叙事节奏和引导没有问题，第二幕修改后明显更舒服。网页启动失败发生于普通窗口，而同一试验网址在无痕窗口可以正常启动，说明旧浏览器状态与失败有关。盒盖卡顿由用户判断为电脑问题，依其要求暂不处理性能。

静态检查覆盖整合源码哈希、构建文件一致性、gzip 完整性、Wasm 有效性、模板展开、团队名称、资源引用和版本一致性。更新后的 Unity/浏览器自动路线检查尚未运行。候选版尚未发布到团队仓库。

正式启用前，用 `RememberMeUnity/QA/Verify-Web.cjs` 检查，设置 `REMEMBER_WEB_ROOT` 指向 `unity-v11/`；再人工检查墓地至厨房转场、面粉交付、提前拿其他材料、存读档，以及折返接奶奶。如需从正式源码重新构建，沿用 `docs/DEPLOY.md` 的构建入口，生成的 HTML 与输出文件名须对应。

检查通过后，将 `release.json` 更新为 v11 并记录真实源码提交和验证证据；把首页试玩入口与 `unity/index.html` 改到 `unity-v11/`，更新两种语言 README 的当前版本链接。本次整理阶段，这些已发布入口仍保留 v10。双语交接文档记录简明团队发布证据。

上传前运行 `node tools/check-repo.cjs` 并阅读报告。现有六份个人 LaiDoc 原始材料缺少双语配对，本次排除它们，不为发布重写原稿。把聚焦的发布分支提交至团队 `main`，经团队审阅合并后检查 Pages 部署，并验证团队 v11 地址。

建议 PR 标题：**Prepare v11 narrative pacing and kitchen onboarding**。说明以上行为变化，并区分用户试玩反馈、静态验证与尚待完成的运行时/线上验证。


整理验证记录：正式源码在 Windows 和 WebGL 引用下编译通过，保留原有序列化字段警告；候选包静态完整性检查通过。已运行并阅读仓库预检报告：六份原有个人 LaiDoc 材料缺少中文配对，正式源码变化仍被列为需要重新构建及完整浏览器 QA。运行时 QA 和线上部署尚待完成。


## 配合 GitHub Desktop 导入

把团队仓库另克隆到新文件夹，从其当前 main 建立并选中 `release/v11-narrative`。然后在原个人工作目录双击 `Import-V11-To-Team.cmd`，在目录选择框中选择新的团队仓库。工具要求团队 origin、上述分支名及干净的工作区；复制前核对所有现有目标文件的 Git 对象与准备时基线，并核对来源 SHA-256，按 19 份文件白名单导入。如团队文件有变化，会在复制前停止并列出文件。工具不复制个人材料或导入工具自身，不提交、不推送，也不切换默认试玩入口。语法与清单校验通过，新团队副本的实际导入待完成。

导入成功后检查 19 份变化，以 `Prepare v11 narrative pacing and kitchen onboarding` 为说明提交，发布分支并向团队 main 创建 PR。如发布时提示写入权限或 fork，先处理对应提示。PR 合并且 Pages 部署成功后，v11 专用网址即可使用；默认试玩入口在正式启用前仍遵循原 release.json。
