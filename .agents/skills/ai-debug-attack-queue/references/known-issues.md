# AI QAツールの問題記録

## 2026-09-16: #1612 基準攻撃の成立待ちタイムアウト

- 報告: https://github.com/HIBIKI5201/SymphonyKillChord/issues/1612
- 報告環境: `feature/demo/demo-movie/yahata`、`eef2ff5f6`。実マウスの攻撃は成立するが、QAキューは基準攻撃待ちで2回タイムアウトした。
- 指定列: `green:5,orange:9,blue:4,purple:3,cyan:3,yellow:4`。
- ソース確認: 基準攻撃は既に`prime=true`で自動実行される。LLMの応答待ちではなく、`EditorApplication.update`からの`InputState.Change`と成立通知の経路を調査した。
- Input System 1.19のソースでは、`InputActionState.NotifyControlStateChanged`は`InputState.currentUpdateType == Editor`のとき通知処理を省略する。`InputState.Change`の引数をDynamicにしても、この現在の更新種別は切り替わらない。
- 修正方針: ゲーム用Input System更新の終了時に押下・解放を処理する。Editor更新は期限監視だけに使い、予約後最初に攻撃可能な入力更新で基準攻撃を送る。キャンセル時の解放も同じ入力経路で処理する。
- 診断: 失敗直前の入力待機、入力更新数、Mouse、入力抑制、Editor停止、ゲーム速度、フォーカスを状態JSONへ残す。
- 確認状況: 報告の再現・Unityコンパイル・PlayModeは未実施。ソースの経路確認、変更JS 2ファイルの`node --check`、`git diff --check`、追加.metaのGUID一意性を確認。Runtime/Packages/ProjectSettings差分なし。
- 要実動確認: 通常のゲーム入力で基準攻撃→指定28攻撃が完了すること。Editor更新だけでは押下しないこと。入力抑制・停止中の期限、押下直後のキャンセル→再開時解放、PlayMode終了、Reload、実マウス押下との競合で残留入力がないこと。
