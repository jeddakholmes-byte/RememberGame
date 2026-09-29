# 剧本草稿 drafts/

新写的剧本先放这里。格式、命名、语气规范见上级目录的 [`../README.md`](../README.md)。

## 命名

```
ACT2_<场景名>_script.md        例：ACT2_Attic_script.md
```

一个场景一个文件。不要几个人往同一个文件里写。

## 改已有台词的写法

不要直接改 `../ACT1_SCRIPT.md`（那是从工程导出的，一改就被覆盖）。在这里新建一个文件，写清楚"原文 / 改成"：

```markdown
# 修改：Act I OldGrave_4 的错别字

## 事件
Map001 → 017 OldGrave_4（坐标 18,4）

## 原文
It seems to be abondoned a long time ago.

## 改成
It looks like no one has been here
for a long time.

## 原因
修 `abondoned` 拼写；同时更接近孩子的具体观察。
```

带上**事件名和坐标**，做事件的人才能找到位置。

## 定稿之后

由一个人把草稿合并进工程（RPG Maker MV 的事件里），然后**从 New Game 跑一遍**确认台词出现的位置和顺序对。合并完成后，`../ACT1_SCRIPT.md` 之类由数据导出的文件要重新导出，别让仓库里的文本和游戏对不上。
