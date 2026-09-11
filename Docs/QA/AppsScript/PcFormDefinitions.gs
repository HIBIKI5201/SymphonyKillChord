/**
 * PC版 QAフォーム生成 — 設問定義データ。
 *
 * 責務: 生成するフォームの内容を宣言的に保持する。Google Forms API は呼ばない。
 * ITEM_TYPE / JUDGEMENT_COLUMNS / JUDGEMENT_HELP は FormDefinitions.gs のものを共用する。
 *
 * 原本は Docs/QA/PC版_QAシート_2026-09-07.md。
 * 設問を足す・直すときは、md と本ファイルの両方を更新すること。
 */

/** P1〜P4 すべての冒頭に置くヘッダ設問。Android版とは異なりOS/端末情報を聞く。 */
function buildPcCommonHeaderItems() {
  return [
    {
      type: ITEM_TYPE.TEXT_SHORT,
      title: 'テスターID',
      helpText: '運営から配布された英数字のIDを入れてください。氏名は書かないでください。',
      required: true,
    },
    {
      type: ITEM_TYPE.DATE,
      title: '実施日',
      helpText: '今日の日付を選んでください。',
      required: true,
    },
    {
      type: ITEM_TYPE.SINGLE,
      title: '配布経路',
      choices: ['Steam', '配信用zip'],
      required: true,
    },
    {
      type: ITEM_TYPE.TEXT_SHORT,
      title: '検証環境 (CPU/GPU/OS/解像度)',
      helpText: '例: i7-12700K / RTX3070 / Windows11 / 1920x1080',
      required: true,
    },
    {
      type: ITEM_TYPE.TEXT_SHORT,
      title: 'ビルドバージョン',
      helpText: 'タイトル画面に表示されている番号を入れてください。',
      required: true,
    },
  ];
}

/** 各チェックフォームの末尾に置く共通設問。 */
function buildPcTrailingItems() {
  return [
    {
      type: ITEM_TYPE.TEXT_SHORT,
      title: 'NGにした項目番号',
      helpText:
        '例: 2-5, 3-7。この後「不具合報告 (PC版)」フォームで1件ずつ報告してください。NGが無ければ空欄で構いません。',
      required: false,
    },
    {
      type: ITEM_TYPE.TEXT_LONG,
      title: '気づいたこと・言いたいこと',
      helpText: '任意です。項目に当てはまらない気づきがあれば書いてください。',
      required: false,
    },
  ];
}

const PC_NG_FOLLOW_UP_MESSAGE =
  '回答ありがとうございます。NGを付けた項目がある場合は、「不具合報告 (PC版)」フォームから1件ずつ報告してください。';

/**
 * 生成するフォームの定義一覧。
 * items は上から順にフォームへ追加される。
 */
function buildPcFormDefinitions() {
  return [
    buildPcLaunchWindowInputFormDefinition(),
    buildPcGamepadAudioPerformanceFormDefinition(),
    buildPcStabilitySaveUiSteamFormDefinition(),
    buildPcDefectReportFormDefinition(),
  ];
}

