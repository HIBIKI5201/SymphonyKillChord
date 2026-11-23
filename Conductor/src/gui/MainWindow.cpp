#include "MainWindow.h"

#include "ExitCodes.h"
#include "RepositoryRoot.h"
#include "TextConvert.h"

#include <shellapi.h>
#include <uxtheme.h>
#include <windowsx.h>

#include <algorithm>
#include <optional>
#include <thread>

namespace skc::conductor::gui
{
    namespace
    {
        using win32::Utf8ToWide;
        using win32::WideToUtf8;

        constexpr wchar_t WINDOW_CLASS_NAME[] = L"SymphonyKillChordConductor";
        constexpr wchar_t WINDOW_TITLE[] = L"KillChord Conductor ― プロジェクト管理";

        /// <summary>ホーム画面の登録ファイル（リポジトリのルートから）。</summary>
        constexpr wchar_t HOME_CONFIG_RELATIVE_PATH[] = L"Conductor/home.json";

        /// <summary>CLI の exe 名（GUI と同じフォルダに置く）。</summary>
        constexpr wchar_t CONDUCTOR_EXE_NAME[] = L"conductor.exe";

        /// <summary>状態の読み取りが終わった。lParam は new した CliOutput*（受け取った側が delete する）。</summary>
        constexpr UINT WM_APP_STATUS_LOADED = WM_APP + 10;

        /// <summary>前に出てきたとき、状態を読み直すまでの間隔（ミリ秒）。</summary>
        constexpr ULONGLONG STATUS_REFRESH_INTERVAL_MS = 15000;

        // コントロールの ID。
        enum ControlId : int
        {
            ID_HOME_TAB = 100,
            ID_SCRIPTS_TAB,
            ID_SETTINGS_TAB,
            ID_STATUS_REFRESH,
            ID_LIST,
            ID_PRESETS,
            ID_RUN,
            ID_STOP,
            ID_FOLDER,
            ID_RELOAD,
            ID_ROLE_COMBO,
            ID_ROLE_SAVE,
            ID_OUTPUT,
            ID_STATUS_BAR,
            ID_TILE_BASE = 1000,
        };

        // 一覧の列。
        constexpr int COLUMN_NAME = 0;
        constexpr int COLUMN_RESULT = 1;

        // 配置の寸法（96 DPI のときのピクセル）。
        constexpr int MARGIN = 8;
        constexpr int TAB_WIDTH = 132;
        constexpr int TAB_HEIGHT = 30;
        constexpr int ROLE_TEXT_WIDTH = 320;
        constexpr int LIST_WIDTH = 320;
        constexpr int NAME_COLUMN_WIDTH = 200;
        constexpr int TITLE_HEIGHT = 30;
        constexpr int LINE_HEIGHT = 22;
        constexpr int STATUS_TEXT_HEIGHT = 92;
        constexpr int DESCRIPTION_HEIGHT = 96;
        constexpr int CONTROL_HEIGHT = 26;
        constexpr int TILE_HEIGHT = 64;
        constexpr int PRESET_LABEL_WIDTH = 56;
        constexpr int COMBO_DROP_HEIGHT = 200;
        constexpr int BUTTON_WIDTH = 104;
        constexpr int WIDE_BUTTON_WIDTH = 132;
        constexpr int SETTINGS_COMBO_WIDTH = 240;
        constexpr int SETTINGS_NOTE_HEIGHT = 160;
        constexpr int OUTPUT_MIN_HEIGHT = 120;
        constexpr int OUTPUT_PERCENT = 26;
        constexpr int MIN_WINDOW_WIDTH = 760;
        constexpr int MIN_WINDOW_HEIGHT = 560;
        constexpr int DEFAULT_WINDOW_WIDTH = 1060;
        constexpr int DEFAULT_WINDOW_HEIGHT = 800;
        constexpr int MONO_FONT_POINT = 10;
        constexpr int TITLE_FONT_PERCENT = 135;

        /// <summary>出力欄に使う等幅フォント（日本語の Windows に必ずある）。</summary>
        constexpr wchar_t MONO_FONT_FACE[] = L"MS Gothic";

        /// <summary>
        ///     このプロセスが管理者として動いているかを返す。
        /// </summary>
        bool IsProcessElevated()
        {
            HANDLE token = nullptr;
            if (!OpenProcessToken(GetCurrentProcess(), TOKEN_QUERY, &token)) { return false; }

            TOKEN_ELEVATION elevation{};
            DWORD size = 0;
            bool isElevated = GetTokenInformation(token, TokenElevation, &elevation, sizeof(elevation), &size) && elevation.TokenIsElevated != 0;
            CloseHandle(token);
            return isElevated;
        }

        /// <summary>
        ///     このプログラムの exe のあるフォルダを返す。
        /// </summary>
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

        /// <summary>
        ///     ファイルの更新日時を返す。読めなければ既定値。
        /// </summary>
        std::filesystem::file_time_type WriteTimeOf(const std::filesystem::path& path)
        {
            std::error_code error;
            std::filesystem::file_time_type time = std::filesystem::last_write_time(path, error);
            return error ? std::filesystem::file_time_type{} : time;
        }

        /// <summary>
        ///     動かし方の説明を返す。
        /// </summary>
        std::wstring DescribeMode(RunMode mode, bool isElevated)
        {
            if (isElevated) { return L"管理者として新しいウィンドウで開く（UAC の確認が出る）"; }

            return mode == RunMode::Console ? L"新しいウィンドウで開く" : L"出力をこの画面に出す";
        }

        /// <summary>
        ///     JSON に書いた改行（\n）を、Windows のコントロール用の改行（\r\n）にする。
        /// </summary>
        std::wstring ToControlNewlines(const std::wstring& text)
        {
            std::wstring converted;
            converted.reserve(text.size());
            for (wchar_t c : text)
            {
                if (c == L'\n') { converted += L"\r\n"; }
                else if (c != L'\r') { converted.push_back(c); }
            }

            return converted;
        }

        /// <summary>
        ///     CLI の標準エラーを、画面に出す1つの文にする（先頭の "conductor: " は外す）。
        /// </summary>
        std::wstring DescribeCliError(const CliOutput& output)
        {
            std::string text = output.Error;
            if (text.rfind("conductor: ", 0) == 0) { text.erase(0, 11); }

            while (!text.empty() && (text.back() == '\n' || text.back() == '\r')) { text.pop_back(); }

            if (text.empty()) { text = "conductor.exe が失敗しました（終了コード " + std::to_string(output.ExitCode) + "）。"; }

            return ToControlNewlines(Utf8ToWide(text));
        }

        /// <summary>
        ///     サブモジュールの状態を短い言葉にする。
        /// </summary>
        std::wstring DescribeSubmoduleState(const std::string& state)
        {
            if (state == "ok") { return L"OK"; }
            if (state == "missing") { return L"取っていない"; }
            if (state == "modified") { return L"記録と違うコミット"; }

            return L"衝突";
        }
    }

    bool MainWindow::RegisterWindowClass(HINSTANCE instance)
    {
        WNDCLASSEXW windowClass{};
        windowClass.cbSize = sizeof(windowClass);
        windowClass.style = CS_HREDRAW | CS_VREDRAW;
        windowClass.lpfnWndProc = WindowProc;
        windowClass.hInstance = instance;
        windowClass.hCursor = LoadCursorW(nullptr, IDC_ARROW);
        windowClass.hbrBackground = reinterpret_cast<HBRUSH>(COLOR_BTNFACE + 1);
        windowClass.lpszClassName = WINDOW_CLASS_NAME;
        windowClass.hIcon = LoadIconW(nullptr, IDI_APPLICATION);
        windowClass.hIconSm = LoadIconW(nullptr, IDI_APPLICATION);
        return RegisterClassExW(&windowClass) != 0;
    }

