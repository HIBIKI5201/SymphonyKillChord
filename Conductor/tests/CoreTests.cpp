// core（設定読込・コマンドの組立・出力の整形・JSON の出し入れ）の単体テスト。外部のテストライブラリは使わない。
#include "Actions.h"
#include "CliArgs.h"
#include "CliJson.h"
#include "CommandLine.h"
#include "HomeConfig.h"
#include "OutputText.h"
#include "RepositoryRoot.h"
#include "ProjectStatus.h"
#include "ScriptConfig.h"
#include "Settings.h"

#include <filesystem>
#include <fstream>
#include <functional>
#include <iostream>
#include <sstream>
#include <string>
#include <vector>

namespace
{
    using namespace skc::conductor;

    int g_failures = 0;

    /// <summary>
    ///     条件が偽なら失敗として記録する。
    /// </summary>
    void Expect(bool condition, const std::string& message)
    {
        if (!condition)
        {
            ++g_failures;
            std::cerr << "  失敗: " << message << "\n";
        }
    }

    /// <summary>
    ///     2つの文字列が等しいことを確かめる。
    /// </summary>
    void ExpectEqual(const std::string& expected, const std::string& actual, const std::string& message)
    {
        if (expected != actual)
        {
            ++g_failures;
            std::cerr << "  失敗: " << message << "\n    期待: " << expected << "\n    実際: " << actual << "\n";
        }
    }

    /// <summary>
    ///     ファイルを丸ごと読む。
    /// </summary>
    std::string ReadFile(const std::filesystem::path& path)
    {
        std::ifstream stream(path, std::ios::binary);
        std::ostringstream buffer;
        buffer << stream.rdbuf();
        return buffer.str();
    }

