# Discord Log Exporter

設定したDiscordチャンネルの全メッセージをテキストファイルへ出力するWindows向けツールです。
通常チャンネルはチャンネルごと、フォーラムはアクティブ・アーカイブ済みのページごとに1ファイルを生成します。

## 初期設定

1. Discord Developer PortalでBotを作成し、対象サーバーへ追加します。
2. Developer PortalのBot設定で`Message Content Intent`を有効にします。
3. Botに対象チャンネルの「チャンネルを見る」と「メッセージ履歴を読む」権限を付与します。
4. 公開設定`SinfoniaOperator/sinfonia-operator.env.json`の`DISCORD_LOG_CHANNEL_IDS`へ取得対象IDを配列で設定します。
5. 秘密設定`SinfoniaOperator/sinfonia-operator.secrets.json`の`DISCORD_BOT_TOKEN`へBotトークンを設定します。

```json
{
  "DISCORD_LOG_CHANNEL_IDS": [
    "123456789012345678",
    "234567890123456789"
  ]
}
```

トークンは公開設定へ記載しないでください。

## 実行

`SinfoniaOperator/DiscordLogExporter.exe`を実行します。
公開設定、秘密設定の順に自動で読み込み、`Docs/DiscordLog/`へ出力します。
出力先は変更できません。

コマンドラインから設定ファイルを明示する場合:

```powershell
./DiscordLogExporter.exe --config ./sinfonia-operator.env.json
```

## 出力形式

- 通常チャンネル: `<チャンネル名>_<チャンネルID>.txt`
- フォーラムページ: `<フォーラム名>_<ページ名>_<ページID>.txt`

メッセージは投稿日時の古い順で、本文、添付ファイルURL、埋め込みの主要テキストを出力します。

## 本文が取得できない場合

取得件数があるのに本文、添付、埋め込みがすべて空の場合は、Discord Developer Portalの
`Bot > Privileged Gateway Intents > Message Content Intent`を有効にして保存し、再実行してください。
検証済み、または検証対象規模のBotではDiscordによるIntentの承認も必要です。

この状態を検出した場合、ツールは空のログで既存ファイルを上書きせず、エラーを表示して終了します。
