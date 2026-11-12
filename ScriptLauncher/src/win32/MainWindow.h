#pragma once

#include "ProcessRunner.h"
#include "ScriptConfig.h"

#include <windows.h>
#include <commctrl.h>

#include <filesystem>
#include <map>
#include <memory>
#include <string>

namespace skc::launcher::win32
{
    /// <summary>
    ///     スクリプト管理ツールのメインウィンドウ。
    ///     左にスクリプトの一覧（分類ごと）、右に説明・引数の組・実行ボタン・出力欄を置く。
    /// </summary>
    class MainWindow
    {
    public:
        /// <summary>
        ///     ウィンドウクラスを登録する。
        /// </summary>
        /// <param name="instance">このプログラムのインスタンス。</param>
        /// <returns>登録できたら true。</returns>
        static bool RegisterWindowClass(HINSTANCE instance);

        /// <summary>
        ///     ウィンドウを作って表示する。
        /// </summary>
        /// <param name="instance">このプログラムのインスタンス。</param>
        /// <param name="showCommand">表示のしかた（wWinMain の引数）。</param>
        /// <returns>作れたら true。</returns>
        bool Create(HINSTANCE instance, int showCommand);

        /// <summary>
        ///     ウィンドウのハンドルを返す。
        /// </summary>
        /// <returns>ハンドル。</returns>
        HWND Handle() const { return _window; }

    private:
        HINSTANCE _instance = nullptr;
        HWND _window = nullptr;
        HWND _list = nullptr;
        HWND _title = nullptr;
        HWND _path = nullptr;
        HWND _description = nullptr;
        HWND _presetLabel = nullptr;
        HWND _presets = nullptr;
        HWND _runButton = nullptr;
        HWND _stopButton = nullptr;
        HWND _folderButton = nullptr;
        HWND _reloadButton = nullptr;
        HWND _outputLabel = nullptr;
        HWND _output = nullptr;
        HWND _status = nullptr;
        HFONT _font = nullptr;
        HFONT _titleFont = nullptr;
        HFONT _monoFont = nullptr;
        HIMAGELIST _images = nullptr;
        UINT _dpi = USER_DEFAULT_SCREEN_DPI;

        std::filesystem::path _root;
        std::filesystem::file_time_type _configWriteTime{};
        ScriptConfig _config;
        std::wstring _configError;
        bool _isSelfElevated = false;

        std::unique_ptr<ProcessRunner> _runner;
        std::map<std::string, std::wstring> _results;   // スクリプトの id → 前回の結果。
        std::map<WPARAM, std::string> _runs;            // 実行番号 → スクリプトの id。
        WPARAM _stopRequestedRunId = 0;
        std::string _shownPresetsId;                    // 引数の組の一覧を作ったスクリプトの id。

        static LRESULT CALLBACK WindowProc(HWND window, UINT message, WPARAM wParam, LPARAM lParam);
        LRESULT HandleMessage(UINT message, WPARAM wParam, LPARAM lParam);
        void OnCreate();
        void CreateControls();
        void ApplyDpi(UINT dpi);
        void Layout();
        int Scale(int value) const;

        void LoadConfig();
        void ReloadConfigIfChanged();
        void PopulateList(const std::string& selectId);
        void UpdateListResults();
        void UpdateDetails();
        void UpdateButtons();
        void UpdateStatus();

        const ScriptEntry* SelectedEntry() const;
        bool ScriptExists(const ScriptEntry& entry) const;
        std::wstring ResultText(const ScriptEntry& entry) const;

        void RunSelected();
        void StopCapture();
        void OpenScriptFolder();
        void OnRunOutput(WPARAM runId, std::wstring* line);
        void OnRunExited(WPARAM runId, DWORD exitCode);
        void AppendOutput(const std::wstring& line);
        void ShowError(const std::wstring& message) const;
    };
}
