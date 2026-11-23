#include "Commands.h"

#include "Actions.h"
#include "CliArgs.h"
#include "CliJson.h"
#include "ExitCodes.h"
#include "HomeConfig.h"
#include "Platform.h"
#include "ProjectStatus.h"
#include "RepositoryRoot.h"
#include "ScriptConfig.h"
#include "Settings.h"

#include <algorithm>
#include <fstream>
#include <optional>
#include <sstream>

namespace skc::conductor::cli
{
    namespace
    {
        /// <summary>ホーム画面の登録ファイル（リポジトリのルートから）。</summary>
        constexpr const char* HOME_CONFIG_RELATIVE_PATH = "Conductor/home.json";

        /// <summary>使い方。</summary>
        constexpr const char* USAGE = R"(conductor — SymphonyKillChord のプロジェクト管理（KillChord Conductor の CLI）

使い方:
  conductor home [--role <役職>] [--json]          ホーム画面のボタン（役職でよく使うものが前）と、同じことをするコマンド
  conductor status [--json]                       ブランチ・未コミット・サブモジュール・Unity・CRI・Drive の状態
  conductor scripts list [--json]                 登録したスクリプトと引数の組
  conductor scripts run <id> [--preset <名前か番号>] [--mode console|capture] [--yes] [--hold]
                                                  スクリプトを動かす（終了コードはスクリプトのもの）
  conductor unity open                            このプロジェクトを Unity で開く（Unity CLI → エディタの順に探す）
  conductor unity install                         この版の Unity を Unity Hub で入れる画面を開く
  conductor unity menu <メニューのパス>             開いている Unity のメニューを動かす（Unity CLI を使う）
  conductor cri open                              CRI Atom Craft でサウンドのプロジェクトを開く
  conductor assets open                           Google Drive の SymphonyKillChord フォルダを開く
  conductor assets import                         Drive の素材を Unity へ取り込む（開いている Unity で同期する）
  conductor docs list [--json]                    よく見るドキュメント（Notion など）の一覧
  conductor docs open <id>                        ドキュメントをブラウザで開く
  conductor docs viewer                           開いている Unity で仕様書ビューアを開く
  conductor repo open                             リポジトリのフォルダを開く
  conductor config list [--json]                  この PC だけの設定
  conductor config get <キー>
  conductor config set <キー> <値>
  conductor config unset <キー>
  conductor version

共通のオプション:
  --root <パス>   リポジトリのルート（省くと、今のフォルダとこの exe の場所から上へ探す）
  --json          GUI・AI 向けに JSON で出す
  -y, --yes       確認を出さずに進める
  -h, --help      この説明

終了コード: 0 成功、1 失敗、2 使い方の誤り、3 前提が足りない、4 取り消した。
)";

        /// <summary>
        ///     ファイルを丸ごと読む。先頭の BOM は外す。
        /// </summary>
        bool ReadAllText(const std::filesystem::path& path, std::string& text)
        {
            std::ifstream stream(path, std::ios::binary);
            if (!stream) { return false; }

            std::ostringstream buffer;
            buffer << stream.rdbuf();
            text = buffer.str();
            if (text.rfind("\xEF\xBB\xBF", 0) == 0) { text.erase(0, 3); }

            return true;
        }

        /// <summary>
        ///     エラーを1行出して、終了コードを返す。
        /// </summary>
        int Fail(int exitCode, const std::string& message)
        {
            WriteError("conductor: " + message + "\n");
            return exitCode;
        }

        /// <summary>
        ///     コマンドを動かすのに要るもの（リポジトリ・登録）を読んだもの。
        /// </summary>
        struct Context
        {
            /// <summary>引数。</summary>
            CliArgs Args;

            /// <summary>リポジトリのルート。</summary>
            std::filesystem::path Root;

            /// <summary>スクリプトの登録。</summary>
            ScriptConfig Scripts;
        };

        /// <summary>
        ///     リポジトリのルートを決める。
        /// </summary>
        std::optional<std::filesystem::path> ResolveRoot(const CliArgs& args)
        {
            std::string explicitRoot = args.Value("root");
            if (!explicitRoot.empty())
            {
                std::filesystem::path root = Utf8ToPath(explicitRoot);
                std::error_code error;
                if (std::filesystem::is_regular_file(root / SCRIPT_CONFIG_RELATIVE_PATH, error)) { return root; }

                return std::nullopt;
            }

            std::optional<std::filesystem::path> root = FindRepositoryRoot(std::filesystem::current_path());
            if (!root.has_value()) { root = FindRepositoryRoot(ExecutableDirectory()); }

            return root;
        }