/** P1 起動・ウィンドウ・入力チェック。 */
function buildPcLaunchWindowInputFormDefinition() {
  return {
    key: 'P1',
    title: '①起動・ウィンドウ・入力チェック',
    description:
      '配布物の展開・起動から、ウィンドウ表示、キーボード/マウス操作までを確認してください。所要およそ30分。\n\n' +
      JUDGEMENT_HELP,
    confirmationMessage: PC_NG_FOLLOW_UP_MESSAGE + '\n次は「②ゲームパッド・音声・性能チェック」に進んでください。',
    items: buildPcCommonHeaderItems()
      .concat([
        {
          type: ITEM_TYPE.SECTION,
          title: '1. 配布・インストール・起動',
        },
        {
          type: ITEM_TYPE.GRID,
          title: '配布・インストール・起動',
          helpText: JUDGEMENT_HELP,
          required: true,
          rows: [
            '1-1. 配布された zip を展開するだけで起動できる (追加のランタイム導入が不要)',
            '1-2. 実行ファイル名と表示名が Symphony Kill Chord になっている',
            '1-3. パスに日本語・空白が含まれるフォルダに置いても起動する',
            '1-4. デスクトップ以外 (別ドライブ、外付けドライブ) からも起動する',
            '1-5. Windows Defender / SmartScreen の警告が出た場合の挙動を記録した',
            '1-6. 管理者権限なしで起動できる',
            '1-7. 初回起動でクラッシュせずタイトルまで到達する',
            '1-8. 初回起動にかかった時間を記録した (秒)',
            '1-9. *_Data フォルダや DLL を消さずに配布物一式が揃っている',
          ],
        },
        {
          type: ITEM_TYPE.SECTION,
          title: '2. ウィンドウ・ディスプレイ',
          helpText:
            'リサイズ不可・ネイティブ解像度フルスクリーン起動は仕様です。リサイズできてしまった場合はNGにしてください。',
        },
        {
          type: ITEM_TYPE.GRID,
          title: 'ウィンドウ・ディスプレイ',
          helpText: JUDGEMENT_HELP,
          required: true,
          rows: [
            '2-1. 起動時にネイティブ解像度のフルスクリーンで立ち上がる',
            '2-2. Alt + Enter でウィンドウ / フルスクリーンを切り替えられる',
            '2-3. ウィンドウモードでウィンドウの端をドラッグしてもサイズが変わらない',
            '2-4. ウィンドウモードでウィンドウを移動できる',
            '2-5. 1920×1080 以外の解像度 (1280×720 / 2560×1440 / 3840×2160) で UI が破綻しない',
            '2-6. 16:9 以外のアスペクト比 (16:10 / 21:9) で見切れ・引き伸ばしが起きない',
            '2-7. マルチモニタ環境で、意図したモニタに表示される',
            '2-8. モニタ間でウィンドウを移動しても描画が壊れない',
            '2-9. Windows の拡大縮小 (DPI スケーリング 125% / 150% / 200%) で文字がぼやけない',
            '★ 2-10. リフレッシュレート 60Hz 以外のモニタ (120Hz / 144Hz) で挙動を記録した',
          ],
        },
        {
          type: ITEM_TYPE.SECTION,
          title: '3. 入力 — キーボード / マウス',
          helpText: '3-11 / 3-14 は入力間隔が武器と必殺技を決めるPC固有の最重要項目です。',
        },
        {
          type: ITEM_TYPE.GRID,
          title: '入力 — キーボード / マウス',
          helpText: JUDGEMENT_HELP,
          required: true,
          rows: [
            '3-1. 移動 (Move) がキーボードで操作できる',
            '3-2. 視点 (Look) がマウスで操作できる',
            '3-3. 攻撃 (Attack) が入力できる',
            '3-4. 回避 (Dodge) が入力できる',
            '3-5. ロックオン (LockOn) と対象切替 (LockOnSelect) が動く',
            '3-6. ポーズ (BattlePause) と設定 (Option) が開く',
            '3-7. アウトゲームで 決定 (Submit) / 取消 (Cancel) が動く',
            '3-8. シナリオで 送り / 早送り / スキップ / オート / UI非表示 が動く',
            '3-9. マウスカーソルがインゲーム中に画面外へ出ない',
            '3-10. Alt+Tab 後にマウスカーソルの状態が正しく戻る',
            '★ 3-11. 高速連打しても攻撃入力が取りこぼされない (リズム判定に直結)',
            '3-12. マウスのポーリングレートを変えても視点操作の挙動が破綻しない',
            '3-13. IME が有効な状態でもキー入力が誤変換されない',
            '★ 3-14. キーの同時押し (移動 + 攻撃 + 回避) が取りこぼされない',
          ],
        },
      ])
      .concat(buildPcTrailingItems()),
  };
}

