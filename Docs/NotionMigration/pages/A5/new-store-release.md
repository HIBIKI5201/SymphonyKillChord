# 配信・ストア登録

- page_id: （新規。親ページ: ワークフロー 2ef7c2c6-cc02-8001-b4f0-d69b499c64f0）
- Notion パス: Symphony Kill Chord / ワークフロー / 配信・ストア登録（新規）
- スナップショットの最終更新: —（新規ページ）
- 書き込み許可: 可（親ページのワークフローが許可範囲の中）
- 処置: 新規作成

## 判定

- 信頼性: 新規 — 決定 R105（2026-09-22 八幡）で作ることになったページ。本文は `Docs/ストア仮登録_手順と事前決定事項_2026-08-14.md`（211 行、2026-08-14 作成）の内容を移した。技術的な設定は 73fefeb45 の `ProjectSettings/ProjectSettings.asset` で確かめ、元文書と一致した（companyName `Sinfonia Studio`:15、productName:16、applicationIdentifier `jp.sinfoniastudio.symphonykillchord`:172-174、AndroidMinSdkVersion 25:182、AndroidTargetSdkVersion 36:183、AndroidTargetArchitectures 2 = ARM64 のみ:273、AndroidKeystoreName 空:277、androidUseCustomKeystore 0:290、AndroidAppBundleSizeToValidate 150:299、scriptingBackend Android 1 = IL2CPP:902-903）。支払い・アカウント開設・鍵の作成が済んだかは記録から確かめられない。確認したコミット: 73fefeb45
- 整合性:
  - 元文書との差: 元文書の「プライバシーポリシーURL 未作成（HomePage は Astro 初期テンプレのまま）」は古い。ホームページに `HomePage/src/pages/policy.astro`（最終更新 a7e2dc24a、2026-09-15）がある。公開 URL はホームページの base（`HomePage/astro.config.mjs` の `site` + `/SymphonyKillChord/`）から `https://hibiki5201.github.io/SymphonyKillChord/policy/` になる
  - 元文書との差: ホームページに Steam / Google Play のボタンがある（7d78982ff、2026-09-15 `PlatformLinks.astro`）。リンク先が未設定のため「準備中」と表示している（`HomePage/src/pages/home.astro:20` は href を渡していない）
  - 元文書との差: 元文書の Steam の根拠 `Build Profiles/Windows.asset` は、今は `Release_Master_Windows.asset` など 16 構成に分かれている（#1534）
  - 他ページとの関係: CBT（Google Play のクローズドテスト）の条件と QA は QA（new-qa.md）にある。本ページでは重複させず、リンクだけ置いた
  - 他ページとの関係: CRI ADX LE の規約は、ストアの詳細テキストの末尾に著作表記を求めている（クレジット 3127c2c6-cc02-80d6-a0d9-e1215ca78df0 の「クレジット表示」）。ストアに載せるかは未決（クレジットの原稿の要確認と同じ）
  - 決定との関係: 体験版は Android と Windows の 2 つでビルドする（決定 R72、#2079）。ホームページの公開 URL は決定 R82
- 変更点:
  - 新規作成。構成は QA（new-qa.md）・ビルドとリリースと同じく「このページについて」のコールアウト → 番号なしの節 → 表 にした
  - 元文書の日付に依存する記述（「今日やるべきこと」「17 日後」「今週中」、8/15〜10/31 の逆算）は移していない。期限の規則（30 日・14 日・連続 14 日）だけを残した
  - 元文書の「私が代行できない」（AI の作業記録）は、「ディレクター本人が行う」に書き換えた
  - 秘密情報: 元文書にパスワード・鍵・口座などの値は無い。本文にも書かず、「このページに書かない」と明記した