        /// <summary>
        ///     リポジトリとスクリプトの登録を読む。
        /// </summary>
        int LoadContext(Context& context)
        {
            std::optional<std::filesystem::path> root = ResolveRoot(context.Args);
            if (!root.has_value())
            {
                return Fail(EXIT_PRECONDITION, std::string("リポジトリが見つかりません。リポジトリの中で動かすか、--root で指定してください（")
                    + SCRIPT_CONFIG_RELATIVE_PATH + " のあるフォルダを探しています）。");
            }

            context.Root = *root;
            std::string text;
            if (!ReadAllText(context.Root / SCRIPT_CONFIG_RELATIVE_PATH, text))
            {
                return Fail(EXIT_FAILED, std::string("登録ファイルを開けません: ") + SCRIPT_CONFIG_RELATIVE_PATH);
            }

            ScriptConfigResult result = ParseScriptConfig(text);
            if (!result.IsSuccess())
            {
                return Fail(EXIT_FAILED, std::string(SCRIPT_CONFIG_RELATIVE_PATH) + " の書き方に誤りがあります: " + result.Error);
            }

            context.Scripts = std::move(result.Config);
            return EXIT_OK;
        }

        /// <summary>
        ///     ホーム画面の登録を読む。
        /// </summary>
        int LoadHome(const Context& context, HomeConfig& home)
        {
            std::string text;
            if (!ReadAllText(context.Root / HOME_CONFIG_RELATIVE_PATH, text))
            {
                return Fail(EXIT_FAILED, std::string("ホーム画面の登録ファイルを開けません: ") + HOME_CONFIG_RELATIVE_PATH);
            }

            HomeConfigResult result = ParseHomeConfig(text, context.Scripts);
            if (!result.IsSuccess())
            {
                return Fail(EXIT_FAILED, std::string(HOME_CONFIG_RELATIVE_PATH) + " の書き方に誤りがあります: " + result.Error);
            }

            home = std::move(result.Config);
            return EXIT_OK;
        }

        /// <summary>
        ///     この PC だけの設定を読む。ファイルが無ければ空の設定。
        /// </summary>
        int LoadSettings(Settings& settings)
        {
            std::filesystem::path path = SettingsFilePath();
            std::string text;
            if (path.empty() || !ReadAllText(path, text))
            {
                settings = Settings();
                return EXIT_OK;
            }

            std::string error;
            settings = ParseSettings(text, error);
            if (!error.empty()) { return Fail(EXIT_FAILED, error + "（" + PathToUtf8(path) + "）"); }

            return EXIT_OK;
        }

        /// <summary>
        ///     設定を書く（いったん別名で書いてから置き換える）。
        /// </summary>
        int SaveSettings(const Settings& settings)
        {
            std::filesystem::path path = SettingsFilePath();
            if (path.empty()) { return Fail(EXIT_PRECONDITION, "設定ファイルの場所を決められません（LOCALAPPDATA・HOME がありません）。"); }

            std::error_code error;
            std::filesystem::create_directories(path.parent_path(), error);
            std::filesystem::path temporary = path;
            temporary += ".tmp";
            {
                std::ofstream stream(temporary, std::ios::binary | std::ios::trunc);
                stream << SerializeSettings(settings);
                if (!stream) { return Fail(EXIT_FAILED, "設定ファイルを書けません: " + PathToUtf8(temporary)); }
            }

            std::filesystem::rename(temporary, path, error);
            if (error) { return Fail(EXIT_FAILED, "設定ファイルを書けません: " + PathToUtf8(path) + "（" + error.message() + "）"); }

            return EXIT_OK;
        }

        /// <summary>
        ///     スクリプトのファイルがこの PC にあるかを返す。
        /// </summary>
        bool ScriptExists(const Context& context, const ScriptEntry& entry)
        {
            std::string path = ScriptPathForThisOs(entry);
            if (path.empty()) { return false; }

            std::error_code error;
            return std::filesystem::is_regular_file(context.Root / Utf8ToPath(path), error);
        }