    bool MainWindow::Create(HINSTANCE instance, int showCommand)
    {
        _instance = instance;
        HWND window = CreateWindowExW(0, WINDOW_CLASS_NAME, WINDOW_TITLE, WS_OVERLAPPEDWINDOW | WS_CLIPCHILDREN,
            CW_USEDEFAULT, CW_USEDEFAULT, DEFAULT_WINDOW_WIDTH, DEFAULT_WINDOW_HEIGHT, nullptr, nullptr, instance, this);
        if (window == nullptr) { return false; }

        // 作った時点の DPI に合わせて大きさを直す（CreateWindowEx の大きさは 96 DPI のつもりで渡している）。
        SetWindowPos(window, nullptr, 0, 0, Scale(DEFAULT_WINDOW_WIDTH), Scale(DEFAULT_WINDOW_HEIGHT), SWP_NOMOVE | SWP_NOZORDER | SWP_NOACTIVATE);

        ShowWindow(window, showCommand);
        UpdateWindow(window);
        return true;
    }

    LRESULT CALLBACK MainWindow::WindowProc(HWND window, UINT message, WPARAM wParam, LPARAM lParam)
    {
        MainWindow* self = nullptr;
        if (message == WM_NCCREATE)
        {
            auto* create = reinterpret_cast<CREATESTRUCTW*>(lParam);
            self = static_cast<MainWindow*>(create->lpCreateParams);
            self->_window = window;
            SetWindowLongPtrW(window, GWLP_USERDATA, reinterpret_cast<LONG_PTR>(self));
        }
        else
        {
            self = reinterpret_cast<MainWindow*>(GetWindowLongPtrW(window, GWLP_USERDATA));
        }

        if (self == nullptr) { return DefWindowProcW(window, message, wParam, lParam); }

        return self->HandleMessage(message, wParam, lParam);
    }

    LRESULT MainWindow::HandleMessage(UINT message, WPARAM wParam, LPARAM lParam)
    {
        switch (message)
        {
            case WM_CREATE:
                OnCreate();
                return 0;

            case WM_SIZE:
                Layout();
                return 0;

            case WM_GETMINMAXINFO:
            {
                auto* info = reinterpret_cast<MINMAXINFO*>(lParam);
                info->ptMinTrackSize.x = Scale(MIN_WINDOW_WIDTH);
                info->ptMinTrackSize.y = Scale(MIN_WINDOW_HEIGHT);
                return 0;
            }

            case WM_DPICHANGED:
            {
                ApplyDpi(HIWORD(wParam));
                const RECT* suggested = reinterpret_cast<const RECT*>(lParam);
                SetWindowPos(_window, nullptr, suggested->left, suggested->top,
                    suggested->right - suggested->left, suggested->bottom - suggested->top, SWP_NOZORDER | SWP_NOACTIVATE);
                Layout();
                return 0;
            }

            case WM_ACTIVATE:
                // ブランチを切り替えたあとなどに備え、前に出てきたときに登録ファイルが変わっていれば読み直す。
                if (LOWORD(wParam) != WA_INACTIVE) { ReloadIfChanged(); }
                break;

            case WM_COMMAND:
                OnCommand(LOWORD(wParam), HIWORD(wParam));
                return 0;

            case WM_NOTIFY:
            {
                const auto* header = reinterpret_cast<const NMHDR*>(lParam);
                if (header->idFrom == ID_LIST)
                {
                    if (header->code == LVN_ITEMCHANGED)
                    {
                        const auto* change = reinterpret_cast<const NMLISTVIEW*>(lParam);
                        if ((change->uChanged & LVIF_STATE) != 0 && ((change->uNewState ^ change->uOldState) & LVIS_SELECTED) != 0)
                        {
                            UpdateDetails();
                        }
                    }
                    else if (header->code == LVN_ITEMACTIVATE)
                    {
                        RunSelected();
                    }
                }

                break;
            }

            case WM_APP_RUN_OUTPUT:
                OnRunOutput(wParam, reinterpret_cast<std::wstring*>(lParam));
                return 0;

            case WM_APP_RUN_EXITED:
                OnRunExited(wParam, static_cast<DWORD>(lParam));
                return 0;

            case WM_APP_STATUS_LOADED:
                OnStatusLoaded(reinterpret_cast<CliOutput*>(lParam));
                return 0;

            case WM_DESTROY:
                _runner.reset();
                if (_font != nullptr) { DeleteObject(_font); }
                if (_titleFont != nullptr) { DeleteObject(_titleFont); }
                if (_monoFont != nullptr) { DeleteObject(_monoFont); }
                if (_images != nullptr) { ImageList_Destroy(_images); }
                PostQuitMessage(0);
                return 0;

            default:
                break;
        }

        return DefWindowProcW(_window, message, wParam, lParam);
    }

    void MainWindow::OnCreate()
    {
        _dpi = GetDpiForWindow(_window);
        _isSelfElevated = IsProcessElevated();
        _runner = std::make_unique<ProcessRunner>(_window);

        std::filesystem::path exeDirectory = ExecutableDirectory();
        std::optional<std::filesystem::path> root = FindRepositoryRoot(exeDirectory);
        if (!root.has_value()) { root = FindRepositoryRoot(std::filesystem::current_path()); }
        if (root.has_value()) { _root = *root; }

        _cli = std::make_unique<CliClient>(exeDirectory / CONDUCTOR_EXE_NAME, _root);

        CreateControls();
        ApplyDpi(_dpi);
        LoadAll();

        // 初めて開いたとき（役職が未設定）は、設定ページで役職を選んでもらう。
        if (_homeError.empty() && _home.Role.empty())
        {
            ShowPage(Page::Settings);
            AppendOutput(L"ようこそ。最初に「設定」で役職を選んでください。ホームで、その役職がよく使うボタンが前に並びます。");
        }
        else
        {
            ShowPage(Page::Home);
        }

        StartStatusRefresh();
    }

    void MainWindow::OnCommand(int id, int code)
    {
        if (id >= ID_TILE_BASE && id < ID_TILE_BASE + static_cast<int>(_tileButtons.size()))
        {
            if (code == BN_CLICKED) { RunTile(static_cast<std::size_t>(id - ID_TILE_BASE)); }
            return;
        }

        switch (id)
        {
            case ID_HOME_TAB:
                ShowPage(Page::Home);
                break;
            case ID_SCRIPTS_TAB:
                ShowPage(Page::Scripts);
                break;
            case ID_SETTINGS_TAB:
                ShowPage(Page::Settings);
                break;
            case ID_STATUS_REFRESH:
                StartStatusRefresh();
                break;
            case ID_RUN:
                RunSelected();
                break;
            case ID_STOP:
                StopCapture();
                break;
            case ID_FOLDER:
                OpenScriptFolder();
                break;
            case ID_RELOAD:
                LoadAll();
                break;
            case ID_ROLE_SAVE:
                SaveRole();
                break;
            case ID_PRESETS:
                if (code == CBN_SELCHANGE) { UpdateDetails(); }
                break;
            default:
                break;
        }
    }

