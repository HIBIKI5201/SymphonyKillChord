/**
 * QAフォーム生成 — 実行の入口。
 *
 * 責務: 前提条件の検証、生成の順次実行、結果の出力。
 * フォームの作り方そのものは FormFactory / FormItemBuilder の責務。
 *
 * 使い方は README.md を参照。エディタで createQaForms (Android/CBT) または
 * createPcQaForms (PC/Steam) を選んで実行する。
 */

/** 二重実行を検知するために、生成済みフォームIDを保存するキー (Android/CBT用)。 */
const CREATED_FORMS_PROPERTY_KEY = 'SKC_CBT_QA_CREATED_FORMS';

/** 二重実行を検知するために、生成済みフォームIDを保存するキー (PC用)。 */
const CREATED_PC_FORMS_PROPERTY_KEY = 'SKC_PC_QA_CREATED_FORMS';

/** Android/CBT用の回答スプレッドシートのタブ名。 */
const FORM_URL_SHEET_NAME = 'フォームURL一覧';

/** PC用の回答スプレッドシートのタブ名。Android用と同じシートを汚さないよう別タブにする。 */
const PC_FORM_URL_SHEET_NAME = 'フォームURL一覧 (PC)';

/**
 * エントリポイント (Android/CBT)。5つのフォームを作り、URL一覧をログに出す。
 */
function createQaForms() {
  createForms_({
    definitions: buildFormDefinitions(),
    propertyKey: CREATED_FORMS_PROPERTY_KEY,
    sheetName: FORM_URL_SHEET_NAME,
  });
}

/**
 * エントリポイント (PC/Steam)。4つのフォームを作り、URL一覧をログに出す。
 * Android/CBT用フォームとは二重実行の記録・出力タブを分けているため、
 * 片方だけを作り直すことができる。
 */
function createPcQaForms() {
  createForms_({
    definitions: buildPcFormDefinitions(),
    propertyKey: CREATED_PC_FORMS_PROPERTY_KEY,
    sheetName: PC_FORM_URL_SHEET_NAME,
  });
}

/**
 * 生成済みフォームの記録を消す (Android/CBT)。
 * ALLOW_RECREATE を使わずに作り直したい場合や、
 * 手作業でフォームを消した後に記録だけ残った場合に実行する。
 */
function resetCreatedFormsRecord() {
  resetCreatedFormsRecord_(CREATED_FORMS_PROPERTY_KEY, 'createQaForms');
}

/** 生成済みフォームの記録を消す (PC)。用途は resetCreatedFormsRecord と同じ。 */
function resetCreatedPcFormsRecord() {
  resetCreatedFormsRecord_(CREATED_PC_FORMS_PROPERTY_KEY, 'createPcQaForms');
}

/**
 * フォーム定義一式からフォームを作り、記録・出力タブ・ログを揃える。
 * Android/CBT と PC のどちらの実行にも使う共通処理。
 *
 * @param {{definitions: Array<Object>, propertyKey: string, sheetName: string}} options
 */
function createForms_(options) {
  const propertyKey = options.propertyKey;
  const sheetName = options.sheetName;

  assertNotAlreadyCreated_(propertyKey, sheetName);

  const spreadsheetId = resolveDestinationSpreadsheetId_();
  const results = [];

  options.definitions.forEach(function (definition) {
    const result = createForm(definition, spreadsheetId);
    results.push(result);
    Logger.log('作成しました: ' + result.title);
  });

  saveCreatedForms_(propertyKey, results);
  writeSummarySheet_(spreadsheetId, sheetName, results);
  logSummary_(spreadsheetId, sheetName, results);
}

function resetCreatedFormsRecord_(propertyKey, entryPointName) {
  PropertiesService.getScriptProperties().deleteProperty(propertyKey);
  Logger.log('生成済みの記録を消しました。次回 ' + entryPointName + ' を実行すると作り直します。');
}