        /// <summary>
        ///     conductor scripts list。
        /// </summary>
        int ScriptsList(const Context& context)
        {
            std::vector<ListedScript> listed;
            for (const ScriptEntry& entry : context.Scripts.Scripts) { listed.push_back(ListedScript{ entry, ScriptExists(context, entry) }); }

            if (context.Args.Has("json"))
            {
                WriteOut(ScriptListToJson(listed) + "\n");
                return EXIT_OK;
            }

            std::string text;
            std::string category;
            for (const ListedScript& item : listed)
            {
                const ScriptEntry& entry = item.Entry;
                if (entry.Category != category)
                {
                    category = entry.Category;
                    text += "\n[" + category + "]\n";
                }

                text += "  " + entry.Id + "  " + entry.Name;
                if (entry.IsElevated) { text += "（管理者）"; }
                if (!item.Exists) { text += entry.IsLocalOnly ? "（この PC に無い）" : "（ファイルが無い）"; }

                text += "\n";
                for (std::size_t i = 0; i < entry.Presets.size(); ++i)
                {
                    text += "      --preset " + std::to_string(i + 1) + "  " + entry.Presets[i].Label + "\n";
                }
            }

            WriteOut(text.empty() ? "登録されたスクリプトはありません。\n" : text.substr(1));
            return EXIT_OK;
        }

        /// <summary>
        ///     確認の文を出し、y で答えたかを返す。
        /// </summary>
        bool AskYesNo(const std::string& message)
        {
            WriteError(message + "\n[y/N] ");
            std::string answer;
            if (!ReadLine(answer)) { return false; }

            return answer == "y" || answer == "Y" || answer == "yes";
        }

        /// <summary>
        ///     conductor scripts run。
        /// </summary>
        int ScriptsRun(const Context& context)
        {
            std::string id = context.Args.Positional(2);
            if (id.empty()) { return Fail(EXIT_USAGE, "動かすスクリプトの id を指定してください（conductor scripts list で一覧が出ます）。"); }

            auto entry = std::find_if(context.Scripts.Scripts.begin(), context.Scripts.Scripts.end(),
                [&](const ScriptEntry& script) { return script.Id == id; });
            if (entry == context.Scripts.Scripts.end()) { return Fail(EXIT_USAGE, "スクリプト \"" + id + "\" は登録されていません。"); }

            std::optional<std::size_t> presetIndex = FindPresetIndex(*entry, context.Args.Value("preset"));
            if (!presetIndex.has_value())
            {
                return Fail(EXIT_USAGE, "引数の組 \"" + context.Args.Value("preset") + "\" がありません（conductor scripts list で一覧が出ます）。");
            }

            RunMode mode = entry->ModeFor(*presetIndex);
            std::string modeName = context.Args.Value("mode");
            if (modeName == "capture") { mode = RunMode::Capture; }
            else if (modeName == "console") { mode = RunMode::Console; }
            else if (!modeName.empty()) { return Fail(EXIT_USAGE, "--mode は console か capture です。"); }

            if (!ScriptExists(context, *entry))
            {
                std::string path = ScriptPathForThisOs(*entry);
                if (path.empty()) { return Fail(EXIT_PRECONDITION, "「" + entry->Name + "」には、この OS で動かすファイルがありません。"); }

                return Fail(EXIT_PRECONDITION, entry->IsLocalOnly
                    ? "「" + entry->Name + "」の " + path + " はこの PC にありません（git の管理外なので、置いたときだけ使えます）。"
                    : "「" + entry->Name + "」の " + path + " が見つかりません。");
            }

            if (!entry->Confirm.empty() && !context.Args.Has("yes"))
            {
                if (!AskYesNo(entry->Confirm)) { return Fail(EXIT_CANCELLED, "取り消しました。"); }
            }

            ScriptRunResult result = RunScript(*entry, *presetIndex, context.Root, mode, context.Args.Has("hold"));
            if (result.IsCancelled) { return Fail(EXIT_CANCELLED, result.Error); }
            if (!result.IsStarted) { return Fail(EXIT_FAILED, "「" + entry->Name + "」を起動できませんでした: " + result.Error); }

            return result.ExitCode;
        }

