// core（設定読込・コマンドの組立・出力の整形）の単体テスト。外部のテストライブラリは使わない。
#include "CommandLine.h"
#include "OutputText.h"
#include "RepositoryRoot.h"
#include "ScriptConfig.h"

#include <filesystem>
#include <fstream>
#include <functional>
#include <iostream>
#include <sstream>
#include <string>
#include <vector>

namespace
{
    using namespace skc::launcher;

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

        LaunchCommand batch = BuildLaunchCommand(MakeEntry("Setup.bat", RunMode::Capture, { "-Check" }), 0, root, tools);
        ExpectEqual(tools.CmdExe, batch.Application, "bat は cmd.exe で動かす");
        ExpectEqual("/d /s /c \"\"C:\\Game Project\\SKC\\Setup.bat\" -Check\"", batch.Arguments, "bat の引数");
        ExpectEqual("C:\\Game Project\\SKC", batch.WorkingDirectory, "作業フォルダはスクリプトのフォルダ");
        ExpectEqual("\"C:\\Windows\\System32\\cmd.exe\" /d /s /c \"\"C:\\Game Project\\SKC\\Setup.bat\" -Check\"",
            batch.FullCommandLine(), "コマンドライン全体");

        LaunchCommand nested = BuildLaunchCommand(MakeEntry("BuildRunner/runner-menu.bat", RunMode::Console, {}), 0, root, tools);
        ExpectEqual("/d /s /c \"\"C:\\Game Project\\SKC\\BuildRunner\\runner-menu.bat\"\"", nested.Arguments, "引数なしの bat");
        ExpectEqual("C:\\Game Project\\SKC\\BuildRunner", nested.WorkingDirectory, "サブフォルダの作業フォルダ");

        LaunchCommand captured = BuildLaunchCommand(MakeEntry("Builds/Install_App.ps1", RunMode::Capture, { "a b" }), 0, root, tools);
        ExpectEqual(tools.PowerShellExe, captured.Application, "ps1 は powershell.exe で動かす");
        ExpectEqual("-NoProfile -NonInteractive -ExecutionPolicy Bypass -File \"C:\\Game Project\\SKC\\Builds\\Install_App.ps1\" \"a b\"",
            captured.Arguments, "出力を受ける ps1 の引数");

        LaunchCommand console = BuildLaunchCommand(MakeEntry("Builds/Install_App.ps1", RunMode::Console, { "it's" }), 0, root, tools);
        Expect(console.Arguments.find("-Command") != std::string::npos, "コンソールの ps1 は -Command で包む");
        Expect(console.Arguments.find("& 'C:\\Game Project\\SKC\\Builds\\Install_App.ps1' 'it''s'") != std::string::npos,
            "コンソールの ps1 はパスと引数を ' で囲む: " + console.Arguments);
        Expect(console.Arguments.find("exit $code") != std::string::npos, "終了コードを返す");

        LaunchCommand exe = BuildLaunchCommand(MakeEntry("Tools/a.exe", RunMode::Capture, { "x" }), 0, root, tools);
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
        std::filesystem::path root = std::filesystem::temp_directory_path() / "skc-launcher-test";
        std::filesystem::remove_all(root);
        std::filesystem::create_directories(root / "ScriptLauncher" / "bin");
        std::ofstream(root / SCRIPT_CONFIG_RELATIVE_PATH) << "{}";

        std::optional<std::filesystem::path> found = FindRepositoryRoot(root / "ScriptLauncher" / "bin");
        Expect(found.has_value() && std::filesystem::equivalent(*found, root), "上へたどってルートを見つける");

        std::filesystem::remove(root / SCRIPT_CONFIG_RELATIVE_PATH);
        Expect(!FindRepositoryRoot(root / "ScriptLauncher" / "bin").has_value(), "登録ファイルが無ければ見つからない");
        std::filesystem::remove_all(root);
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

    std::cout << (g_failures == 0 ? "すべて成功" : "失敗あり: " + std::to_string(g_failures) + " 件") << "\n";
    return g_failures == 0 ? 0 : 1;
}