    HWND MainWindow::CreateChild(DWORD exStyle, const wchar_t* className, const wchar_t* text, DWORD style, int id)
    {
        return CreateWindowExW(exStyle, className, text, WS_CHILD | style, 0, 0, 0, 0,
            _window, reinterpret_cast<HMENU>(static_cast<INT_PTR>(id)), _instance, nullptr);
    }

    void MainWindow::CreateControls()
    {
        // 上の帯。ページの切り替えは、押し込んだままに見えるラジオボタンにする。
        _homeTab = CreateChild(0, WC_BUTTONW, L"ホーム", WS_VISIBLE | WS_TABSTOP | WS_GROUP | BS_AUTORADIOBUTTON | BS_PUSHLIKE, ID_HOME_TAB);
        _scriptsTab = CreateChild(0, WC_BUTTONW, L"スクリプト一覧", WS_VISIBLE | BS_AUTORADIOBUTTON | BS_PUSHLIKE, ID_SCRIPTS_TAB);
        _settingsTab = CreateChild(0, WC_BUTTONW, L"設定", WS_VISIBLE | BS_AUTORADIOBUTTON | BS_PUSHLIKE, ID_SETTINGS_TAB);
        _roleText = CreateChild(0, WC_STATICW, L"", WS_VISIBLE | SS_RIGHT | SS_CENTERIMAGE | SS_NOPREFIX | SS_ENDELLIPSIS, 0);

        // ホーム。
        _statusTitle = CreateChild(0, WC_STATICW, L"プロジェクトの状態", SS_LEFT | SS_NOPREFIX, 0);
        _statusText = CreateChild(0, WC_STATICW, L"", SS_LEFT | SS_NOPREFIX, 0);
        _statusRefreshButton = CreateChild(0, WC_BUTTONW, L"状態を読み直す", WS_TABSTOP | BS_PUSHBUTTON, ID_STATUS_REFRESH);
        _tilesTitle = CreateChild(0, WC_STATICW, L"よく使う操作", SS_LEFT | SS_NOPREFIX, 0);

        // スクリプト一覧。
        _list = CreateChild(WS_EX_CLIENTEDGE, WC_LISTVIEWW, L"", WS_TABSTOP | LVS_REPORT | LVS_SINGLESEL | LVS_SHOWSELALWAYS, ID_LIST);
        ListView_SetExtendedListViewStyle(_list, LVS_EX_FULLROWSELECT | LVS_EX_DOUBLEBUFFER);
        SetWindowTheme(_list, L"Explorer", nullptr);

        LVCOLUMNW column{};
        column.mask = LVCF_TEXT | LVCF_WIDTH;
        column.pszText = const_cast<wchar_t*>(L"スクリプト");
        ListView_InsertColumn(_list, COLUMN_NAME, &column);
        column.pszText = const_cast<wchar_t*>(L"前回の結果");
        ListView_InsertColumn(_list, COLUMN_RESULT, &column);
        ListView_EnableGroupView(_list, TRUE);

        _title = CreateChild(0, WC_STATICW, L"", SS_LEFT | SS_NOPREFIX | SS_ENDELLIPSIS, 0);
        _path = CreateChild(0, WC_STATICW, L"", SS_LEFT | SS_NOPREFIX | SS_PATHELLIPSIS, 0);
        _description = CreateChild(0, WC_STATICW, L"", SS_LEFT | SS_NOPREFIX, 0);
        _presetLabel = CreateChild(0, WC_STATICW, L"引数", SS_LEFT | SS_CENTERIMAGE, 0);
        _presets = CreateChild(0, WC_COMBOBOXW, L"", WS_TABSTOP | WS_VSCROLL | CBS_DROPDOWNLIST, ID_PRESETS);
        _runButton = CreateChild(0, WC_BUTTONW, L"実行", WS_TABSTOP | BS_PUSHBUTTON, ID_RUN);
        _folderButton = CreateChild(0, WC_BUTTONW, L"フォルダを開く", WS_TABSTOP | BS_PUSHBUTTON, ID_FOLDER);
        _reloadButton = CreateChild(0, WC_BUTTONW, L"登録を読み直す", WS_TABSTOP | BS_PUSHBUTTON, ID_RELOAD);

        // 設定。
        _roleLabel = CreateChild(0, WC_STATICW, L"役職", SS_LEFT | SS_CENTERIMAGE, 0);
        _roleCombo = CreateChild(0, WC_COMBOBOXW, L"", WS_TABSTOP | WS_VSCROLL | CBS_DROPDOWNLIST, ID_ROLE_COMBO);
        _roleSaveButton = CreateChild(0, WC_BUTTONW, L"保存", WS_TABSTOP | BS_PUSHBUTTON, ID_ROLE_SAVE);
        _settingsNote = CreateChild(0, WC_STATICW, L"", SS_LEFT | SS_NOPREFIX, 0);

        // 実行の記録。
        _commandText = CreateChild(0, WC_STATICW, L"", WS_VISIBLE | SS_LEFT | SS_CENTERIMAGE | SS_NOPREFIX | SS_ENDELLIPSIS, 0);
        _stopButton = CreateChild(0, WC_BUTTONW, L"止める", WS_VISIBLE | WS_TABSTOP | BS_PUSHBUTTON, ID_STOP);
        _output = CreateChild(WS_EX_CLIENTEDGE, WC_EDITW, L"",
            WS_VISIBLE | WS_TABSTOP | WS_VSCROLL | WS_HSCROLL | ES_MULTILINE | ES_READONLY | ES_AUTOVSCROLL | ES_AUTOHSCROLL, ID_OUTPUT);
        SendMessageW(_output, EM_SETLIMITTEXT, 0, 0);
        _statusBar = CreateChild(0, STATUSCLASSNAMEW, L"", WS_VISIBLE | SBARS_SIZEGRIP, ID_STATUS_BAR);
        SetCommandText(L"");
    }

    void MainWindow::ApplyDpi(UINT dpi)
    {
        _dpi = dpi;

        NONCLIENTMETRICSW metrics{};
        metrics.cbSize = sizeof(metrics);
        SystemParametersInfoForDpi(SPI_GETNONCLIENTMETRICS, sizeof(metrics), &metrics, 0, dpi);

        HFONT oldFont = _font;
        HFONT oldTitleFont = _titleFont;
        HFONT oldMonoFont = _monoFont;

        _font = CreateFontIndirectW(&metrics.lfMessageFont);
        LOGFONTW titleFont = metrics.lfMessageFont;
        titleFont.lfHeight = MulDiv(titleFont.lfHeight, TITLE_FONT_PERCENT, 100);
        titleFont.lfWeight = FW_BOLD;
        _titleFont = CreateFontIndirectW(&titleFont);
        _monoFont = CreateFontW(-MulDiv(MONO_FONT_POINT, static_cast<int>(dpi), 72), 0, 0, 0, FW_NORMAL, FALSE, FALSE, FALSE,
            DEFAULT_CHARSET, OUT_DEFAULT_PRECIS, CLIP_DEFAULT_PRECIS, CLEARTYPE_QUALITY, FIXED_PITCH | FF_MODERN, MONO_FONT_FACE);

        ApplyFonts();

        if (oldFont != nullptr) { DeleteObject(oldFont); }
        if (oldTitleFont != nullptr) { DeleteObject(oldTitleFont); }
        if (oldMonoFont != nullptr) { DeleteObject(oldMonoFont); }

        // 管理者で動かすスクリプトの印（盾）を、この DPI の大きさで作り直す。
        HIMAGELIST oldImages = _images;
        int iconSize = GetSystemMetricsForDpi(SM_CXSMICON, dpi);
        _images = ImageList_Create(iconSize, iconSize, ILC_COLOR32 | ILC_MASK, 1, 1);
        SHSTOCKICONINFO shield{};
        shield.cbSize = sizeof(shield);
        if (SUCCEEDED(SHGetStockIconInfo(SIID_SHIELD, SHGSI_ICON | SHGSI_SMALLICON, &shield)))
        {
            ImageList_AddIcon(_images, shield.hIcon);
            DestroyIcon(shield.hIcon);
        }

        ListView_SetImageList(_list, _images, LVSIL_SMALL);
        if (oldImages != nullptr) { ImageList_Destroy(oldImages); }

        ListView_SetColumnWidth(_list, COLUMN_NAME, Scale(NAME_COLUMN_WIDTH));
        ListView_SetColumnWidth(_list, COLUMN_RESULT, LVSCW_AUTOSIZE_USEHEADER);
    }

