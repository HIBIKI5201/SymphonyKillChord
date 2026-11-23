// Windows でのプロセス起動・入出力。
#include "Platform.h"

#include "CommandLine.h"
#include "TextConvert.h"

#include <windows.h>
#include <shellapi.h>

#include <cstdlib>

namespace skc::conductor::cli
{
    namespace
    {
        using win32::Utf8ToWide;
        using win32::WideToUtf8;

        /// <summary>1回に読むバイト数。</summary>
        constexpr DWORD READ_BUFFER_SIZE = 4096;

        /// <summary>
        ///     Windows のエラー番号を文にする。
        /// </summary>
        std::string DescribeError(DWORD error)
        {
            wchar_t* buffer = nullptr;
            DWORD length = FormatMessageW(
                FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
                nullptr, error, 0, reinterpret_cast<wchar_t*>(&buffer), 0, nullptr);
            std::wstring message = length > 0 ? std::wstring(buffer, length) : std::wstring();
            if (buffer != nullptr) { LocalFree(buffer); }

            while (!message.empty() && (message.back() == L'\r' || message.back() == L'\n')) { message.pop_back(); }

            return WideToUtf8(message) + "（エラー " + std::to_string(error) + "）";
        }

        /// <summary>
        ///     System32 の cmd.exe・powershell.exe の場所を返す（PATH を書き換えられていても System32 のものを使う）。
        /// </summary>
        ToolPaths SystemToolPaths()
        {
            std::wstring system(MAX_PATH, L'\0');
            UINT length = GetSystemDirectoryW(system.data(), static_cast<UINT>(system.size()));
            system.resize(length);

            ToolPaths tools;
            tools.CmdExe = WideToUtf8(system + L"\\cmd.exe");
            tools.PowerShellExe = WideToUtf8(system + L"\\WindowsPowerShell\\v1.0\\powershell.exe");
            return tools;
        }

        /// <summary>
        ///     標準のハンドルが、コンソールではなくパイプ・ファイルにつながっているかを返す。
        /// </summary>
        bool IsRedirected(HANDLE handle)
        {
            if (handle == nullptr || handle == INVALID_HANDLE_VALUE) { return false; }

            return GetFileType(handle) != FILE_TYPE_CHAR;
        }

        /// <summary>
        ///     標準のハンドルへ書く。コンソールなら UTF-16 で、そうでなければ UTF-8 のまま書く。
        /// </summary>
        void WriteTo(DWORD which, const std::string& text)
        {
            HANDLE handle = GetStdHandle(which);
            if (handle == nullptr || handle == INVALID_HANDLE_VALUE || text.empty()) { return; }

            DWORD written = 0;
            if (!IsRedirected(handle))
            {
                std::wstring wide = Utf8ToWide(text);
                WriteConsoleW(handle, wide.data(), static_cast<DWORD>(wide.size()), &written, nullptr);
                return;
            }

            WriteFile(handle, text.data(), static_cast<DWORD>(text.size()), &written, nullptr);
        }

        /// <summary>
        ///     Ctrl+C を無視する（子のスクリプトに任せ、こちらは終わるのを待つ）。
        /// </summary>
        BOOL WINAPI IgnoreControlEvent(DWORD)
        {
            return TRUE;
        }

        /// <summary>
        ///     プロセスが終わるのを待ち、終了コードを返す。
        /// </summary>
        int WaitExitCode(HANDLE process)
        {
            WaitForSingleObject(process, INFINITE);
            DWORD exitCode = 0;
            GetExitCodeProcess(process, &exitCode);
            CloseHandle(process);
            return static_cast<int>(exitCode);
        }
    }

    std::string PathToUtf8(const std::filesystem::path& path)
    {
        return WideToUtf8(path.wstring());
    }

    std::filesystem::path Utf8ToPath(const std::string& text)
    {
        return std::filesystem::path(Utf8ToWide(text));
    }

    std::filesystem::path ExecutableDirectory()
    {
        std::wstring buffer(MAX_PATH, L'\0');
        while (true)
        {
            DWORD length = GetModuleFileNameW(nullptr, buffer.data(), static_cast<DWORD>(buffer.size()));
            if (length == 0) { return std::filesystem::current_path(); }
            if (length < buffer.size())
            {
                buffer.resize(length);
                return std::filesystem::path(buffer).parent_path();
            }

            buffer.resize(buffer.size() * 2);
        }
    }