        /// <summary>
        ///     conductor home。
        /// </summary>
        int Home(const Context& context)
        {
            HomeConfig home;
            if (int code = LoadHome(context, home); code != EXIT_OK) { return code; }

            Settings settings;
            if (int code = LoadSettings(settings); code != EXIT_OK) { return code; }

            HomeView view;
            view.Roles = home.Roles;
            view.Role = context.Args.Value("role");
            if (view.Role.empty())
            {
                view.Role = settings.Get(SETTING_ROLE);
                if (home.FindRole(view.Role) == nullptr) { view.Role.clear(); }
            }
            else if (home.FindRole(view.Role) == nullptr)
            {
                return Fail(EXIT_USAGE, "役職 \"" + view.Role + "\" はありません。");
            }

            view.Tiles = OrderTilesForRole(home.Tiles, view.Role);
            view.Links = home.Links;
            if (context.Args.Has("json"))
            {
                WriteOut(HomeViewToJson(view) + "\n");
                return EXIT_OK;
            }

            const RoleEntry* role = home.FindRole(view.Role);
            std::string text = "役職: " + (role != nullptr ? role->Label + "（" + role->Id + "）" : std::string("未設定")) + "\n";
            text += "  変えるとき: conductor config set role <";
            for (std::size_t i = 0; i < home.Roles.size(); ++i) { text += (i == 0 ? "" : "|") + home.Roles[i].Id; }

            text += ">\n\n";
            for (std::size_t i = 0; i < view.Tiles.size(); ++i)
            {
                const HomeTile& tile = view.Tiles[i];
                text += std::to_string(i + 1) + ". " + tile.Title;
                if (!tile.Note.empty()) { text += " — " + tile.Note; }

                std::string command;
                if (const BuiltinAction* action = FindBuiltinAction(tile.Action); action != nullptr) { command = FormatConductorCommand(action->Args); }
                else if (!tile.Page.empty()) { command = "（GUI の「" + tile.Page + "」ページ）"; }
                else { command = FormatScriptRunCommand(tile.ScriptId, tile.PresetIndex); }

                text += "\n   " + command + "\n";
            }

            WriteOut(text);
            return EXIT_OK;
        }

        /// <summary>
        ///     ファイルがあるかを返す（例外を出さない）。
        /// </summary>
        bool IsFile(const std::filesystem::path& path)
        {
            std::error_code error;
            return !path.empty() && std::filesystem::is_regular_file(path, error);
        }

        /// <summary>
        ///     フォルダがあるかを返す（例外を出さない）。
        /// </summary>
        bool IsDirectory(const std::filesystem::path& path)
        {
            std::error_code error;
            return !path.empty() && std::filesystem::is_directory(path, error);
        }

        /// <summary>
        ///     このプロジェクトの Unity の版とリビジョンを読む。
        /// </summary>
        void ReadUnityVersion(const Context& context, ProjectStatus& status)
        {
            std::string versionText;
            if (ReadAllText(context.Root / "ProjectSettings" / "ProjectVersion.txt", versionText))
            {
                status.UnityVersion = ParseUnityProjectVersion(versionText);
                status.UnityRevision = ParseUnityRevision(versionText);
            }
        }

        /// <summary>
        ///     この版の Unity エディタを探す（設定 → Unity Hub の入れ先）。
        /// </summary>
        std::string FindUnityEditor(const Settings& settings, const std::string& version)
        {
            std::filesystem::path configured = Utf8ToPath(settings.Get(SETTING_UNITY_EDITOR));
            if (IsFile(configured)) { return PathToUtf8(configured); }
            if (version.empty()) { return std::string(); }

            for (const std::filesystem::path& candidate : UnityEditorCandidates(version))
            {
                if (IsFile(candidate)) { return PathToUtf8(candidate); }
            }

            return std::string();
        }

        /// <summary>
        ///     サウンドのサブモジュールから CRI Atom Craft のプロジェクト（.atmcproject）を探す（深さ 3 まで）。
        /// </summary>
        std::string FindCriProject(const std::filesystem::path& root)
        {
            std::filesystem::path folder = root / "Sound" / "CRIAtomCraft";
            std::error_code error;
            std::filesystem::recursive_directory_iterator it(folder, std::filesystem::directory_options::skip_permission_denied, error);
            for (std::filesystem::recursive_directory_iterator end; !error && it != end; it.increment(error))
            {
                if (it.depth() >= 3) { it.disable_recursion_pending(); }

                std::error_code typeError;
                if (it->is_regular_file(typeError) && it->path().extension() == ".atmcproject") { return PathToUtf8(it->path()); }
            }

            return std::string();
        }

        /// <summary>
        ///     CRI Atom Craft の exe を探す（設定 → よくある場所）。
        /// </summary>
        std::string FindCriApp(const Settings& settings)
        {
            std::filesystem::path configured = Utf8ToPath(settings.Get(SETTING_CRI_APP));
            if (IsFile(configured)) { return PathToUtf8(configured); }

            std::optional<std::filesystem::path> found = FindCriAtomCraft();
            return found.has_value() ? PathToUtf8(*found) : std::string();
        }