    void MainWindow::ApplyFonts()
    {
        for (HWND child : { _homeTab, _scriptsTab, _settingsTab, _roleText, _statusText, _statusRefreshButton,
                 _list, _path, _description, _presetLabel, _presets, _runButton, _folderButton, _reloadButton,
                 _roleLabel, _roleCombo, _roleSaveButton, _settingsNote, _commandText, _stopButton, _statusBar })
        {
            SendMessageW(child, WM_SETFONT, reinterpret_cast<WPARAM>(_font), TRUE);
        }

        for (HWND tile : _tileButtons) { SendMessageW(tile, WM_SETFONT, reinterpret_cast<WPARAM>(_font), TRUE); }

        for (HWND title : { _statusTitle, _tilesTitle, _title }) { SendMessageW(title, WM_SETFONT, reinterpret_cast<WPARAM>(_titleFont), TRUE); }

        SendMessageW(_output, WM_SETFONT, reinterpret_cast<WPARAM>(_monoFont), TRUE);
    }

    void MainWindow::Layout()
    {
        RECT client{};
        GetClientRect(_window, &client);

        SendMessageW(_statusBar, WM_SIZE, 0, 0);
        RECT statusRect{};
        GetWindowRect(_statusBar, &statusRect);
        int statusHeight = statusRect.bottom - statusRect.top;

        int margin = Scale(MARGIN);
        int width = client.right - client.left;
        int height = client.bottom - client.top - statusHeight;
        int innerWidth = std::max(0, width - margin * 2);

        // 上の帯。
        int x = margin;
        for (HWND tab : { _homeTab, _scriptsTab, _settingsTab })
        {
            MoveWindow(tab, x, margin, Scale(TAB_WIDTH), Scale(TAB_HEIGHT), TRUE);
            x += Scale(TAB_WIDTH) + margin / 2;
        }

        int roleWidth = std::min(Scale(ROLE_TEXT_WIDTH), std::max(0, width - margin - x));
        MoveWindow(_roleText, width - margin - roleWidth, margin, roleWidth, Scale(TAB_HEIGHT), TRUE);

        // 下の実行の記録。
        int outputHeight = std::max(Scale(OUTPUT_MIN_HEIGHT), height * OUTPUT_PERCENT / 100);
        int outputTop = height - margin - outputHeight;
        int commandTop = outputTop - Scale(CONTROL_HEIGHT) - margin / 2;
        MoveWindow(_stopButton, width - margin - Scale(BUTTON_WIDTH), commandTop, Scale(BUTTON_WIDTH), Scale(CONTROL_HEIGHT), TRUE);
        MoveWindow(_commandText, margin, commandTop, std::max(0, innerWidth - Scale(BUTTON_WIDTH) - margin), Scale(CONTROL_HEIGHT), TRUE);
        MoveWindow(_output, margin, outputTop, innerWidth, outputHeight, TRUE);

        // ページの中身。
        int top = margin * 2 + Scale(TAB_HEIGHT);
        int bottom = commandTop - margin;
        LayoutHome(margin, top, innerWidth, bottom);
        LayoutScripts(margin, top, innerWidth, bottom);
        LayoutSettings(margin, top, innerWidth, bottom);
    }

    void MainWindow::LayoutHome(int left, int top, int width, int bottom)
    {
        int margin = Scale(MARGIN);
        int y = top;
        MoveWindow(_statusTitle, left, y, std::max(0, width - Scale(WIDE_BUTTON_WIDTH) - margin), Scale(TITLE_HEIGHT), TRUE);
        MoveWindow(_statusRefreshButton, left + width - Scale(WIDE_BUTTON_WIDTH), y, Scale(WIDE_BUTTON_WIDTH), Scale(CONTROL_HEIGHT), TRUE);
        y += Scale(TITLE_HEIGHT);
        MoveWindow(_statusText, left, y, width, Scale(STATUS_TEXT_HEIGHT), TRUE);
        y += Scale(STATUS_TEXT_HEIGHT) + margin;
        MoveWindow(_tilesTitle, left, y, width, Scale(TITLE_HEIGHT), TRUE);
        y += Scale(TITLE_HEIGHT);

        // ボタンは2列に並べる。入りきらない分は、ウィンドウを大きくすると見える。
        int columnWidth = std::max(0, (width - margin) / 2);
        int tileHeight = Scale(TILE_HEIGHT);
        for (std::size_t i = 0; i < _tileButtons.size(); ++i)
        {
            int column = static_cast<int>(i % 2);
            int row = static_cast<int>(i / 2);
            int tileTop = y + row * (tileHeight + margin / 2);
            int visibleHeight = std::max(0, std::min(tileHeight, bottom - tileTop));
            MoveWindow(_tileButtons[i], left + column * (columnWidth + margin), tileTop, columnWidth, visibleHeight, TRUE);
        }
    }

    void MainWindow::LayoutScripts(int left, int top, int width, int bottom)
    {
        int margin = Scale(MARGIN);
        int listWidth = std::min(Scale(LIST_WIDTH), width / 2);
        MoveWindow(_list, left, top, listWidth, std::max(0, bottom - top), TRUE);
        ListView_SetColumnWidth(_list, COLUMN_RESULT, LVSCW_AUTOSIZE_USEHEADER);

        int detailLeft = left + listWidth + margin;
        int detailWidth = std::max(0, left + width - detailLeft);
        int y = top;
        MoveWindow(_title, detailLeft, y, detailWidth, Scale(TITLE_HEIGHT), TRUE);
        y += Scale(TITLE_HEIGHT);
        MoveWindow(_path, detailLeft, y, detailWidth, Scale(LINE_HEIGHT), TRUE);
        y += Scale(LINE_HEIGHT);
        MoveWindow(_description, detailLeft, y, detailWidth, Scale(DESCRIPTION_HEIGHT), TRUE);
        y += Scale(DESCRIPTION_HEIGHT) + margin;

        int labelWidth = Scale(PRESET_LABEL_WIDTH);
        MoveWindow(_presetLabel, detailLeft, y, labelWidth, Scale(CONTROL_HEIGHT), TRUE);
        MoveWindow(_presets, detailLeft + labelWidth, y, std::max(0, detailWidth - labelWidth), Scale(COMBO_DROP_HEIGHT), TRUE);
        y += Scale(CONTROL_HEIGHT) + margin;

        int x = detailLeft;
        int buttonHeight = Scale(CONTROL_HEIGHT);
        MoveWindow(_runButton, x, y, Scale(BUTTON_WIDTH), buttonHeight, TRUE);
        x += Scale(BUTTON_WIDTH) + margin;
        MoveWindow(_folderButton, x, y, Scale(WIDE_BUTTON_WIDTH), buttonHeight, TRUE);
        x += Scale(WIDE_BUTTON_WIDTH) + margin;
        MoveWindow(_reloadButton, x, y, Scale(WIDE_BUTTON_WIDTH), buttonHeight, TRUE);
    }

