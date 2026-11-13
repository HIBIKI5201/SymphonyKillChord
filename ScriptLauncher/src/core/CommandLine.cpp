#include "CommandLine.h"

#include "ScriptConfig.h"

namespace skc::launcher
{
    namespace
    {
        /// <summary>コンソールで開いた ps1 が終わったときに出す文。</summary>
        constexpr const char* POWERSHELL_DONE_PROMPT = "終わりました。Enter キーでこのウィンドウを閉じます";

        /// <summary>
        ///     パスをダブルクォートで囲む（Windows のパスにダブルクォートは入らない）。
        /// </summary>
        std::string QuotePath(const std::string& path)
        {
            return "\"" + path + "\"";
        }

        /// <summary>
        ///     bat を cmd.exe で動かす引数を作る。/s を付けると、/c のあとの最初と最後の " だけが外される。
        /// </summary>
        std::string BuildBatchArguments(const std::string& scriptPath, const std::vector<std::string>& args)
        {
            std::string inner = QuotePath(scriptPath);
            if (!args.empty()) { inner += " " + JoinArguments(args); }

            return "/d /s /c \"" + inner + "\"";
        }

        /// <summary>
        ///     ps1 をコンソールで動かす引数を作る。終わったら Enter を待ち、スクリプトの終了コードで抜ける。
        /// </summary>
        std::string BuildPowerShellConsoleArguments(const std::string& scriptPath, const std::vector<std::string>& args)
        {
            std::string invocation = "& " + QuotePowerShellLiteral(scriptPath);
            for (const std::string& arg : args) { invocation += " " + QuotePowerShellLiteral(arg); }

            std::string command =
                "$global:LASTEXITCODE = 0; "
                "try { " + invocation + " } catch { Write-Host $_ -ForegroundColor Red; $global:LASTEXITCODE = 1 }; "
                "$code = $LASTEXITCODE; Write-Host ''; "
                "Read-Host " + QuotePowerShellLiteral(POWERSHELL_DONE_PROMPT) + " | Out-Null; "
                "exit $code";

            return JoinArguments({ "-NoProfile", "-ExecutionPolicy", "Bypass", "-Command", command });
        }

        /// <summary>
        ///     ps1 を出力を受けて動かす引数を作る。入力を待たせないよう -NonInteractive を付ける。
        /// </summary>
        std::string BuildPowerShellCaptureArguments(const std::string& scriptPath, const std::vector<std::string>& args)
        {
            std::vector<std::string> all = { "-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-File", scriptPath };
            all.insert(all.end(), args.begin(), args.end());
            return JoinArguments(all);
        }

        /// <summary>
        ///     パスのフォルダ部分を返す。
        /// </summary>
        std::string DirectoryOf(const std::string& path)
        {
            std::size_t separator = path.find_last_of("\\/");
            return separator == std::string::npos ? std::string() : path.substr(0, separator);
        }
    }

    std::string LaunchCommand::FullCommandLine() const
    {
        std::string commandLine = QuotePath(Application);
        if (!Arguments.empty()) { commandLine += " " + Arguments; }

        return commandLine;
    }

    std::string QuoteArgument(const std::string& argument)
    {
        if (!argument.empty() && argument.find_first_of(" \t\n\v\"") == std::string::npos)
        {
            return argument;
        }

        std::string quoted = "\"";
        std::size_t backslashes = 0;
        for (char c : argument)
        {
            if (c == '\\')
            {
                ++backslashes;
                continue;
            }

            if (c == '"')
            {
                // 引用符の前の \ は2倍にし、引用符そのものも \ で逃がす。
                quoted.append(backslashes * 2 + 1, '\\');
            }
            else
            {
                quoted.append(backslashes, '\\');
            }

            quoted.push_back(c);
            backslashes = 0;
        }

        // 閉じの引用符の前の \ は2倍にする。
        quoted.append(backslashes * 2, '\\');
        quoted.push_back('"');
        return quoted;
    }

    std::string JoinArguments(const std::vector<std::string>& arguments)
    {
        std::string joined;
        for (const std::string& argument : arguments)
        {
            if (!joined.empty()) { joined.push_back(' '); }
            joined += QuoteArgument(argument);
        }

        return joined;
    }

    std::string QuotePowerShellLiteral(const std::string& text)
    {
        std::string quoted = "'";
        for (char c : text)
        {
            if (c == '\'') { quoted.push_back('\''); }
            quoted.push_back(c);
        }

        quoted.push_back('\'');
        return quoted;
    }

    std::string ToWindowsPath(const std::string& repositoryRoot, const std::string& relativePath)
    {
        std::string path = repositoryRoot;
        if (!path.empty() && path.back() != '\\' && path.back() != '/') { path.push_back('\\'); }

        for (char c : relativePath) { path.push_back(c == '/' ? '\\' : c); }

        return path;
    }

    LaunchCommand BuildLaunchCommand(
        const ScriptEntry& entry,
        std::size_t presetIndex,
        const std::string& repositoryRoot,
        const ToolPaths& tools)
    {
        static const std::vector<std::string> NO_ARGS;
        const std::vector<std::string>& args = presetIndex < entry.Presets.size() ? entry.Presets[presetIndex].Args : NO_ARGS;
        std::string scriptPath = ToWindowsPath(repositoryRoot, entry.WindowsPath);

        LaunchCommand command;
        command.WorkingDirectory = DirectoryOf(scriptPath);
        if (IsBatchFile(entry.WindowsPath))
        {
            command.Application = tools.CmdExe;
            command.Arguments = BuildBatchArguments(scriptPath, args);
        }
        else if (IsPowerShellFile(entry.WindowsPath))
        {
            command.Application = tools.PowerShellExe;
            command.Arguments = entry.ModeFor(presetIndex) == RunMode::Console
                ? BuildPowerShellConsoleArguments(scriptPath, args)
                : BuildPowerShellCaptureArguments(scriptPath, args);
        }
        else
        {
            command.Application = scriptPath;
            command.Arguments = JoinArguments(args);
        }

        return command;
    }
}
