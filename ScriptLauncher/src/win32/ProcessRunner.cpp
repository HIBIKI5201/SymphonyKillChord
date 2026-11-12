#include "ProcessRunner.h"

#include "OutputText.h"
#include "TextConvert.h"

#include <shellapi.h>

#include <memory>
#include <thread>
#include <vector>

namespace skc::launcher::win32
{
    namespace
    {
        /// <summary>出力の読み取りでプロセスの終了を確かめる間隔（ミリ秒）。</summary>
        constexpr DWORD POLL_INTERVAL_MS = 50;

        /// <summary>1回に読むバイト数。</summary>
        constexpr DWORD READ_BUFFER_SIZE = 4096;

        /// <summary>
        ///     Windows のエラー番号を文にする。
        /// </summary>
        std::wstring DescribeError(DWORD error)
        {
            wchar_t* buffer = nullptr;
            DWORD length = FormatMessageW(
                FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
                nullptr, error, 0, reinterpret_cast<wchar_t*>(&buffer), 0, nullptr);
            std::wstring message = length > 0 ? std::wstring(buffer, length) : std::wstring();
            if (buffer != nullptr) { LocalFree(buffer); }

            while (!message.empty() && (message.back() == L'\r' || message.back() == L'\n')) { message.pop_back(); }

            return message + L"（エラー " + std::to_wstring(error) + L"）";
        }

        /// <summary>
        ///     ウィンドウへ1行を送る。送れなければ（ウィンドウが閉じたあとなど）捨てる。
        /// </summary>
        void PostLine(HWND window, WPARAM runId, std::wstring line)
        {
            auto* payload = new std::wstring(std::move(line));
            if (!PostMessageW(window, WM_APP_RUN_OUTPUT, runId, reinterpret_cast<LPARAM>(payload)))
            {
                delete payload;
            }
        }

        /// <summary>
        ///     受け取ったバイト列を行に分け、整形してウィンドウへ送る。
        /// </summary>
        void PostLines(HWND window, WPARAM runId, const std::vector<std::string>& lines)
        {
            for (const std::string& line : lines)
            {
                PostLine(window, runId, DecodeOutputLine(StripAnsi(line)));
            }
        }

        /// <summary>
        ///     パイプに今ある分を読んで、行にして送る。
        /// </summary>
        /// <returns>パイプが閉じたら false。</returns>
        bool DrainPipe(HANDLE pipe, OutputLineBuffer& buffer, HWND window, WPARAM runId)
        {
            char data[READ_BUFFER_SIZE];
            while (true)
            {
                DWORD available = 0;
                if (!PeekNamedPipe(pipe, nullptr, 0, nullptr, &available, nullptr)) { return false; }
                if (available == 0) { return true; }

                DWORD read = 0;
                if (!ReadFile(pipe, data, READ_BUFFER_SIZE, &read, nullptr) || read == 0) { return false; }

                PostLines(window, runId, buffer.Feed(std::string(data, read)));
            }
        }
    }

    ProcessRunner::ProcessRunner(HWND notifyWindow)
        : _notifyWindow(notifyWindow)
    {
    }

    ProcessRunner::~ProcessRunner()
    {
        if (_captureJob != nullptr)
        {
            // kill-on-close のジョブなので、閉じると出力を受けていたプロセスも止まる。
            CloseHandle(_captureJob);
            _captureJob = nullptr;
        }
    }

    StartResult ProcessRunner::Start(const LaunchCommand& command, RunMode mode, bool isElevated)
    {
        if (isElevated) { return StartElevated(command); }
        if (mode == RunMode::Capture) { return StartCapture(command); }

        return StartConsole(command);
    }

    void ProcessRunner::StopCapture()
    {
        if (_captureJob != nullptr) { TerminateJobObject(_captureJob, 1); }
    }

    void ProcessRunner::OnExited(WPARAM runId)
    {
        if (runId != _captureRunId) { return; }

        if (_captureJob != nullptr)
        {
            CloseHandle(_captureJob);
            _captureJob = nullptr;
        }

        _captureRunId = 0;
    }