        /// <summary>
        ///     Google Drive の SymphonyKillChord フォルダを探す（設定 → よくある場所）。
        /// </summary>
        std::string FindDriveFolder(const Settings& settings)
        {
            std::filesystem::path configured = Utf8ToPath(settings.Get(SETTING_DRIVE_FOLDER));
            if (IsDirectory(configured)) { return PathToUtf8(configured); }

            for (const std::filesystem::path& candidate : DriveFolderCandidates())
            {
                if (IsDirectory(candidate)) { return PathToUtf8(candidate); }
            }

            return std::string();
        }

        /// <summary>
        ///     仕様書の写し（Docs/NotionSpecifications）に中身があるかを返す。
        /// </summary>
        bool HasSpecMirror(const std::filesystem::path& root)
        {
            std::error_code error;
            std::filesystem::directory_iterator it(root / "Docs" / "NotionSpecifications", error);
            return !error && it != std::filesystem::directory_iterator();
        }

        /// <summary>
        ///     Unity・CRI・Drive・仕様書の状態を入れる。
        /// </summary>
        void FillToolStatus(const Context& context, ProjectStatus& status)
        {
            Settings settings;
            LoadSettings(settings);
            ReadUnityVersion(context, status);
            status.UnityCliPath = FindOnPath("unity").value_or(std::string());
            status.UnityEditorPath = FindUnityEditor(settings, status.UnityVersion);
            status.IsUnityOpen = IsFile(context.Root / "Temp" / "UnityLockfile");
            status.CriProjectPath = FindCriProject(context.Root);
            status.CriAppPath = FindCriApp(settings);
            status.DriveFolder = FindDriveFolder(settings);
            status.HasSpecMirror = HasSpecMirror(context.Root);
        }

        /// <summary>
        ///     開いている Unity のメニューを Unity CLI で動かす。
        /// </summary>
        int RunUnityMenu(const Context& context, const std::string& menuPath)
        {
            std::string code = BuildMenuItemCode(menuPath);
            if (code.empty()) { return Fail(EXIT_USAGE, "メニューのパスが空か、使えない文字（\" \\ 改行）を含んでいます。"); }
            if (!FindOnPath("unity").has_value())
            {
                return Fail(EXIT_PRECONDITION, "Unity CLI（unity）が見つかりません。「プロジェクトを初期化する」で入れられます。");
            }

            if (!IsFile(context.Root / "Temp" / "UnityLockfile"))
            {
                return Fail(EXIT_PRECONDITION, "Unity でこのプロジェクトが開いていません。先に conductor unity open で開いてください。");
            }

            CaptureResult result = RunAndCapture({ "unity", "command", "script", "eval", "--result-only",
                "--project-path", PathToUtf8(context.Root), "--", "--code", code, "--timeout", "15000" }, context.Root);
            if (!result.IsStarted) { return Fail(EXIT_FAILED, "Unity CLI を起動できませんでした: " + result.Error); }
            if (result.ExitCode != 0)
            {
                return Fail(EXIT_FAILED, "Unity のメニュー「" + menuPath + "」を動かせませんでした（終了コード " + std::to_string(result.ExitCode) + "）。");
            }

            // ExecuteMenuItem はメニューが無いと false を返す。
            if (result.Output.find("False") != std::string::npos || result.Output.find("false") != std::string::npos)
            {
                return Fail(EXIT_FAILED, "Unity にメニュー「" + menuPath + "」がありません（コンパイルが終わっていないかもしれません）。");
            }

            WriteOut("Unity のメニュー「" + menuPath + "」を動かしました。\n");
            return EXIT_OK;
        }

