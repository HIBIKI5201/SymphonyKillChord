/**
 * CBT QAフォーム生成 — 実行の入口。
 *
 * 責務: 前提条件の検証、生成の順次実行、結果の出力。
 * フォームの作り方そのものは FormFactory / FormItemBuilder の責務。
 *
 * 使い方は README.md を参照。エディタで createQaForms を選んで実行する。
 */

/** 二重実行を検知するために、生成済みフォームIDを保存するキー。 */
const CREATED_FORMS_PROPERTY_KEY = 'SKC_CBT_QA_CREATED_FORMS';

/**
 * エントリポイント。5つのフォームを作り、URL一覧をログに出す。
 */
function createQaForms() {
  assertNotAlreadyCreated_();

  const spreadsheetId = resolveDestinationSpreadsheetId_();
  const definitions = buildFormDefinitions();
  const results = [];

  definitions.forEach(function (definition) {
    const result = createForm(definition, spreadsheetId);
    results.push(result);
    Logger.log('作成しました: ' + result.title);
  });

  saveCreatedForms_(results);
  writeSummarySheet_(spreadsheetId, results);
  logSummary_(spreadsheetId, results);
}

/**
 * 生成済みフォームの記録を消す。
 * ALLOW_RECREATE を使わずに作り直したい場合や、
 * 手作業でフォームを消した後に記録だけ残った場合に実行する。
 */
function resetCreatedFormsRecord() {
  PropertiesService.getScriptProperties().deleteProperty(CREATED_FORMS_PROPERTY_KEY);
  Logger.log('生成済みの記録を消しました。次回 createQaForms を実行すると作り直します。');
}

/**
 * 二重実行を止める。
 * フォームが重複すると、どちらに回答されたか分からなくなり集計が壊れる。
 */
function assertNotAlreadyCreated_() {
  if (CONFIG.ALLOW_RECREATE) {
    return;
  }
  const saved = PropertiesService.getScriptProperties().getProperty(CREATED_FORMS_PROPERTY_KEY);
  if (saved) {
    throw new Error(
      'フォームは既に作成済みです。作り直す場合は Config.gs の ALLOW_RECREATE を true にするか、' +
        'resetCreatedFormsRecord を実行してください。作成済みのURLはログまたは回答スプレッドシートの' +
        '「フォームURL一覧」タブで確認できます。'
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

function saveCreatedForms_(results) {
  PropertiesService.getScriptProperties().setProperty(
    CREATED_FORMS_PROPERTY_KEY,
    JSON.stringify(results)
  );
}

/**
 * 回答スプレッドシートに、配布用のURL一覧タブを作る。
 * QAメンバーへ配るときにここを見れば済むようにする。
 */
function writeSummarySheet_(spreadsheetId, results) {
  const spreadsheet = SpreadsheetApp.openById(spreadsheetId);
  const sheetName = 'フォームURL一覧';
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

function logSummary_(spreadsheetId, results) {
  Logger.log('--- 作成結果 ---');
  Logger.log('回答スプレッドシート: ' + SpreadsheetApp.openById(spreadsheetId).getUrl());
  results.forEach(function (result) {
    Logger.log(result.key + ' ' + result.title + ' : ' + result.publishedUrl);
  });
  Logger.log('配布用URLは回答スプレッドシートの「フォームURL一覧」タブにも書き出しました。');
}