    StartResult ProcessRunner::StartConsole(const LaunchCommand& command)
    {
        StartResult result;
        std::wstring commandLine = Utf8ToWide(command.FullCommandLine());
        std::wstring application = Utf8ToWide(command.Application);
        std::wstring directory = Utf8ToWide(command.WorkingDirectory);

        STARTUPINFOW startup{};
        startup.cb = sizeof(startup);
        PROCESS_INFORMATION process{};
        if (!CreateProcessW(application.c_str(), commandLine.data(), nullptr, nullptr, FALSE,
                CREATE_NEW_CONSOLE | CREATE_UNICODE_ENVIRONMENT, nullptr,
                directory.empty() ? nullptr : directory.c_str(), &startup, &process))
        {
            result.Error = DescribeError(GetLastError());
            return result;
        }

        CloseHandle(process.hThread);
        result.IsStarted = true;
        result.IsWatched = true;
        result.RunId = _nextRunId++;
        WatchExit(process.hProcess, result.RunId);
        return result;
    }

    StartResult ProcessRunner::StartElevated(const LaunchCommand& command)
    {
        StartResult result;
        std::wstring application = Utf8ToWide(command.Application);
        std::wstring arguments = Utf8ToWide(command.Arguments);
        std::wstring directory = Utf8ToWide(command.WorkingDirectory);

        SHELLEXECUTEINFOW info{};
        info.cbSize = sizeof(info);
        info.fMask = SEE_MASK_NOCLOSEPROCESS | SEE_MASK_NOASYNC | SEE_MASK_FLAG_NO_UI;
        info.hwnd = _notifyWindow;
        info.lpVerb = L"runas";
        info.lpFile = application.c_str();
        info.lpParameters = arguments.empty() ? nullptr : arguments.c_str();
        info.lpDirectory = directory.empty() ? nullptr : directory.c_str();
        info.nShow = SW_SHOWNORMAL;
        if (!ShellExecuteExW(&info))
        {
            DWORD error = GetLastError();
            result.IsCancelled = error == ERROR_CANCELLED;
            result.Error = result.IsCancelled ? L"UAC の確認で取り消しました。" : DescribeError(error);
            return result;
        }

        result.IsStarted = true;
        result.RunId = _nextRunId++;
        if (info.hProcess != nullptr)
        {
            result.IsWatched = true;
            WatchExit(info.hProcess, result.RunId);
        }

        return result;
    }

