#include "PipeReader.h"

namespace skc::conductor::win32
{
    namespace
    {
        /// <summary>子が終わったかを確かめる間隔（ミリ秒）。</summary>
        constexpr DWORD POLL_INTERVAL_MS = 20;

        /// <summary>1回に読むバイト数。</summary>
        constexpr DWORD READ_BUFFER_SIZE = 4096;

        /// <summary>
        ///     パイプに今ある分を読む。
        /// </summary>
        /// <returns>パイプが閉じたら false。</returns>
        bool DrainAvailable(const PipeTarget& target)
        {
            char data[READ_BUFFER_SIZE];
            while (true)
            {
                DWORD available = 0;
                if (!PeekNamedPipe(target.Pipe, nullptr, 0, nullptr, &available, nullptr)) { return false; }
                if (available == 0) { return true; }

                DWORD read = 0;
                DWORD wanted = available < READ_BUFFER_SIZE ? available : READ_BUFFER_SIZE;
                if (!ReadFile(target.Pipe, data, wanted, &read, nullptr) || read == 0) { return false; }

                target.Output->append(data, read);
            }
        }
    }

    bool ReadPipesUntilExit(HANDLE process, const std::vector<PipeTarget>& pipes, DWORD timeoutMs)
    {
        std::vector<bool> isOpen(pipes.size(), true);
        ULONGLONG started = GetTickCount64();
        while (true)
        {
            bool anyOpen = false;
            for (std::size_t i = 0; i < pipes.size(); ++i)
            {
                if (isOpen[i]) { isOpen[i] = DrainAvailable(pipes[i]); }
                anyOpen = anyOpen || isOpen[i];
            }

            DWORD wait = anyOpen ? POLL_INTERVAL_MS : INFINITE;
            if (timeoutMs != INFINITE)
            {
                ULONGLONG elapsed = GetTickCount64() - started;
                if (elapsed >= timeoutMs) { return WaitForSingleObject(process, 0) == WAIT_OBJECT_0; }

                ULONGLONG left = timeoutMs - elapsed;
                if (wait == INFINITE || left < wait) { wait = static_cast<DWORD>(left); }
            }

            if (WaitForSingleObject(process, wait) == WAIT_OBJECT_0)
            {
                // 終わったあとにパイプに残った分を読み切る。孫が書く側を持っていても、ここで抜ける。
                for (std::size_t i = 0; i < pipes.size(); ++i)
                {
                    if (isOpen[i]) { DrainAvailable(pipes[i]); }
                }

                return true;
            }
        }
    }
}
