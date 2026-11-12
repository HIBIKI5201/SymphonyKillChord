#pragma once

#include "CommandLine.h"
#include "ScriptEntry.h"

#include <windows.h>

#include <atomic>
#include <string>

namespace skc::launcher::win32
{
    /// <summary>出力の1行が届いた。wParam は実行番号、lParam は new した std::wstring*（受け取った側が delete する）。</summary>
    inline constexpr UINT WM_APP_RUN_OUTPUT = WM_APP + 1;

    /// <summary>プロセスが終わった。wParam は実行番号、lParam は終了コード（DWORD）。</summary>
    inline constexpr UINT WM_APP_RUN_EXITED = WM_APP + 2;

    /// <summary>
    ///     スクリプトを起動した結果。
    /// </summary>
    struct StartResult
    {
        /// <summary>起動できたか。</summary>
        bool IsStarted = false;

        /// <summary>終わるのを見張っているか（false なら WM_APP_RUN_EXITED は届かない）。</summary>
        bool IsWatched = false;

        /// <summary>利用者が UAC の確認で取り消したか。</summary>
        bool IsCancelled = false;

        /// <summary>実行番号。</summary>
        WPARAM RunId = 0;

        /// <summary>起動できなかった理由。</summary>
        std::wstring Error;
    };

    /// <summary>
    ///     スクリプトのプロセスを起動し、出力と終了をウィンドウへメッセージで知らせる。
    ///     コンソールで開くものは新しいコンソールで起動し、ツールを閉じても止めない（常駐するランナー・サーバーがあるため）。
    ///     出力を受けるものはジョブにまとめ、「止める」とツールの終了で子孫ごと止める。出力を受けるのは同時に1つだけ。
    /// </summary>
    class ProcessRunner
    {
    public:
        /// <summary>
        ///     起動役を生成する。
        /// </summary>
        /// <param name="notifyWindow">出力と終了を知らせるウィンドウ。</param>
        explicit ProcessRunner(HWND notifyWindow);

        /// <summary>
        ///     ジョブを閉じ、出力を受けているプロセスを止める。
        /// </summary>
        ~ProcessRunner();

        ProcessRunner(const ProcessRunner&) = delete;
        ProcessRunner& operator=(const ProcessRunner&) = delete;

        /// <summary>
        ///     スクリプトを起動する。
        /// </summary>
        /// <param name="command">起動するコマンド。</param>
        /// <param name="mode">動かし方。</param>
        /// <param name="isElevated">管理者として起動するか（UAC の確認が出る）。</param>
        /// <returns>起動の結果。</returns>
        StartResult Start(const LaunchCommand& command, RunMode mode, bool isElevated);

        /// <summary>
        ///     出力を受けているプロセスがあるかを返す。
        /// </summary>
        /// <returns>あれば true。</returns>
        bool IsCapturing() const { return _captureRunId != 0; }

        /// <summary>
        ///     出力を受けている実行番号を返す。
        /// </summary>
        /// <returns>実行番号。受けていなければ 0。</returns>
        WPARAM CaptureRunId() const { return _captureRunId; }

        /// <summary>
        ///     出力を受けているプロセスを、子孫ごと止める。
        /// </summary>
        void StopCapture();

        /// <summary>
        ///     終了の知らせを受けたときに呼ぶ。出力を受けていた実行なら、ジョブを片付ける。
        /// </summary>
        /// <param name="runId">終わった実行番号。</param>
        void OnExited(WPARAM runId);

    private:
        HWND _notifyWindow;
        std::atomic<WPARAM> _nextRunId{ 1 };
        HANDLE _captureJob = nullptr;   // 作れなかったときは null（止められないだけで、出力は受ける）。
        WPARAM _captureRunId = 0;       // 出力を受けている実行番号。受けていなければ 0。

        StartResult StartConsole(const LaunchCommand& command);
        StartResult StartElevated(const LaunchCommand& command);
        StartResult StartCapture(const LaunchCommand& command);
        void WatchExit(HANDLE process, WPARAM runId);
    };
}