        /// <summary>
        ///     conductor unity。
        /// </summary>
        int Unity(const Context& context)
        {
            std::string action = context.Args.Positional(1);
            ProjectStatus status;
            ReadUnityVersion(context, status);
            std::string hubUrl = UnityHubInstallUrl(status.UnityVersion, status.UnityRevision);

            if (action == "install")
            {
                if (hubUrl.empty()) { return Fail(EXIT_FAILED, "ProjectSettings/ProjectVersion.txt から Unity の版を読めません。"); }

                std::string error;
                if (!OpenWithShell(hubUrl, error)) { return Fail(EXIT_PRECONDITION, "Unity Hub を開けません（Unity Hub を入れてください）: " + error); }

                WriteOut("Unity Hub で Unity " + status.UnityVersion + " を入れる画面を開きました。\n");
                return EXIT_OK;
            }

            if (action == "menu") { return RunUnityMenu(context, context.Args.Positional(2)); }

            if (action != "open") { return Fail(EXIT_USAGE, "unity のあとは open・install・menu のどれかです。"); }

            if (IsFile(context.Root / "Temp" / "UnityLockfile") && !context.Args.Has("yes"))
            {
                WriteOut("Unity でこのプロジェクトはもう開いています（開き直すときは --yes）。\n");
                return EXIT_OK;
            }

            Settings settings;
            LoadSettings(settings);
            std::string root = PathToUtf8(context.Root);
            std::string error;
            std::filesystem::path configured = Utf8ToPath(settings.Get(SETTING_UNITY_EDITOR));
            if (!IsFile(configured))
            {
                if (std::optional<std::string> cli = FindOnPath("unity"); cli.has_value())
                {
                    if (!LaunchDetached(*cli, { "open", root }, context.Root, error)) { return Fail(EXIT_FAILED, "Unity CLI を起動できませんでした: " + error); }

                    WriteOut("Unity CLI で Unity " + status.UnityVersion + " を開いています。\n");
                    return EXIT_OK;
                }
            }

            std::string editor = FindUnityEditor(settings, status.UnityVersion);
            if (editor.empty())
            {
                return Fail(EXIT_PRECONDITION, "Unity " + status.UnityVersion + " が見つかりません。conductor unity install で Unity Hub から入れてください"
                    + (hubUrl.empty() ? std::string("。") : "（" + hubUrl + "）。"));
            }

            if (!LaunchDetached(editor, { "-projectPath", root }, context.Root, error)) { return Fail(EXIT_FAILED, "Unity を起動できませんでした: " + error); }

            WriteOut("Unity " + status.UnityVersion + " を開いています。\n");
            return EXIT_OK;
        }

        /// <summary>
        ///     conductor cri。
        /// </summary>
        int Cri(const Context& context)
        {
            if (context.Args.Positional(1) != "open") { return Fail(EXIT_USAGE, "cri のあとは open です。"); }

            std::string project = FindCriProject(context.Root);
            if (project.empty())
            {
                return Fail(EXIT_PRECONDITION, "Sound/CRIAtomCraft に CRI Atom Craft のプロジェクトがありません。「プロジェクトを初期化する」でサブモジュールを取ってください。");
            }

            Settings settings;
            LoadSettings(settings);
            std::string app = FindCriApp(settings);
            std::string error;
            if (!app.empty())
            {
                if (!LaunchDetached(app, { project }, Utf8ToPath(project).parent_path(), error)) { return Fail(EXIT_FAILED, "CRI Atom Craft を起動できませんでした: " + error); }
            }
            else if (!OpenWithShell(project, error))
            {
                return Fail(EXIT_PRECONDITION, "CRI Atom Craft が見つかりません。設定の criAtomCraftPath に exe の場所を入れてください（" + error + "）。");
            }

            WriteOut("CRI Atom Craft でプロジェクトを開いています。\n");
            return EXIT_OK;
        }

        /// <summary>
        ///     conductor assets。
        /// </summary>
        int Assets(const Context& context)
        {
            std::string action = context.Args.Positional(1);
            if (action == "import") { return RunUnityMenu(context, "Tools/KillChord/DriveImportSync/Sync Now"); }
            if (action != "open") { return Fail(EXIT_USAGE, "assets のあとは open か import です。"); }

            Settings settings;
            LoadSettings(settings);
            std::string folder = FindDriveFolder(settings);
            if (folder.empty())
            {
                return Fail(EXIT_PRECONDITION, "Google Drive の SymphonyKillChord フォルダが見つかりません。Google Drive（パソコン版）を入れるか、設定の driveFolder に場所を入れてください。");
            }

            std::string error;
            if (!OpenWithShell(folder, error)) { return Fail(EXIT_FAILED, "フォルダを開けません: " + error); }

            WriteOut("Google Drive のフォルダを開きました。\n");
            return EXIT_OK;
        }