    void ParsesMinimalEntryWithDefaults()
    {
        ScriptConfigResult result = ParseScriptConfig(R"({"version":1,"scripts":[
            {"id":"a","name":"A","category":"C","windows":"Setup.bat"}]})");
        Expect(result.IsSuccess(), "最小の設定が読める: " + result.Error);
        if (!result.IsSuccess()) { return; }

        const ScriptEntry& entry = result.Config.Scripts.at(0);
        Expect(entry.Mode == RunMode::Console, "mode の既定は console");
        Expect(!entry.IsElevated, "elevate の既定は false");
        Expect(entry.Presets.size() == 1 && entry.Presets[0].Args.empty(), "presets が無ければ「引数なし」が1つ入る");
    }

    void PresetModeOverridesEntryMode()
    {
        ScriptConfigResult result = ParseScriptConfig(R"({"version":1,"scripts":[
            {"id":"a","name":"A","category":"C","windows":"Setup.bat","presets":[
                {"label":"x","args":[]},{"label":"y","args":["-Check"],"mode":"capture"}]}]})");
        Expect(result.IsSuccess(), "presets 付きの設定が読める: " + result.Error);
        if (!result.IsSuccess()) { return; }

        const ScriptEntry& entry = result.Config.Scripts.at(0);
        Expect(entry.ModeFor(0) == RunMode::Console, "mode の無い組はスクリプトの mode");
        Expect(entry.ModeFor(1) == RunMode::Capture, "組の mode が優先される");
    }

    void RejectsInvalidConfigs()
    {
        struct Case
        {
            const char* Name;
            const char* Json;
        };

        const std::vector<Case> cases = {
            { "JSON の誤り", "{" },
            { "版が違う", R"({"version":2,"scripts":[]})" },
            { "id の重複", R"({"version":1,"scripts":[
                {"id":"a","name":"A","category":"C","windows":"a.bat"},
                {"id":"a","name":"B","category":"C","windows":"b.bat"}]})" },
            { "name が無い", R"({"version":1,"scripts":[{"id":"a","category":"C","windows":"a.bat"}]})" },
            { "動かせない拡張子", R"({"version":1,"scripts":[{"id":"a","name":"A","category":"C","windows":"a.sh"}]})" },
            { "絶対パス", R"({"version":1,"scripts":[{"id":"a","name":"A","category":"C","windows":"C:/a.bat"}]})" },
            { "親フォルダへ出るパス", R"({"version":1,"scripts":[{"id":"a","name":"A","category":"C","windows":"../a.bat"}]})" },
            { "\\ 区切り", R"({"version":1,"scripts":[{"id":"a","name":"A","category":"C","windows":"x\\a.bat"}]})" },
            { "bat に & を渡す", R"({"version":1,"scripts":[{"id":"a","name":"A","category":"C","windows":"a.bat",
                "presets":[{"label":"x","args":["a&b"]}]}]})" },
            { "mode の誤り", R"({"version":1,"scripts":[{"id":"a","name":"A","category":"C","windows":"a.bat","mode":"window"}]})" },
            { "管理者で出力を受ける", R"({"version":1,"scripts":[{"id":"a","name":"A","category":"C","windows":"a.bat",
                "elevate":true,"presets":[{"label":"x","mode":"capture"}]}]})" },
        };

        for (const Case& testCase : cases)
        {
            ScriptConfigResult result = ParseScriptConfig(testCase.Json);
            Expect(!result.IsSuccess(), std::string("読めないこと: ") + testCase.Name);
            Expect(result.Config.Scripts.empty(), std::string("失敗したら一覧は空: ") + testCase.Name);
        }
    }

    void QuotesArgumentsForCommandLineToArgv()
    {
        ExpectEqual("abc", QuoteArgument("abc"), "空白の無い引数はそのまま");
        ExpectEqual("\"\"", QuoteArgument(""), "空の引数は \"\"");
        ExpectEqual("\"a b\"", QuoteArgument("a b"), "空白を含むと囲む");
        ExpectEqual("\"a\\\"b\"", QuoteArgument("a\"b"), "引用符は \\\" にする");
        ExpectEqual("\"C:\\a b\\\\\"", QuoteArgument("C:\\a b\\"), "閉じの前の \\ は2倍にする");
        ExpectEqual("\"a\\\\\\\"b c\"", QuoteArgument("a\\\"b c"), "引用符の前の \\ は2倍にして足す");
        ExpectEqual("C:\\a\\b", QuoteArgument("C:\\a\\b"), "囲まないときの \\ はそのまま");
        ExpectEqual("'it''s'", QuotePowerShellLiteral("it's"), "PowerShell の ' は '' にする");
    }

    ScriptEntry MakeEntry(const std::string& path, RunMode mode, const std::vector<std::string>& args)
    {
        ScriptEntry entry;
        entry.Id = "x";
        entry.WindowsPath = path;
        entry.Mode = mode;
        entry.Presets.push_back(ScriptPreset{ "p", args, std::nullopt });
        return entry;
    }

    void BuildsLaunchCommands()
    {
        ToolPaths tools{ "C:\\Windows\\System32\\cmd.exe", "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe" };
        const std::string root = "C:\\Game Project\\SKC";

        LaunchCommand batch = BuildLaunchCommand(MakeEntry("Setup.bat", RunMode::Capture, { "-Check" }), 0, root, tools, RunMode::Capture, true);
        ExpectEqual(tools.CmdExe, batch.Application, "bat は cmd.exe で動かす");
        ExpectEqual("/d /s /c \"\"C:\\Game Project\\SKC\\Setup.bat\" -Check\"", batch.Arguments, "bat の引数");
        ExpectEqual("C:\\Game Project\\SKC", batch.WorkingDirectory, "作業フォルダはスクリプトのフォルダ");
        ExpectEqual("\"C:\\Windows\\System32\\cmd.exe\" /d /s /c \"\"C:\\Game Project\\SKC\\Setup.bat\" -Check\"",
            batch.FullCommandLine(), "コマンドライン全体");

        LaunchCommand nested = BuildLaunchCommand(MakeEntry("BuildRunner/runner-menu.bat", RunMode::Console, {}), 0, root, tools, RunMode::Console, true);
        ExpectEqual("/d /s /c \"\"C:\\Game Project\\SKC\\BuildRunner\\runner-menu.bat\"\"", nested.Arguments, "引数なしの bat");
        ExpectEqual("C:\\Game Project\\SKC\\BuildRunner", nested.WorkingDirectory, "サブフォルダの作業フォルダ");

        LaunchCommand captured = BuildLaunchCommand(MakeEntry("Builds/Install_App.ps1", RunMode::Capture, { "a b" }), 0, root, tools, RunMode::Capture, true);
        ExpectEqual(tools.PowerShellExe, captured.Application, "ps1 は powershell.exe で動かす");
        ExpectEqual("-NoProfile -NonInteractive -ExecutionPolicy Bypass -File \"C:\\Game Project\\SKC\\Builds\\Install_App.ps1\" \"a b\"",
            captured.Arguments, "出力を受ける ps1 の引数");

        LaunchCommand console = BuildLaunchCommand(MakeEntry("Builds/Install_App.ps1", RunMode::Console, { "it's" }), 0, root, tools, RunMode::Console, true);
        Expect(console.Arguments.find("-Command") != std::string::npos, "コンソールの ps1 は -Command で包む");
        Expect(console.Arguments.find("& 'C:\\Game Project\\SKC\\Builds\\Install_App.ps1' 'it''s'") != std::string::npos,
            "コンソールの ps1 はパスと引数を ' で囲む: " + console.Arguments);
        Expect(console.Arguments.find("exit $code") != std::string::npos, "終了コードを返す");

        LaunchCommand exe = BuildLaunchCommand(MakeEntry("Tools/a.exe", RunMode::Capture, { "x" }), 0, root, tools, RunMode::Capture, true);
        ExpectEqual("C:\\Game Project\\SKC\\Tools\\a.exe", exe.Application, "exe は直接動かす");
        ExpectEqual("x", exe.Arguments, "exe の引数");
    }

    void SplitsOutputIntoLines()
    {
        OutputLineBuffer buffer;
        std::vector<std::string> first = buffer.Feed("abc\r\nde");
        Expect(first.size() == 1 && first[0] == "abc", "CRLF で切り、\\r を外す");
        std::vector<std::string> second = buffer.Feed("f\n\nxyz");
        Expect(second.size() == 2 && second[0] == "def" && second[1].empty(), "塊をまたいだ行をつなぐ");
        ExpectEqual("xyz", buffer.Flush(), "最後の行の途中を返す");
        ExpectEqual("", buffer.Flush(), "Flush のあとは空");
    }

    void DetectsUtf8()
    {
        Expect(IsValidUtf8("ascii only"), "ASCII は UTF-8");
        Expect(IsValidUtf8("\xE7\xA2\xBA\xE8\xAA\x8D"), "「確認」の UTF-8");
        Expect(!IsValidUtf8("\x8A\x6D\x94\x46"), "「確認」の CP932 は UTF-8 ではない");
        Expect(!IsValidUtf8("\xE7\xA2"), "途中で切れた UTF-8");
        Expect(!IsValidUtf8("\xC0\x80"), "冗長な表し方");
        Expect(!IsValidUtf8("\xED\xA0\x80"), "サロゲート");
    }

    void StripsAnsiSequences()
    {
        ExpectEqual("OK done", StripAnsi("\x1b[32mOK\x1b[0m done"), "色の指定を外す");
        ExpectEqual("title", StripAnsi("\x1b]0;window\x07title"), "OSC（BEL 終わり）を外す");
        ExpectEqual("x", StripAnsi("\x1b]0;w\x1b\\x"), "OSC（ESC \\ 終わり）を外す");
        ExpectEqual("ab", StripAnsi("a\x1b" "7b"), "2文字のシーケンスを外す");
        ExpectEqual("a", StripAnsi("a\x1b"), "行末の ESC を外す");
    }

    void FindsRepositoryRoot()
    {
        std::filesystem::path root = std::filesystem::temp_directory_path() / "skc-conductor-test";
        std::filesystem::remove_all(root);
        std::filesystem::create_directories(root / "Conductor" / "bin");
        std::ofstream(root / SCRIPT_CONFIG_RELATIVE_PATH) << "{}";

        std::optional<std::filesystem::path> found = FindRepositoryRoot(root / "Conductor" / "bin");
        Expect(found.has_value() && std::filesystem::equivalent(*found, root), "上へたどってルートを見つける");

        std::filesystem::remove(root / SCRIPT_CONFIG_RELATIVE_PATH);
        Expect(!FindRepositoryRoot(root / "Conductor" / "bin").has_value(), "登録ファイルが無ければ見つからない");
        std::filesystem::remove_all(root);
    }


    /// <summary>
    ///     ホーム画面のテスト用のスクリプト登録。
    /// </summary>
    ScriptConfig MakeScripts()
    {
        ScriptConfigResult result = ParseScriptConfig(R"({"version":1,"scripts":[
            {"id":"setup","name":"S","category":"C","windows":"Setup.bat","presets":[{"label":"run","args":[]},{"label":"check","args":["-Check"],"mode":"capture"}]},
            {"id":"other","name":"O","category":"C","windows":"o.bat"}]})");
        return result.Config;
    }

    void ParsesHomeConfig()
    {
        ScriptConfig scripts = MakeScripts();
        HomeConfigResult result = ParseHomeConfig(R"({"version":1,
            "roles":[{"id":"sound","label":"サウンド"},{"id":"lead","label":"リード"}],
            "tiles":[
                {"id":"a","title":"A","script":"other"},
                {"id":"b","title":"B","script":"setup","preset":"check","roles":["sound"]},
                {"id":"c","title":"C","page":"scripts","roles":["lead","sound"]}]})", scripts);
        Expect(result.IsSuccess(), "ホーム画面の登録が読める: " + result.Error);
        if (!result.IsSuccess()) { return; }

        Expect(result.Config.Tiles.at(1).PresetIndex == 1, "引数の組を名前で引く");
        std::vector<HomeTile> ordered = OrderTilesForRole(result.Config.Tiles, "sound");
        ExpectEqual("b,c,a", ordered[0].Id + "," + ordered[1].Id + "," + ordered[2].Id, "役職のボタンを前に、ファイルの順を保って並べる");
        std::vector<HomeTile> plain = OrderTilesForRole(result.Config.Tiles, "");
        ExpectEqual("a", plain[0].Id, "役職が空ならファイルの順");
    }

    void RejectsInvalidHomeConfigs()
    {
        ScriptConfig scripts = MakeScripts();
        const std::vector<std::pair<const char*, const char*>> cases = {
            { "無いスクリプト", R"({"version":1,"roles":[{"id":"r","label":"R"}],"tiles":[{"id":"a","title":"A","script":"none"}]})" },
            { "無い引数の組", R"({"version":1,"roles":[{"id":"r","label":"R"}],"tiles":[{"id":"a","title":"A","script":"setup","preset":"x"}]})" },
            { "無い役職", R"({"version":1,"roles":[{"id":"r","label":"R"}],"tiles":[{"id":"a","title":"A","script":"setup","roles":["x"]}]})" },
            { "script と page の両方", R"({"version":1,"roles":[{"id":"r","label":"R"}],"tiles":[{"id":"a","title":"A","script":"setup","page":"scripts"}]})" },
            { "知らないページ", R"({"version":1,"roles":[{"id":"r","label":"R"}],"tiles":[{"id":"a","title":"A","page":"x"}]})" },
            { "役職が無い", R"({"version":1,"roles":[],"tiles":[]})" },
            { "id の重複", R"({"version":1,"roles":[{"id":"r","label":"R"}],"tiles":[{"id":"a","title":"A","page":"scripts"},{"id":"a","title":"B","page":"settings"}]})" },
        };

        for (const auto& [name, json] : cases)
        {
            Expect(!ParseHomeConfig(json, scripts).IsSuccess(), std::string("ホーム画面の登録を読めないこと: ") + name);
        }
    }

    void FindsPresetsByLabelOrNumber()
    {
        ScriptConfig scripts = MakeScripts();
        const ScriptEntry& entry = scripts.Scripts.at(0);
        Expect(FindPresetIndex(entry, "") == std::optional<std::size_t>(0), "空なら最初の組");
        Expect(FindPresetIndex(entry, "check") == std::optional<std::size_t>(1), "名前で引く");
        Expect(FindPresetIndex(entry, "2") == std::optional<std::size_t>(1), "1 から数えた番号で引く");
        Expect(!FindPresetIndex(entry, "3").has_value(), "範囲外の番号");
        Expect(!FindPresetIndex(entry, "0").has_value(), "0 は無い");
    }

    void ParsesCliArgs()
    {
        CliArgs args = ParseCliArgs({ "scripts", "run", "setup", "--preset", "2", "-y", "--mode=capture", "--json" });
        Expect(args.Error.empty(), "引数を分けられる: " + args.Error);
        ExpectEqual("setup", args.Positional(2), "位置の引数");
        ExpectEqual("2", args.Value("preset"), "値のオプション");
        ExpectEqual("capture", args.Value("mode"), "--name=value");
        Expect(args.Has("yes") && args.Has("json"), "-y と --json");
        Expect(!ParseCliArgs({ "--unknown" }).Error.empty(), "知らないオプション");
        Expect(!ParseCliArgs({ "--preset" }).Error.empty(), "値の無い値オプション");
        Expect(!ParseCliArgs({ "--json=1" }).Error.empty(), "値を取らないオプションに値");
        Expect(!ParseCliArgs({ "-x" }).Error.empty(), "知らない短いオプション");
    }

    void ReadsAndWritesSettings()
    {
        std::string error;
        Settings settings = ParseSettings("", error);
        Expect(error.empty() && settings.Values.empty(), "空のファイルは空の設定");

        settings = ParseSettings(R"({"role":"sound","future":"x","number":1})", error);
        Expect(error.empty(), "設定が読める");
        ExpectEqual("sound", settings.Get("role"), "role を読む");
        ExpectEqual("x", settings.Get("future"), "知らないキーも残す");

        Settings reread = ParseSettings(SerializeSettings(settings), error);
        ExpectEqual("sound", reread.Get("role"), "書いて読み直せる");
        Expect(IsKnownSettingKey("role") && !IsKnownSettingKey("future"), "書けるキー");

        ParseSettings("[1]", error);
        Expect(!error.empty(), "オブジェクトでなければ失敗");
    }

    void ParsesGitStatus()
    {
        ProjectStatus status;
        ParseGitStatusPorcelain(
            "# branch.oid abc\n# branch.head develop\n# branch.upstream origin/develop\n# branch.ab +1 -3\n"
            "1 .M N... 100644 100644 100644 a b Assets/a.cs\n? new.txt\n! ignored.txt\n", status);
        ExpectEqual("develop", status.Branch, "ブランチ");
        ExpectEqual("origin/develop", status.Upstream, "追いかけているブランチ");
        Expect(status.Ahead == 1 && status.Behind == 3, "進み・遅れ");
        Expect(status.Changes == 2, "変更の数（無視されたものは数えない）");

        std::vector<std::string> paths = ParseGitmodulesPaths(
            "submodule.Assets/AssetStoreTools.path Assets/AssetStoreTools\nsubmodule.cri.path Sound/CRIAtomCraft\n"
            "submodule.spec.path Docs/NotionSpecifications\n");
        Expect(paths.size() == 3, "サブモジュールの数");
        if (paths.size() == 3) { ExpectEqual("Sound/CRIAtomCraft", paths[1], "パス"); }

        std::vector<GitlinkEntry> entries = ParseGitlinkEntries(
            "160000 1234 0\tAssets/AssetStoreTools\n160000 5678 0\tSound/CRIAtomCraft\n100644 ffff 0\tREADME.md\n"
            "160000 9abc 0\tDocs/NotionSpecifications\n160000 aaaa 2\tX/Conflict\n160000 bbbb 3\tX/Conflict\n");
        Expect(entries.size() == 5, "gitlink の行だけ取る");
        ExpectEqual("ok", DecideSubmoduleState("Assets/AssetStoreTools", entries, std::string("1234")), "取ってある");
        ExpectEqual("missing", DecideSubmoduleState("Sound/CRIAtomCraft", entries, std::nullopt), "取っていない");
        ExpectEqual("modified", DecideSubmoduleState("Docs/NotionSpecifications", entries, std::string("0000")), "記録と違う");
        ExpectEqual("conflict", DecideSubmoduleState("X/Conflict", entries, std::string("aaaa")), "衝突");
        ExpectEqual("missing", DecideSubmoduleState("Not/Recorded", entries, std::string("1234")), "記録に無い");

        ExpectEqual("6000.3.10f1", ParseUnityProjectVersion("m_EditorVersion: 6000.3.10f1\r\nm_EditorVersionWithRevision: x\r\n"), "Unity の版");
    }

    void RoundTripsCliJson()
    {
        ScriptConfig scripts = MakeScripts();
        std::vector<ListedScript> listed = { ListedScript{ scripts.Scripts[0], true } };
        listed[0].Entry.Confirm = "確認";
        std::vector<ListedScript> read;
        std::string error;
        Expect(ScriptListFromJson(ScriptListToJson(listed), read, error), "一覧の JSON を読める: " + error);
        if (read.size() == 1)
        {
            ExpectEqual("setup", read[0].Entry.Id, "id");
            ExpectEqual("確認", read[0].Entry.Confirm, "確認の文");
            Expect(read[0].Exists, "ファイルがある");
            Expect(read[0].Entry.Presets.size() == 2 && read[0].Entry.ModeFor(1) == RunMode::Capture, "組ごとの動かし方");
        }

        HomeView view;
        view.Role = "sound";
        view.Roles = { RoleEntry{ "sound", "サウンド" } };
        HomeTile tile;
        tile.Id = "b";
        tile.Title = "B";
        tile.ScriptId = "setup";
        tile.PresetIndex = 1;
        view.Tiles = { tile };
        HomeView readView;
        Expect(HomeViewFromJson(HomeViewToJson(view), readView, error), "ホーム画面の JSON を読める: " + error);
        Expect(readView.Tiles.size() == 1 && readView.Tiles[0].PresetIndex == 1 && readView.Role == "sound", "ホーム画面の中身");
        Expect(HomeViewToJson(view).find("conductor scripts run setup --preset 2") != std::string::npos, "同じことをするコマンドを入れる");

        ProjectStatus status;
        status.Branch = "develop";
        status.Behind = 2;
        status.Submodules = { SubmoduleState{ "a", "missing" } };
        ProjectStatus readStatus;
        Expect(StatusFromJson(StatusToJson(status), readStatus, error), "状態の JSON を読める: " + error);
        Expect(readStatus.Branch == "develop" && readStatus.Behind == 2 && readStatus.Submodules.size() == 1, "状態の中身");
        Expect(!StatusFromJson("{", readStatus, error), "壊れた JSON は失敗");
    }

    void ParsesHomeActionsAndLinks()
    {
        ScriptConfig scripts = MakeScripts();
        HomeConfigResult result = ParseHomeConfig(R"({"version":1,
            "roles":[{"id":"sound","label":"サウンド"}],
            "tiles":[{"id":"u","title":"Unity","action":"unity-open","roles":["sound"]}],
            "links":[{"id":"spec","title":"仕様","url":"https://example.com/a"}]})", scripts);
        Expect(result.IsSuccess(), "組み込みの操作とリンクを読める: " + result.Error);
        if (!result.IsSuccess()) { return; }

        ExpectEqual("unity-open", result.Config.Tiles.at(0).Action, "操作の名前");
        ExpectEqual("https://example.com/a", result.Config.Links.at(0).Url, "リンクの URL");

        HomeView view;
        view.Tiles = result.Config.Tiles;
        view.Links = result.Config.Links;
        std::string json = HomeViewToJson(view);
        Expect(json.find("\"conductor unity open\"") != std::string::npos, "操作のコマンドを JSON に入れる");
        HomeView read;
        std::string error;
        Expect(HomeViewFromJson(json, read, error) && read.Links.size() == 1 && read.Tiles.at(0).Action == "unity-open", "操作とリンクの JSON を読める");

        const std::vector<std::pair<const char*, const char*>> cases = {
            { "知らない操作", R"({"version":1,"roles":[{"id":"r","label":"R"}],"tiles":[{"id":"a","title":"A","action":"x"}]})" },
            { "操作と script の両方", R"({"version":1,"roles":[{"id":"r","label":"R"}],"tiles":[{"id":"a","title":"A","action":"unity-open","script":"setup"}]})" },
            { "https でないリンク", R"({"version":1,"roles":[{"id":"r","label":"R"}],"tiles":[],"links":[{"id":"a","title":"A","url":"http://x"}]})" },
            { "リンクの id の重複", R"({"version":1,"roles":[{"id":"r","label":"R"}],"tiles":[],"links":[{"id":"a","title":"A","url":"https://x"},{"id":"a","title":"B","url":"https://y"}]})" },
        };
        for (const auto& [name, text] : cases)
        {
            Expect(!ParseHomeConfig(text, scripts).IsSuccess(), std::string("ホーム画面の登録を読めないこと: ") + name);
        }
    }

    void BuildsActionCommands()
    {
        ExpectEqual("conductor unity open", FormatConductorCommand({ "unity", "open" }), "コマンドの1行");
        ExpectEqual("conductor unity menu \"Tools/A B\"", FormatConductorCommand({ "unity", "menu", "Tools/A B" }), "空白は囲む");
        Expect(FindBuiltinAction("cri-open") != nullptr && FindBuiltinAction("nothing") == nullptr, "操作を名前で探す");
        ExpectEqual("UnityEditor.EditorApplication.ExecuteMenuItem(\"Window/A\")", BuildMenuItemCode("Window/A"), "メニューを動かす式");
        Expect(BuildMenuItemCode("a\"); x(\"").empty() && BuildMenuItemCode("").empty(), "危ない文字を含むパスは断る");
    }

    void ReadsUnityRevision()
    {
        const char* text = "m_EditorVersion: 6000.3.10f1\r\nm_EditorVersionWithRevision: 6000.3.10f1 (e35f0c77bd8e)\r\n";
        ExpectEqual("e35f0c77bd8e", ParseUnityRevision(text), "リビジョン");
        ExpectEqual("", ParseUnityRevision("m_EditorVersion: 6000.3.10f1\n"), "リビジョンが無い");
        ExpectEqual("unityhub://6000.3.10f1/e35f0c77bd8e", UnityHubInstallUrl("6000.3.10f1", "e35f0c77bd8e"), "Unity Hub の URL");
        ExpectEqual("", UnityHubInstallUrl("6000.3.10f1", ""), "リビジョンが無ければ空");

        ProjectStatus status;
        status.UnityVersion = "6000.3.10f1";
        status.UnityRevision = "e35f0c77bd8e";
        status.IsUnityOpen = true;
        status.CriProjectPath = "Sound/CRIAtomCraft/a.atmcproject";
        status.DriveFolder = "G:/マイドライブ/SymphonyKillChord";
        status.HasSpecMirror = true;
        ProjectStatus read;
        std::string error;
        std::string json = StatusToJson(status);
        Expect(StatusFromJson(json, read, error), "状態の JSON を読める: " + error);
        Expect(read.IsUnityOpen && read.UnityRevision == "e35f0c77bd8e" && read.CriProjectPath == status.CriProjectPath
            && read.DriveFolder == status.DriveFolder && read.HasSpecMirror, "Unity・CRI・Drive・仕様書の状態");
        Expect(json.find("unityhub://6000.3.10f1/e35f0c77bd8e") != std::string::npos, "Unity Hub の URL を JSON に入れる");
    }

    void RepositoryHomeConfigIsValid(const std::filesystem::path& configPath, const std::filesystem::path& homePath)
    {
        ScriptConfigResult scripts = ParseScriptConfig(ReadFile(configPath));
        HomeConfigResult home = ParseHomeConfig(ReadFile(homePath), scripts.Config);
        Expect(home.IsSuccess(), "リポジトリの home.json が読める: " + home.Error);
    }

    void RepositoryConfigIsValid(const std::filesystem::path& configPath)
    {
        ScriptConfigResult result = ParseScriptConfig(ReadFile(configPath));
        Expect(result.IsSuccess(), "リポジトリの launcher-scripts.json が読める: " + result.Error);
        Expect(!result.Config.Scripts.empty(), "リポジトリの launcher-scripts.json に登録がある");
    }
}