    void MainWindow::LayoutSettings(int left, int top, int width, int bottom)
    {
        (void)bottom;
        int margin = Scale(MARGIN);
        int labelWidth = Scale(PRESET_LABEL_WIDTH);
        MoveWindow(_roleLabel, left, top, labelWidth, Scale(CONTROL_HEIGHT), TRUE);
        MoveWindow(_roleCombo, left + labelWidth, top, Scale(SETTINGS_COMBO_WIDTH), Scale(COMBO_DROP_HEIGHT), TRUE);
        MoveWindow(_roleSaveButton, left + labelWidth + Scale(SETTINGS_COMBO_WIDTH) + margin, top, Scale(BUTTON_WIDTH), Scale(CONTROL_HEIGHT), TRUE);
        MoveWindow(_settingsNote, left, top + Scale(CONTROL_HEIGHT) + margin * 2, width, Scale(SETTINGS_NOTE_HEIGHT), TRUE);
    }

    int MainWindow::Scale(int value) const
    {
        return MulDiv(value, static_cast<int>(_dpi), USER_DEFAULT_SCREEN_DPI);
    }

    void MainWindow::ShowPage(Page page)
    {
        _page = page;
        auto show = [](std::initializer_list<HWND> controls, bool isShown)
        {
            for (HWND control : controls) { ShowWindow(control, isShown ? SW_SHOW : SW_HIDE); }
        };

        show({ _statusTitle, _statusText, _statusRefreshButton, _tilesTitle }, page == Page::Home);
        for (HWND tile : _tileButtons) { ShowWindow(tile, page == Page::Home ? SW_SHOW : SW_HIDE); }

        show({ _list, _title, _path, _description, _presetLabel, _presets, _runButton, _folderButton, _reloadButton }, page == Page::Scripts);
        show({ _roleLabel, _roleCombo, _roleSaveButton, _settingsNote }, page == Page::Settings);

        Button_SetCheck(_homeTab, page == Page::Home ? BST_CHECKED : BST_UNCHECKED);
        Button_SetCheck(_scriptsTab, page == Page::Scripts ? BST_CHECKED : BST_UNCHECKED);
        Button_SetCheck(_settingsTab, page == Page::Settings ? BST_CHECKED : BST_UNCHECKED);
        if (page == Page::Settings) { PopulateRoles(); }

        InvalidateRect(_window, nullptr, TRUE);
    }

    void MainWindow::LoadAll()
    {
        LoadScripts();
        LoadHome();
        UpdateStatusBar();
    }

    void MainWindow::LoadScripts()
    {
        const ListedScript* selected = SelectedScript();
        std::string selectId = selected != nullptr ? selected->Entry.Id : std::string();

        _scripts.clear();
        _scriptsError.clear();
        if (_root.empty())
        {
            _scriptsError = L"リポジトリが見つかりません。このアプリは、リポジトリのルートに置いて使ってください（"
                + Utf8ToWide(SCRIPT_CONFIG_RELATIVE_PATH) + L" を上のフォルダへ順に探しています）。";
        }
        else if (!_cli->Exists())
        {
            _scriptsError = L"CLI（" + _cli->ConductorPath().wstring() + L"）が見つかりません。このアプリと同じフォルダに conductor.exe を置いてください。";
        }
        else
        {
            _scriptsWriteTime = WriteTimeOf(_root / SCRIPT_CONFIG_RELATIVE_PATH);
            CliOutput output = _cli->Run({ "scripts", "list", "--json" });
            std::string error;
            if (!output.IsSuccess()) { _scriptsError = DescribeCliError(output); }
            else if (!ScriptListFromJson(output.Output, _scripts, error)) { _scriptsError = Utf8ToWide(error); }
        }

        PopulateList(selectId);
    }

    void MainWindow::LoadHome()
    {
        _home = HomeView();
        _homeError.clear();
        if (!_scriptsError.empty())
        {
            _homeError = _scriptsError;
        }
        else
        {
            _homeWriteTime = WriteTimeOf(_root / HOME_CONFIG_RELATIVE_PATH);
            CliOutput output = _cli->Run({ "home", "--json" });
            std::string error;
            if (!output.IsSuccess()) { _homeError = DescribeCliError(output); }
            else if (!HomeViewFromJson(output.Output, _home, error)) { _homeError = Utf8ToWide(error); }
        }

        PopulateTiles();
        UpdateRoleText();
        if (_page == Page::Settings) { PopulateRoles(); }
    }

    void MainWindow::ReloadIfChanged()
    {
        if (_root.empty()) { return; }

        bool isScriptsChanged = WriteTimeOf(_root / SCRIPT_CONFIG_RELATIVE_PATH) != _scriptsWriteTime;
        bool isHomeChanged = WriteTimeOf(_root / HOME_CONFIG_RELATIVE_PATH) != _homeWriteTime;
        if (isScriptsChanged || isHomeChanged) { LoadAll(); }

        if (!_isStatusLoading && GetTickCount64() - _statusLoadedAt > STATUS_REFRESH_INTERVAL_MS) { StartStatusRefresh(); }
    }

    void MainWindow::StartStatusRefresh()
    {
        if (_isStatusLoading || _root.empty() || !_cli->Exists()) { return; }

        _isStatusLoading = true;
        EnableWindow(_statusRefreshButton, FALSE);
        UpdateStatusText();

        // git を呼ぶので時間がかかることがある。画面を止めないよう別のスレッドで動かし、結果をメッセージで受け取る。
        CliClient client = *_cli;
        HWND window = _window;
        std::thread([client, window]()
        {
            auto* output = new CliOutput(client.Run({ "status", "--json" }));
            if (!PostMessageW(window, WM_APP_STATUS_LOADED, 0, reinterpret_cast<LPARAM>(output))) { delete output; }
        }).detach();
    }

    void MainWindow::OnStatusLoaded(CliOutput* output)
    {
        std::unique_ptr<CliOutput> owned(output);
        _isStatusLoading = false;
        _statusLoadedAt = GetTickCount64();
        EnableWindow(_statusRefreshButton, TRUE);

        // git が失敗しても（終了コード 1）、JSON には読めたところまで入っている。
        std::string error;
        if (!owned->IsStarted || owned->Output.empty()) { _statusError = DescribeCliError(*owned); }
        else if (!StatusFromJson(owned->Output, _status, error)) { _statusError = Utf8ToWide(error); }
        else { _statusError.clear(); }

        UpdateStatusText();
    }