        /// <summary>
        ///     conductor docs。
        /// </summary>
        int Docs(const Context& context)
        {
            std::string action = context.Args.Positional(1);
            if (action == "viewer") { return RunUnityMenu(context, "Window/KillChord/SpecViewer"); }

            HomeConfig home;
            if (int code = LoadHome(context, home); code != EXIT_OK) { return code; }

            if (action == "list" || action.empty())
            {
                if (context.Args.Has("json"))
                {
                    HomeView view;
                    view.Links = home.Links;
                    WriteOut(HomeViewToJson(view) + "\n");
                    return EXIT_OK;
                }

                std::string text;
                for (const HomeLink& link : home.Links) { text += "  " + link.Id + "  " + link.Title + "\n      " + link.Url + "\n"; }

                WriteOut(text.empty() ? "登録されたドキュメントはありません。\n" : text);
                return EXIT_OK;
            }

            if (action != "open") { return Fail(EXIT_USAGE, "docs のあとは list・open・viewer のどれかです。"); }

            std::string id = context.Args.Positional(2);
            auto link = std::find_if(home.Links.begin(), home.Links.end(), [&](const HomeLink& known) { return known.Id == id; });
            if (link == home.Links.end()) { return Fail(EXIT_USAGE, "ドキュメント \"" + id + "\" は登録されていません（conductor docs list で一覧が出ます）。"); }

            std::string error;
            if (!OpenWithShell(link->Url, error)) { return Fail(EXIT_FAILED, "ブラウザで開けません: " + error); }

            WriteOut("「" + link->Title + "」を開きました。\n");
            return EXIT_OK;
        }

        /// <summary>
        ///     conductor repo。
        /// </summary>
        int Repo(const Context& context)
        {
            if (context.Args.Positional(1) != "open") { return Fail(EXIT_USAGE, "repo のあとは open です。"); }

            std::string error;
            if (!OpenWithShell(PathToUtf8(context.Root), error)) { return Fail(EXIT_FAILED, "フォルダを開けません: " + error); }

            WriteOut("リポジトリのフォルダを開きました。\n");
            return EXIT_OK;
        }

        /// <summary>
        ///     conductor status。
        /// </summary>
        int Status(const Context& context)
        {
            ProjectStatus status;
            status.Root = PathToUtf8(context.Root);

            CaptureResult git = RunAndCapture({ "git", "status", "--porcelain=v2", "--branch" }, context.Root);
            if (!git.IsStarted) { status.GitError = git.Error; }
            else if (git.ExitCode != 0) { status.GitError = "git status が失敗しました（終了コード " + std::to_string(git.ExitCode) + "）。"; }
            else
            {
                ParseGitStatusPorcelain(git.Output, status);
                CaptureResult submodules = RunAndCapture({ "git", "submodule", "status" }, context.Root);
                if (submodules.IsStarted && submodules.ExitCode == 0) { status.Submodules = ParseSubmoduleStatus(submodules.Output); }
            }

            FillToolStatus(context, status);
            if (context.Args.Has("json"))
            {
                WriteOut(StatusToJson(status) + "\n");
                return status.GitError.empty() ? EXIT_OK : EXIT_FAILED;
            }

            std::string text = "リポジトリ      " + status.Root + "\n";
            if (!status.GitError.empty()) { text += "git             調べられません: " + status.GitError + "\n"; }
            else
            {
                text += "ブランチ        " + status.Branch;
                if (!status.Upstream.empty())
                {
                    text += "（" + status.Upstream + " より " + std::to_string(status.Behind) + " 遅れ・" + std::to_string(status.Ahead) + " 進み。最後に fetch した時点）";
                }

                text += "\n未コミット      " + std::to_string(status.Changes) + " 件\n";
                for (const SubmoduleState& submodule : status.Submodules)
                {
                    std::string state = submodule.State == "ok" ? "OK" : submodule.State == "missing" ? "取っていない（Setup.bat で取れる）"
                        : submodule.State == "modified" ? "記録と違うコミット" : "衝突";
                    text += "サブモジュール  " + submodule.Path + "  " + state + "\n";
                }
            }

            text += "Unity           " + (status.UnityVersion.empty() ? std::string("版を読めません") : status.UnityVersion);
            text += status.UnityCliPath.empty() ? "（Unity CLI が無い。Setup.bat で入れられる）" : "（Unity CLI あり）";
            text += status.IsUnityOpen ? "・開いている\n" : "\n";
            text += "Unity エディタ  " + (status.UnityEditorPath.empty() ? std::string("見つかりません（conductor unity install で入れられる）") : status.UnityEditorPath) + "\n";
            text += "CRI             " + (status.CriProjectPath.empty() ? std::string("プロジェクトがありません（サブモジュールを取っていない）") : status.CriProjectPath) + "\n";
            text += "CRI Atom Craft  " + (status.CriAppPath.empty() ? std::string("見つかりません（ファイルの関連付けで開く）") : status.CriAppPath) + "\n";
            text += "Google Drive    " + (status.DriveFolder.empty() ? std::string("見つかりません（conductor config set driveFolder <場所>）") : status.DriveFolder) + "\n";
            text += "仕様書の写し    " + std::string(status.HasSpecMirror ? "あり" : "なし（Docs/NotionSpecifications）") + "\n";
            WriteOut(text);
            return status.GitError.empty() ? EXIT_OK : EXIT_FAILED;
        }