/** P2 ゲームパッド・音声・性能チェック。 */
function buildPcGamepadAudioPerformanceFormDefinition() {
  return {
    key: 'P2',
    title: '②ゲームパッド・音声・性能チェック',
    description:
      'ゲームパッド操作、音声出力、性能・安定性を確認してください。所要およそ30分。\n' +
      '★が付いた項目は特に重点的に確認してください。\n\n' +
      JUDGEMENT_HELP,
    confirmationMessage:
      PC_NG_FOLLOW_UP_MESSAGE + '\n次は「③中断・セーブ・UI・Steamチェック」に進んでください。',
    items: buildPcCommonHeaderItems()
      .concat([
        {
          type: ITEM_TYPE.SECTION,
          title: '4. 入力 — ゲームパッド',
          helpText:
            'control scheme は PC_Keyboard-Mouse のみのため、ゲームパッド用のボタン表示切替は' +
            '成立しない可能性があります。想定どおりの挙動かどうかで判定してください。',
        },
        {
          type: ITEM_TYPE.GRID,
          title: '入力 — ゲームパッド',
          helpText: JUDGEMENT_HELP,
          required: true,
          rows: [
            '4-1. ゲームパッドを接続した状態で起動し、操作できる',
            '4-2. 起動後にゲームパッドを接続しても認識される (ホットプラグ)',
            '4-3. プレイ中にゲームパッドを抜いても操作不能にならず、キーボードへ戻る',
            '4-4. ゲームパッドとキーボードを交互に使っても入力が競合しない',
            '4-5. アウトゲームのメニューをゲームパッドだけで移動・決定できる',
            '★ 4-6. 画面のボタン表示がゲームパッド用に切り替わるか記録した',
            '4-7. スティックのデッドゾーンが極端でない',
            '4-8. 振動の有無を記録した',
          ],
        },
        {
          type: ITEM_TYPE.SECTION,
          title: '5. 音声出力 (PC固有)',
          helpText: '5-3 / 5-4 は本作のコアである音楽同期に直結します。特に重点的に確認してください。',
        },
        {
          type: ITEM_TYPE.GRID,
          title: '音声出力 (PC固有)',
          helpText: JUDGEMENT_HELP,
          required: true,
          rows: [
            '5-1. 既定の音声出力デバイスから音が出る',
            '5-2. プレイ中に出力デバイスを切り替えても音が復帰する',
            '★ 5-3. Bluetooth イヤホンで音と拍のズレが判定を壊さない',
            '★ 5-4. 有線 / USB DAC / HDMI 出力それぞれで遅延の体感を記録した',
            '5-5. 他アプリ (Discord / ブラウザ) と同時に音を出しても排他エラーにならない',
            '5-6. OS 側の音量変更が反映される',
            '5-7. サンプルレートが異なる出力デバイス (44.1kHz / 48kHz) で BGM が破綻しない',
          ],
        },
        {
          type: ITEM_TYPE.SECTION,
          title: '6. 性能・安定性 (GS-08)',
          helpText:
            '判定基準はAndroid版とは異なり60 FPS以上です。6-7は画質設定変更手段が無いため重点的に確認してください。',
        },
        {
          type: ITEM_TYPE.GRID,
          title: '性能・安定性 (GS-08)',
          helpText: JUDGEMENT_HELP,
          required: true,
          rows: [
            '★ 6-1. 60 FPS 以上を維持する (計測値を記録)',
            '6-2. インゲームのロードが3秒以内に完了する',
            '6-3. 15分間プレイしてエラーが出ない',
            '6-4. 15分間プレイしてメモリ使用量が単調増加しない',
            '6-5. ステージ再入場を5回繰り返しても FPS が落ちない',
            '6-6. 敵が多数出現する場面でも 60 FPS を割らない',
            '★ 6-7. ノートPC (内蔵GPU) での動作を記録した',
            '6-8. 電源接続時とバッテリー駆動時で挙動が変わらないか記録した',
          ],
        },
        {
          type: ITEM_TYPE.NUMBER,
          title: '平均FPS',
          helpText: '計測できた平均フレームレートを入れてください。',
          required: true,
        },
        {
          type: ITEM_TYPE.NUMBER,
          title: '最低FPS',
          helpText: '計測できた最低フレームレートを入れてください。',
          required: true,
        },
        {
          type: ITEM_TYPE.NUMBER,
          title: 'インゲームのロード時間 (秒)',
          helpText: 'ストップウォッチが無ければ体感で構いません。',
          required: true,
        },
        {
          type: ITEM_TYPE.NUMBER,
          title: '15分プレイ後のメモリ使用量 (MB)',
          helpText: 'タスクマネージャ等で確認した値を入れてください。',
          required: true,
        },
      ])
      .concat(buildPcTrailingItems()),
  };
}

