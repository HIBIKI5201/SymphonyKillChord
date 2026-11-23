// ブラウザで画面だけを見るときの、仮の橋渡し（KillChordConductor.exe の外で開いたときだけ使う）。
// conductor.exe は呼ばず、それらしい答えを返す。URL に ?state=fresh を付けると、初期化前の PC のつもりで答える。
"use strict";

(function installDevMock() {
  if (window.chrome && window.chrome.webview) { return; }

  const params = new URLSearchParams(location.search);
  const fresh = params.get("state") === "fresh";
  const listeners = [];
  const settings = fresh ? {} : { role: "sound", driveFolder: "G:\\マイドライブ\\SymphonyKillChord" };
  let nextRunId = 1;
  let capture = null;

  const scripts = [
    { id: "setup", name: "プロジェクトの初期化", category: "初期化", windows: "Builds/Setup.bat", mac: "Builds/Setup.sh",
      description: "クローンしたあとに1回動かす。サブモジュール・git の設定・Unity CLI を確かめて、足りないものを入れる。",
      confirm: "", mode: "console", elevate: false, localOnly: false, exists: true,
      presets: [{ label: "初期化する", args: [], mode: "console" }, { label: "確認だけ（何も変えない）", args: ["-Check"], mode: "capture" }] },
    { id: "clean-branches", name: "マージ済みのブランチを消す", category: "Git", windows: "Builds/CleanBranches.ps1", mac: "",
      description: "develop に取り込み済みのローカルブランチを消す。", confirm: "消したブランチは元に戻せません。続けますか？",
      mode: "console", elevate: false, localOnly: false, exists: true,
      presets: [{ label: "消す（確認あり）", args: [], mode: "console" }, { label: "候補を見るだけ", args: ["-WhatIf"], mode: "capture" }] },
    { id: "github-collaborators", name: "GitHub のコラボレーターを管理する", category: "Git", windows: "Builds/GitHub/Collaborators.ps1", mac: "",
      description: "KillChord 系列のリポジトリのコラボレーターを一覧・招待する。", confirm: "", mode: "console", elevate: false, localOnly: false, exists: true,
      presets: [{ label: "一覧を表で見る", args: ["-List"], mode: "capture" }, { label: "アカウントを KillChord 系列へ招待する（確認あり）", args: ["-Invite"], mode: "console" }] },
    { id: "install-app", name: "Android 端末へアプリを入れる", category: "Android", windows: "Builds/Install_App.ps1", mac: "",
      description: "ビルドしたアプリを USB でつないだ端末へ入れる。", confirm: "端末のアプリと署名が合わないときは、アプリを自動でアンインストールします。続けますか？",
      mode: "console", elevate: false, localOnly: false, exists: true, presets: [{ label: "引数なし", args: [], mode: "console" }] },
    { id: "runner-menu", name: "ビルドランナーの管理", category: "ビルドランナー", windows: "Builds/Runner/Menu.ps1", mac: "",
      description: "セルフホストランナーの状態・ログを見る。", confirm: "", mode: "console", elevate: true, localOnly: true, exists: !fresh,
      presets: [{ label: "メニューを開く", args: [], mode: "console" }, { label: "状態を見る", args: ["-Status"], mode: "capture" }] },
    { id: "start-runner", name: "ビルドランナーを起動する", category: "ビルドランナー", windows: "Builds/Runner/Start.ps1", mac: "",
      description: "", confirm: "", mode: "console", elevate: true, localOnly: true, exists: false, presets: [{ label: "引数なし", args: [], mode: "console" }] },
    { id: "homepage-dev", name: "ホームページをプレビューする", category: "ホームページ", windows: "HomePage/dev.ps1", mac: "HomePage/dev.sh",
      description: "ホームページをローカルで動かしてブラウザで確かめる。", confirm: "", mode: "console", elevate: false, localOnly: false, exists: true,
      presets: [{ label: "引数なし", args: [], mode: "console" }] },
  ];

  const roles = [
    { id: "programmer", label: "プログラマー" }, { id: "planner", label: "プランナー" }, { id: "designer", label: "デザイナー" },
    { id: "sound", label: "サウンド" }, { id: "lead", label: "リード" },
  ];

  const tiles = [
    { id: "unity-open", title: "Unity を開く", note: "このプロジェクトを、決められた版の Unity で開く", roles: ["programmer", "planner", "designer", "sound", "lead"], action: "unity-open", args: ["unity", "open"], command: "conductor unity open" },
    { id: "cri-open", title: "CRI Atom Craft を開く", note: "サウンドのプロジェクト（Sound/CRIAtomCraft）を開く", roles: ["sound"], action: "cri-open", args: ["cri", "open"], command: "conductor cri open" },
    { id: "assets-open", title: "Drive の素材フォルダを開く", note: "Google Drive の SymphonyKillChord フォルダを開く", roles: ["designer", "sound", "planner"], action: "assets-open", args: ["assets", "open"], command: "conductor assets open" },
    { id: "assets-import", title: "Drive の素材を Unity へ取り込む", note: "開いている Unity で、Drive の素材を同期する", roles: ["designer", "sound"], action: "assets-import", args: ["assets", "import"], command: "conductor assets import" },
    { id: "docs-viewer", title: "仕様書ビューアを開く", note: "開いている Unity で、リポジトリの仕様書を読む", roles: ["planner", "programmer"], action: "docs-viewer", args: ["docs", "viewer"], command: "conductor docs viewer" },
    { id: "repo-open", title: "リポジトリのフォルダを開く", note: "エクスプローラーでプロジェクトのフォルダを開く", roles: [], action: "repo-open", args: ["repo", "open"], command: "conductor repo open" },
    { id: "setup-check", title: "初期化できているか確かめる", note: "サブモジュール・git の設定・Unity を確かめるだけ。何も変えない", roles: ["programmer", "planner", "designer", "sound", "lead"], script: "setup", presetIndex: 1, command: "conductor scripts run setup --preset 2" },
    { id: "setup", title: "プロジェクトを初期化する", note: "クローンしたあとに1回。足りないものを確認してから入れる", roles: [], script: "setup", presetIndex: 0, command: "conductor scripts run setup --preset 1" },
    { id: "install-app", title: "Android 端末へアプリを入れる", note: "ビルドしたアプリを USB でつないだ端末へ入れて試す", roles: ["planner", "designer", "sound", "lead"], script: "install-app", presetIndex: 0, command: "conductor scripts run install-app --preset 1" },
    { id: "clean-branches", title: "マージ済みのブランチを消す", note: "develop に取り込み済みのローカルブランチを片付ける", roles: ["programmer", "lead"], script: "clean-branches", presetIndex: 0, command: "conductor scripts run clean-branches --preset 1" },
    { id: "runner-menu", title: "ビルドランナーの管理", note: "セルフホストランナーの状態・ログを見る", roles: ["lead"], script: "runner-menu", presetIndex: 0, command: "conductor scripts run runner-menu --preset 1" },
    { id: "github-collaborators", title: "GitHub のコラボレーターを見る", note: "KillChord 系列のリポジトリに誰が入っているかを表で出す", roles: ["lead"], script: "github-collaborators", presetIndex: 0, command: "conductor scripts run github-collaborators --preset 1" },
    { id: "homepage-dev", title: "ホームページをプレビューする", note: "ホームページをローカルで動かしてブラウザで確かめる", roles: ["lead"], script: "homepage-dev", presetIndex: 0, command: "conductor scripts run homepage-dev --preset 1" },
  ];

  const links = [
    { id: "specs", title: "仕様リスト", note: "機能ごとの仕様（Notion のプロジェクト ドキュメント）", url: "https://www.notion.so/27d7c2c6cc028131aee3cf5d08b0b03a" },
    { id: "systems", title: "システムリスト", note: "インゲーム・アウトゲーム・共通基盤などのシステム", url: "https://www.notion.so/27d7c2c6cc0281ee963ff0e4195b52e0" },
    { id: "glossary", title: "用語", note: "プロジェクトで使う言葉の決まり", url: "https://www.notion.so/27d7c2c6cc02813ba93fe6baf4d4f679" },
    { id: "qa", title: "QA", note: "QA の進め方とシート", url: "https://www.notion.so/3e37c2c6cc0281988919e7d9fba1f7b8" },
    { id: "code-guidelines", title: "コード規定", note: "プログラマー向けの書き方の決まり", url: "https://www.notion.so/2a87c2c6cc0280f9a10af0b72b95fbb3" },
    { id: "github", title: "GitHub リポジトリ", note: "Issue・プルリクエスト・Actions", url: "https://github.com/HIBIKI5201/SymphonyKillChord" },
  ];

  function status() {
    return {
      root: "C:\\Users\\you\\GameProject\\SymphonyKillChord",
      git: { error: "", branch: fresh ? "develop" : "feature/sound/boss-bgm/agent", upstream: "origin/develop", ahead: fresh ? 0 : 2, behind: fresh ? 14 : 3, changes: fresh ? 0 : 5 },
      submodules: [
        { path: "Assets/AssetStoreTools", state: fresh ? "missing" : "ok" },
        { path: "Docs/NotionSpecifications", state: fresh ? "missing" : "ok" },
        { path: "Sound/CRIAtomCraft", state: fresh ? "missing" : "modified" },
      ],
      unity: { projectVersion: "6000.3.10f1", revision: "e35f0c77bd8e", cli: fresh ? "" : "C:\\Program Files\\Unity CLI\\unity.exe",
        editor: fresh ? "" : "C:\\Program Files\\Unity\\Hub\\Editor\\6000.3.10f1\\Editor\\Unity.exe", isOpen: false,
        hubInstallUrl: "unityhub://6000.3.10f1/e35f0c77bd8e" },
      cri: { project: fresh ? "" : "C:\\...\\Sound\\CRIAtomCraft\\SymphonyKillChord.atmcproject", app: fresh ? "" : "C:\\CRI\\tools\\criatomex\\CriAtomCraft.exe" },
      assets: { driveFolder: settings.driveFolder || "" },
      docs: { hasSpecMirror: !fresh },
    };
  }

  function ok(stdout) { return { started: true, exitCode: 0, stdout, stderr: "" }; }
  function fail(code, stderr) { return { started: true, exitCode: code, stdout: "", stderr: "conductor: " + stderr + "\n" }; }

  function cli(args) {
    const key = args.filter((a) => !a.startsWith("--")).slice(0, 2).join(" ");
    const roleArg = args.indexOf("--role");
    switch (key) {
      case "home": {
        const role = roleArg >= 0 ? args[roleArg + 1] : (settings.role || "");
        const ordered = role ? [...tiles.filter((t) => t.roles.includes(role)), ...tiles.filter((t) => !t.roles.includes(role))] : tiles;
        return ok(JSON.stringify({ role, roles, tiles: ordered, links }));
      }
      case "scripts list": return ok(JSON.stringify({ scripts }));
      case "status": return ok(JSON.stringify(status()));
      case "config list": return ok(JSON.stringify(settings));
      case "config set": settings[args[2]] = args[3]; return ok("");
      case "config unset": delete settings[args[2]]; return ok("");
      case "version": return ok("conductor 1.1.0\n");
      case "unity open": return fresh ? fail(3, "Unity 6000.3.10f1 が見つかりません。conductor unity install で Unity Hub から入れてください。") : ok("Unity CLI で Unity 6000.3.10f1 を開いています。\n");
      case "unity install": return ok("Unity Hub で Unity 6000.3.10f1 を入れる画面を開きました。\n");
      case "cri open": return fresh ? fail(3, "Sound/CRIAtomCraft に CRI Atom Craft のプロジェクトがありません。") : ok("CRI Atom Craft でプロジェクトを開いています。\n");
      case "assets open": return settings.driveFolder ? ok("Google Drive のフォルダを開きました。\n") : fail(3, "Google Drive の SymphonyKillChord フォルダが見つかりません。");
      case "assets import": return fail(3, "Unity でこのプロジェクトが開いていません。先に conductor unity open で開いてください。");
      case "docs viewer": return fail(3, "Unity でこのプロジェクトが開いていません。先に conductor unity open で開いてください。");
      case "docs open": return ok("開きました。\n");
      case "repo open": return ok("リポジトリのフォルダを開きました。\n");
      default: return fail(2, "知らないコマンドです: " + key);
    }
  }

  function emit(message) { setTimeout(() => listeners.forEach((listener) => listener({ data: message })), 0); }

  function startCapture(runId) {
    const lines = [
      "== プロジェクトの初期化（確認だけ） ==", "", "[OK] git 2.47.1", "[OK] Git LFS 3.6.0",
      fresh ? "[!!] サブモジュール Assets/AssetStoreTools を取っていません" : "[OK] サブモジュール 3 件",
      fresh ? "[!!] Unity CLI が入っていません" : "[OK] Unity CLI 1.4.2", "[OK] core.autocrlf = false", "",
      fresh ? "足りないものがあります。「プロジェクトを初期化する」で入れられます。" : "すべてそろっています。",
    ];
    let index = 0;
    capture = setInterval(() => {
      if (index < lines.length) { emit({ type: "output", runId, line: lines[index++] }); return; }
      clearInterval(capture);
      capture = null;
      emit({ type: "exited", runId, exitCode: fresh ? 1 : 0 });
    }, 220);
  }

  function handle(message) {
    const reply = (result) => emit({ type: "reply", id: message.id, result });
    switch (message.type) {
      case "hello": reply({ root: status().root, cli: "C:\\...\\conductor.exe", hasCli: true, isDark: matchMedia("(prefers-color-scheme: dark)").matches }); break;
      case "cli": setTimeout(() => reply(cli(message.args)), 120 + Math.random() * 200); break;
      case "run": {
        if (message.kind === "capture" && capture) { reply({ started: false, runId: 0, error: "ほかの実行の出力を受けています。" }); break; }
        const runId = nextRunId++;
        reply({ started: true, runId, error: "" });
        if (message.kind === "capture") { startCapture(runId); } else { setTimeout(() => emit({ type: "exited", runId, exitCode: 0 }), 2500); }
        break;
      }
      case "stop":
        if (capture) { clearInterval(capture); capture = null; emit({ type: "exited", runId: nextRunId - 1, exitCode: 1 }); }
        reply({ stopped: true });
        break;
      case "pick": reply({ path: message.kind === "folder" ? "D:\\Google Drive\\マイドライブ\\SymphonyKillChord" : "C:\\Program Files\\CRI\\CriAtomCraft.exe" }); break;
      case "copy": reply({ copied: true }); break;
      default: reply({ error: "unknown" });
    }
  }

  window.chrome = window.chrome || {};
  window.chrome.webview = {
    isDevMock: true,
    postMessage(message) { handle(JSON.parse(JSON.stringify(message))); },
    addEventListener(type, listener) { if (type === "message") { listeners.push(listener); } },
  };
})();
