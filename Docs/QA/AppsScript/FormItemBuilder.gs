/**
 * CBT QAフォーム生成 — 設問定義から Google Forms のアイテムを組み立てる。
 *
 * 責務: 定義データ1件を Form のアイテム1件へ変換する。
 * フォーム本体の生成・設定・出力先の接続は FormFactory の責務。
 */

/**
 * 定義1件をフォームへ追加する。
 * 未知の型は握りつぶさず即エラーにする (定義の書き間違いを黙って落とさないため)。
 *
 * @param {GoogleAppsScript.Forms.Form} form 追加先のフォーム
 * @param {Object} definition 設問定義 (FormDefinitions.gs)
 */
function addItemToForm(form, definition) {
  switch (definition.type) {
    case ITEM_TYPE.SECTION:
      addSectionItem_(form, definition);
      return;
    case ITEM_TYPE.TEXT_SHORT:
      addTextItem_(form, definition);
      return;
    case ITEM_TYPE.TEXT_LONG:
      addParagraphItem_(form, definition);
      return;
    case ITEM_TYPE.NUMBER:
      addNumberItem_(form, definition);
      return;
    case ITEM_TYPE.DATE:
      addDateItem_(form, definition);
      return;
    case ITEM_TYPE.SINGLE:
      addSingleChoiceItem_(form, definition);
      return;
    case ITEM_TYPE.GRID:
      addGridItem_(form, definition);
      return;
    default:
      throw new Error(
        '未知の設問型です: ' + definition.type + ' (設問: ' + definition.title + ')'
      );
  }
}

/** セクション区切り。長いフォームを画面ごとに分けて離脱を減らす。 */
function addSectionItem_(form, definition) {
  const item = form.addPageBreakItem().setTitle(definition.title);
  applyHelpText_(item, definition);
}

function addTextItem_(form, definition) {
  const item = form.addTextItem().setTitle(definition.title);
  applyHelpText_(item, definition);
  item.setRequired(isRequired_(definition));
}

function addParagraphItem_(form, definition) {
  const item = form.addParagraphTextItem().setTitle(definition.title);
  applyHelpText_(item, definition);
  item.setRequired(isRequired_(definition));
}

/** 数値入力。0未満を弾くことで、単位の取り違えや記号混入を入口で落とす。 */
function addNumberItem_(form, definition) {
  const item = form.addTextItem().setTitle(definition.title);
  applyHelpText_(item, definition);
  item.setRequired(isRequired_(definition));
  item.setValidation(
    FormApp.createTextValidation()
      .setHelpText('0以上の数値を入れてください。')
      .requireNumberGreaterThanOrEqualTo(0)
      .build()
  );
}

function addDateItem_(form, definition) {
  const item = form.addDateItem().setTitle(definition.title);
  applyHelpText_(item, definition);
  item.setRequired(isRequired_(definition));
}

function addSingleChoiceItem_(form, definition) {
  if (!definition.choices || definition.choices.length === 0) {
    throw new Error('選択肢がありません: ' + definition.title);
  }
  const item = form.addMultipleChoiceItem().setTitle(definition.title);
  applyHelpText_(item, definition);
  item.setChoiceValues(definition.choices);
  item.setRequired(isRequired_(definition));
}

/**
 * 選択式グリッド。列は JUDGEMENT_COLUMNS で3択に固定する。
 * setRequired(true) は「各行に回答を1つ必須」を意味し、
 * 「OK」と「未確認」が空欄で混ざるのを防ぐ。
 */
function addGridItem_(form, definition) {
  if (!definition.rows || definition.rows.length === 0) {
    throw new Error('グリッドの行がありません: ' + definition.title);
  }
  const item = form.addGridItem().setTitle(definition.title);
  applyHelpText_(item, definition);
  item.setRows(definition.rows);
  item.setColumns(definition.columns || JUDGEMENT_COLUMNS);
  item.setRequired(isRequired_(definition));
}

function applyHelpText_(item, definition) {
  if (definition.helpText) {
    item.setHelpText(definition.helpText);
  }
}

function isRequired_(definition) {
  return definition.required === true;
}
