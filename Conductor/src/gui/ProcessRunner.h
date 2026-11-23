#pragma once

#include "CommandLine.h"

#include <windows.h>

#include <atomic>
#include <string>

namespace skc::conductor::gui
{
    /// <summary>出力の1行が届いた。wParam は実行番号、lParam は new した std::wstring*（受け取った側が delete する）。</summary>
    inline constexpr UINT WM_APP_RUN_OUTPUT = WM_APP + 1;

    /// <summary>プロセスが終わった。wParam は実行番号、lParam は終了コード（DWORD）。</summary>
    inline constexpr UINT WM_APP_RUN_EXITED = WM_APP + 2;

    /// <summary>
    ///     プロセスの起動のしかた。
    /// </summary>
    enum class LaunchKind
    {
        /// <summary>新しいコンソールウィンドウで開く。ツールを閉じても止めない。</summary>
        Console,

        /// <summary>出力をパイプで受ける。止めるとツールの終了で子孫ごと止める。</summary>
        Capture,

        /// <summary>ウィンドウを出さずに動かし、終わるのを待つだけ（CLI が UAC で管理者のスクリプトを開くとき）。</summary>
        Hidden,
    };

    /// <summary>
    ///     プロセスを起動した結果。
    /// </summary>
    struct StartResult
    {
        /// <summary>起動できたか。</summary>
        bool IsStarted = false;

        /// <summary>終わるのを見張っているか（false なら WM_APP_RUN_EXITED は届かない）。</summary>
        bool IsWatched = false;

        /// <summary>実行番号。</summary>
        WPARAM RunId = 0;

        /// <summary>起動できなかった理由。</summary>
        std::wstring Error;
    };

    /// <summary>
    ///     CLI（conductor.exe）のプロセスを起動し、出力と終了をウィンドウへメッセージで知らせる。
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
        /// <param name="kind">起動のしかた。</param>
        /// <returns>起動の結果。</returns>
        StartResult Start(const LaunchCommand& command, LaunchKind kind);

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

        StartResult StartWindowed(const LaunchCommand& command, DWORD creationFlags);
        StartResult StartCapture(const LaunchCommand& command);
        void WatchExit(HANDLE process, WPARAM runId);
    };
}