int main(int argc, char** argv)
{
    const std::vector<std::pair<const char*, std::function<void()>>> tests = {
        { "ParsesMinimalEntryWithDefaults", ParsesMinimalEntryWithDefaults },
        { "PresetModeOverridesEntryMode", PresetModeOverridesEntryMode },
        { "RejectsInvalidConfigs", RejectsInvalidConfigs },
        { "QuotesArgumentsForCommandLineToArgv", QuotesArgumentsForCommandLineToArgv },
        { "BuildsLaunchCommands", BuildsLaunchCommands },
        { "SplitsOutputIntoLines", SplitsOutputIntoLines },
        { "DetectsUtf8", DetectsUtf8 },
        { "StripsAnsiSequences", StripsAnsiSequences },
        { "FindsRepositoryRoot", FindsRepositoryRoot },
        { "ParsesHomeConfig", ParsesHomeConfig },
        { "RejectsInvalidHomeConfigs", RejectsInvalidHomeConfigs },
        { "FindsPresetsByLabelOrNumber", FindsPresetsByLabelOrNumber },
        { "ParsesCliArgs", ParsesCliArgs },
        { "ReadsAndWritesSettings", ReadsAndWritesSettings },
        { "ParsesGitStatus", ParsesGitStatus },
        { "RoundTripsCliJson", RoundTripsCliJson },
        { "ParsesHomeActionsAndLinks", ParsesHomeActionsAndLinks },
        { "BuildsActionCommands", BuildsActionCommands },
        { "ReadsUnityRevision", ReadsUnityRevision },
    };

    for (const auto& [name, test] : tests)
    {
        std::cout << name << "\n";
        test();
    }

    if (argc > 1)
    {
        std::cout << "RepositoryConfigIsValid\n";
        RepositoryConfigIsValid(argv[1]);
    }

    if (argc > 2)
    {
        std::cout << "RepositoryHomeConfigIsValid\n";
        RepositoryHomeConfigIsValid(argv[1], argv[2]);
    }

    std::cout << (g_failures == 0 ? "すべて成功" : "失敗あり: " + std::to_string(g_failures) + " 件") << "\n";
    return g_failures == 0 ? 0 : 1;
}