- 織り込んだ反映項目: RL-16（配信・ストア登録のページ）, 01 棚卸し `Docs/ストア仮登録_手順と事前決定事項_2026-08-14.md` の行（移植先）, 決定 R72・R82・R105
- 出典: リポ文書 `Docs/ストア仮登録_手順と事前決定事項_2026-08-14.md` §0〜§6、`Docs/QA/CBT_QAシート_2026-09-06.md` §0-4・§14 / 実装 `ProjectSettings/ProjectSettings.asset`（上記の行）、`HomePage/astro.config.mjs`、`HomePage/src/pages/policy.astro`、`HomePage/src/components/PlatformLinks.astro`、`HomePage/src/pages/home.astro:20`、`Assets/Settings/Build Profiles/` / PR #1534 / 決定（八幡）R72・R82・R105
- 要確認:
  - 支払い（Steam Direct・Google Play デベロッパー登録）、アカウント開設、AppID の発行、クローズドテストの開始が済んだか（「今の状況」の表。八幡さんに）
  - アップロード鍵（keystore）を作ったか。`ProjectSettings.asset` には設定が無い（鍵そのものとパスワードはこのページに書かない。八幡さんに）
  - 元文書の未決 6 件（公式アカウントの実体、公開する連絡先メール、売上の受取名義とバンタン／G-Lab の許諾、Google Play の有料/無料、IARC の申告内容、ARM64 実機での動作確認）の現状（八幡さんに）
  - リリース目標 2026-10-31（元文書、声優契約条件 A1）が今も同じか（八幡さんに）
  - ホームページのストアボタンのリンク先を入れる時期（ストアページの公開後）

## 適用する本文

## 配信・ストア登録
> 💡 **このページについて**
製品版をSteamとGoogle Playで配信するために、ストアへ何をどの順で登録するかをまとめたページである。支払い・規約への同意・本人確認・鍵の作成は、認証情報と決済が絡むため、ディレクター本人が行う。パスワード・鍵・口座などの値はこのページに書かない。

## 配信先
| ストア | プラットフォーム | アカウント | 費用 |
|---|---|---|---|
| Steam | Windows | Steamworksのパートナー登録 | Steam Directの手数料 $100（1タイトルごと） |
| Google Play | Android | 個人のデベロッパーアカウント（2026-08-14決定） | 登録料 $25（1回のみ） |
Google Playの組織アカウントはD-U-N-S番号（法人登記）が要るため、学生チームでは取れない。そのため個人アカウントにした。
ビルドの作り方は、ビルドとリリースのページにある。
[[ビルドとリリース]]

## 決まっている設定
| 項目 | 値 | 正本 |
|---|---|---|
| アプリ名 | Symphony Kill Chord | `ProjectSettings.asset`の`productName` |
| デベロッパー名 | Sinfonia Studio | `ProjectSettings.asset`の`companyName` |
| パッケージ名 | `jp.sinfoniastudio.symphonykillchord`（AndroidとWindowsで同じ） | `ProjectSettings.asset`の`applicationIdentifier` |
| Androidのアーキテクチャ | ARM64のみ | `AndroidTargetArchitectures` |
| AndroidのScripting Backend | IL2CPP | `scriptingBackend` |
| Target SDK | 36（Android 16） | `AndroidTargetSdkVersion` |
| Min SDK | 25 | `AndroidMinSdkVersion` |
- ARM64のみにしたのは、ARMv7との両対応ではAABに2つ分のネイティブコードが入り、容量が膨らむためである（AABの容量の検証値は150MB）。ARMv7だけの32bit端末は動作対象外である
- Google Playは、2026-08-31以降の新規アプリと更新にAPI 36以上を求めている。そのためTarget SDKを自動ではなく36に固定している

## Steamの手順
| # | 作業 | 実施者 | 備考 |
|---|---|---|---|
| 1 | Steamworksのパートナー登録 | ディレクター | ゲーム用の公式アカウントで行う。個人の遊び用アカウントに紐づけると、後で分けられない |
| 2 | 本人確認・税務情報・銀行口座の登録 | ディレクター | 米国以外の個人はW-8BENを出す |
| 3 | Steam Directの手数料 $100 を支払う | ディレクター | 支払った時点から30日後にリリースできるようになる |
| 4 | App Creditを使ってAppIDを発行する | ディレクター | ここまでが仮登録である。ストアページはまだ公開されない |
| 5 | ストアページを作り、Valveの審査を受ける | — | 審査は3〜5営業日 |
| 6 | Coming Soonページを公開する | — | リリース日の14日以上前に公開する |

