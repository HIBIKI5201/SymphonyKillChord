#pragma once

#include "CliClient.h"
#include "CliJson.h"
#include "ProcessRunner.h"

#include <windows.h>
#include <commctrl.h>

#include <filesystem>
#include <map>
#include <memory>
#include <string>
#include <vector>

namespace skc::conductor::gui
{
    /// <summary>
    ///     KillChord Conductor のメインウィンドウ。
    ///     上にページの切り替え（ホーム・スクリプト一覧・設定）、ページごとの中身、下に実行の記録を置く。
    ///     自分では何も実行せず、一覧・状態の読み取りもスクリプトの実行も CLI（conductor.exe）を通す。
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
        /// <summary>
        ///     ページ。
        /// </summary>
        enum class Page
        {
            Home,
            Scripts,
            Settings,
        };

        HINSTANCE _instance = nullptr;
        HWND _window = nullptr;

        // 上の帯（ページの切り替えと役職）。
        HWND _homeTab = nullptr;
        HWND _scriptsTab = nullptr;
        HWND _settingsTab = nullptr;
        HWND _roleText = nullptr;

        // ホーム。
        HWND _statusTitle = nullptr;
        HWND _statusText = nullptr;
        HWND _statusRefreshButton = nullptr;
        HWND _tilesTitle = nullptr;
        std::vector<HWND> _tileButtons;

        // スクリプト一覧。
        HWND _list = nullptr;
        HWND _title = nullptr;
        HWND _path = nullptr;
        HWND _description = nullptr;
        HWND _presetLabel = nullptr;
        HWND _presets = nullptr;
        HWND _runButton = nullptr;
        HWND _folderButton = nullptr;
        HWND _reloadButton = nullptr;

        // 設定。
        HWND _roleLabel = nullptr;
        HWND _roleCombo = nullptr;
        HWND _roleSaveButton = nullptr;
        HWND _settingsNote = nullptr;

        // 実行の記録（どのページでも下に出す）。
        HWND _commandText = nullptr;
        HWND _stopButton = nullptr;
        HWND _output = nullptr;
        HWND _statusBar = nullptr;

        HFONT _font = nullptr;
        HFONT _titleFont = nullptr;
        HFONT _monoFont = nullptr;
        HIMAGELIST _images = nullptr;
        UINT _dpi = USER_DEFAULT_SCREEN_DPI;

        Page _page = Page::Home;
        std::filesystem::path _root;
        std::unique_ptr<CliClient> _cli;
        std::filesystem::file_time_type _scriptsWriteTime{};
        std::filesystem::file_time_type _homeWriteTime{};
        std::vector<ListedScript> _scripts;
        std::wstring _scriptsError;
        HomeView _home;
        std::wstring _homeError;
        ProjectStatus _status;
        std::wstring _statusError;
        bool _isStatusLoading = false;
        ULONGLONG _statusLoadedAt = 0;
        bool _isSelfElevated = false;

        std::unique_ptr<ProcessRunner> _runner;
        std::map<std::string, std::wstring> _results;   // スクリプトの id → 前回の結果。
        std::map<WPARAM, std::string> _runs;            // 実行番号 → スクリプトの id。
        WPARAM _stopRequestedRunId = 0;
        std::string _shownPresetsId;                    // 引数の組の一覧を作ったスクリプトの id。

        static LRESULT CALLBACK WindowProc(HWND window, UINT message, WPARAM wParam, LPARAM lParam);
        LRESULT HandleMessage(UINT message, WPARAM wParam, LPARAM lParam);
        void OnCreate();
        void OnCommand(int id, int code);
        void CreateControls();
        HWND CreateChild(DWORD exStyle, const wchar_t* className, const wchar_t* text, DWORD style, int id);
        void ApplyDpi(UINT dpi);
        void ApplyFonts();
        void Layout();
        void LayoutHome(int left, int top, int width, int bottom);
        void LayoutScripts(int left, int top, int width, int bottom);
        void LayoutSettings(int left, int top, int width, int bottom);
        int Scale(int value) const;

        void ShowPage(Page page);
        void LoadAll();
        void LoadScripts();
        void LoadHome();
        void ReloadIfChanged();
        void StartStatusRefresh();
        void OnStatusLoaded(CliOutput* output);

        void PopulateTiles();
        void PopulateList(const std::string& selectId);
        void PopulateRoles();
        void UpdateListResults();
        void UpdateDetails();
        void UpdateButtons();
        void UpdateStatusText();
        void UpdateRoleText();
        void UpdateStatusBar();

        const ListedScript* FindScript(const std::string& id) const;
        const ListedScript* SelectedScript() const;
        std::wstring ResultText(const ListedScript& script) const;

        void RunTile(std::size_t index);
        void RunSelected();
        void RunScript(const ListedScript& script, std::size_t presetIndex);
        void StopCapture();
        void OpenScriptFolder();
        void SaveRole();
        void OnRunOutput(WPARAM runId, std::wstring* line);
        void OnRunExited(WPARAM runId, DWORD exitCode);
        void SetCommandText(const std::wstring& command);
        void AppendOutput(const std::wstring& line);
        void ShowError(const std::wstring& message) const;
    };
}