/** P3 中断・セーブ・UI・Steamチェック。 */
function buildPcStabilitySaveUiSteamFormDefinition() {
  return {
    key: 'P3',
    title: '③中断・セーブ・UI・Steamチェック',
    description:
      'フォーカス喪失・多重起動・セーブ・UI表示・Steam配布を確認してください。所要およそ30分。\n' +
      '★が付いた項目は特に重点的に確認してください。\n\n' +
      JUDGEMENT_HELP,
    confirmationMessage: PC_NG_FOLLOW_UP_MESSAGE,
    items: buildPcCommonHeaderItems()
      .concat([
        {
          type: ITEM_TYPE.SECTION,
          title: '7. 中断・フォーカス・多重起動',
          helpText:
            '7-6 / 7-7 は二重起動を防いでいないための重点確認項目です。' +
            '二重起動できた場合は、両方でセーブしてセーブ競合が起きるかも必ず確認してください。',
        },
        {
          type: ITEM_TYPE.GRID,
          title: '中断・フォーカス・多重起動',
          helpText: JUDGEMENT_HELP,
          required: true,
          rows: [
            '7-1. Alt + Tab で他アプリへ切り替えられる',
            '7-2. 非アクティブ時の挙動を記録した (停止するか、動き続けるか)',
            '7-3. 復帰後にゲームが正常に続行できる',
            '7-4. 最小化 → 復元で描画が壊れない',
            '7-5. Windows のスリープ → 復帰後にゲームが続行できる',
            '★ 7-6. 同じゲームを2つ同時に起動できてしまわないか確認した',
            '★ 7-7. 2つ起動した状態で両方セーブした場合の挙動を記録した',
            '7-8. タスクマネージャから強制終了 → 再起動で進行不能にならない',
            '7-9. ウィンドウの × ボタンで正常終了する (プロセスが残らない)',
          ],
        },
        {
          type: ITEM_TYPE.SECTION,
          title: '8. セーブ (Windows)',
        },
        {
          type: ITEM_TYPE.GRID,
          title: 'セーブ (Windows)',
          helpText: JUDGEMENT_HELP,
          required: true,
          rows: [
            '8-1. セーブファイルの保存場所を記録した (パス)',
            '8-2. ゲームを終了 → 再起動しても進捗が残る',
            '8-3. PC を再起動しても進捗が残る',
            '8-4. セーブ中にプロセスを強制終了してもデータが壊れない',
            '8-5. セーブファイルを削除すると初回起動状態に戻る',
            '8-6. セーブファイルが平文でないことを確認した',
            '8-7. 別のPCへセーブファイルをコピーして続きから遊べるか記録した',
            '8-8. Windows ユーザー名に日本語が含まれる環境でもセーブできる',
          ],
        },
        {
          type: ITEM_TYPE.SECTION,
          title: '9. UI・表示',
        },
        {
          type: ITEM_TYPE.GRID,
          title: 'UI・表示',
          helpText: JUDGEMENT_HELP,
          required: true,
          rows: [
            '9-1. すべての解像度でテキストが枠から溢れない',
            '9-2. 4K 解像度で文字が小さすぎて読めなくならない',
            '9-3. UI のクリック判定が見た目の位置と一致する',
            '9-4. マウスホバーの反応がある要素で、実際にクリックできる',
            '9-5. スクロールホイールが効くべき画面 (スキルツリー / 一覧) で効く',
            '9-6. ドラッグ&ドロップ (改造画面のスキル入替) がマウスで行える',
            '9-7. 音を切っても、拍・被弾・キルコード成立が視覚だけで分かる',
            '9-8. 仮テキスト (「仮」「TODO」「Lorem」等) が残っていない',
          ],
        },
        {
          type: ITEM_TYPE.SECTION,
          title: '10. Steam 配布 (Steam版のみ)',
          helpText:
            '配信用zip版で回答している場合、この項目群は「試せなかった」を選んでください。',
        },
        {
          type: ITEM_TYPE.GRID,
          title: 'Steam 配布 (Steam版のみ)',
          helpText: JUDGEMENT_HELP,
          required: true,
          rows: [
            '10-1. Steam クライアント経由で起動できる',
            '10-2. Steam オーバーレイ (Shift + Tab) が開き、閉じてもゲームが続行する',
            '10-3. Steam オーバーレイを開いている間、ゲームが誤操作されない',
            '10-4. Steam のスクリーンショット機能が動く',
            '10-5. Steam のコントローラ設定と競合しない',
            '10-6. ストアページの表記と実際の対応内容が一致する (言語 / 対応コントローラ)',
          ],
        },
      ])
      .concat(buildPcTrailingItems()),
  };
}

