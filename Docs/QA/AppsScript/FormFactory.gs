/**
 * CBT QAフォーム生成 — フォーム本体の生成と設定。
 *
 * 責務: 定義1件からフォームを1つ作り、回答方針の設定と出力先の接続まで行う。
 * 設問アイテムの組み立ては FormItemBuilder の責務。
 */

/**
 * 定義1件からフォームを作る。
 *
 * @param {Object} definition フォーム定義 (FormDefinitions.gs)
 * @param {string} spreadsheetId 回答の集約先スプレッドシートID
 * @return {{key: string, title: string, publishedUrl: string, editUrl: string}}
 */
function createForm(definition, spreadsheetId) {
  const title = CONFIG.FORM_TITLE_PREFIX + ' ' + definition.title;
  const form = FormApp.create(title);

  form.setDescription(definition.description);
  applyResponsePolicy_(form);

  if (definition.confirmationMessage) {
    form.setConfirmationMessage(definition.confirmationMessage);
  }

  definition.items.forEach(function (itemDefinition) {
    addItemToForm(form, itemDefinition);
  });

  form.setDestination(FormApp.DestinationType.SPREADSHEET, spreadsheetId);

  return {
    key: definition.key,
    title: title,
    publishedUrl: form.getPublishedUrl(),
    editUrl: form.getEditUrl(),
  };
}

/**
 * 回答の受け付け方を設定する。
 *
 * メールアドレス収集と1人1回制限をどちらも切ることで、
 * 回答者に Google ログインを要求しない状態にする。
 * QAメンバーは同じフォームへ何度も回答するため、1人1回制限は特に外す必要がある。
 */
function applyResponsePolicy_(form) {
  form.setCollectEmail(false);
  form.setLimitOneResponsePerUser(false);
  form.setAllowResponseEdits(false);
  form.setProgressBar(true);
  form.setShuffleQuestions(false);
}
