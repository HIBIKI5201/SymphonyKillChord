// mac・Linux でのプロセス起動・入出力。
#include "Platform.h"

#include <cerrno>
#include <csignal>
#include <cstdlib>
#include <cstring>
#include <iostream>
#include <sstream>

#include <sys/wait.h>
#include <unistd.h>

#if defined(__APPLE__)
#include <mach-o/dyld.h>
#endif

namespace skc::conductor::cli
{
    namespace
    {
        /// <summary>
        ///     子プロセスの終了の状態を終了コードにする（シグナルで終わったら 128 + シグナル番号）。
        /// </summary>
        int ToExitCode(int status)
        {
            if (WIFEXITED(status)) { return WEXITSTATUS(status); }
            if (WIFSIGNALED(status)) { return 128 + WTERMSIG(status); }

            return 1;
        }

        /// <summary>
        ///     子プロセスが終わるのを待つ。待つ間は Ctrl+C を子に任せる。
        /// </summary>
        int WaitChild(pid_t child)
        {
            struct sigaction ignore{};
            ignore.sa_handler = SIG_IGN;
            struct sigaction previous{};
            sigaction(SIGINT, &ignore, &previous);

            int status = 0;
            while (waitpid(child, &status, 0) < 0 && errno == EINTR) {}

            sigaction(SIGINT, &previous, nullptr);
            return ToExitCode(status);
        }

        /// <summary>
        ///     文字列の配列を execv に渡せる形にする。
        /// </summary>
        std::vector<char*> ToArgv(std::vector<std::string>& args)
        {
            std::vector<char*> argv;
            for (std::string& arg : args) { argv.push_back(arg.data()); }

            argv.push_back(nullptr);
            return argv;
        }
    }

    std::string PathToUtf8(const std::filesystem::path& path)
    {
        return path.string();
    }

    std::filesystem::path Utf8ToPath(const std::string& text)
    {
        return std::filesystem::path(text);
    }

    std::filesystem::path ExecutableDirectory()
    {
#if defined(__APPLE__)
        uint32_t size = 0;
        _NSGetExecutablePath(nullptr, &size);
        std::string buffer(size, '\0');
        if (_NSGetExecutablePath(buffer.data(), &size) == 0)
        {
            std::error_code error;
            std::filesystem::path path = std::filesystem::canonical(buffer.c_str(), error);
            if (!error) { return path.parent_path(); }
        }
#else
        std::error_code error;
        std::filesystem::path path = std::filesystem::read_symlink("/proc/self/exe", error);
        if (!error) { return path.parent_path(); }
#endif
        return std::filesystem::current_path();
    }

    std::filesystem::path SettingsFilePath()
    {
        if (const char* overridePath = std::getenv("KILLCHORD_CONDUCTOR_SETTINGS"); overridePath != nullptr && *overridePath != '\0')
        {
            return std::filesystem::path(overridePath);
        }

        if (const char* config = std::getenv("XDG_CONFIG_HOME"); config != nullptr && *config != '\0')
        {
            return std::filesystem::path(config) / "killchord-conductor" / "settings.json";
        }

        const char* home = std::getenv("HOME");
        if (home == nullptr || *home == '\0') { return std::filesystem::path(); }

        return std::filesystem::path(home) / ".config" / "killchord-conductor" / "settings.json";
    }

    std::optional<std::string> FindOnPath(const std::string& name)
    {
        const char* path = std::getenv("PATH");
        if (path == nullptr) { return std::nullopt; }

        std::istringstream stream(path);
        std::string directory;
        while (std::getline(stream, directory, ':'))
        {
            if (directory.empty()) { continue; }

            std::filesystem::path candidate = std::filesystem::path(directory) / name;
            if (access(candidate.c_str(), X_OK) == 0) { return candidate.string(); }
        }

        return std::nullopt;
    }

    CaptureResult RunAndCapture(const std::vector<std::string>& argv, const std::filesystem::path& workingDirectory)
    {
        CaptureResult result;
        if (argv.empty()) { return result; }

        std::optional<std::string> program = FindOnPath(argv[0]);
        if (!program.has_value())
        {
            result.Error = argv[0] + " が見つかりません（PATH にありません）。";
            return result;
        }

        int pipeEnds[2];
        if (pipe(pipeEnds) != 0)
        {
            result.Error = std::strerror(errno);
            return result;
        }

        std::vector<std::string> args = argv;
        args[0] = *program;
        pid_t child = fork();
        if (child < 0)
        {
            close(pipeEnds[0]);
            close(pipeEnds[1]);
            result.Error = std::strerror(errno);
            return result;
        }

        if (child == 0)
        {
            dup2(pipeEnds[1], STDOUT_FILENO);
            close(pipeEnds[0]);
            close(pipeEnds[1]);
            FILE* devNull = std::freopen("/dev/null", "w", stderr);
            (void)devNull;
            if (!workingDirectory.empty() && chdir(workingDirectory.c_str()) != 0) { _exit(127); }

            std::vector<char*> childArgv = ToArgv(args);
            execv(childArgv[0], childArgv.data());
            _exit(127);
        }

        close(pipeEnds[1]);
        char buffer[4096];
        ssize_t read = 0;
        while ((read = ::read(pipeEnds[0], buffer, sizeof(buffer))) > 0 || (read < 0 && errno == EINTR))
        {
            if (read > 0) { result.Output.append(buffer, static_cast<std::size_t>(read)); }
        }

        close(pipeEnds[0]);
        int status = 0;
        while (waitpid(child, &status, 0) < 0 && errno == EINTR) {}

        result.IsStarted = true;
        result.ExitCode = ToExitCode(status);
        return result;
    }