## Google Playの手順
| # | 作業 | 備考 |
|---|---|---|
| 1 | デベロッパー登録（$25）と本人確認 | 審査に数日かかる |
| 2 | アプリを作る（アプリ名・言語・アプリかゲームか・無料か有料か） | パッケージ名はまだ決まらない |
| 3 | ストア掲載情報を入力する | 下の「ストア掲載情報」の素材が要る |
| 4 | 初回のAABをクローズドテストのトラックへアップロードする | **ここでパッケージ名が永久に確定する** |
| 5 | クローズドテスト（CBT）を行う | 12人以上が連続14日間オプトインする。条件はQAのページにある |
| 6 | 本番アクセスを申請し、審査を受ける | |
- 1〜2までが仮登録である
- 無料で登録すると、後から有料に変えられない。有料から無料には変えられる。有料にする場合は販売者アカウントの設定が追加で要る
- アップロードには、アップロード鍵（keystore）が要る。鍵はディレクター本人が作り、複数の場所にバックアップする。**鍵を失うとアプリを二度と更新できない**
[[QA]]

## ストア掲載情報
| 項目 | 内容 |
|---|---|
| アプリ名 | Symphony Kill Chord |
| ジャンル | リズム / 三人称 / 3Dアクション（基本情報） |
| カテゴリ（Google Play） | ゲーム > アクション |
| キャッチコピー | 独自のビートで爽快バトル（基本情報） |
| 短い説明（Google Play） | 80字以内 |
| 詳細な説明（Google Play） | 4000字以内 |
| 対応言語 | 日本語 |
| 連絡先メール | 未定 |
| プライバシーポリシー | ホームページのプライバシーポリシー（https://hibiki5201.github.io/SymphonyKillChord/policy/） |
| デベロッパー名 | Sinfonia Studio |
必要な素材は次のとおりである。
- アプリアイコン 512×512 PNG
- フィーチャーグラフィック 1024×500
- スクリーンショット 4枚以上
IARCの年齢レーティングは、銃器・暴力表現の申告内容を先に文書にしてから回答する。虚偽の申告はストアから削除される対象になる。産学向けの表現規制リストと合わせてから回答する。
CRI ADX LEの規約では、ストアの詳細な説明の末尾などに著作表記が求められている。規約はクレジットのページにある。
[[クレジット]]

## 今の状況
| 項目 | 状況 |
|---|---|
| アカウント種別（Google Play） | 個人で確定（2026-08-14） |
| パッケージ名・デベロッパー名 | 確定し、Unityの設定に反映済み |
| Target SDK 36・ARM64 | Unityの設定に反映済み |
| Steam Directの支払い・AppIDの発行 | 【要確認: 八幡さんに】 |
| Google Playのデベロッパー登録・本人確認 | 【要確認: 八幡さんに】 |
| アップロード鍵の作成とバックアップ | Unityの設定には鍵が設定されていない。【要確認: 八幡さんに】 |
| ARM64の実機での動作確認 | 【要確認: 八幡さんに】 |
| プライバシーポリシー | ホームページに掲載済み |
| ホームページのストアボタン | ボタンはあるが、リンク先が無いため「準備中」と表示している |

## 未決事項
| # | 確認すること | 影響 |
|---|---|---|
| 1 | 登録に使う公式アカウントの実体（どのGoogle・Steamアカウントか） | Steamは後から紐づけを変えられない |
| 2 | ストアに公開する連絡先メールアドレス | 個人のアドレスを公開するかどうか |
| 3 | Steamの売上の受取名義と、産学連携作品を商用で公開することへのバンタン／G-Labの許諾 | 支払いの前に確かめないと、後で戻せない |
| 4 | Google Playの有料・無料 | 無料から有料には変えられない |
| 5 | IARCの申告内容（銃器・暴力表現） | ディレクターが決める |
| 6 | CRI ADX LEの著作表記をストアの説明文に載せるか | 規約はストアの詳細テキストの末尾を例に挙げている |
【要確認: 各項目の現状を八幡さんに】
