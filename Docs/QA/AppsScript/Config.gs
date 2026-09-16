/**
 * CBT QAフォーム生成 — 設定値。
 *
 * 責務: 実行前に人が書き換える設定値だけを保持する。ロジックは持たない。
 */

const CONFIG = {
  /**
   * 回答の集約先スプレッドシートのID。
   * スプレッドシートのURL https://docs.google.com/spreadsheets/d/【ここ】/edit の部分。
   * 空文字のままにする場合は CREATE_SPREADSHEET_IF_MISSING を true にすること。
   */
  DESTINATION_SPREADSHEET_ID: '1g3NfpsxYKHK5FMxiSqd3pAEkX7CCiHfw9fWVXzvBiqE',

  /**
   * DESTINATION_SPREADSHEET_ID が空のときに、新しいスプレッドシートを作るかどうか。
   * 既定は false。設定漏れに黙って別の入れ物を作ってしまうのを避けるため、
   * 新規作成は明示的に選んだときだけ行う。
   */
  CREATE_SPREADSHEET_IF_MISSING: false,

  /** CREATE_SPREADSHEET_IF_MISSING が true のときに作るスプレッドシートの名前。 */
  NEW_SPREADSHEET_NAME: 'Symphony Kill Chord CBT QA 回答',

  /**
   * 作成済みのフォームがあっても、もう一度作り直すかどうか。
   * 既定は false。二重実行でフォームが重複するのを防ぐ。
   * 作り直したいときだけ true にして実行し、終わったら false に戻すこと。
   */
  ALLOW_RECREATE: false,

  /** フォーム名の接頭辞。回答スプレッドシートのタブ名にもなる。 */
  FORM_TITLE_PREFIX: '[SKC CBT]',
};