    void MainWindow::PopulateTiles()
    {
        for (HWND tile : _tileButtons) { DestroyWindow(tile); }

        _tileButtons.clear();
        for (std::size_t i = 0; i < _home.Tiles.size(); ++i)
        {
            const HomeTile& tile = _home.Tiles[i];
            std::wstring note = Utf8ToWide(tile.Note);
            bool isEnabled = true;
            if (tile.Page.empty())
            {
                const ListedScript* script = FindScript(tile.ScriptId);
                if (script == nullptr || !script->Exists)
                {
                    isEnabled = false;
                    note = (script != nullptr && script->Entry.IsLocalOnly ? L"【この PC に無い】" : L"【ファイルが無い】") + note;
                }
            }

            HWND button = CreateChild(0, WC_BUTTONW, Utf8ToWide(tile.Title).c_str(),
                (_page == Page::Home ? WS_VISIBLE : 0) | WS_TABSTOP | BS_COMMANDLINK, ID_TILE_BASE + static_cast<int>(i));
            SendMessageW(button, BCM_SETNOTE, 0, reinterpret_cast<LPARAM>(note.c_str()));
            SendMessageW(button, WM_SETFONT, reinterpret_cast<WPARAM>(_font), TRUE);
            EnableWindow(button, isEnabled);
            _tileButtons.push_back(button);
        }

        // 設定を読めなかったときは、ボタンの代わりに理由を出す。
        SetWindowTextW(_tilesTitle, _homeError.empty() ? L"よく使う操作" : (L"ホーム画面を読めません: " + _homeError).c_str());
        Layout();
    }

    void MainWindow::PopulateList(const std::string& selectId)
    {
        SendMessageW(_list, WM_SETREDRAW, FALSE, 0);
        _shownPresetsId.clear();
        ListView_DeleteAllItems(_list);
        ListView_RemoveAllGroups(_list);

        // 分類は、登録ファイルに最初に出てきた順に並べる。
        std::vector<std::string> categories;
        for (const ListedScript& script : _scripts)
        {
            if (std::find(categories.begin(), categories.end(), script.Entry.Category) == categories.end())
            {
                categories.push_back(script.Entry.Category);
            }
        }

        for (std::size_t i = 0; i < categories.size(); ++i)
        {
            std::wstring header = Utf8ToWide(categories[i]);
            LVGROUP group{};
            group.cbSize = sizeof(group);
            group.mask = LVGF_HEADER | LVGF_GROUPID;
            group.pszHeader = header.data();
            group.iGroupId = static_cast<int>(i);
            ListView_InsertGroup(_list, -1, &group);
        }

        int selectIndex = 0;
        for (std::size_t i = 0; i < _scripts.size(); ++i)
        {
            const ScriptEntry& entry = _scripts[i].Entry;
            std::wstring name = Utf8ToWide(entry.Name);
            LVITEMW item{};
            item.mask = LVIF_TEXT | LVIF_PARAM | LVIF_GROUPID | LVIF_IMAGE;
            item.iItem = static_cast<int>(i);
            item.pszText = name.data();
            item.lParam = static_cast<LPARAM>(i);
            item.iImage = entry.IsElevated ? 0 : I_IMAGENONE;
            item.iGroupId = static_cast<int>(std::find(categories.begin(), categories.end(), entry.Category) - categories.begin());
            int inserted = ListView_InsertItem(_list, &item);
            if (entry.Id == selectId) { selectIndex = inserted; }
        }

        UpdateListResults();
        SendMessageW(_list, WM_SETREDRAW, TRUE, 0);
        InvalidateRect(_list, nullptr, TRUE);

        if (!_scripts.empty())
        {
            ListView_SetItemState(_list, selectIndex, LVIS_SELECTED | LVIS_FOCUSED, LVIS_SELECTED | LVIS_FOCUSED);
            ListView_EnsureVisible(_list, selectIndex, FALSE);
        }

        UpdateDetails();
    }

    void MainWindow::PopulateRoles()
    {
        SendMessageW(_roleCombo, CB_RESETCONTENT, 0, 0);
        SendMessageW(_roleCombo, CB_ADDSTRING, 0, reinterpret_cast<LPARAM>(L"（選ばない）"));
        int selected = 0;
        for (std::size_t i = 0; i < _home.Roles.size(); ++i)
        {
            std::wstring label = Utf8ToWide(_home.Roles[i].Label);
            SendMessageW(_roleCombo, CB_ADDSTRING, 0, reinterpret_cast<LPARAM>(label.c_str()));
            if (_home.Roles[i].Id == _home.Role) { selected = static_cast<int>(i) + 1; }
        }

        SendMessageW(_roleCombo, CB_SETCURSEL, selected, 0);
        EnableWindow(_roleCombo, _homeError.empty());
        EnableWindow(_roleSaveButton, _homeError.empty());

        std::wstring note =
            L"役職を選ぶと、ホームでその役職がよく使うボタンが前に並びます。ほかのボタンも消えずに後ろに並びます。\r\n"
            L"役職はこの PC だけの設定で、git には入りません。\r\n\r\n"
            L"このアプリは、ボタンを押すと CLI（conductor.exe）を呼びます。画面の下に、同じことをするコマンドが出ます。\r\n"
            L"コマンドプロンプトや PowerShell から、同じコマンドを直接打っても動きます（使い方は conductor --help）。\r\n\r\n"
            L"リポジトリ: " + (_root.empty() ? std::wstring(L"見つかりません") : _root.wstring()) + L"\r\n"
            L"CLI: " + _cli->ConductorPath().wstring();
        if (!_homeError.empty()) { note = L"ホーム画面の登録を読めません: " + _homeError + L"\r\n\r\n" + note; }

        SetWindowTextW(_settingsNote, note.c_str());
    }

    void MainWindow::UpdateListResults()
    {
        int count = ListView_GetItemCount(_list);
        for (int index = 0; index < count; ++index)
        {
            LVITEMW item{};
            item.mask = LVIF_PARAM;
            item.iItem = index;
            if (!ListView_GetItem(_list, &item)) { continue; }

            std::size_t scriptIndex = static_cast<std::size_t>(item.lParam);
            if (scriptIndex >= _scripts.size()) { continue; }

            std::wstring result = ResultText(_scripts[scriptIndex]);
            ListView_SetItemText(_list, index, COLUMN_RESULT, result.data());
        }
    }

    void MainWindow::UpdateDetails()
    {
        const ListedScript* script = SelectedScript();
        if (script == nullptr)
        {
            SetWindowTextW(_title, _scriptsError.empty() ? L"スクリプトを選んでください" : L"登録を読めませんでした");
            SetWindowTextW(_path, L"");
            SetWindowTextW(_description, _scriptsError.c_str());
            SendMessageW(_presets, CB_RESETCONTENT, 0, 0);
            _shownPresetsId.clear();
            UpdateButtons();
            return;
        }

        const ScriptEntry& entry = script->Entry;
        SetWindowTextW(_title, Utf8ToWide(entry.Name).c_str());

        // 引数の組の一覧は、選んだスクリプトが変わったときだけ作り直す。
        int presetIndex = static_cast<int>(SendMessageW(_presets, CB_GETCURSEL, 0, 0));
        if (_shownPresetsId != entry.Id)
        {
            _shownPresetsId = entry.Id;
            SendMessageW(_presets, CB_RESETCONTENT, 0, 0);
            for (const ScriptPreset& preset : entry.Presets)
            {
                std::wstring label = Utf8ToWide(preset.Label);
                if (!preset.Args.empty()) { label += L"（" + Utf8ToWide(JoinArguments(preset.Args)) + L"）"; }

                SendMessageW(_presets, CB_ADDSTRING, 0, reinterpret_cast<LPARAM>(label.c_str()));
            }

            presetIndex = 0;
            SendMessageW(_presets, CB_SETCURSEL, 0, 0);
        }

        if (presetIndex < 0) { presetIndex = 0; }

        std::size_t index = static_cast<std::size_t>(presetIndex);
        std::wstring path = Utf8ToWide(entry.WindowsPath) + L"　・　" + DescribeMode(entry.ModeFor(index), entry.IsElevated) +
            L"　・　" + Utf8ToWide(FormatScriptRunCommand(entry.Id, index));
        SetWindowTextW(_path, path.c_str());

        std::wstring description = ToControlNewlines(Utf8ToWide(entry.Description));
        if (!script->Exists)
        {
            description = (entry.IsLocalOnly
                ? L"【この PC にはありません】" + Utf8ToWide(entry.WindowsPath) + L" を置くと使えます。\r\n"
                : L"【ファイルが見つかりません】ブランチを切り替えた・ファイルを消したなどで無くなっています。\r\n") + description;
        }

        SetWindowTextW(_description, description.c_str());
        UpdateButtons();
    }

