---
name: uloop-record-input
description: "V3で削除されたコマンド。PlayMode入力の記録はUnity Editor GUI (Window > Unity CLI Loop > Recordings) で行う。CLIからは呼び出せない。"
---

# uloop record-input (V3で削除)

`uloop record-input` はV3で削除された。CLIからPlayMode入力を記録する手段はなくなり、
Unity Editorの **Window > Unity CLI Loop > Recordings** で操作する。

このタスクをCLIエージェント経由で自動化したい場合は、Unity Editorを直接操作できる別の手段
(uloop-execute-dynamic-codeでの代替実装、または人手によるGUI操作)を検討すること。

決定論的リプレイに関する設計上の注意点は [references/deterministic-replay.md](references/deterministic-replay.md) を参照 (記録方法が変わっても再生の決定論性要件自体は変わらない)。
