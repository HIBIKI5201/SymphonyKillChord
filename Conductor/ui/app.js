// KillChord Conductor の画面。自分では何も実行せず、ホスト（KillChordConductor.exe）を通して conductor.exe を呼ぶ。
//   cli  : conductor を動かして出力を受け取る（状態・一覧・Unity を開くなど、すぐ終わるもの）
//   run  : スクリプトを動かす（新しいウィンドウ・出力をこの画面で受ける・管理者）
//   stop : 出力を受けている実行を止める
//   pick : ファイル・フォルダを選ぶ画面を出す
"use strict";

/* ── ホストとのやりとり ─────────────────────────────── */

const host = (() => {
  const webview = window.chrome && window.chrome.webview;
  const pending = new Map();
  const listeners = new Set();
  let nextId = 1;

  if (webview) {
    webview.addEventListener("message", (event) => {
      const message = typeof event.data === "string" ? JSON.parse(event.data) : event.data;
      if (message.type === "reply") {
        const resolve = pending.get(message.id);
        pending.delete(message.id);
        if (resolve) { resolve(message.result || {}); }
        return;
      }
      listeners.forEach((listener) => listener(message));
    });
  }

  function request(type, payload) {
    if (!webview) { return Promise.resolve({ started: false, exitCode: -1, stdout: "", stderr: "ホストにつながっていません。" }); }
    const id = String(nextId++);
    return new Promise((resolve) => {
      pending.set(id, resolve);
      webview.postMessage({ type, id, ...payload });
    });
  }

  return {
    isDevMock: Boolean(webview && webview.isDevMock),
    request,
    on(listener) { listeners.add(listener); },
    cli(args) { return request("cli", { args }); },
    async cliJson(args) {
      const result = await request("cli", { args: [...args, "--json"] });
      let data = null;
      try { data = result.stdout ? JSON.parse(result.stdout) : null; } catch (error) { data = null; }
      return { ...result, data };
    },
  };
})();

/* ── 小さな道具 ─────────────────────────────────────── */

const $ = (selector, root) => (root || document).querySelector(selector);