/**
 * 二重実行を止める。
 * フォームが重複すると、どちらに回答されたか分からなくなり集計が壊れる。
 */
function assertNotAlreadyCreated_(propertyKey, sheetName) {
  if (CONFIG.ALLOW_RECREATE) {
    return;
  }
  const saved = PropertiesService.getScriptProperties().getProperty(propertyKey);
  if (saved) {
    throw new Error(
      'フォームは既に作成済みです。作り直す場合は Config.gs の ALLOW_RECREATE を true にするか、' +
        '対応する resetCreated...FormsRecord を実行してください。作成済みのURLはログまたは回答スプレッドシートの' +
        '「' + sheetName + '」タブで確認できます。'
    );
  }
}

/**
 * 回答の集約先スプレッドシートIDを決める。
 *
 * 未設定のまま黙って新規作成すると、意図しない場所に回答が溜まって発見が遅れる。
 * 新規作成は CREATE_SPREADSHEET_IF_MISSING で明示的に選んだときだけ行う。
 */
function resolveDestinationSpreadsheetId_() {
  const configured = (CONFIG.DESTINATION_SPREADSHEET_ID || '').trim();
  if (configured) {
    assertSpreadsheetAccessible_(configured);
    return configured;
  }

  if (!CONFIG.CREATE_SPREADSHEET_IF_MISSING) {
    throw new Error(
      'Config.gs の DESTINATION_SPREADSHEET_ID が空です。' +
        '回答をまとめるスプレッドシートを作ってIDを設定するか、' +
        'CREATE_SPREADSHEET_IF_MISSING を true にして新規作成を選んでください。'
    );
  }

  const created = SpreadsheetApp.create(CONFIG.NEW_SPREADSHEET_NAME);
  Logger.log('回答スプレッドシートを新規作成しました: ' + created.getUrl());
  return created.getId();
}

/** 設定されたIDが実在し、開ける権限があるかを入口で確かめる。 */
function assertSpreadsheetAccessible_(spreadsheetId) {
  try {
    SpreadsheetApp.openById(spreadsheetId);
  } catch (error) {
    throw new Error(
      'DESTINATION_SPREADSHEET_ID のスプレッドシートを開けません。' +
        'IDが正しいか、このアカウントに編集権限があるかを確認してください。ID: ' +
        spreadsheetId
    );
  }
}

function saveCreatedForms_(propertyKey, results) {
  PropertiesService.getScriptProperties().setProperty(propertyKey, JSON.stringify(results));
}

/**
 * 回答スプレッドシートに、配布用のURL一覧タブを作る。
 * QAメンバーへ配るときにここを見れば済むようにする。
 */
function writeSummarySheet_(spreadsheetId, sheetName, results) {
  const spreadsheet = SpreadsheetApp.openById(spreadsheetId);
  const existing = spreadsheet.getSheetByName(sheetName);
  const sheet = existing ? existing : spreadsheet.insertSheet(sheetName, 0);
  sheet.clear();

  const rows = [['ID', 'フォーム名', '配布用URL (QAメンバーに渡す)', '編集用URL (運営のみ)']];
  results.forEach(function (result) {
    rows.push([result.key, result.title, result.publishedUrl, result.editUrl]);
  });

  sheet.getRange(1, 1, rows.length, rows[0].length).setValues(rows);
  sheet.setFrozenRows(1);
  sheet.autoResizeColumns(1, rows[0].length);
}

function logSummary_(spreadsheetId, sheetName, results) {
  Logger.log('--- 作成結果 ---');
  Logger.log('回答スプレッドシート: ' + SpreadsheetApp.openById(spreadsheetId).getUrl());
  results.forEach(function (result) {
    Logger.log(result.key + ' ' + result.title + ' : ' + result.publishedUrl);
  });
  Logger.log('配布用URLは回答スプレッドシートの「' + sheetName + '」タブにも書き出しました。');
}