    void MainWindow::UpdateButtons()
    {
        const ListedScript* script = SelectedScript();
        bool canRun = script != nullptr && script->Exists;
        if (canRun)
        {
            std::size_t presetIndex = static_cast<std::size_t>(std::max(0, static_cast<int>(SendMessageW(_presets, CB_GETCURSEL, 0, 0))));
            bool isCapture = !script->Entry.IsElevated && script->Entry.ModeFor(presetIndex) == RunMode::Capture;
            if (isCapture && _runner->IsCapturing()) { canRun = false; }
        }

        EnableWindow(_runButton, canRun);
        EnableWindow(_presets, script != nullptr);
        EnableWindow(_folderButton, script != nullptr);
        EnableWindow(_stopButton, _runner != nullptr && _runner->IsCapturing());
        bool isShieldShown = script != nullptr && script->Entry.IsElevated && !_isSelfElevated;
        SendMessageW(_runButton, BCM_SETSHIELD, 0, isShieldShown ? TRUE : FALSE);
    }

    void MainWindow::UpdateStatusText()
    {
        std::wstring text;
        if (_isStatusLoading && _statusLoadedAt == 0)
        {
            text = L"読み込み中…";
        }
        else if (!_statusError.empty())
        {
            text = L"状態を読めません: " + _statusError;
        }
        else if (!_status.GitError.empty())
        {
            text = L"git で調べられません: " + Utf8ToWide(_status.GitError);
        }
        else
        {
            text = L"ブランチ: " + Utf8ToWide(_status.Branch);
            if (!_status.Upstream.empty())
            {
                text += L"（" + Utf8ToWide(_status.Upstream) + L" より " + std::to_wstring(_status.Behind) + L" 遅れ・" +
                    std::to_wstring(_status.Ahead) + L" 進み。最後に fetch した時点）";
            }

            text += L"\r\nコミットしていない変更: " + std::to_wstring(_status.Changes) + L" 件\r\nサブモジュール: ";
            if (_status.Submodules.empty()) { text += L"なし"; }

            for (std::size_t i = 0; i < _status.Submodules.size(); ++i)
            {
                if (i > 0) { text += L"　・　"; }
                text += Utf8ToWide(_status.Submodules[i].Path) + L" " + DescribeSubmoduleState(_status.Submodules[i].State);
            }
        }

        if (_statusError.empty() && !(_isStatusLoading && _statusLoadedAt == 0))
        {
            text += L"\r\nUnity: " + (_status.UnityVersion.empty() ? std::wstring(L"版を読めません") : Utf8ToWide(_status.UnityVersion)) +
                (_status.UnityCliPath.empty() ? L"（Unity CLI が無い。「プロジェクトを初期化する」で入れられる）" : L"（Unity CLI あり）");
        }

        if (_isStatusLoading && _statusLoadedAt != 0) { text += L"\r\n（読み直し中…）"; }

        SetWindowTextW(_statusText, text.c_str());
    }

    void MainWindow::UpdateRoleText()
    {
        std::wstring text = L"役職: ";
        auto role = std::find_if(_home.Roles.begin(), _home.Roles.end(), [&](const RoleEntry& entry) { return entry.Id == _home.Role; });
        text += role != _home.Roles.end() ? Utf8ToWide(role->Label) : std::wstring(L"未設定（「設定」で選べます）");
        SetWindowTextW(_roleText, text.c_str());
    }

    void MainWindow::UpdateStatusBar()
    {
        std::wstring text;
        if (_root.empty()) { text = L"リポジトリが見つかりません"; }
        else { text = L"リポジトリ: " + _root.wstring(); }

        if (_scriptsError.empty()) { text += L"　・　スクリプト " + std::to_wstring(_scripts.size()) + L" 本"; }
        else { text += L"　・　登録を読めません"; }

        if (_isSelfElevated)
        {
            text += L"　・　このアプリが管理者で動いています。スクリプトもすべて管理者で動くので、普段は通常の権限で開いてください";
        }

        SendMessageW(_statusBar, SB_SETTEXTW, 0, reinterpret_cast<LPARAM>(text.c_str()));
    }

    const ListedScript* MainWindow::FindScript(const std::string& id) const
    {
        auto it = std::find_if(_scripts.begin(), _scripts.end(), [&](const ListedScript& script) { return script.Entry.Id == id; });
        return it == _scripts.end() ? nullptr : &*it;
    }

    const ListedScript* MainWindow::SelectedScript() const
    {
        if (_list == nullptr) { return nullptr; }

        int index = ListView_GetNextItem(_list, -1, LVNI_SELECTED);
        if (index < 0) { return nullptr; }

        LVITEMW item{};
        item.mask = LVIF_PARAM;
        item.iItem = index;
        if (!ListView_GetItem(_list, &item)) { return nullptr; }

        std::size_t scriptIndex = static_cast<std::size_t>(item.lParam);
        return scriptIndex < _scripts.size() ? &_scripts[scriptIndex] : nullptr;
    }

    std::wstring MainWindow::ResultText(const ListedScript& script) const
    {
        auto it = _results.find(script.Entry.Id);
        if (it != _results.end()) { return it->second; }

        if (!script.Exists) { return script.Entry.IsLocalOnly ? L"この PC に無い" : L"ファイルが無い"; }

        return L"";
    }

    void MainWindow::RunTile(std::size_t index)
    {
        if (index >= _home.Tiles.size()) { return; }

        const HomeTile& tile = _home.Tiles[index];
        if (tile.Page == "scripts")
        {
            ShowPage(Page::Scripts);
            return;
        }

        if (tile.Page == "settings")
        {
            ShowPage(Page::Settings);
            return;
        }

        const ListedScript* script = FindScript(tile.ScriptId);
        if (script == nullptr)
        {
            ShowError(L"スクリプト「" + Utf8ToWide(tile.ScriptId) + L"」が登録されていません。");
            return;
        }

        // 実行の途中で一覧が読み直されても困らないよう、写しを使う。
        ListedScript copy = *script;
        RunScript(copy, tile.PresetIndex);
    }