/** PC版 不具合報告 (見つけた時に1件ずつ)。 */
function buildPcDefectReportFormDefinition() {
  return {
    key: 'P4',
    title: '不具合報告 (PC版)',
    description:
      '不具合を1件見つけるごとに1回送信してください。まとめて書かないでください。所要およそ2分。\n' +
      'スクリーンショットや動画は Discord の不具合報告スレッドに投げて、そのメッセージのリンクを最後の欄に貼ってください。',
    confirmationMessage:
      '報告ありがとうございます。別の不具合も見つけている場合は、もう一度このフォームから送信してください。',
    items: buildPcCommonHeaderItems().concat([
      {
        type: ITEM_TYPE.TEXT_SHORT,
        title: 'QAシートの項目番号',
        helpText: '例: 6-1。チェックリスト以外で見つけた場合は空欄で構いません。',
        required: false,
      },
      {
        type: ITEM_TYPE.SINGLE,
        title: '分類',
        choices: [
          '進行不能',
          'クラッシュ・強制終了',
          'セーブが消えた・戻った',
          '表示崩れ',
          '音が出ない・ずれる',
          '操作できない',
          '動作が重い',
          'ウィンドウ・解像度異常',
          '誤字・文章',
          'その他',
        ],
        required: true,
      },
      {
        type: ITEM_TYPE.SINGLE,
        title: '発生した場面',
        choices: [
          '配布・インストール・起動',
          'ウィンドウ・ディスプレイ',
          '入力 (キーボード・マウス・ゲームパッド)',
          '音声出力',
          '性能・安定性',
          '中断・フォーカス・多重起動',
          'セーブ',
          'UI・表示',
          'Steam配布',
          'ゲームロジック (CBTシート流用範囲)',
          'その他',
        ],
        required: true,
      },
      {
        type: ITEM_TYPE.TEXT_LONG,
        title: '何が起きましたか',
        required: true,
      },
      {
        type: ITEM_TYPE.TEXT_LONG,
        title: '本来どうなるべきだと思いましたか',
        helpText:
          '「不具合」と「仕様どおりだが不満」を分けるために必要です。期待した動きを書いてください。',
        required: true,
      },
      {
        type: ITEM_TYPE.TEXT_LONG,
        title: '再現手順 (順番に書く)',
        helpText: '1. タイトルから… 2. …のように、なぞれば同じことが起きる形で書いてください。',
        required: true,
      },
      {
        type: ITEM_TYPE.SINGLE,
        title: '再現性',
        choices: ['毎回起きる', 'たまに起きる', '1回だけ'],
        required: true,
      },
      {
        type: ITEM_TYPE.SINGLE,
        title: '重要度 (自己申告)',
        helpText: '迷ったら B を選んでください。開発側で再判定します。',
        choices: ['A: これでは遊べない', 'B: 遊べるが気になる', 'C: 細かい点'],
        required: true,
      },
      {
        type: ITEM_TYPE.TEXT_SHORT,
        title: 'スクショ・動画のリンク',
        helpText: 'Discord に投げたメッセージのリンクを貼ってください。無ければ空欄で構いません。',
        required: false,
      },
    ]),
  };
}