function esc(value) {
  return String(value == null ? "" : value).replace(/[&<>"']/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));
}

function storageGet(key, fallback) {
  try { const raw = localStorage.getItem(key); return raw == null ? fallback : JSON.parse(raw); } catch (error) { return fallback; }
}

function storageSet(key, value) {
  try { localStorage.setItem(key, JSON.stringify(value)); } catch (error) { /* 保存できなくても画面は動かす */ }
}

/// CLI の標準エラーから、人に見せる1文を取り出す。
function cliMessage(result, fallback) {
  const text = (result.stderr || "").split(/\r?\n/).map((line) => line.replace(/^conductor:\s*/, "").trim()).filter(Boolean).join("\n");
  return text || (result.stdout || "").trim() || fallback || `終了コード ${result.exitCode}`;
}

function quoteArg(arg) { return /[\s"]/.test(arg) || arg === "" ? `"${arg.replace(/"/g, '\\"')}"` : arg; }
function formatCommand(args) { return ["conductor", ...args].map((arg, i) => (i === 0 ? arg : quoteArg(arg))).join(" "); }

function relativeTime(time) {
  const seconds = Math.round((Date.now() - time) / 1000);
  if (seconds < 10) { return "たった今"; }
  if (seconds < 60) { return `${seconds} 秒前`; }
  const minutes = Math.round(seconds / 60);
  if (minutes < 60) { return `${minutes} 分前`; }
  const hours = Math.round(minutes / 60);
  if (hours < 24) { return `${hours} 時間前`; }
  return new Date(time).toLocaleDateString("ja-JP", { month: "numeric", day: "numeric" });
}

function duration(ms) {
  if (ms == null) { return ""; }
  const seconds = Math.max(0, Math.round(ms / 1000));
  return seconds < 60 ? `${seconds} 秒` : `${Math.floor(seconds / 60)} 分 ${seconds % 60} 秒`;
}

function normalize(text) {
  // カタカナはひらがなに、英字は小文字にそろえて探す。
  return String(text || "").toLowerCase().replace(/[\u30a1-\u30f6]/g, (c) => String.fromCharCode(c.charCodeAt(0) - 0x60));
}

function matches(query, ...fields) {
  const tokens = normalize(query).split(/\s+/).filter(Boolean);
  if (tokens.length === 0) { return true; }
  const haystack = normalize(fields.join(" "));
  return tokens.every((token) => haystack.includes(token));
}

function highlight(text, query) {
  const safe = esc(text);
  const tokens = String(query || "").split(/\s+/).filter(Boolean).map((t) => t.replace(/[.*+?^${}()|[\]\\]/g, "\\$&"));
  if (tokens.length === 0) { return safe; }
  return safe.replace(new RegExp(`(${tokens.join("|")})`, "gi"), "<mark>$1</mark>");
}

/* ── 決まりごと ─────────────────────────────────────── */

const ROLE_INFO = {
  programmer: { icon: "code", note: "コード・コンパイル確認・ブランチの片付け" },
  planner: { icon: "clipboard", note: "仕様・データ・実機での確認" },
  designer: { icon: "palette", note: "素材の取り込み・見た目の確認" },
  sound: { icon: "music", note: "CRI Atom Craft・サウンドの素材" },
  lead: { icon: "crown", note: "ランナー・メンバー・公開の管理" },
};

const CATEGORY_ICON = { 初期化: "sparkle", Git: "branch", Android: "phone", ビルドランナー: "server", ホームページ: "globe" };

/// 組み込みの操作（conductor のコマンド）。home.json の action の名前と同じ。
const BUILTINS = {
  "unity-open": { title: "Unity を開く", note: "このプロジェクトを、決められた版の Unity で開く", icon: "cube", args: ["unity", "open"], done: "Unity を開いています" },
  "unity-install": { title: "Unity を入れる", note: "この版の Unity を Unity Hub で入れる画面を開く", icon: "download", args: ["unity", "install"], done: "Unity Hub を開きました" },
  "cri-open": { title: "CRI Atom Craft を開く", note: "サウンドのプロジェクトを開く", icon: "wave", args: ["cri", "open"], done: "CRI Atom Craft を開いています" },
  "assets-open": { title: "Drive の素材フォルダを開く", note: "Google Drive の SymphonyKillChord フォルダを開く", icon: "folder", args: ["assets", "open"], done: "Drive のフォルダを開きました" },
  "assets-import": { title: "Drive の素材を Unity へ取り込む", note: "開いている Unity で、Drive の素材を同期する", icon: "import", args: ["assets", "import"], done: "取り込みを始めました" },
  "docs-viewer": { title: "仕様書ビューアを開く", note: "開いている Unity で、リポジトリの仕様書を読む", icon: "book", args: ["docs", "viewer"], done: "仕様書ビューアを開きました" },
  "repo-open": { title: "リポジトリのフォルダを開く", note: "エクスプローラーでプロジェクトのフォルダを開く", icon: "folder", args: ["repo", "open"], done: "フォルダを開きました" },
};

const LAUNCH_ACTIONS = new Set(["unity-open", "unity-install", "cri-open", "assets-open", "assets-import", "docs-viewer"]);

const PATH_SETTINGS = [
  { key: "unityEditorPath", title: "Unity エディタ（Unity.exe）", kind: "file", note: "空なら Unity CLI か、Unity Hub の入れ先から自動で探します。", detected: () => state.status && state.status.unity.editor },
  { key: "criAtomCraftPath", title: "CRI Atom Craft（exe）", kind: "file", note: "空なら、よくある場所を探し、無ければプロジェクトのファイルを関連付けで開きます。", detected: () => state.status && state.status.cri.app },
  { key: "driveFolder", title: "Google Drive の SymphonyKillChord フォルダ", kind: "folder", note: "空なら G:\\マイドライブ などを探します。", detected: () => state.status && state.status.assets.driveFolder },
];

/* ── 状態 ─────────────────────────────────────────── */

const state = {
  page: "home",
  hostInfo: null,
  home: null,
  homeError: "",
  scripts: [],
  scriptsError: "",
  status: null,
  statusError: "",
  statusLoadedAt: 0,
  statusLoading: false,
  settings: {},
  version: "",
  runs: storageGet("conductor.runs", []).filter((run) => run.status !== "running"),
  lastResults: storageGet("conductor.lastResults", {}),
  drawerRunId: null,
  drawerCollapsed: false,
  toolsQuery: "",
  toolsFilter: "all",
  toolsSelected: storageGet("conductor.toolsSelected", null),
  selectedPreset: {},
  busy: new Set(),
  theme: storageGet("conductor.theme", "system"),
};

function saveRuns() {
  const trimmed = state.runs.slice(0, 40).map((run) => ({ ...run, lines: run.lines.slice(-1500) }));
  storageSet("conductor.runs", trimmed);
}

/* ── テーマ ───────────────────────────────────────── */

const darkQuery = window.matchMedia("(prefers-color-scheme: dark)");

function applyTheme() {
  const isDark = state.theme === "dark" || (state.theme === "system" && darkQuery.matches);
  document.documentElement.dataset.theme = isDark ? "dark" : "light";
}

darkQuery.addEventListener("change", applyTheme);

/* ── データの読み込み ───────────────────────────────── */

async function loadHome() {
  const result = await host.cliJson(["home"]);
  if (result.exitCode === 0 && result.data) { state.home = result.data; state.homeError = ""; }
  else { state.homeError = cliMessage(result, "ホーム画面の登録を読めません。"); }
}

async function loadScripts() {
  const result = await host.cliJson(["scripts", "list"]);
  if (result.exitCode === 0 && result.data) { state.scripts = result.data.scripts || []; state.scriptsError = ""; }
  else { state.scriptsError = cliMessage(result, "スクリプトの一覧を読めません。"); }
}

async function loadSettings() {
  const result = await host.cliJson(["config", "list"]);
  if (result.exitCode === 0 && result.data) { state.settings = result.data; }
}

async function loadStatus(options) {
  if (state.statusLoading) { return; }
  state.statusLoading = true;
  if (!options || !options.quiet) { renderPartial(); }
  const result = await host.cliJson(["status"]);
  state.statusLoading = false;
  state.statusLoadedAt = Date.now();
  // git が失敗しても（終了コード 1）、JSON には読めたところまで入っている。
  if (result.data) { state.status = result.data; state.statusError = result.data.git && result.data.git.error ? result.data.git.error : ""; }
  else { state.statusError = cliMessage(result, "状態を読めません。"); }
  renderPartial();
}

async function loadAll() {
  await Promise.all([loadHome(), loadScripts(), loadSettings(), loadStatus({ quiet: true }),
    host.cli(["version"]).then((result) => { state.version = (result.stdout || "").trim(); })]);
  render();
}

/* ── 操作の一覧 ─────────────────────────────────────── */

/// ホーム画面のボタン（役職の順）。
function homeTiles() { return state.home ? state.home.tiles : []; }

function roleTileIds() {
  const role = state.home && state.home.role;
  return new Set(homeTiles().filter((tile) => role && (tile.roles || []).includes(role)).map((tile) => tile.id));
}

function findScript(id) { return state.scripts.find((script) => script.id === id); }

function scriptIcon(script) { return CATEGORY_ICON[script && script.category] || "terminal"; }

/// 操作1つ分の説明（ボタン・パレット・一覧で共通に使う）。
function describeTile(tile) {
  if (tile.action) {
    const builtin = BUILTINS[tile.action] || {};
    return { key: `action:${tile.action}`, kind: "action", action: tile.action, title: tile.title || builtin.title, note: tile.note || builtin.note, icon: builtin.icon || "play", args: builtin.args || tile.args, command: tile.command || formatCommand(builtin.args || []) };
  }
  if (tile.page) {
    return { key: `page:${tile.page}`, kind: "page", page: tile.page, title: tile.title, note: tile.note, icon: "grid", command: "" };
  }
  const script = findScript(tile.script);
  return { key: `script:${tile.script}:${tile.presetIndex}`, kind: "script", scriptId: tile.script, presetIndex: tile.presetIndex, title: tile.title, note: tile.note, icon: scriptIcon(script), command: tile.command, script };
}

function scriptRunArgs(script, presetIndex) {
  return ["scripts", "run", script.id, "--preset", String(presetIndex + 1)];
}

/* ── 実行 ─────────────────────────────────────────── */

async function runAction(actionId, sourceButton) {
  const builtin = BUILTINS[actionId];
  if (!builtin || state.busy.has(actionId)) { return; }

  state.busy.add(actionId);
  setButtonsLoading(actionId, true);
  const startedAt = Date.now();
  const result = await host.cli(builtin.args);
  state.busy.delete(actionId);
  setButtonsLoading(actionId, false);

  const isOk = result.exitCode === 0;
  const message = isOk ? (result.stdout || "").trim() : cliMessage(result);
  const run = {
    id: `r${startedAt}`, title: builtin.title, command: formatCommand(builtin.args), kind: "action", key: `action:${actionId}`,
    startedAt, endedAt: Date.now(), status: isOk ? "ok" : "bad", exitCode: result.exitCode,
    lines: [`> ${formatCommand(builtin.args)}`, ...((result.stdout || "") + (result.stderr || "")).split(/\r?\n/).filter(Boolean)],
  };
  addRun(run);
  rememberResult(run.key, run);

  if (isOk) { toast("ok", builtin.done, message); }
  else { toast("bad", `「${builtin.title}」に失敗しました`, message, fixesFor(actionId, result)); }
  setTimeout(() => loadStatus({ quiet: true }), 1500);
  void sourceButton;
}

/// 失敗したときに出す「直す」ボタン。
function fixesFor(actionId, result) {
  if (result.exitCode !== 3) { return []; }
  const status = state.status;
  if (actionId === "unity-open" && status && !status.unity.editor && !status.unity.cli) { return [{ label: "Unity Hub で入れる", run: () => runAction("unity-install") }]; }
  if ((actionId === "assets-import" || actionId === "docs-viewer") && status && !status.unity.isOpen) { return [{ label: "Unity を開く", run: () => runAction("unity-open") }]; }
  if (actionId === "assets-open") { return [{ label: "場所を選ぶ", run: () => pickSetting("driveFolder") }]; }
  if (actionId === "cri-open" && status && !status.cri.project) { return [{ label: "初期化する", run: () => runScript("setup", 0) }]; }
  if (actionId === "cri-open") { return [{ label: "exe の場所を選ぶ", run: () => pickSetting("criAtomCraftPath") }]; }
  return [];
}

async function runScript(scriptId, presetIndex) {
  const script = findScript(scriptId);
  if (!script) { toast("bad", "スクリプトが見つかりません", scriptId); return; }
  const preset = script.presets[presetIndex] || script.presets[0];
  presetIndex = script.presets.indexOf(preset);
  if (!script.exists) {
    toast("bad", `「${script.name}」はこの PC にありません`, script.localOnly ? `${script.windows} は git の管理外です。置いたときだけ使えます。` : `${script.windows} が見つかりません。`);
    return;
  }

  if (script.confirm) {
    const accepted = await confirmDialog(script.name, script.confirm, `${preset.label}を動かす`);
    if (!accepted) { return; }
  }

  const mode = preset.mode || script.mode;
  const kind = script.elevate ? "hidden" : (mode === "capture" ? "capture" : "console");
  if (kind === "capture" && currentCapture()) { toast("info", "ほかの実行の出力を受けています", "終わるか「止める」を押してから動かしてください。"); openDrawer(currentCapture().id); return; }

  // 確認は画面で済ませたので --yes を付ける。新しいウィンドウで開くものは、終わってもすぐ閉じないよう --hold を付ける。
  const args = [...scriptRunArgs(script, presetIndex), "--yes", "--mode", mode === "capture" ? "capture" : "console"];
  if (kind !== "capture") { args.push("--hold"); }
  const shownCommand = formatCommand(scriptRunArgs(script, presetIndex));

  const result = await host.request("run", { args, kind });
  if (!result.started) { toast("bad", `「${script.name}」を起動できませんでした`, result.error || ""); return; }

  const run = {
    id: `r${Date.now()}`, runId: result.runId, title: script.name, preset: preset.label, command: shownCommand, kind,
    key: `script:${script.id}:${presetIndex}`, startedAt: Date.now(), status: "running", exitCode: null,
    lines: [`> ${shownCommand}`, ""],
  };
  addRun(run);
  if (kind === "capture") { openDrawer(run.id); }
  else { toast("info", `「${script.name}」を${script.elevate ? "管理者として" : ""}新しいウィンドウで開きました`, `${preset.label}\n終わったら、ここに結果が残ります。`); }
}

function currentCapture() { return state.runs.find((run) => run.kind === "capture" && run.status === "running"); }

function addRun(run) {
  state.runs.unshift(run);
  saveRuns();
  updateRunsBadge();
  if (state.page === "runs") { render(); }
}

function rememberResult(key, run) {
  state.lastResults[key] = { status: run.status, at: run.endedAt || Date.now() };
  storageSet("conductor.lastResults", state.lastResults);
}

host.on((message) => {
  if (message.type === "focus") {
    if (Date.now() - state.statusLoadedAt > 10000) { loadStatus({ quiet: true }); }
    return;
  }

  const run = state.runs.find((item) => item.runId === message.runId && item.status === "running");
  if (!run) { return; }

  if (message.type === "output") {
    run.lines.push(message.line);
    if (state.drawerRunId === run.id) { appendDrawerLine(message.line); }
    return;
  }

  if (message.type === "exited") {
    run.status = message.exitCode === 0 ? "ok" : "bad";
    run.exitCode = message.exitCode;
    run.endedAt = Date.now();
    run.lines.push("", run.status === "ok" ? `✓ 終わりました（${duration(run.endedAt - run.startedAt)}）` : `✗ 終了コード ${message.exitCode}（${duration(run.endedAt - run.startedAt)}）`);
    rememberResult(run.key, run);
    saveRuns();
    updateRunsBadge();
    if (state.drawerRunId === run.id) { renderDrawer(); }
    else if (run.kind !== "capture") { toast(run.status === "ok" ? "ok" : "bad", `「${run.title}」が${run.status === "ok" ? "終わりました" : "失敗しました"}`, run.status === "ok" ? run.preset || "" : `終了コード ${message.exitCode}`); }
    renderPartial();
    setTimeout(() => loadStatus({ quiet: true }), 500);
  }
});

/* ── 設定 ─────────────────────────────────────────── */

async function setSetting(key, value) {
  const result = value ? await host.cli(["config", "set", key, value]) : await host.cli(["config", "unset", key]);
  if (result.exitCode !== 0) { toast("bad", "設定を保存できませんでした", cliMessage(result)); return false; }
  await loadSettings();
  return true;
}

async function setRole(role) {
  if (!(await setSetting("role", role))) { return; }
  await loadHome();
  const label = (state.home.roles.find((item) => item.id === role) || {}).label;
  toast("ok", role ? `役職を「${label}」にしました` : "役職を外しました", role ? "ホームの並びを、よく使う順に変えました。" : "");
  render();
}

async function pickSetting(key) {
  const setting = PATH_SETTINGS.find((item) => item.key === key);
  const result = await host.request("pick", { kind: setting.kind, title: setting.title });
  if (!result.path) { return; }
  if (await setSetting(key, result.path)) {
    toast("ok", `${setting.title}を設定しました`, result.path);
    await loadStatus({ quiet: true });
    render();
  }
}

/* ── 描画 ─────────────────────────────────────────── */

function render() {
  for (const item of document.querySelectorAll(".nav-item")) {
    item.setAttribute("aria-current", item.dataset.page === state.page ? "page" : "false");
  }
  const main = $("#main");
  const scroll = main.scrollTop;
  const pages = { home: renderHome, tools: renderTools, runs: renderRuns, settings: renderSettings };
  main.innerHTML = (pages[state.page] || renderHome)();
  main.scrollTop = scroll;
  renderSidebarFoot();
  hydrateIcons(main);
  bindPage();
}

/// 入力中の欄を壊さないよう、ページを作り直す代わりに一部だけ描き直す。
function renderPartial() {
  if (state.page === "tools") {
    const list = $("#tool-list");
    if (list) { list.innerHTML = renderToolList(); hydrateIcons(list); }
    const detail = $("#tool-detail");
    if (detail) { detail.innerHTML = renderToolDetail(); hydrateIcons(detail); }
    renderSidebarFoot();
    return;
  }
  render();
}

function renderSidebarFoot() {
  const foot = $("#sidebar-foot");
  const status = state.status;
  const role = state.home && state.home.roles.find((item) => item.id === state.home.role);
  const roleIcon = role ? (ROLE_INFO[role.id] || {}).icon || "user" : "user";
  const branch = status && status.git && status.git.branch;
  foot.innerHTML = `
    ${branch ? `<div class="foot-card">
      <div class="foot-row"><span class="icon" data-icon="branch"></span><span class="truncate mono" title="${esc(branch)}">${esc(branch)}</span></div>
      <div class="foot-row">${status.git.changes ? `<span class="pill warn">未コミット ${status.git.changes}</span>` : `<span class="pill ok">変更なし</span>`}
        ${status.git.behind ? `<span class="pill">${icon("arrowDown")}${status.git.behind}</span>` : ""}${status.git.ahead ? `<span class="pill">${icon("arrowUp")}${status.git.ahead}</span>` : ""}</div>
    </div>` : ""}
    <button class="role-chip" data-action="goto" data-page="settings" title="役職を変える">
      <span class="avatar">${icon(roleIcon)}</span>
      <span><small>役職</small><strong>${role ? esc(role.label) : "未設定"}</strong></span>
    </button>`;
  hydrateIcons(foot);
}

function greeting() {
  const hour = new Date().getHours();
  if (hour < 5) { return "遅くまでおつかれさまです"; }
  if (hour < 11) { return "おはようございます"; }
  if (hour < 18) { return "こんにちは"; }
  return "こんばんは";
}

function renderError(title, message) {
  return `<div class="check bad"><span class="check-icon">${icon("alert")}</span><div class="check-text"><strong>${esc(title)}</strong><span>${esc(message)}</span></div>
    <button class="btn small" data-action="reload">${icon("refresh")}読み直す</button></div>`;
}

function renderHome() {
  const role = state.home && state.home.roles.find((item) => item.id === state.home.role);
  const subtitle = role ? `${role.label}のよく使うものを前に並べています。` : "役職を選ぶと、よく使うものが前に並びます。";
  return `<div class="page">
    <header class="page-head">
      <div><h1>${greeting()}</h1><p>${esc(subtitle)}${role ? "" : ` <button class="btn small ghost" data-action="goto" data-page="settings">役職を選ぶ ${icon("chevronRight")}</button>`}</p></div>
      <div style="display:flex;gap:8px;align-items:center">
        <span class="faint" style="font-size:12px">${state.statusLoading ? "状態を読んでいます…" : state.statusLoadedAt ? `状態: ${relativeTime(state.statusLoadedAt)}` : ""}</span>
        <button class="btn small" data-action="refresh-status" ${state.statusLoading ? "disabled" : ""}>${icon("refresh")}更新</button>
      </div>
    </header>
    ${state.homeError ? renderError("ホーム画面を読めません", state.homeError) : ""}
    ${renderStatusStrip()}
    <div class="section-title"><h2>起動する</h2><span class="hint">Unity・CRI・素材・ドキュメント</span></div>
    ${renderLaunchCards()}
    ${renderChecks()}
    ${renderHomeActions()}
  </div>`;
}

function renderStatusStrip() {
  const status = state.status;
  if (!status) {
    return state.statusError ? renderError("状態を読めません", state.statusError) : `<div class="skeleton" style="height:76px"></div>`;
  }
  const git = status.git || {};
  const missing = (status.submodules || []).filter((item) => item.state === "missing");
  const odd = (status.submodules || []).filter((item) => item.state !== "ok" && item.state !== "missing");
  const unity = status.unity || {};
  const branchSub = git.error ? esc(git.error) : git.upstream ? `${esc(git.upstream)} より ${git.behind} 遅れ・${git.ahead} 進み` : "追いかけるブランチなし";
  return `<div class="status-strip">
    <div class="status-cell ${git.error ? "bad" : ""}"><span class="status-icon">${icon("branch")}</span><div class="truncate"><div class="label">ブランチ</div><div class="value truncate mono" title="${esc(git.branch)}">${esc(git.branch || "—")}</div><div class="sub truncate">${branchSub}</div></div></div>
    <div class="status-cell ${git.changes ? "warn" : "ok"}"><span class="status-icon">${icon(git.changes ? "edit" : "check")}</span><div><div class="label">コミットしていない変更</div><div class="value">${git.changes || 0} 件</div><div class="sub">${git.changes ? "コミットか片付けを忘れずに" : "きれいな状態です"}</div></div></div>
    <div class="status-cell ${missing.length ? "bad" : odd.length ? "warn" : "ok"}"><span class="status-icon">${icon(missing.length || odd.length ? "alert" : "check")}</span><div><div class="label">サブモジュール</div><div class="value">${missing.length ? `${missing.length} 件 取っていない` : odd.length ? `${odd.length} 件 記録と違う` : "そろっています"}</div><div class="sub">${(status.submodules || []).length} 件中</div></div></div>
    <div class="status-cell ${unity.editor || unity.cli ? (unity.isOpen ? "ok" : "") : "bad"}"><span class="status-icon">${icon("cube")}</span><div class="truncate"><div class="label">Unity</div><div class="value">${esc(unity.projectVersion || "—")}</div><div class="sub">${unity.isOpen ? "開いています" : unity.editor || unity.cli ? "入っています" : "この版が入っていません"}</div></div></div>
  </div>`;
}

function launchOrder() {
  // 役職の順に並んだボタンから、起動カードの順を決める。
  const order = [];
  for (const tile of homeTiles()) {
    const group = tile.action === "unity-open" ? "unity" : tile.action === "cri-open" ? "cri" : tile.action && tile.action.startsWith("assets") ? "assets" : tile.action === "docs-viewer" ? "docs" : null;
    if (group && !order.includes(group)) { order.push(group); }
  }
  for (const group of ["unity", "cri", "assets", "docs"]) { if (!order.includes(group)) { order.push(group); } }
  return order;
}

function actionButton(actionId, label, primary, iconName) {
  const busy = state.busy.has(actionId);
  return `<button class="btn ${primary ? "primary" : ""} ${busy ? "loading" : ""}" data-action="run-action" data-id="${actionId}" title="${esc(formatCommand(BUILTINS[actionId].args))}">${iconName ? icon(iconName) : ""}${esc(label)}</button>`;
}

function renderLaunchCards() {
  const status = state.status;
  const unity = status ? status.unity : {};
  const cri = status ? status.cri : {};
  const assets = status ? status.assets : {};
  const docs = status ? status.docs : {};
  const links = (state.home && state.home.links) || [];
  const loading = !status;

  const cards = {
    unity: () => {
      const installed = unity.editor || unity.cli;
      const pills = loading ? `<span class="pill">読んでいます…</span>` : [
        `<span class="pill">${esc(unity.projectVersion || "版を読めません")}</span>`,
        unity.isOpen ? `<span class="pill ok live">開いています</span>` : installed ? `<span class="pill ok">${icon("check")}入っています</span>` : `<span class="pill bad">${icon("alert")}この版が入っていません</span>`,
        unity.cli ? `<span class="pill info">Unity CLI</span>` : "",
      ].join("");
      const primary = loading || installed ? actionButton("unity-open", unity.isOpen ? "前に出す" : "Unity を開く", true, "play") : actionButton("unity-install", "Unity Hub で入れる", true, "download");
      return `<article class="launch-card unity">
        <div class="launch-top"><span class="launch-icon">${icon("cube")}</span><div class="truncate"><div class="launch-title">Unity</div><div class="launch-desc">決められた版のエディタで、このプロジェクトを開きます。</div></div></div>
        <div class="launch-state">${pills}</div>
        <div class="launch-actions">${primary}${actionButton("docs-viewer", "仕様書ビューア", false, "book")}${installed || loading ? "" : actionButton("unity-open", "それでも開く", false)}</div>
      </article>`;
    },
    cri: () => {
      const pills = loading ? `<span class="pill">読んでいます…</span>` : [
        cri.project ? `<span class="pill ok">${icon("check")}プロジェクトあり</span>` : `<span class="pill bad">${icon("alert")}サブモジュールを取っていません</span>`,
        cri.app ? `<span class="pill">アプリを見つけました</span>` : `<span class="pill warn">アプリは関連付けで開く</span>`,
      ].join("");
      const primary = loading || cri.project ? actionButton("cri-open", "CRI Atom Craft を開く", true, "play") : `<button class="btn primary" data-action="run-script" data-id="setup" data-preset="0">${icon("sparkle")}初期化して取る</button>`;
      return `<article class="launch-card cri">
        <div class="launch-top"><span class="launch-icon">${icon("wave")}</span><div class="truncate"><div class="launch-title">CRI Atom Craft</div><div class="launch-desc">サウンドのプロジェクト（Sound/CRIAtomCraft）を開きます。</div></div></div>
        <div class="launch-state">${pills}</div>
        <div class="launch-actions">${primary}${cri.app || loading ? "" : `<button class="btn" data-action="pick" data-key="criAtomCraftPath">${icon("edit")}exe を選ぶ</button>`}</div>
      </article>`;
    },
    assets: () => {
      const found = assets.driveFolder;
      const pills = loading ? `<span class="pill">読んでいます…</span>` : found ? `<span class="pill ok">${icon("check")}Drive を見つけました</span>` : `<span class="pill warn">${icon("alert")}Drive が見つかりません</span>`;
      return `<article class="launch-card assets">
        <div class="launch-top"><span class="launch-icon">${icon("folder")}</span><div class="truncate"><div class="launch-title">素材（Google Drive）</div><div class="launch-desc">${found ? `<span class="launch-path" title="${esc(found)}">${esc(found)}</span>` : "Drive の素材を開いたり、Unity へ取り込んだりします。"}</div></div></div>
        <div class="launch-state">${pills}${!loading && unity.isOpen ? `<span class="pill info">取り込みできます</span>` : ""}</div>
        <div class="launch-actions">${found || loading ? actionButton("assets-open", "フォルダを開く", true, "folder") : `<button class="btn primary" data-action="pick" data-key="driveFolder">${icon("folder")}場所を選ぶ</button>`}${actionButton("assets-import", "Unity へ取り込む", false, "import")}</div>
      </article>`;
    },
    docs: () => `<article class="launch-card docs">
        <div class="launch-top"><span class="launch-icon">${icon("book")}</span><div class="truncate"><div class="launch-title">ドキュメント</div><div class="launch-desc">仕様・用語・決まりごとをブラウザで開きます。${docs.hasSpecMirror ? "" : ""}</div></div></div>
        <div class="link-chips">${links.map((link) => `<button class="link-chip" data-action="open-link" data-id="${esc(link.id)}" title="${esc(link.note || link.url)}">${esc(link.title)}${icon("external")}</button>`).join("") || `<span class="faint">home.json の links に登録するとここに出ます。</span>`}</div>
      </article>`,
  };

  return `<div class="launch-grid">${launchOrder().map((group) => cards[group]()).join("")}</div>`;
}

function healthChecks() {
  const status = state.status;
  const checks = [];
  if (!status) { return checks; }
  const missing = (status.submodules || []).filter((item) => item.state === "missing");
  if (missing.length) {
    checks.push({ level: "bad", title: `サブモジュールを ${missing.length} 件取っていません`, text: missing.map((item) => item.path).join("・"), fix: { label: "初期化する", attrs: `data-action="run-script" data-id="setup" data-preset="0"` } });
  }
  if (!status.unity.cli) {
    checks.push({ level: "warn", title: "Unity CLI が入っていません", text: "Unity を開く・素材の取り込みに使います。初期化で入れられます。", fix: { label: "初期化する", attrs: `data-action="run-script" data-id="setup" data-preset="0"` } });
  }
  if (!status.unity.editor && !status.unity.cli) {
    checks.push({ level: "bad", title: `Unity ${status.unity.projectVersion} が入っていません`, text: "Unity Hub で、この版を入れてください。", fix: { label: "Unity Hub で入れる", attrs: `data-action="run-action" data-id="unity-install"` } });
  }
  if (status.git && status.git.behind > 0) {
    checks.push({ level: "info", title: `${status.git.upstream} より ${status.git.behind} コミット遅れています`, text: "最後に fetch した時点の数です。作業の前に pull しておくと安心です。" });
  }
  if (!state.home || !state.home.role) {
    checks.push({ level: "info", title: "役職が決まっていません", text: "選ぶと、ホームの並びがあなたの仕事に合わせて変わります。", fix: { label: "役職を選ぶ", attrs: `data-action="goto" data-page="settings"` } });
  }
  return checks;
}

function renderChecks() {
  if (!state.status) { return ""; }
  const checks = healthChecks();
  const body = checks.length === 0
    ? `<div class="all-good">${icon("check")}このプロジェクトは準備ができています。</div>`
    : `<div class="checks">${checks.map((check) => `<div class="check ${check.level}">
        <span class="check-icon">${icon(check.level === "info" ? "info" : "alert")}</span>
        <div class="check-text"><strong>${esc(check.title)}</strong><span>${esc(check.text)}</span></div>
        ${check.fix ? `<button class="btn small" ${check.fix.attrs}>${esc(check.fix.label)}</button>` : ""}</div>`).join("")}</div>`;
  return `<div class="section-title"><h2>気をつけること</h2><span class="hint">${checks.length ? `${checks.length} 件` : ""}</span></div>${body}`;
}

function renderActionCard(info, featured) {
  const last = state.lastResults[info.key];
  const lastLine = last ? `<div class="action-last ${last.status}">${icon(last.status === "ok" ? "check" : "x")}${last.status === "ok" ? "成功" : "失敗"}・${relativeTime(last.at)}</div>` : "";
  const attrs = info.kind === "action" ? `data-action="run-action" data-id="${info.action}"`
    : info.kind === "page" ? `data-action="goto" data-page="tools"`
    : `data-action="run-script" data-id="${esc(info.scriptId)}" data-preset="${info.presetIndex}"`;
  const missing = info.kind === "script" && info.script && !info.script.exists;
  return `<div class="action-card ${featured ? "featured" : ""}" title="${esc(info.command || "")}">
    <span class="action-icon">${icon(info.icon)}</span>
    <div class="action-body"><div class="action-title">${esc(info.title)}${info.script && info.script.elevate ? `<span class="pill warn" title="管理者で動かします">${icon("shield")}管理者</span>` : ""}${missing ? `<span class="pill">この PC に無い</span>` : ""}</div>
      <div class="action-note">${esc(info.note || "")}</div>${lastLine}</div>
    <button class="btn small icon-only run-btn ${info.kind === "action" && state.busy.has(info.action) ? "loading" : ""}" ${attrs} ${missing ? "disabled" : ""} title="動かす">${icon(info.kind === "page" ? "chevronRight" : "play")}</button>
  </div>`;
}

function renderHomeActions() {
  const roleIds = roleTileIds();
  const tiles = homeTiles().filter((tile) => !LAUNCH_ACTIONS.has(tile.action));
  if (tiles.length === 0) { return ""; }
  const featured = tiles.filter((tile) => roleIds.has(tile.id));
  const others = tiles.filter((tile) => !roleIds.has(tile.id));
  return `<div class="section-title"><h2>${featured.length ? "あなたがよく使う操作" : "操作"}</h2><button class="btn small ghost" data-action="goto" data-page="tools">すべての操作 ${icon("chevronRight")}</button></div>
    <div class="action-grid">${featured.map((tile) => renderActionCard(describeTile(tile), true)).join("")}${others.map((tile) => renderActionCard(describeTile(tile), false)).join("")}</div>`;
}

/* ── すべての操作 ─────────────────────────────────── */

function toolItems() {
  const items = Object.entries(BUILTINS).map(([id, builtin]) => ({ key: `action:${id}`, kind: "action", id, title: builtin.title, note: builtin.note, icon: builtin.icon, category: "起動・ツール", command: formatCommand(builtin.args) }));
  for (const script of state.scripts) {
    items.push({ key: `script:${script.id}`, kind: "script", id: script.id, title: script.name, note: script.description, icon: scriptIcon(script), category: script.category, script,
      command: formatCommand(scriptRunArgs(script, 0)), presets: script.presets.map((preset) => preset.label).join(" ") });
  }
  return items;
}

function toolCategories() {
  const categories = [];
  for (const item of toolItems()) { if (!categories.includes(item.category)) { categories.push(item.category); } }
  return categories;
}

function filteredToolItems() {
  return toolItems().filter((item) => (state.toolsFilter === "all" || item.category === state.toolsFilter)
    && matches(state.toolsQuery, item.title, item.note, item.category, item.command, item.presets || "", item.id));
}

function renderTools() {
  return `<div class="page">
    <header class="page-head"><div><h1>すべての操作</h1><p>conductor でできることと、登録したスクリプトの一覧です。選ぶと、引数の組と同じことをするコマンドが出ます。</p></div></header>
    ${state.scriptsError ? renderError("スクリプトの一覧を読めません", state.scriptsError) : ""}
    <div class="tools-layout">
      <div>
        <label class="search-box">${icon("search")}<input id="tools-search" type="search" placeholder="名前・説明・コマンドで探す（/ で移動）" value="${esc(state.toolsQuery)}" autocomplete="off" spellcheck="false"></label>
        <div class="filters" role="group" aria-label="分類">
          ${["all", ...toolCategories()].map((category) => `<button class="filter" data-action="tools-filter" data-id="${esc(category)}" aria-pressed="${state.toolsFilter === category}">${category === "all" ? "すべて" : esc(category)}</button>`).join("")}
        </div>
        <div id="tool-list">${renderToolList()}</div>
      </div>
      <aside class="detail" id="tool-detail">${renderToolDetail()}</aside>
    </div>
  </div>`;
}

function renderToolList() {
  const items = filteredToolItems();
  if (items.length === 0) { return `<div class="empty">${icon("search")}<strong>見つかりません</strong>ほかの言葉で探すか、分類を「すべて」にしてください。</div>`; }
  const groups = new Map();
  for (const item of items) { if (!groups.has(item.category)) { groups.set(item.category, []); } groups.get(item.category).push(item); }
  return [...groups.entries()].map(([category, groupItems]) => `<div class="tool-group"><h3>${esc(category)}</h3><div class="tool-list">
    ${groupItems.map((item) => {
      const last = state.lastResults[item.kind === "action" ? item.key : `script:${item.id}:${state.selectedPreset[item.id] || 0}`];
      return `<button class="tool-row" data-action="tools-select" data-id="${esc(item.key)}" aria-selected="${state.toolsSelected === item.key}">
        <span class="action-icon">${icon(item.icon)}</span>
        <span class="action-body"><span class="action-title">${highlight(item.title, state.toolsQuery)}</span><span class="action-note">${highlight(item.note || item.command, state.toolsQuery)}</span></span>
        <span class="badges">${item.script && item.script.elevate ? `<span class="pill warn">${icon("shield")}管理者</span>` : ""}${item.script && !item.script.exists ? `<span class="pill">この PC に無い</span>` : ""}${last ? `<span class="pill ${last.status}">${icon(last.status === "ok" ? "check" : "x")}</span>` : ""}</span>
      </button>`;
    }).join("")}</div></div>`).join("");
}

function renderToolDetail() {
  const item = toolItems().find((candidate) => candidate.key === state.toolsSelected);
  if (!item) { return `<div class="detail-empty">${icon("grid")}左の一覧から操作を選んでください。<br>Enter で動かせます。</div>`; }

  if (item.kind === "action") {
    const builtin = BUILTINS[item.id];
    return `<div class="launch-top"><span class="launch-icon" style="background:var(--accent-soft);color:var(--accent-text)">${icon(item.icon)}</span><div><h2>${esc(item.title)}</h2><div class="desc">${esc(item.note)}</div></div></div>
      <div><div class="field-label">同じことをするコマンド</div>${commandBox(formatCommand(builtin.args))}</div>
      ${actionButton(item.id, "動かす", true, "play")}`;
  }

  const script = item.script;
  const presetIndex = Math.min(state.selectedPreset[script.id] || 0, script.presets.length - 1);
  const preset = script.presets[presetIndex];
  const mode = preset.mode || script.mode;
  const modeText = script.elevate ? "管理者として新しいウィンドウで開く" : mode === "capture" ? "この画面に出力を出す" : "新しいウィンドウで開く";
  return `<div class="launch-top"><span class="launch-icon" style="background:var(--accent-soft);color:var(--accent-text)">${icon(item.icon)}</span><div class="truncate"><h2>${esc(script.name)}</h2><span class="pill">${esc(script.category)}</span></div></div>
    ${script.description ? `<div class="desc">${esc(script.description)}</div>` : ""}
    <div><div class="field-label">引数の組</div><div class="presets" role="radiogroup">
      ${script.presets.map((candidate, index) => `<button class="preset" role="radio" aria-checked="${index === presetIndex}" data-action="tools-preset" data-id="${esc(script.id)}" data-preset="${index}">
        <span class="radio"></span><span class="preset-label">${esc(candidate.label)}</span><span class="preset-mode">${(candidate.mode || script.mode) === "capture" ? "この画面に出す" : "新しいウィンドウ"}</span></button>`).join("")}
    </div></div>
    <div><div class="field-label">同じことをするコマンド</div>${commandBox(formatCommand(scriptRunArgs(script, presetIndex)))}</div>
    <dl class="meta-list">
      <dt>動かし方</dt><dd>${esc(modeText)}</dd>
      <dt>ファイル</dt><dd class="mono">${esc(script.windows || script.mac || "—")}</dd>
      ${preset.args && preset.args.length ? `<dt>渡す引数</dt><dd class="mono">${esc(preset.args.join(" "))}</dd>` : ""}
      ${script.confirm ? `<dt>確認</dt><dd>動かす前に確認します</dd>` : ""}
    </dl>
    ${script.exists ? `<button class="btn primary" data-action="run-script" data-id="${esc(script.id)}" data-preset="${presetIndex}">${icon(script.elevate ? "shield" : "play")}${esc(preset.label)}を動かす</button>`
      : `<div class="check warn"><span class="check-icon">${icon("alert")}</span><div class="check-text"><strong>この PC にファイルがありません</strong><span>${script.localOnly ? "git の管理外のスクリプトです。置いたときだけ使えます。" : esc(script.windows)}</span></div></div>`}`;
}

function commandBox(command) {
  return `<div class="command"><code title="${esc(command)}">${esc(command)}</code><button class="btn small icon-only ghost" data-action="copy" data-text="${esc(command)}" title="コピー">${icon("copy")}</button></div>`;
}

/* ── 実行の記録 ─────────────────────────────────── */

function renderRuns() {
  const runs = state.runs;
  return `<div class="page">
    <header class="page-head"><div><h1>実行の記録</h1><p>この PC で動かしたものです。選ぶと出力を見られます（新しいウィンドウで開いたものは、終わったかどうかだけ残ります）。</p></div>
      ${runs.length ? `<button class="btn small ghost" data-action="clear-runs">${icon("x")}記録を消す</button>` : ""}</header>
    ${runs.length === 0 ? `<div class="empty">${icon("history")}<strong>まだ何も動かしていません</strong>ホームか「すべての操作」から動かすと、ここに残ります。</div>`
      : `<div class="run-list">${runs.map((run) => `<button class="run-row" data-action="open-run" data-id="${esc(run.id)}">
        <span class="run-state ${run.status === "running" ? "running" : run.status}">${run.status === "running" ? `<span class="pill live accent" style="padding:0;background:none"></span>` : icon(run.status === "ok" ? "check" : "x")}</span>
        <span class="truncate"><span class="run-title">${esc(run.title)}${run.preset ? `<span class="faint">・${esc(run.preset)}</span>` : ""}</span><br><span class="run-cmd">${esc(run.command)}</span></span>
        <span>${run.kind === "capture" ? `<span class="pill">この画面</span>` : run.kind === "action" ? `<span class="pill">すぐ終わる</span>` : `<span class="pill">${run.kind === "hidden" ? "管理者" : "ウィンドウ"}</span>`}</span>
        <span class="run-time">${relativeTime(run.startedAt)}<br>${run.endedAt ? duration(run.endedAt - run.startedAt) : "動いています"}</span>
      </button>`).join("")}</div>`}
  </div>`;
}

function updateRunsBadge() {
  const badge = $("#runs-badge");
  const running = state.runs.filter((run) => run.status === "running").length;
  badge.hidden = running === 0;
  badge.textContent = running;
}

/* ── 設定 ─────────────────────────────────────────── */

function renderSettings() {
  const roles = (state.home && state.home.roles) || [];
  const current = state.home && state.home.role;
  const themeButton = (value, label, iconName) => `<button role="radio" aria-checked="${state.theme === value}" data-action="theme" data-id="${value}">${icon(iconName)}${label}</button>`;
  return `<div class="page">
    <header class="page-head"><div><h1>設定</h1><p>この PC だけの設定です（git には入りません）。CLI の <code>conductor config</code> と同じものです。</p></div></header>

    <section class="settings-section">
      <h2>役職</h2><p>ホームで、この役職がよく使う操作を前に並べます。隠すものはありません。</p>
      <div class="role-grid" role="radiogroup">
        ${roles.map((role) => {
          const info = ROLE_INFO[role.id] || { icon: "user", note: "" };
          return `<button class="role-card" role="radio" aria-checked="${current === role.id}" data-action="set-role" data-id="${esc(role.id)}">
            <span class="check-mark">${icon("check")}</span><span class="avatar">${icon(info.icon)}</span><strong>${esc(role.label)}</strong><span>${esc(info.note)}</span></button>`;
        }).join("")}
      </div>
      ${current ? `<div style="margin-top:12px"><button class="btn small ghost" data-action="set-role" data-id="">役職を外す</button></div>` : ""}
    </section>

    <section class="settings-section">
      <h2>アプリの場所</h2><p>ふつうは自動で見つかります。見つからないときや、別の場所を使うときだけ選んでください。</p>
      ${PATH_SETTINGS.map((setting) => {
        const value = state.settings[setting.key];
        const detected = setting.detected();
        return `<div class="path-setting">
          <div class="title">${esc(setting.title)}</div>
          <div class="buttons"><button class="btn small" data-action="pick" data-key="${setting.key}">${icon(setting.kind === "folder" ? "folder" : "edit")}選ぶ</button>${value ? `<button class="btn small ghost" data-action="unset" data-key="${setting.key}">自動に戻す</button>` : ""}</div>
          <div class="current">${value ? `<span class="pill accent">指定</span><code title="${esc(value)}">${esc(value)}</code>` : detected ? `<span class="pill ok">自動</span><code title="${esc(detected)}">${esc(detected)}</code>` : `<span class="pill warn">見つかりません</span>`}</div>
          <div class="note">${esc(setting.note)}</div>
        </div>`;
      }).join("")}
    </section>

    <section class="settings-section">
      <h2>見た目</h2><p>Windows の設定に合わせるか、明るい・暗いを選べます。</p>
      <div class="segmented" role="radiogroup">${themeButton("system", "Windows に合わせる", "monitor")}${themeButton("light", "明るい", "sun")}${themeButton("dark", "暗い", "moon")}</div>
    </section>

    <section class="settings-section">
      <h2>このアプリについて</h2><p>画面は CLI（conductor.exe）を呼ぶだけです。同じことはコマンドでもできます。</p>
      <dl class="meta-list">
        <dt>CLI</dt><dd>${esc(state.version || "—")}</dd>
        <dt>リポジトリ</dt><dd class="mono">${esc(state.hostInfo && state.hostInfo.root)}</dd>
        <dt>CLI の場所</dt><dd class="mono">${esc(state.hostInfo && state.hostInfo.cli)}</dd>
      </dl>
      <div style="margin-top:12px">${commandBox("conductor --help")}</div>
    </section>
  </div>`;
}

/* ── 出力の引き出し ─────────────────────────────── */

function lineClass(line) {
  if (/^> /.test(line)) { return "cmd"; }
  if (/^\s*(\[OK\]|✓)/.test(line)) { return "ok"; }
  if (/^\s*(\[!!\]|\[NG\]|✗|error|エラー|失敗)/i.test(line)) { return "err"; }
  if (/^\s*(\[\?\?\]|warning|警告)/i.test(line)) { return "warn"; }
  return "";
}

function openDrawer(runId) {
  state.drawerRunId = runId;
  state.drawerCollapsed = false;
  renderDrawer();
}

function renderDrawer() {
  const drawer = $("#drawer");
  const run = state.runs.find((item) => item.id === state.drawerRunId);
  if (!run) { drawer.hidden = true; $("#main").classList.remove("with-drawer"); return; }
  drawer.hidden = false;
  drawer.classList.toggle("collapsed", state.drawerCollapsed);
  $("#main").classList.toggle("with-drawer", !state.drawerCollapsed);
  const statusPill = run.status === "running" ? `<span class="pill accent live">動いています</span>` : run.status === "ok" ? `<span class="pill ok">${icon("check")}成功</span>` : `<span class="pill bad">${icon("x")}終了コード ${run.exitCode}</span>`;
  drawer.innerHTML = `<div class="drawer-head">
      <span class="title">${icon("terminal")}<span class="truncate">${esc(run.title)}${run.preset ? `<span class="faint">・${esc(run.preset)}</span>` : ""}</span>${statusPill}</span>
      ${run.status === "running" && run.kind === "capture" ? `<button class="btn small danger" data-action="stop">${icon("stop")}止める</button>` : ""}
      <button class="btn small icon-only ghost" data-action="copy-output" title="出力をコピー">${icon("copy")}</button>
      <button class="btn small icon-only ghost" data-action="toggle-drawer" title="${state.drawerCollapsed ? "広げる" : "たたむ"}">${icon(state.drawerCollapsed ? "arrowUp" : "chevronDown")}</button>
      <button class="btn small icon-only ghost" data-action="close-drawer" title="閉じる">${icon("x")}</button>
    </div>
    <div class="drawer-body" id="drawer-body">${run.lines.map((line) => `<div class="line ${lineClass(line)}">${esc(line) || " "}</div>`).join("")}</div>`;
  const body = $("#drawer-body");
  body.scrollTop = body.scrollHeight;
}

function appendDrawerLine(line) {
  const body = $("#drawer-body");
  if (!body) { return; }
  const isAtBottom = body.scrollHeight - body.scrollTop - body.clientHeight < 40;
  const element = document.createElement("div");
  element.className = `line ${lineClass(line)}`;
  element.textContent = line || " ";
  body.appendChild(element);
  if (isAtBottom) { body.scrollTop = body.scrollHeight; }
}

/* ── コマンドパレット ─────────────────────────────── */

const palette = { open: false, query: "", index: 0, items: [] };

function paletteEntries() {
  const entries = [];
  const pages = [["home", "ホーム", "home"], ["tools", "すべての操作", "grid"], ["runs", "実行の記録", "history"], ["settings", "設定", "settings"]];
  for (const tile of homeTiles()) {
    const info = describeTile(tile);
    if (info.kind === "page") { continue; }
    entries.push({ group: "よく使う操作", title: info.title, note: info.command, icon: info.icon, kind: "動かす", run: () => (info.kind === "action" ? runAction(info.action) : runScript(info.scriptId, info.presetIndex)), search: `${info.title} ${info.note} ${info.command}` });
  }
  for (const [id, builtin] of Object.entries(BUILTINS)) {
    if (entries.some((entry) => entry.title === builtin.title)) { continue; }
    entries.push({ group: "起動・ツール", title: builtin.title, note: formatCommand(builtin.args), icon: builtin.icon, kind: "動かす", run: () => runAction(id), search: `${builtin.title} ${builtin.note} ${id}` });
  }
  for (const script of state.scripts) {
    script.presets.forEach((preset, index) => {
      const command = formatCommand(scriptRunArgs(script, index));
      entries.push({ group: "スクリプト", title: `${script.name}`, sub: preset.label, note: command, icon: scriptIcon(script), kind: script.exists ? "動かす" : "この PC に無い", run: () => runScript(script.id, index), search: `${script.name} ${preset.label} ${script.category} ${script.description} ${command}` });
    });
  }
  for (const link of (state.home && state.home.links) || []) {
    entries.push({ group: "ドキュメント", title: link.title, note: link.note || link.url, icon: "book", kind: "開く", run: () => openLink(link.id), search: `${link.title} ${link.note} ${link.id} ドキュメント` });
  }
  for (const [page, title, iconName] of pages) {
    entries.push({ group: "ページ", title: `${title}へ`, note: "", icon: iconName, kind: "移る", run: () => gotoPage(page), search: `${title} ページ` });
  }
  for (const role of (state.home && state.home.roles) || []) {
    entries.push({ group: "設定", title: `役職を「${role.label}」にする`, note: (ROLE_INFO[role.id] || {}).note || "", icon: (ROLE_INFO[role.id] || {}).icon || "user", kind: "設定", run: () => setRole(role.id), search: `役職 ${role.label} ${role.id}` });
  }
  entries.push({ group: "設定", title: "状態を読み直す", note: "conductor status", icon: "refresh", kind: "動かす", run: () => loadStatus(), search: "状態 更新 読み直す status" });
  entries.push({ group: "設定", title: "明るい・暗いを切り替える", note: "", icon: "moon", kind: "設定", run: () => setTheme(document.documentElement.dataset.theme === "dark" ? "light" : "dark"), search: "テーマ ダーク ライト 見た目" });
  return entries;
}

function openPalette() {
  palette.open = true;
  palette.query = "";
  palette.index = 0;
  const backdrop = $("#palette");
  backdrop.hidden = false;
  backdrop.innerHTML = `<div class="palette" role="dialog" aria-label="探す・動かす">
    <div class="palette-input">${icon("search")}<input id="palette-input" placeholder="やりたいことを入れる（例: unity、初期化、仕様）" autocomplete="off" spellcheck="false"><kbd>Esc</kbd></div>
    <div class="palette-list" id="palette-list" role="listbox"></div>
    <div class="palette-foot"><span><kbd>↑</kbd><kbd>↓</kbd>選ぶ</span><span><kbd>Enter</kbd>動かす</span><span><kbd>Esc</kbd>閉じる</span></div>
  </div>`;
  const input = $("#palette-input");
  input.addEventListener("input", () => { palette.query = input.value; palette.index = 0; renderPaletteList(); });
  input.addEventListener("keydown", paletteKeydown);
  renderPaletteList();
  input.focus();
}

function closePalette() {
  palette.open = false;
  $("#palette").hidden = true;
  $("#palette").innerHTML = "";
}

function renderPaletteList() {
  const all = paletteEntries();
  palette.items = palette.query ? all.filter((entry) => matches(palette.query, entry.search, entry.sub || "")) : all.filter((entry) => entry.group !== "スクリプト" && entry.group !== "設定");
  palette.items = palette.items.slice(0, 40);
  const list = $("#palette-list");
  if (palette.items.length === 0) { list.innerHTML = `<div class="detail-empty">${icon("search")}見つかりません</div>`; return; }
  let group = "";
  list.innerHTML = palette.items.map((entry, index) => {
    const header = entry.group !== group ? `<div class="palette-group">${esc(entry.group)}</div>` : "";
    group = entry.group;
    return `${header}<button class="palette-item" role="option" data-index="${index}" aria-selected="${index === palette.index}">
      <span class="action-icon">${icon(entry.icon)}</span>
      <span class="palette-text"><strong>${highlight(entry.title, palette.query)}${entry.sub ? ` <span class="faint" style="display:inline">› ${highlight(entry.sub, palette.query)}</span>` : ""}</strong>${entry.note ? `<span>${esc(entry.note)}</span>` : ""}</span>
      <span class="palette-kind">${esc(entry.kind)}</span></button>`;
  }).join("");
  const selected = list.querySelector('[aria-selected="true"]');
  if (selected) { selected.scrollIntoView({ block: "nearest" }); }
}

function paletteKeydown(event) {
  if (event.key === "ArrowDown" || event.key === "ArrowUp") {
    event.preventDefault();
    const step = event.key === "ArrowDown" ? 1 : -1;
    palette.index = (palette.index + step + palette.items.length) % Math.max(1, palette.items.length);
    renderPaletteList();
  } else if (event.key === "Enter") {
    event.preventDefault();
    choosePalette(palette.index);
  } else if (event.key === "Escape") {
    event.preventDefault();
    closePalette();
  }
}

function choosePalette(index) {
  const entry = palette.items[index];
  closePalette();
  if (entry) { entry.run(); }
}

/* ── 確認の窓・通知 ───────────────────────────────── */

function confirmDialog(title, message, okLabel) {
  return new Promise((resolve) => {
    const backdrop = $("#modal");
    backdrop.hidden = false;
    backdrop.innerHTML = `<div class="modal" role="alertdialog" aria-modal="true" aria-labelledby="modal-title">
      <div class="modal-head"><span class="modal-icon">${icon("alert")}</span><div><h2 id="modal-title">${esc(title)}</h2><p>${esc(message)}</p></div></div>
      <div class="modal-actions"><button class="btn" data-answer="no">やめる</button><button class="btn primary" data-answer="yes">${esc(okLabel || "続ける")}</button></div>
    </div>`;
    const finish = (answer) => { backdrop.hidden = true; backdrop.innerHTML = ""; document.removeEventListener("keydown", onKey, true); resolve(answer); };
    const onKey = (event) => { if (event.key === "Escape") { event.stopPropagation(); finish(false); } };
    document.addEventListener("keydown", onKey, true);
    backdrop.addEventListener("click", (event) => {
      const answer = event.target.closest("[data-answer]");
      if (answer) { finish(answer.dataset.answer === "yes"); } else if (event.target === backdrop) { finish(false); }
    });
    backdrop.querySelector('[data-answer="no"]').focus();
  });
}

function toast(kind, title, message, actions) {
  const container = $("#toasts");
  const element = document.createElement("div");
  element.className = `toast ${kind}`;
  element.setAttribute("role", kind === "bad" ? "alert" : "status");
  element.innerHTML = `<span class="toast-icon">${icon(kind === "ok" ? "check" : kind === "bad" ? "x" : "info")}</span>
    <div class="toast-body"><strong>${esc(title)}</strong>${message ? `<span>${esc(message)}</span>` : ""}
      ${actions && actions.length ? `<div class="toast-actions">${actions.map((action, index) => `<button class="btn small ${index === 0 ? "primary" : ""}" data-toast-action="${index}">${esc(action.label)}</button>`).join("")}</div>` : ""}</div>
    <button class="btn small icon-only ghost" data-toast-close title="閉じる">${icon("x")}</button>`;
  const remove = () => { element.classList.add("leaving"); setTimeout(() => element.remove(), 200); };
  element.addEventListener("click", (event) => {
    const button = event.target.closest("[data-toast-action]");
    if (button) { remove(); actions[Number(button.dataset.toastAction)].run(); }
    else if (event.target.closest("[data-toast-close]")) { remove(); }
  });
  container.prepend(element);
  while (container.children.length > 4) { container.lastElementChild.remove(); }
  setTimeout(remove, kind === "bad" ? 9000 : 4500);
}

/* ── 画面の操作 ───────────────────────────────────── */

function gotoPage(page) {
  if (state.page === page) { return; }
  state.page = page;
  $("#main").scrollTop = 0;
  render();
  if (page === "tools") { const input = $("#tools-search"); if (input) { input.focus(); } }
}

function setTheme(theme) {
  state.theme = theme;
  storageSet("conductor.theme", theme);
  applyTheme();
  if (state.page === "settings") { render(); }
}

async function openLink(id) {
  const link = ((state.home && state.home.links) || []).find((item) => item.id === id);
  const result = await host.cli(["docs", "open", id]);
  if (result.exitCode === 0) { toast("ok", `「${link ? link.title : id}」を開きました`, "ブラウザを見てください。"); }
  else { toast("bad", "開けませんでした", cliMessage(result)); }
}

async function copyText(text) {
  const result = await host.request("copy", { text });
  if (result.copied) { toast("ok", "コピーしました", text.length > 120 ? `${text.slice(0, 120)}…` : text); return; }
  try { await navigator.clipboard.writeText(text); toast("ok", "コピーしました", text); } catch (error) { toast("bad", "コピーできませんでした", ""); }
}

function setButtonsLoading(actionId, loading) {
  for (const button of document.querySelectorAll(`[data-action="run-action"][data-id="${actionId}"]`)) {
    button.classList.toggle("loading", loading);
  }
}

function bindPage() {
  const search = $("#tools-search");
  if (search) {
    search.addEventListener("input", () => { state.toolsQuery = search.value; renderPartial(); });
    search.addEventListener("keydown", (event) => {
      const items = filteredToolItems();
      if (items.length === 0) { return; }
      const index = items.findIndex((item) => item.key === state.toolsSelected);
      if (event.key === "ArrowDown" || event.key === "ArrowUp") {
        event.preventDefault();
        const next = event.key === "ArrowDown" ? Math.min(items.length - 1, index + 1) : Math.max(0, index - 1);
        selectTool(items[next].key, true);
      } else if (event.key === "Enter") {
        event.preventDefault();
        const item = items[Math.max(0, index)];
        selectTool(item.key);
        runToolItem(item);
      }
    });
  }
}

function selectTool(key, scrollIntoView) {
  state.toolsSelected = key;
  storageSet("conductor.toolsSelected", key);
  renderPartial();
  const row = document.querySelector(`.tool-row[aria-selected="true"]`);
  if (row && scrollIntoView) { row.scrollIntoView({ block: "nearest" }); }
}

function runToolItem(item) {
  if (item.kind === "action") { runAction(item.id); }
  else { runScript(item.id, state.selectedPreset[item.id] || 0); }
}

document.addEventListener("click", (event) => {
  const nav = event.target.closest(".nav-item");
  if (nav) { gotoPage(nav.dataset.page); return; }

  const paletteItem = event.target.closest(".palette-item");
  if (paletteItem) { choosePalette(Number(paletteItem.dataset.index)); return; }
  if (event.target.id === "palette") { closePalette(); return; }

  const target = event.target.closest("[data-action]");
  if (!target || target.disabled) { return; }
  const { action, id, page, key } = target.dataset;
  switch (action) {
    case "palette": openPalette(); break;
    case "goto": gotoPage(page); break;
    case "reload": loadAll(); break;
    case "refresh-status": loadStatus(); break;
    case "run-action": runAction(id, target); break;
    case "run-script": runScript(id, Number(target.dataset.preset || 0)); break;
    case "open-link": openLink(id); break;
    case "copy": copyText(target.dataset.text); break;
    case "tools-filter": state.toolsFilter = id; render(); break;
    case "tools-select": selectTool(id); break;
    case "tools-preset": state.selectedPreset[id] = Number(target.dataset.preset); renderPartial(); break;
    case "set-role": setRole(id); break;
    case "pick": pickSetting(key); break;
    case "unset": setSetting(key, "").then(() => loadStatus({ quiet: true })).then(() => { toast("ok", "自動で探すように戻しました", ""); render(); }); break;
    case "theme": setTheme(id); break;
    case "open-run": openDrawer(id); break;
    case "clear-runs": state.runs = state.runs.filter((run) => run.status === "running"); saveRuns(); render(); break;
    case "stop": host.request("stop", {}); break;
    case "toggle-drawer": state.drawerCollapsed = !state.drawerCollapsed; renderDrawer(); break;
    case "close-drawer": state.drawerRunId = null; renderDrawer(); break;
    case "copy-output": {
      const run = state.runs.find((item) => item.id === state.drawerRunId);
      if (run) { copyText(run.lines.join("\n")); }
      break;
    }
    default: break;
  }
});

document.addEventListener("dblclick", (event) => {
  const row = event.target.closest(".tool-row");
  if (!row) { return; }
  const item = toolItems().find((candidate) => candidate.key === row.dataset.id);
  if (item) { runToolItem(item); }
});

document.addEventListener("keydown", (event) => {
  const isTyping = /^(INPUT|TEXTAREA)$/.test(document.activeElement && document.activeElement.tagName);
  if ((event.ctrlKey || event.metaKey) && (event.key === "k" || event.key === "K" || event.key === "p" || event.key === "P")) {
    event.preventDefault();
    if (palette.open) { closePalette(); } else { openPalette(); }
    return;
  }
  if (palette.open || !$("#modal").hidden) { return; }
  if (event.ctrlKey && /^[1-4]$/.test(event.key)) {
    event.preventDefault();
    gotoPage(["home", "tools", "runs", "settings"][Number(event.key) - 1]);
    return;
  }
  if (event.key === "F5") { event.preventDefault(); loadAll(); return; }
  if (event.key === "Escape" && state.drawerRunId) { state.drawerRunId = null; renderDrawer(); return; }
  if (event.key === "/" && !isTyping) {
    event.preventDefault();
    if (state.page !== "tools") { gotoPage("tools"); }
    const input = $("#tools-search");
    if (input) { input.focus(); input.select(); }
  }
});

/* ── 始まり ───────────────────────────────────────── */

(async function start() {
  applyTheme();
  hydrateIcons(document);
  render();
  updateRunsBadge();
  state.hostInfo = await host.request("hello", {});
  if (state.hostInfo && state.hostInfo.hasCli === false) {
    toast("bad", "conductor.exe が見つかりません", "KillChordConductor.exe と同じフォルダ（リポジトリのルート）に置いてください。");
  }
  if (host.isDevMock) {
    const banner = document.createElement("div");
    banner.className = "dev-banner";
    banner.textContent = "ブラウザで見ています（仮の答え。conductor は呼んでいません）";
    document.body.appendChild(banner);
  }
  await loadAll();
  // 15 秒ごとに、前に出ているときだけ状態を読み直す。
  setInterval(() => { if (document.hasFocus() && Date.now() - state.statusLoadedAt > 15000) { loadStatus({ quiet: true }); } }, 5000);
})();