    void MainWindow::RunSelected()
    {
        const ListedScript* selected = SelectedScript();
        if (selected == nullptr || !IsWindowEnabled(_runButton)) { return; }

        ListedScript copy = *selected;
        std::size_t presetIndex = static_cast<std::size_t>(std::max(0, static_cast<int>(SendMessageW(_presets, CB_GETCURSEL, 0, 0))));
        RunScript(copy, presetIndex);
    }

    void MainWindow::RunScript(const ListedScript& script, std::size_t presetIndex)
    {
        const ScriptEntry& entry = script.Entry;
        if (presetIndex >= entry.Presets.size()) { presetIndex = 0; }

        if (!script.Exists)
        {
            ShowError(L"「" + Utf8ToWide(entry.Name) + L"」のファイル（" + Utf8ToWide(entry.WindowsPath) + L"）がこの PC にありません。");
            return;
        }

        RunMode mode = entry.ModeFor(presetIndex);
        LaunchKind kind = entry.IsElevated ? LaunchKind::Hidden : (mode == RunMode::Capture ? LaunchKind::Capture : LaunchKind::Console);
        if (kind == LaunchKind::Capture && _runner->IsCapturing())
        {
            ShowError(L"ほかの実行の出力を受けています。終わるか「止める」を押してから動かしてください。");
            return;
        }

        if (!entry.Confirm.empty())
        {
            std::wstring confirm = ToControlNewlines(Utf8ToWide(entry.Confirm));
            int answer = MessageBoxW(_window, confirm.c_str(), Utf8ToWide(entry.Name).c_str(), MB_YESNO | MB_ICONWARNING | MB_DEFBUTTON2);
            if (answer != IDYES) { return; }
        }

        // 確認は GUI で済ませたので --yes を付ける。新しいウィンドウで開くものは、終わってもすぐ閉じないよう --hold を付ける。
        std::vector<std::string> args = { "scripts", "run", entry.Id, "--preset", std::to_string(presetIndex + 1), "--yes",
            "--mode", mode == RunMode::Capture ? "capture" : "console" };
        if (kind != LaunchKind::Capture) { args.push_back("--hold"); }

        StartResult result = _runner->Start(_cli->BuildCommand(args), kind);
        if (!result.IsStarted)
        {
            ShowError(L"「" + Utf8ToWide(entry.Name) + L"」を起動できませんでした。\r\n\r\n" + result.Error);
            return;
        }

        std::wstring command = Utf8ToWide(FormatScriptRunCommand(entry.Id, presetIndex));
        std::wstring presetLabel = Utf8ToWide(entry.Presets[presetIndex].Label);
        SetCommandText(command);
        if (kind == LaunchKind::Capture)
        {
            SetWindowTextW(_output, L"");
            AppendOutput(L"> " + Utf8ToWide(entry.Name) + L"（" + presetLabel + L"）");
            AppendOutput(L"> " + command);
            AppendOutput(L"");
        }
        else
        {
            AppendOutput(L"――「" + Utf8ToWide(entry.Name) + L"（" + presetLabel + L"）」を" +
                (entry.IsElevated ? L"管理者として" : L"") + L"新しいウィンドウで開きました（" + command + L"）――");
        }

        _runs[result.RunId] = entry.Id;
        _results[entry.Id] = L"実行中";
        UpdateListResults();
        UpdateButtons();
    }

    void MainWindow::StopCapture()
    {
        if (!_runner->IsCapturing()) { return; }

        _stopRequestedRunId = _runner->CaptureRunId();
        _runner->StopCapture();
    }

    void MainWindow::OpenScriptFolder()
    {
        const ListedScript* script = SelectedScript();
        if (script == nullptr || _root.empty()) { return; }

        std::filesystem::path folder = (_root / std::filesystem::path(Utf8ToWide(script->Entry.WindowsPath))).parent_path();
        std::error_code error;
        if (!std::filesystem::is_directory(folder, error)) { folder = _root; }

        ShellExecuteW(_window, L"open", folder.c_str(), nullptr, nullptr, SW_SHOWNORMAL);
    }

    void MainWindow::SaveRole()
    {
        int selected = static_cast<int>(SendMessageW(_roleCombo, CB_GETCURSEL, 0, 0));
        std::vector<std::string> args;
        std::string command;
        if (selected <= 0 || static_cast<std::size_t>(selected) > _home.Roles.size())
        {
            args = { "config", "unset", "role" };
            command = "conductor config unset role";
        }
        else
        {
            const std::string& role = _home.Roles[static_cast<std::size_t>(selected - 1)].Id;
            args = { "config", "set", "role", role };
            command = "conductor config set role " + role;
        }

        CliOutput output = _cli->Run(args);
        SetCommandText(Utf8ToWide(command));
        if (!output.IsSuccess())
        {
            ShowError(L"役職を保存できませんでした。\r\n\r\n" + DescribeCliError(output));
            return;
        }

        LoadHome();
        AppendOutput(L"―― 役職を保存しました（" + Utf8ToWide(command) + L"）――");
        ShowPage(Page::Home);
    }

    void MainWindow::OnRunOutput(WPARAM runId, std::wstring* line)
    {
        std::unique_ptr<std::wstring> owned(line);
        (void)runId;
        AppendOutput(*owned);
    }

    void MainWindow::OnRunExited(WPARAM runId, DWORD exitCode)
    {
        bool isCaptureRun = _runner->IsCapturing() && runId == _runner->CaptureRunId();
        _runner->OnExited(runId);

        auto it = _runs.find(runId);
        if (it == _runs.end()) { return; }

        const ListedScript* script = FindScript(it->second);
        bool isElevated = script != nullptr && script->Entry.IsElevated;
        std::wstring result;
        if (runId == _stopRequestedRunId) { result = L"止めた"; }
        else if (exitCode == 0) { result = L"成功"; }
        else if (isElevated && exitCode == static_cast<DWORD>(EXIT_CANCELLED)) { result = L"取り消した"; }
        else { result = L"失敗（終了コード " + std::to_wstring(static_cast<long>(exitCode)) + L"）"; }

        _results[it->second] = result;
        if (isCaptureRun)
        {
            AppendOutput(L"");
            AppendOutput(L"―― " + result + L" ――");
        }
        else
        {
            std::wstring name = script != nullptr ? Utf8ToWide(script->Entry.Name) : Utf8ToWide(it->second);
            AppendOutput(L"――「" + name + L"」が終わりました: " + result + L" ――");
        }

        _runs.erase(it);
        if (runId == _stopRequestedRunId) { _stopRequestedRunId = 0; }

        UpdateListResults();
        UpdateButtons();

        // スクリプトで状態が変わることが多い（初期化・ブランチ削除など）ので、読み直す。
        StartStatusRefresh();
    }

    void MainWindow::SetCommandText(const std::wstring& command)
    {
        std::wstring text = command.empty()
            ? L"ボタンを押すと、同じことをする CLI のコマンドがここに出ます。"
            : L"同じことをするコマンド:  > " + command;
        SetWindowTextW(_commandText, text.c_str());
    }

    void MainWindow::AppendOutput(const std::wstring& line)
    {
        std::wstring text = line + L"\r\n";
        int length = GetWindowTextLengthW(_output);
        SendMessageW(_output, EM_SETSEL, length, length);
        SendMessageW(_output, EM_REPLACESEL, FALSE, reinterpret_cast<LPARAM>(text.c_str()));
    }

    void MainWindow::ShowError(const std::wstring& message) const
    {
        MessageBoxW(_window, message.c_str(), WINDOW_TITLE, MB_OK | MB_ICONERROR);
    }
}