    StartResult ProcessRunner::StartCapture(const LaunchCommand& command)
    {
        StartResult result;
        if (IsCapturing())
        {
            result.Error = L"ほかのスクリプトの出力を受けています。終わるか「止める」を押してから動かしてください。";
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

        // 読む側はこちらだけが持つ。子へ渡すのは書く側と、入力を待たせないための NUL だけ。
        SetHandleInformation(readPipe, HANDLE_FLAG_INHERIT, 0);
        HANDLE nullInput = CreateFileW(L"NUL", GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, &inheritable, OPEN_EXISTING, 0, nullptr);

        auto closeAll = [&]()
        {
            if (readPipe != nullptr) { CloseHandle(readPipe); }
            if (writePipe != nullptr) { CloseHandle(writePipe); }
            if (nullInput != INVALID_HANDLE_VALUE) { CloseHandle(nullInput); }
        };

        if (nullInput == INVALID_HANDLE_VALUE)
        {
            result.Error = DescribeError(GetLastError());
            closeAll();
            return result;
        }

        HANDLE inherited[] = { writePipe, nullInput };
        SIZE_T attributeSize = 0;
        InitializeProcThreadAttributeList(nullptr, 1, 0, &attributeSize);
        std::vector<unsigned char> attributeBuffer(attributeSize);
        auto* attributes = reinterpret_cast<LPPROC_THREAD_ATTRIBUTE_LIST>(attributeBuffer.data());
        if (!InitializeProcThreadAttributeList(attributes, 1, 0, &attributeSize) ||
            !UpdateProcThreadAttribute(attributes, 0, PROC_THREAD_ATTRIBUTE_HANDLE_LIST, inherited, sizeof(inherited), nullptr, nullptr))
        {
            result.Error = DescribeError(GetLastError());
            closeAll();
            return result;
        }

        STARTUPINFOEXW startup{};
        startup.StartupInfo.cb = sizeof(startup);
        startup.StartupInfo.dwFlags = STARTF_USESTDHANDLES;
        startup.StartupInfo.hStdInput = nullInput;
        startup.StartupInfo.hStdOutput = writePipe;
        startup.StartupInfo.hStdError = writePipe;
        startup.lpAttributeList = attributes;

        std::wstring commandLine = Utf8ToWide(command.FullCommandLine());
        std::wstring application = Utf8ToWide(command.Application);
        std::wstring directory = Utf8ToWide(command.WorkingDirectory);
        PROCESS_INFORMATION process{};
        BOOL isCreated = CreateProcessW(application.c_str(), commandLine.data(), nullptr, nullptr, TRUE,
            CREATE_NO_WINDOW | CREATE_SUSPENDED | EXTENDED_STARTUPINFO_PRESENT | CREATE_UNICODE_ENVIRONMENT, nullptr,
            directory.empty() ? nullptr : directory.c_str(), &startup.StartupInfo, &process);
        DWORD createError = GetLastError();
        DeleteProcThreadAttributeList(attributes);

        // 子が受け取ったので、こちらの書く側と NUL は閉じる（閉じないとパイプの終わりが来ない）。
        CloseHandle(writePipe);
        writePipe = nullptr;
        CloseHandle(nullInput);
        nullInput = INVALID_HANDLE_VALUE;

        if (!isCreated)
        {
            result.Error = DescribeError(createError);
            closeAll();
            return result;
        }

        // 孫プロセス（cmd から呼ばれる PowerShell など）もまとめて止められるよう、動き出す前にジョブへ入れる。
        HANDLE job = CreateJobObjectW(nullptr, nullptr);
        if (job != nullptr)
        {
            JOBOBJECT_EXTENDED_LIMIT_INFORMATION limits{};
            limits.BasicLimitInformation.LimitFlags = JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;
            SetInformationJobObject(job, JobObjectExtendedLimitInformation, &limits, sizeof(limits));
            if (!AssignProcessToJobObject(job, process.hProcess))
            {
                CloseHandle(job);
                job = nullptr;
            }
        }

        ResumeThread(process.hThread);
        CloseHandle(process.hThread);

        result.IsStarted = true;
        result.IsWatched = true;
        result.RunId = _nextRunId++;
        _captureJob = job;
        _captureRunId = result.RunId;

        HWND window = _notifyWindow;
        WPARAM runId = result.RunId;
        HANDLE processHandle = process.hProcess;
        HANDLE pipe = readPipe;
        std::thread([window, runId, processHandle, pipe]()
        {
            OutputLineBuffer buffer;
            bool isOpen = true;
            while (isOpen)
            {
                isOpen = DrainPipe(pipe, buffer, window, runId);
                if (isOpen && WaitForSingleObject(processHandle, POLL_INTERVAL_MS) == WAIT_OBJECT_0)
                {
                    // 終わったあとにパイプに残った分を読み切る。孫が書く側を持ったままでも、ここで抜ける。
                    DrainPipe(pipe, buffer, window, runId);
                    break;
                }
            }

            std::string rest = buffer.Flush();
            if (!rest.empty()) { PostLine(window, runId, DecodeOutputLine(StripAnsi(rest))); }

            WaitForSingleObject(processHandle, INFINITE);
            DWORD exitCode = 0;
            GetExitCodeProcess(processHandle, &exitCode);
            CloseHandle(processHandle);
            CloseHandle(pipe);
            PostMessageW(window, WM_APP_RUN_EXITED, runId, static_cast<LPARAM>(exitCode));
        }).detach();

        return result;
    }

    void ProcessRunner::WatchExit(HANDLE process, WPARAM runId)
    {
        HWND window = _notifyWindow;
        std::thread([window, process, runId]()
        {
            WaitForSingleObject(process, INFINITE);
            DWORD exitCode = 0;
            GetExitCodeProcess(process, &exitCode);
            CloseHandle(process);
            PostMessageW(window, WM_APP_RUN_EXITED, runId, static_cast<LPARAM>(exitCode));
        }).detach();
    }
}
