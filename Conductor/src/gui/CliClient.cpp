#include "CliClient.h"

#include "PipeReader.h"
#include "ProcessTree.h"
#include "TextConvert.h"

namespace skc::conductor::gui
{
    namespace
    {
        using win32::Utf8ToWide;
        using win32::WideToUtf8;

        /// <summary>conductor が終わるのを待つ長さ（ミリ秒）。Unity のメニューを動かすものでも足りる長さにする。</summary>
        constexpr DWORD RUN_TIMEOUT_MS = 180000;

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
    }

    CliClient::CliClient(std::filesystem::path conductorPath, std::filesystem::path repositoryRoot)
        : _conductorPath(std::move(conductorPath)), _repositoryRoot(std::move(repositoryRoot))
    {
    }

    bool CliClient::Exists() const
    {
        std::error_code error;
        return std::filesystem::is_regular_file(_conductorPath, error);
    }

    LaunchCommand CliClient::BuildCommand(const std::vector<std::string>& args) const
    {
        std::vector<std::string> all = args;
        if (!_repositoryRoot.empty())
        {
            all.push_back("--root");
            all.push_back(WideToUtf8(_repositoryRoot.wstring()));
        }

        LaunchCommand command;
        command.Application = WideToUtf8(_conductorPath.wstring());
        command.Arguments = JoinArguments(all);
        command.WorkingDirectory = WideToUtf8(_repositoryRoot.wstring());
        return command;
    }

    CliOutput CliClient::Run(const std::vector<std::string>& args) const
    {
        CliOutput result;
        LaunchCommand command = BuildCommand(args);

        SECURITY_ATTRIBUTES inheritable{ sizeof(SECURITY_ATTRIBUTES), nullptr, TRUE };
        HANDLE outRead = nullptr;
        HANDLE outWrite = nullptr;
        HANDLE errRead = nullptr;
        HANDLE errWrite = nullptr;
        if (!CreatePipe(&outRead, &outWrite, &inheritable, 0) || !CreatePipe(&errRead, &errWrite, &inheritable, 0))
        {
            result.Error = DescribeError(GetLastError());
            for (HANDLE handle : { outRead, outWrite, errRead, errWrite }) { if (handle != nullptr) { CloseHandle(handle); } }
            return result;
        }

        SetHandleInformation(outRead, HANDLE_FLAG_INHERIT, 0);
        SetHandleInformation(errRead, HANDLE_FLAG_INHERIT, 0);
        HANDLE nullInput = CreateFileW(L"NUL", GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, &inheritable, OPEN_EXISTING, 0, nullptr);

        // 子へ渡すハンドルを、この3つだけに絞る（ほかのパイプを持たせると、終わりが来なくなる）。
        HANDLE inherited[] = { outWrite, errWrite, nullInput };
        SIZE_T attributeSize = 0;
        InitializeProcThreadAttributeList(nullptr, 1, 0, &attributeSize);
        std::vector<unsigned char> attributeBuffer(attributeSize);
        auto* attributes = reinterpret_cast<LPPROC_THREAD_ATTRIBUTE_LIST>(attributeBuffer.data());
        bool hasAttributes = InitializeProcThreadAttributeList(attributes, 1, 0, &attributeSize) &&
            UpdateProcThreadAttribute(attributes, 0, PROC_THREAD_ATTRIBUTE_HANDLE_LIST, inherited, sizeof(inherited), nullptr, nullptr);

        STARTUPINFOEXW startup{};
        startup.StartupInfo.cb = sizeof(startup);
        startup.StartupInfo.dwFlags = STARTF_USESTDHANDLES;
        startup.StartupInfo.hStdInput = nullInput;
        startup.StartupInfo.hStdOutput = outWrite;
        startup.StartupInfo.hStdError = errWrite;
        startup.lpAttributeList = hasAttributes ? attributes : nullptr;

        std::wstring commandLine = Utf8ToWide(command.FullCommandLine());
        std::wstring application = Utf8ToWide(command.Application);
        std::wstring directory = Utf8ToWide(command.WorkingDirectory);
        PROCESS_INFORMATION process{};
        BOOL isCreated = CreateProcessW(application.c_str(), commandLine.data(), nullptr, nullptr, TRUE,
            CREATE_NO_WINDOW | CREATE_SUSPENDED | CREATE_UNICODE_ENVIRONMENT | (hasAttributes ? EXTENDED_STARTUPINFO_PRESENT : 0), nullptr,
            directory.empty() ? nullptr : directory.c_str(), &startup.StartupInfo, &process);
        DWORD createError = GetLastError();
        if (hasAttributes) { DeleteProcThreadAttributeList(attributes); }

        CloseHandle(outWrite);
        CloseHandle(errWrite);
        if (nullInput != INVALID_HANDLE_VALUE) { CloseHandle(nullInput); }

        if (!isCreated)
        {
            CloseHandle(outRead);
            CloseHandle(errRead);
            result.Error = "conductor.exe を起動できませんでした: " + DescribeError(createError);
            return result;
        }

        // conductor と、それが起こした git などを、まとめて止められるようにしてから動かす。
        win32::ProcessTree tree;
        tree.Assign(process.hProcess);
        ResumeThread(process.hThread);
        CloseHandle(process.hThread);

        // パイプの終わりは待たず、conductor が終わるまで読む（孫が書く側を持ったままでも止まらない）。終わらなければ孫ごと止める。
        std::string errorText;
        bool isExited = win32::ReadPipesUntilExit(process.hProcess, { { outRead, &result.Output }, { errRead, &errorText } }, RUN_TIMEOUT_MS);
        CloseHandle(outRead);
        CloseHandle(errRead);
        if (!isExited)
        {
            tree.Terminate(process.hProcess);
            CloseHandle(process.hProcess);
            result.Error = "conductor.exe が " + std::to_string(RUN_TIMEOUT_MS / 1000) + " 秒たっても終わらないので止めました。";
            return result;
        }

        GetExitCodeProcess(process.hProcess, &result.ExitCode);
        CloseHandle(process.hProcess);
        result.IsStarted = true;
        result.Error = errorText;
        return result;
    }
}