        /// <summary>
        ///     conductor config。
        /// </summary>
        int Config(const Context& context)
        {
            std::string action = context.Args.Positional(1);
            std::string key = context.Args.Positional(2);
            Settings settings;
            if (int code = LoadSettings(settings); code != EXIT_OK) { return code; }

            if (action == "list" || action.empty())
            {
                if (context.Args.Has("json"))
                {
                    WriteOut(SerializeSettings(settings));
                    return EXIT_OK;
                }

                std::string text = "設定ファイル: " + PathToUtf8(SettingsFilePath()) + "\n";
                for (const SettingKey& known : KnownSettingKeys())
                {
                    std::string value = settings.Get(known.Key);
                    text += std::string("  ") + known.Key + " = " + (value.empty() ? "（未設定）" : value) + "\n      " + known.Description + "\n";
                }

                WriteOut(text);
                return EXIT_OK;
            }

            if (key.empty()) { return Fail(EXIT_USAGE, "キーを指定してください。"); }
            if (!IsKnownSettingKey(key)) { return Fail(EXIT_USAGE, "キー \"" + key + "\" はありません（conductor config list で一覧が出ます）。"); }

            if (action == "get")
            {
                WriteOut(settings.Get(key) + "\n");
                return EXIT_OK;
            }

            if (action == "unset")
            {
                settings.Values.erase(key);
                return SaveSettings(settings);
            }

            if (action != "set") { return Fail(EXIT_USAGE, "config のあとは list・get・set・unset のどれかです。"); }

            std::string value = context.Args.Positional(3);
            if (value.empty()) { return Fail(EXIT_USAGE, "値を指定してください。"); }

            if (key == SETTING_ROLE)
            {
                HomeConfig home;
                if (int code = LoadHome(context, home); code != EXIT_OK) { return code; }
                if (home.FindRole(value) == nullptr) { return Fail(EXIT_USAGE, "役職 \"" + value + "\" はありません（conductor home で一覧が出ます）。"); }
            }

            settings.Values[key] = value;
            return SaveSettings(settings);
        }
    }

    int RunCommand(const std::vector<std::string>& args)
    {
        Context context;
        context.Args = ParseCliArgs(args);
        if (!context.Args.Error.empty()) { return Fail(EXIT_USAGE, context.Args.Error + "（conductor --help で使い方が出ます）"); }

        std::string command = context.Args.Positional(0);
        if (context.Args.Has("help") || command.empty() || command == "help")
        {
            WriteOut(USAGE);
            return EXIT_OK;
        }

        if (context.Args.Has("version") || command == "version")
        {
            WriteOut(std::string("conductor ") + CONDUCTOR_VERSION + "\n");
            return EXIT_OK;
        }

        bool needsRepository = command == "scripts" || command == "home" || command == "status" || command == "unity" ||
            command == "cri" || command == "assets" || command == "docs" || command == "repo" ||
            (command == "config" && context.Args.Positional(1) == "set" && context.Args.Positional(2) == SETTING_ROLE);
        if (needsRepository)
        {
            if (int code = LoadContext(context); code != EXIT_OK) { return code; }
        }

        if (command == "scripts")
        {
            std::string action = context.Args.Positional(1);
            if (action == "list" || action.empty()) { return ScriptsList(context); }
            if (action == "run") { return ScriptsRun(context); }

            return Fail(EXIT_USAGE, "scripts のあとは list か run です。");
        }

        if (command == "home") { return Home(context); }
        if (command == "status") { return Status(context); }
        if (command == "config") { return Config(context); }
        if (command == "unity") { return Unity(context); }
        if (command == "cri") { return Cri(context); }
        if (command == "assets") { return Assets(context); }
        if (command == "docs") { return Docs(context); }
        if (command == "repo") { return Repo(context); }

        return Fail(EXIT_USAGE, "知らないコマンドです: " + command + "（conductor --help で使い方が出ます）");
    }
}
