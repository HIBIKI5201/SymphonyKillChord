/**
 * CBT QAフォーム生成 — 設問定義データ。
 *
 * 責務: 生成するフォームの内容を宣言的に保持する。Google Forms API は呼ばない。
 *
 * 原本は Docs/QA/CBT_QA回答方式_2026-09-06.md。
 * 設問を足す・直すときは、md と本ファイルの両方を更新すること。
 */

/** 設問型。FormItemBuilder がこの値で分岐する。 */
const ITEM_TYPE = {
  SECTION: 'SECTION',
  TEXT_SHORT: 'TEXT_SHORT',
  TEXT_LONG: 'TEXT_LONG',
  NUMBER: 'NUMBER',
  DATE: 'DATE',
  SINGLE: 'SINGLE',
  GRID: 'GRID',
};

/** 全グリッド共通の判定列。スマホでの横スクロールを避けるため3択に固定する。 */
const JUDGEMENT_COLUMNS = ['OK', 'NG', '試せなかった'];

const JUDGEMENT_HELP =
  '迷ったら「試せなかった」を選んでください。無理に「OK」を付けると不具合が埋もれます。';

const ANDROID_VERSIONS = ['10以下', '11', '12', '13', '14', '15', '16', 'わからない'];

/** F1〜F5 すべての冒頭に置くヘッダ設問。 */
function buildCommonHeaderItems() {
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
      type: ITEM_TYPE.TEXT_SHORT,
      title: '端末名 (機種名)',
      helpText: '例: Pixel 8',
      required: true,
    },
    {
      type: ITEM_TYPE.SINGLE,
      title: 'Android バージョン',
      choices: ANDROID_VERSIONS,
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
function buildTrailingItems() {
  return [
    {
      type: ITEM_TYPE.TEXT_SHORT,
      title: 'NGにした項目番号',
      helpText:
        '例: 2-5, 3-7。この後「不具合報告」フォームで1件ずつ報告してください。NGが無ければ空欄で構いません。',
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

const NG_FOLLOW_UP_MESSAGE =
  '回答ありがとうございます。NGを付けた項目がある場合は、「不具合報告」フォームから1件ずつ報告してください。';

/**
 * 生成するフォームの定義一覧。
 * items は上から順にフォームへ追加される。
 */
function buildFormDefinitions() {
  return [
    buildIntroductionFormDefinition(),
    buildCombatFormDefinition(),
    buildGameLoopFormDefinition(),
    buildQualityFormDefinition(),
    buildDefectReportFormDefinition(),
  ];
}

/** F1 導入チェック (初回1回)。 */
function buildIntroductionFormDefinition() {
  return {
    key: 'F1',
    title: '①導入チェック',
    description:
      'アプリをインストールした直後に、この順で確認してください。まだ遊び込まなくて構いません。\n' +
      '所要およそ20分。CBT期間中に1回だけ回答します。\n\n' +
      JUDGEMENT_HELP,
    confirmationMessage: NG_FOLLOW_UP_MESSAGE + '\n次は「②戦闘チェック」に進んでください。',
    items: buildCommonHeaderItems()
      .concat([
        {
          type: ITEM_TYPE.SECTION,
          title: '1. 環境・インストール',
          helpText:
            '必ず Google Play の招待リンクからインストールしてください。ファイルを直接入れる (sideload) と、Google Play の14日カウントに乗りません。',
        },
        {
          type: ITEM_TYPE.GRID,
          title: 'インストールできましたか',
          helpText: JUDGEMENT_HELP,
          required: true,
          rows: [
            '1-1. 端末が ARM64 (64bit) である',
            '1-2. Android バージョンを確認した',
            '1-3. 招待リンクからオプトインできた',
            '1-4. Google Play からインストールできた (sideload ではない)',
            '1-5. アプリ一覧にアイコンと表示名が正しく出る',
            '1-6. 14日間アンインストールしない状態にした',
            '1-7. ストレージ空き容量が十分だった',
          ],
        },
        {
          type: ITEM_TYPE.SECTION,
          title: '2. 起動・タイトル',
        },
        {
          type: ITEM_TYPE.GRID,
          title: '起動とタイトル画面',
          helpText: JUDGEMENT_HELP,
          required: true,
          rows: [
            '2-1. 初回起動でクラッシュせずタイトルまで到達する',
            '2-2. 音量設定を変更でき、即時反映される',
            '2-3. クレジットを表示できる',
            '2-4. セーブデータリセットを実行できる',
            '2-5. リセットに確認ダイアログがある',
            '2-6. New Game が 最初のstory → battle tutorial → home tutorial → Home の順に進む',
            '2-7. Continue で Home に進み、前回の進捗が復元される',
            '2-8. 2回目以降の起動が待たされずに終わる',
          ],
        },
        {
          type: ITEM_TYPE.NUMBER,
          title: '2回目の起動にかかった時間 (秒)',
          helpText: 'ストップウォッチが無ければ体感で構いません。',
          required: true,
        },
        {
          type: ITEM_TYPE.SECTION,
          title: '3. チュートリアル',
          helpText: 'チュートリアルを最後まで通してから回答してください。',
        },
        {
          type: ITEM_TYPE.GRID,
          title: 'チュートリアル',
          helpText: JUDGEMENT_HELP,
          required: true,
          rows: [
            '3-1. 通常ステージより前に自動で開始される',
            '3-2. リズムアクションの説明が表示される',
            '3-3. 通常戦闘の説明が表示される',
            '3-4. ボス戦の予告が表示される',
            '3-5. プロンプトが操作を妨げず、達成後に消える',
            '3-6. チュートリアル中に中断・再開できる',
            '3-7. 完了後、説明なしで通常ステージの基本操作ができた',
            '3-8. スキップまたは再プレイができる',
          ],
        },
      ])
      .concat(buildTrailingItems()),
  };
}

/** F2 戦闘チェック (週1)。 */
function buildCombatFormDefinition() {
  return {
    key: 'F2',
    title: '②戦闘チェック',
    description:
      'Stage_01 と Stage_02 を1回ずつ遊んでから回答してください。所要およそ30分。\n' +
      '★が付いた項目は特に重点的に確認してください。\n\n' +
      JUDGEMENT_HELP,
    confirmationMessage: NG_FOLLOW_UP_MESSAGE + '\n次は「③ゲームループチェック」に進んでください。',
    items: buildCommonHeaderItems()
      .concat([
        {
          type: ITEM_TYPE.SECTION,
          title: '4. リズム判定',
          helpText: '攻撃と回避の入力間隔で拍が決まります。',
        },
        {
          type: ITEM_TYPE.GRID,
          title: 'リズム判定',
          helpText: JUDGEMENT_HELP,
          required: true,
          rows: [
            '4-1. 攻撃入力がノーツとして記録される',
            '4-2. 回避入力がノーツとして記録される',
            '4-3. 1/2/3/4/6/8 の6拍種すべてを出せる',
            '4-4. 12拍・16拍が出ない',
            '★ 4-5. 最初の入力が All ノーツになる',
            '★ 4-6. 1.5小節以上あけた後の入力が All ノーツになる',
            '★ 4-7. 被弾直後の入力が All ノーツになる',
            '4-8. 音楽グリッドに合う入力が Just 判定になり、表示で分かる',
            '4-9. 攻撃硬直中に入力が詰まらない',
            '4-10. BPMが違うステージでも拍種の対応が変わらない',
          ],
        },
        {
          type: ITEM_TYPE.SECTION,
          title: '5. 戦闘',
        },
        {
          type: ITEM_TYPE.GRID,
          title: '戦闘',
          helpText: JUDGEMENT_HELP,
          required: true,
          rows: [
            '5-1. 攻撃入力で選択中の敵に射撃が飛ぶ',
            '5-2. 2拍でショットガンになる',
            '5-3. 4拍でハンドガンになる',
            '5-4. 8拍でサブマシンガンになる',
            '5-5. 武器が切り替わったことが画面から分かる',
            '★ 5-6. 障害物越しの射撃が当たらない',
            '★ 5-7. 当たらなかった理由が画面から分かる',
            '5-8. 敵が戦闘BGMに同期して攻撃する',
            '5-9. 敵の攻撃前に線・範囲の予告が出る',
            '5-10. クリティカルとそれ以外のダメージが区別できる',
            '5-11. プレイヤーHPが0で戦闘終了になる',
            '5-12. ロックオン対象の切り替えが意図どおり動く',
            '5-13. 横方向に走った時の揺れが不自然でない',
            '5-14. 敵・ボスのモデルが仮モデルのままでない',
          ],
        },
        {
          type: ITEM_TYPE.SECTION,
          title: '6. キルコード・スキル',
        },
        {
          type: ITEM_TYPE.GRID,
          title: 'キルコード・スキル',
          helpText: JUDGEMENT_HELP,
          required: true,
          rows: [
            '6-1. リズムパターン + アクション種別でキルコードが成立する',
            '6-2. 成立時に視覚・音のフィードバックがある',
            '6-3. 直接攻撃スキルが効果を出す',
            '6-4. プレイヤーbuffスキルが効果を出す',
            '6-5. 敵debuffスキルが効果を出す',
            '6-6. HP条件・命中対象数条件つきスキルが条件どおり発動する',
            '6-7. 初期装備枠が2である',
            '6-8. バフ・デバフ1,2 と攻撃2 を取得するとスロット拡張が解放され装備枠が3になる',
            '★ 6-9. コマンド末尾が同じノーツのスキルを同時装備できない',
            '6-10. スキル個別レベルが改造画面の表示と一致する',
          ],
        },
        {
          type: ITEM_TYPE.SECTION,
          title: '7. 音楽同期',
          helpText: '7-8 は端末をミュートして確認してください。',
        },
        {
          type: ITEM_TYPE.GRID,
          title: '音楽同期',
          helpText: JUDGEMENT_HELP,
          required: true,
          rows: [
            '7-1. 戦闘中は常に戦闘BGMが流れる',
            '7-2. BGMが intro → loop → outro で構成される',
            '★ 7-3. 装備スキルに応じて loop phrase が差し替わる',
            '7-4. 2スキル装備時に original → skill1 → original → skill2 の順で鳴る',
            '7-5. 3スキル装備時に original → skill1 → skill2 → skill3 の順で鳴る',
            '7-6. 現在再生中の音楽要素が HUD から識別できる',
            '★ 7-7. テスト用cueではなく本編BGMが鳴る',
            '7-8. 端末をミュートしても拍が視覚から取れる',
            '7-9. Bluetoothイヤホン接続時に音ズレで判定が壊れない',
            '7-10. 通話・通知で音が中断された後、復帰できる',
          ],
        },
      ])
      .concat(buildTrailingItems()),
  };
}

/** F3 ゲームループチェック (週1)。 */
function buildGameLoopFormDefinition() {
  return {
    key: 'F3',
    title: '③ゲームループチェック',
    description:
      'ステージをクリアして報酬を受け取り、スキルを装備し、もう一度出撃するところまで通してください。所要およそ30分。\n' +
      '★が付いた項目は特に重点的に確認してください。\n\n' +
      JUDGEMENT_HELP,
    confirmationMessage: NG_FOLLOW_UP_MESSAGE + '\n次は「④品質チェック」に進んでください。',
    items: buildCommonHeaderItems()
      .concat([
        {
          type: ITEM_TYPE.SECTION,
          title: '8. リザルト・報酬',
          helpText:
            '8-8 と 8-9 は「同じリザルトで報酬を2回受け取れないか」の確認です。報酬を受け取った直後にアプリを強制終了し、再起動してから確認してください。',
        },
        {
          type: ITEM_TYPE.GRID,
          title: 'リザルト・報酬',
          helpText: JUDGEMENT_HELP,
          required: true,
          rows: [
            '8-1. 成功でリザルトが開く',
            '8-2. 失敗で演出後にゲームプレイが終了する',
            '8-3. 勝利リザルトに ステージ/キャラ/ミッション/時間/最大コンボ/ランク が出る',
            '8-4. 敗北リザルトに攻略Tipsが出る',
            '8-5. 敗北リザルトに「完了」と「再出撃」がある',
            '8-6. ランクが同じプレイ内容で毎回同じになる',
            '★ 8-7. 報酬が受け取れる',
            '★ 8-8. 同じリザルトで報酬を2回受け取れない',
            '★ 8-9. 報酬受領直後に強制終了→再起動しても消えず、二重にもならない',
            '8-10. リザルト完了後の戻り先が毎回同じ (Home か 作戦画面かは問わない)',
          ],
        },
        {
          type: ITEM_TYPE.SECTION,
          title: '9. アウトゲーム',
        },
        {
          type: ITEM_TYPE.GRID,
          title: 'アウトゲーム',
          helpText: JUDGEMENT_HELP,
          required: true,
          rows: [
            '9-1. Home から 作戦/研究/改造・編成/設定 すべてに行ける',
            '9-2. 作戦画面が battle と scenario を左→右の解放treeで表示する',
            '9-3. battle detail に ミッション条件/報酬/過去progress が出る',
            '9-4. ステージ選択後、出撃前の編成確認が開く',
            '★ 9-5. Stage_03 以降のノードが選択できない',
            '9-6. 改造画面でスキルの詳細・プレビューが見える',
            '9-7. 改造画面で入れ替え操作ができる',
            '9-8. 明示的な保存操作がある',
            '9-9. 未保存で離脱すると保存/破棄の確認が出る',
            '9-10. 研究treeに 現在値/cost/解放可能・locked が出る',
            '9-11. 研究treeの reset が動く',
            '9-12. スキルツリーの初期フォーカスが未解放ノード付近に合う',
            '9-13. 設定がオーバーレイで開き、決定/取消がある',
            '9-14. アウトゲームUIが画面比率に追従する (見切れ・重なりがない)',
            '9-15. ホーム画面の各ボタン画像が正しく表示される',
          ],
        },
        {
          type: ITEM_TYPE.SECTION,
          title: '10. セーブ・復帰',
          helpText:
            '10-8 はアプリ更新をまたいで確認する項目です。CBT期間中にビルドが更新されたら必ず1回実施してください。' +
            'QAシートの 10-9 (セーブデータが平文でない) は開発側で確認するため、この一覧には含めていません。',
        },
        {
          type: ITEM_TYPE.GRID,
          title: 'セーブ・復帰',
          helpText: JUDGEMENT_HELP,
          required: true,
          rows: [
            '10-1. クリア済みstageが保存される',
            '10-2. skill-tree進捗が保存される',
            '★ 10-3. 装備中キルコードと順序が保存される',
            '★ 10-4. キルコード別levelが保存される',
            '★ 10-5. 設定が保存される',
            '10-6. セーブ中に強制終了してもデータが壊れない',
            '10-7. 端末を再起動しても進捗が残る',
            '★ 10-8. アプリ更新後も進捗が残る',
            '★ 10-10. データ破損時に無限クラッシュにならず復旧できる',
          ],
        },
        {
          type: ITEM_TYPE.SECTION,
          title: '11. 中断・回復・異常系',
        },
        {
          type: ITEM_TYPE.GRID,
          title: '中断・回復・異常系',
          helpText: JUDGEMENT_HELP,
          required: true,
          rows: [
            '11-1. 戦闘中にポーズできる',
            '11-2. ポーズ中は時間が止まる',
            '11-3. 再開時にカウントダウンが出る',
            '11-4. バックグラウンドに送って戻っても復帰できる',
            '11-5. 着信・通知で中断された後、復帰できる',
            '11-6. 画面回転・分割画面で破綻しない',
            '11-7. 省電力モードで極端にフレームが落ちない',
            '11-8. Bluetoothコントローラの接続・切断で操作不能にならない',
            '11-9. タッチ操作だけで全機能に到達できる',
            '11-10. ロード中に進捗表示が出る',
            '11-11. 強制終了→再起動で進行不能にならない',
          ],
        },
      ])
      .concat(buildTrailingItems()),
  };
}

/** F4 品質チェック (週1)。 */
function buildQualityFormDefinition() {
  return {
    key: 'F4',
    title: '④品質チェック',
    description:
      '15分続けて遊んでから回答してください。途中で止めた場合は「試せなかった」を選んでください。所要およそ20分。\n\n' +
      JUDGEMENT_HELP,
    confirmationMessage: NG_FOLLOW_UP_MESSAGE,
    items: buildCommonHeaderItems()
      .concat([
        {
          type: ITEM_TYPE.SECTION,
          title: '12. 性能・安定性',
        },
        {
          type: ITEM_TYPE.GRID,
          title: '性能・安定性',
          helpText: JUDGEMENT_HELP,
          required: true,
          rows: [
            '12-1. 動作がカクつかない (30 FPS 以上を維持)',
            '12-2. インゲームのロードが3秒以内に終わる',
            '12-3. 15分プレイしてエラーが出ない',
            '12-4. 15分プレイして動作が重くなっていかない',
            '12-5. 連続プレイで端末が過度に発熱しない',
            '12-6. ステージ再入場を5回繰り返しても重くならない',
            '12-7. バッテリー消費が極端でない',
          ],
        },
        {
          type: ITEM_TYPE.NUMBER,
          title: 'インゲームのロード時間 (秒)',
          helpText: 'ストップウォッチが無ければ体感で構いません。',
          required: true,
        },
        {
          type: ITEM_TYPE.NUMBER,
          title: '15分プレイでのバッテリー減少 (%)',
          helpText: 'プレイ前後のバッテリー残量の差を入れてください。',
          required: true,
        },
        {
          type: ITEM_TYPE.SINGLE,
          title: '体感のなめらかさ',
          helpText: 'FPS を測る必要はありません。体感で選んでください。',
          choices: ['なめらか', 'たまにカクつく', '常にカクつく', '遊べないほど重い'],
          required: true,
        },
        {
          type: ITEM_TYPE.SECTION,
          title: '13. テキスト・表示',
        },
        {
          type: ITEM_TYPE.GRID,
          title: 'テキスト・表示',
          helpText: JUDGEMENT_HELP,
          required: true,
          rows: [
            '13-1. 誤字脱字がない',
            '13-2. テキストが枠から溢れない・見切れない',
            '13-3. 小さい画面でも文字が読める',
            '13-4. 仮テキスト (「仮」「TODO」等) が残っていない',
            '13-5. キャラクターの立ち絵が正しく表示される',
            '13-6. 音を切っても、拍・被弾・キルコード成立が視覚だけで分かる',
          ],
        },
      ])
      .concat(buildTrailingItems()),
  };
}

/** F5 不具合報告 (見つけた時に1件ずつ)。 */
function buildDefectReportFormDefinition() {
  return {
    key: 'F5',
    title: '不具合報告',
    description:
      '不具合を1件見つけるごとに1回送信してください。まとめて書かないでください。所要およそ2分。\n' +
      'スクリーンショットや動画は Discord の不具合報告スレッドに投げて、そのメッセージのリンクを最後の欄に貼ってください。',
    confirmationMessage:
      '報告ありがとうございます。別の不具合も見つけている場合は、もう一度このフォームから送信してください。',
    items: buildCommonHeaderItems().concat([
      {
        type: ITEM_TYPE.TEXT_SHORT,
        title: 'QAシートの項目番号',
        helpText: '例: 8-8。チェックリスト以外で見つけた場合は空欄で構いません。',
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
          '誤字・文章',
          'その他',
        ],
        required: true,
      },
      {
        type: ITEM_TYPE.SINGLE,
        title: '発生した場面',
        choices: [
          '起動前・インストール',
          'タイトル',
          'チュートリアル',
          'Stage_01',
          'Stage_02',
          'リザルト・報酬',
          'アウトゲーム (Home・作戦・研究・改造)',
          '設定',
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
