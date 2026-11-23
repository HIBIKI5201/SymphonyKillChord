#include "Commands.h"

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
  conductor status [--json]                       ブランチ・未コミット・サブモジュール・Unity の版
  conductor scripts list [--json]                 登録したスクリプトと引数の組
  conductor scripts run <id> [--preset <名前か番号>] [--mode console|capture] [--yes] [--hold]
                                                  スクリプトを動かす（終了コードはスクリプトのもの）
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

                text += "\n   " + (tile.Page.empty() ? FormatScriptRunCommand(tile.ScriptId, tile.PresetIndex) : "（GUI の「" + tile.Page + "」ページ）") + "\n";
            }

            WriteOut(text);
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

            std::string versionText;
            if (ReadAllText(context.Root / "ProjectSettings" / "ProjectVersion.txt", versionText))
            {
                status.UnityVersion = ParseUnityProjectVersion(versionText);
            }

            status.UnityCliPath = FindOnPath("unity").value_or(std::string());
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
            text += status.UnityCliPath.empty() ? "（Unity CLI が無い。Setup.bat で入れられる）\n" : "（Unity CLI あり）\n";
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

        bool needsRepository = command == "scripts" || command == "home" || command == "status" ||
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

        return Fail(EXIT_USAGE, "知らないコマンドです: " + command + "（conductor --help で使い方が出ます）");
    }
}