    std::filesystem::path SettingsFilePath()
    {
        wchar_t buffer[32768];
        DWORD length = GetEnvironmentVariableW(L"KILLCHORD_CONDUCTOR_SETTINGS", buffer, 32768);
        if (length > 0 && length < 32768) { return std::filesystem::path(std::wstring(buffer, length)); }

        length = GetEnvironmentVariableW(L"LOCALAPPDATA", buffer, 32768);
        if (length == 0 || length >= 32768) { return std::filesystem::path(); }

        return std::filesystem::path(std::wstring(buffer, length)) / L"KillChordConductor" / L"settings.json";
    }

    std::optional<std::string> FindOnPath(const std::string& name)
    {
        std::wstring wideName = Utf8ToWide(name);
        for (const wchar_t* extension : { L".exe", L".cmd", L".bat" })
        {
            wchar_t found[MAX_PATH];
            DWORD length = SearchPathW(nullptr, wideName.c_str(), extension, MAX_PATH, found, nullptr);
            if (length > 0 && length < MAX_PATH) { return WideToUtf8(std::wstring(found, length)); }
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

        SECURITY_ATTRIBUTES inheritable{ sizeof(SECURITY_ATTRIBUTES), nullptr, TRUE };
        HANDLE readPipe = nullptr;
        HANDLE writePipe = nullptr;
        if (!CreatePipe(&readPipe, &writePipe, &inheritable, 0))
        {
            result.Error = DescribeError(GetLastError());
            return result;
        }

        SetHandleInformation(readPipe, HANDLE_FLAG_INHERIT, 0);
        HANDLE nullDevice = CreateFileW(L"NUL", GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, &inheritable, OPEN_EXISTING, 0, nullptr);

        STARTUPINFOW startup{};
        startup.cb = sizeof(startup);
        startup.dwFlags = STARTF_USESTDHANDLES;
        startup.hStdInput = nullDevice;
        startup.hStdOutput = writePipe;
        startup.hStdError = nullDevice;

        std::vector<std::string> arguments(argv.begin() + 1, argv.end());
        std::wstring application = Utf8ToWide(*program);
        std::wstring commandLine = L"\"" + application + L"\"";
        if (!arguments.empty()) { commandLine += L" " + Utf8ToWide(JoinArguments(arguments)); }

        std::wstring directory = workingDirectory.wstring();
        PROCESS_INFORMATION process{};
        BOOL isCreated = CreateProcessW(application.c_str(), commandLine.data(), nullptr, nullptr, TRUE,
            CREATE_NO_WINDOW | CREATE_UNICODE_ENVIRONMENT, nullptr, directory.empty() ? nullptr : directory.c_str(), &startup, &process);
        DWORD createError = GetLastError();
        CloseHandle(writePipe);
        if (nullDevice != INVALID_HANDLE_VALUE) { CloseHandle(nullDevice); }

        if (!isCreated)
        {
            CloseHandle(readPipe);
            result.Error = DescribeError(createError);
            return result;
        }

        CloseHandle(process.hThread);
        char data[READ_BUFFER_SIZE];
        DWORD read = 0;
        while (ReadFile(readPipe, data, READ_BUFFER_SIZE, &read, nullptr) && read > 0)
        {
            result.Output.append(data, read);
        }

        CloseHandle(readPipe);
        result.IsStarted = true;
        result.ExitCode = WaitExitCode(process.hProcess);
        return result;
    }

    std::string ScriptPathForThisOs(const ScriptEntry& entry)
    {
        return entry.WindowsPath;
    }

    ScriptRunResult RunScript(const ScriptEntry& entry, std::size_t presetIndex,
        const std::filesystem::path& repositoryRoot, RunMode mode, bool holdWindow)
    {
        ScriptRunResult result;
        LaunchCommand command = BuildLaunchCommand(entry, presetIndex, PathToUtf8(repositoryRoot), SystemToolPaths(), mode, holdWindow);
        std::wstring application = Utf8ToWide(command.Application);
        std::wstring directory = Utf8ToWide(command.WorkingDirectory);

        if (entry.IsElevated)
        {
            // 管理者で動かすものは UAC を通すため、新しいウィンドウで開き、終わるのを待つ。
            std::wstring arguments = Utf8ToWide(command.Arguments);
            SHELLEXECUTEINFOW info{};
            info.cbSize = sizeof(info);
            info.fMask = SEE_MASK_NOCLOSEPROCESS | SEE_MASK_NOASYNC;
            info.lpVerb = L"runas";
            info.lpFile = application.c_str();
            info.lpParameters = arguments.empty() ? nullptr : arguments.c_str();
            info.lpDirectory = directory.empty() ? nullptr : directory.c_str();
            info.nShow = SW_SHOWNORMAL;
            if (!ShellExecuteExW(&info))
            {
                DWORD error = GetLastError();
                result.IsCancelled = error == ERROR_CANCELLED;
                result.Error = result.IsCancelled ? "UAC の確認で取り消しました。" : DescribeError(error);
                return result;
            }

            result.IsStarted = true;
            SetConsoleCtrlHandler(IgnoreControlEvent, TRUE);
            result.ExitCode = info.hProcess != nullptr ? WaitExitCode(info.hProcess) : 0;
            return result;
        }

        // 標準入出力がパイプ（GUI が出力を受けているとき）なら、そのパイプを子へ渡す。
        // コンソールなら何も指定せず、子は同じコンソールを使う。
        STARTUPINFOW startup{};
        startup.cb = sizeof(startup);
        HANDLE input = GetStdHandle(STD_INPUT_HANDLE);
        HANDLE output = GetStdHandle(STD_OUTPUT_HANDLE);
        HANDLE error = GetStdHandle(STD_ERROR_HANDLE);
        if (IsRedirected(input) || IsRedirected(output) || IsRedirected(error))
        {
            for (HANDLE handle : { input, output, error })
            {
                if (handle != nullptr && handle != INVALID_HANDLE_VALUE) { SetHandleInformation(handle, HANDLE_FLAG_INHERIT, HANDLE_FLAG_INHERIT); }
            }

            startup.dwFlags = STARTF_USESTDHANDLES;
            startup.hStdInput = input;
            startup.hStdOutput = output;
            startup.hStdError = error;
        }

        std::wstring commandLine = Utf8ToWide(command.FullCommandLine());
        PROCESS_INFORMATION process{};
        if (!CreateProcessW(application.c_str(), commandLine.data(), nullptr, nullptr, TRUE, CREATE_UNICODE_ENVIRONMENT, nullptr,
                directory.empty() ? nullptr : directory.c_str(), &startup, &process))
        {
            result.Error = DescribeError(GetLastError());
            return result;
        }

        CloseHandle(process.hThread);
        result.IsStarted = true;
        SetConsoleCtrlHandler(IgnoreControlEvent, TRUE);
        result.ExitCode = WaitExitCode(process.hProcess);
        return result;
    }

    void WriteOut(const std::string& text)
    {
        WriteTo(STD_OUTPUT_HANDLE, text);
    }

    void WriteError(const std::string& text)
    {
        WriteTo(STD_ERROR_HANDLE, text);
    }

    bool ReadLine(std::string& line)
    {
        line.clear();
        HANDLE input = GetStdHandle(STD_INPUT_HANDLE);
        if (input == nullptr || input == INVALID_HANDLE_VALUE) { return false; }

        if (!IsRedirected(input))
        {
            wchar_t buffer[1024];
            DWORD read = 0;
            if (!ReadConsoleW(input, buffer, 1024, &read, nullptr) || read == 0) { return false; }

            std::wstring wide(buffer, read);
            while (!wide.empty() && (wide.back() == L'\r' || wide.back() == L'\n')) { wide.pop_back(); }

            line = WideToUtf8(wide);
            return true;
        }

        char c = 0;
        DWORD read = 0;
        bool isAny = false;
        while (ReadFile(input, &c, 1, &read, nullptr) && read == 1)
        {
            isAny = true;
            if (c == '\n') { break; }
            if (c != '\r') { line.push_back(c); }
        }

        return isAny;
    }
}