    std::string ScriptPathForThisOs(const ScriptEntry& entry)
    {
        return entry.MacPath;
    }

    ScriptRunResult RunScript(const ScriptEntry& entry, std::size_t presetIndex,
        const std::filesystem::path& repositoryRoot, RunMode mode, bool holdWindow)
    {
        (void)mode;
        (void)holdWindow;
        ScriptRunResult result;
        if (entry.IsElevated)
        {
            result.Error = "管理者で動かすスクリプトは、Windows でだけ動かせます。";
            return result;
        }

        if (entry.MacPath.empty())
        {
            result.Error = "このスクリプトには mac 版（mac の欄）がありません。";
            return result;
        }

        std::filesystem::path script = repositoryRoot / entry.MacPath;
        std::vector<std::string> args = { "/bin/bash", script.string() };
        if (presetIndex < entry.Presets.size())
        {
            const std::vector<std::string>& presetArgs = entry.Presets[presetIndex].Args;
            args.insert(args.end(), presetArgs.begin(), presetArgs.end());
        }

        pid_t child = fork();
        if (child < 0)
        {
            result.Error = std::strerror(errno);
            return result;
        }

        if (child == 0)
        {
            if (chdir(script.parent_path().c_str()) != 0) { _exit(127); }

            std::vector<char*> childArgv = ToArgv(args);
            execv(childArgv[0], childArgv.data());
            _exit(127);
        }

        result.IsStarted = true;
        result.ExitCode = WaitChild(child);
        return result;
    }

    bool LaunchDetached(const std::string& program, const std::vector<std::string>& args,
        const std::filesystem::path& workingDirectory, std::string& error)
    {
        std::vector<std::string> all = { program };
        all.insert(all.end(), args.begin(), args.end());
        pid_t child = fork();
        if (child < 0)
        {
            error = std::strerror(errno);
            return false;
        }

        if (child == 0)
        {
            // 孫を起動して子はすぐ終わる（起動したプログラムが、こちらの終わりに巻き込まれないように）。
            setsid();
            if (fork() != 0) { _exit(0); }
            if (!workingDirectory.empty() && chdir(workingDirectory.c_str()) != 0) { _exit(127); }

            std::vector<char*> argv = ToArgv(all);
            execv(argv[0], argv.data());
            _exit(127);
        }

        int status = 0;
        while (waitpid(child, &status, 0) < 0 && errno == EINTR) {}
        return true;
    }

    bool OpenWithShell(const std::string& target, std::string& error)
    {
#if defined(__APPLE__)
        const char* opener = "/usr/bin/open";
#else
        std::optional<std::string> found = FindOnPath("xdg-open");
        if (!found.has_value())
        {
            error = "xdg-open が見つかりません。";
            return false;
        }

        std::string openerPath = *found;
        const char* opener = openerPath.c_str();
#endif
        return LaunchDetached(opener, { target }, std::filesystem::path(), error);
    }

    std::vector<std::filesystem::path> UnityEditorCandidates(const std::string& version)
    {
#if defined(__APPLE__)
        return { std::filesystem::path("/Applications/Unity/Hub/Editor") / version / "Unity.app" / "Contents" / "MacOS" / "Unity" };
#else
        const char* home = std::getenv("HOME");
        if (home == nullptr) { return {}; }

        return { std::filesystem::path(home) / "Unity" / "Hub" / "Editor" / version / "Editor" / "Unity" };
#endif
    }

    std::optional<std::filesystem::path> FindCriAtomCraft()
    {
        return std::nullopt;
    }

    std::vector<std::filesystem::path> DriveFolderCandidates()
    {
        std::vector<std::filesystem::path> candidates;
        const char* home = std::getenv("HOME");
        if (home == nullptr) { return candidates; }

        std::error_code error;
        std::filesystem::path cloud = std::filesystem::path(home) / "Library" / "CloudStorage";
        for (std::filesystem::directory_iterator it(cloud, error), end; !error && it != end; it.increment(error))
        {
            if (it->path().filename().string().rfind("GoogleDrive", 0) != 0) { continue; }

            candidates.push_back(it->path() / "マイドライブ" / "SymphonyKillChord");
            candidates.push_back(it->path() / "My Drive" / "SymphonyKillChord");
        }

        return candidates;
    }

    void WriteOut(const std::string& text)
    {
        std::cout << text << std::flush;
    }

    void WriteError(const std::string& text)
    {
        std::cerr << text << std::flush;
    }

    bool ReadLine(std::string& line)
    {
        if (!std::getline(std::cin, line)) { return false; }

        if (!line.empty() && line.back() == '\r') { line.pop_back(); }

        return true;
    }
}
