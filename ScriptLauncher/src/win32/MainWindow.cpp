#include "MainWindow.h"

#include "CommandLine.h"
#include "RepositoryRoot.h"
#include "TextConvert.h"

#include <shellapi.h>
#include <uxtheme.h>
#include <windowsx.h>

#include <algorithm>
#include <fstream>
#include <optional>
#include <sstream>
#include <vector>

namespace skc::launcher::win32
{
    namespace
    {
        constexpr wchar_t WINDOW_CLASS_NAME[] = L"SymphonyKillChordScriptLauncher";
        constexpr wchar_t WINDOW_TITLE[] = L"SymphonyKillChord スクリプト管理";

        // コントロールの ID。
        enum ControlId : int
        {
            ID_LIST = 100,
            ID_PRESETS,
            ID_RUN,
            ID_STOP,
            ID_FOLDER,
            ID_RELOAD,
            ID_OUTPUT,
            ID_STATUS,
        };

        // 一覧の列。
        constexpr int COLUMN_NAME = 0;
        constexpr int COLUMN_RESULT = 1;

        // 配置の寸法（96 DPI のときのピクセル）。
        constexpr int MARGIN = 8;
        constexpr int LIST_WIDTH = 320;
        constexpr int NAME_COLUMN_WIDTH = 200;
        constexpr int TITLE_HEIGHT = 30;
        constexpr int LINE_HEIGHT = 22;
        constexpr int DESCRIPTION_HEIGHT = 72;
        constexpr int CONTROL_HEIGHT = 26;
        constexpr int PRESET_LABEL_WIDTH = 56;
        constexpr int COMBO_DROP_HEIGHT = 200;
        constexpr int BUTTON_WIDTH = 104;
        constexpr int WIDE_BUTTON_WIDTH = 132;
        constexpr int MIN_WINDOW_WIDTH = 760;
        constexpr int MIN_WINDOW_HEIGHT = 480;
        constexpr int DEFAULT_WINDOW_WIDTH = 1040;
        constexpr int DEFAULT_WINDOW_HEIGHT = 680;
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
        ///     ファイルを丸ごと読む。先頭の BOM は外す。
        /// </summary>
        bool ReadAllText(const std::filesystem::path& path, std::string& text)
        {
            std::ifstream stream(path, std::ios::binary);
            if (!stream) { return false; }

            std::ostringstream buffer;
            buffer << stream.rdbuf();
            text = buffer.str();
            if (text.rfind("\xEF\xBB\xBF", 0) == 0) { text.erase(0, 3); }

            return true;
        }

        /// <summary>
        ///     cmd.exe・powershell.exe のフルパスを返す（PATH を書き換えられていても System32 のものを使う）。
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
        ///     動かし方の説明を返す。
        /// </summary>
        std::wstring DescribeMode(RunMode mode, bool isElevated)
        {
            std::wstring text = mode == RunMode::Console ? L"新しいウィンドウで開く" : L"出力をこの画面に出す";
            if (isElevated) { text += L"・管理者として動かす（UAC の確認が出る）"; }

            return text;
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
                if (LOWORD(wParam) != WA_INACTIVE) { ReloadConfigIfChanged(); }
                break;

            case WM_COMMAND:
                switch (LOWORD(wParam))
                {
                    case ID_RUN:
                        RunSelected();
                        return 0;
                    case ID_STOP:
                        StopCapture();
                        return 0;
                    case ID_FOLDER:
                        OpenScriptFolder();
                        return 0;
                    case ID_RELOAD:
                        LoadConfig();
                        return 0;
                    case ID_PRESETS:
                        if (HIWORD(wParam) == CBN_SELCHANGE) { UpdateDetails(); }
                        return 0;
                    default:
                        break;
                }
                break;

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

        std::optional<std::filesystem::path> root = FindRepositoryRoot(ExecutableDirectory());
        if (!root.has_value()) { root = FindRepositoryRoot(std::filesystem::current_path()); }
        if (root.has_value()) { _root = *root; }

        CreateControls();
        ApplyDpi(_dpi);
        LoadConfig();
    }

    void MainWindow::CreateControls()
    {
        auto createChild = [this](DWORD exStyle, const wchar_t* className, const wchar_t* text, DWORD style, int id)
        {
            return CreateWindowExW(exStyle, className, text, WS_CHILD | WS_VISIBLE | style, 0, 0, 0, 0,
                _window, reinterpret_cast<HMENU>(static_cast<INT_PTR>(id)), _instance, nullptr);
        };

        _list = createChild(WS_EX_CLIENTEDGE, WC_LISTVIEWW, L"",
            WS_TABSTOP | LVS_REPORT | LVS_SINGLESEL | LVS_SHOWSELALWAYS, ID_LIST);
        ListView_SetExtendedListViewStyle(_list, LVS_EX_FULLROWSELECT | LVS_EX_DOUBLEBUFFER);
        SetWindowTheme(_list, L"Explorer", nullptr);

        LVCOLUMNW column{};
        column.mask = LVCF_TEXT | LVCF_WIDTH;
        column.pszText = const_cast<wchar_t*>(L"スクリプト");
        ListView_InsertColumn(_list, COLUMN_NAME, &column);
        column.pszText = const_cast<wchar_t*>(L"前回の結果");
        ListView_InsertColumn(_list, COLUMN_RESULT, &column);
        ListView_EnableGroupView(_list, TRUE);

        _title = createChild(0, WC_STATICW, L"", SS_LEFT | SS_NOPREFIX | SS_ENDELLIPSIS, 0);
        _path = createChild(0, WC_STATICW, L"", SS_LEFT | SS_NOPREFIX | SS_PATHELLIPSIS, 0);
        _description = createChild(0, WC_STATICW, L"", SS_LEFT | SS_NOPREFIX, 0);
        _presetLabel = createChild(0, WC_STATICW, L"引数", SS_LEFT | SS_CENTERIMAGE, 0);
        _presets = createChild(0, WC_COMBOBOXW, L"", WS_TABSTOP | WS_VSCROLL | CBS_DROPDOWNLIST, ID_PRESETS);
        _runButton = createChild(0, WC_BUTTONW, L"実行", WS_TABSTOP | BS_DEFPUSHBUTTON, ID_RUN);
        _stopButton = createChild(0, WC_BUTTONW, L"止める", WS_TABSTOP | BS_PUSHBUTTON, ID_STOP);
        _folderButton = createChild(0, WC_BUTTONW, L"フォルダを開く", WS_TABSTOP | BS_PUSHBUTTON, ID_FOLDER);
        _reloadButton = createChild(0, WC_BUTTONW, L"登録を読み直す", WS_TABSTOP | BS_PUSHBUTTON, ID_RELOAD);
        _outputLabel = createChild(0, WC_STATICW, L"出力（「出力をこの画面に出す」スクリプトだけ）", SS_LEFT | SS_NOPREFIX, 0);
        _output = createChild(WS_EX_CLIENTEDGE, WC_EDITW, L"",
            WS_TABSTOP | WS_VSCROLL | WS_HSCROLL | ES_MULTILINE | ES_READONLY | ES_AUTOVSCROLL | ES_AUTOHSCROLL, ID_OUTPUT);
        SendMessageW(_output, EM_SETLIMITTEXT, 0, 0);
        _status = createChild(0, STATUSCLASSNAMEW, L"", SBARS_SIZEGRIP, ID_STATUS);
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

        for (HWND child : { _list, _path, _description, _presetLabel, _presets, _runButton, _stopButton,
                 _folderButton, _reloadButton, _outputLabel, _status })
        {
            SendMessageW(child, WM_SETFONT, reinterpret_cast<WPARAM>(_font), TRUE);
        }

        SendMessageW(_title, WM_SETFONT, reinterpret_cast<WPARAM>(_titleFont), TRUE);
        SendMessageW(_output, WM_SETFONT, reinterpret_cast<WPARAM>(_monoFont), TRUE);

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

    void MainWindow::Layout()
    {
        RECT client{};
        GetClientRect(_window, &client);

        SendMessageW(_status, WM_SIZE, 0, 0);
        RECT statusRect{};
        GetWindowRect(_status, &statusRect);
        int statusHeight = statusRect.bottom - statusRect.top;

        int margin = Scale(MARGIN);
        int width = client.right - client.left;
        int height = client.bottom - client.top - statusHeight;
        int listWidth = std::min(Scale(LIST_WIDTH), width / 2);

        MoveWindow(_list, margin, margin, listWidth, std::max(0, height - margin * 2), TRUE);
        ListView_SetColumnWidth(_list, COLUMN_RESULT, LVSCW_AUTOSIZE_USEHEADER);

        int left = margin * 2 + listWidth;
        int right = width - margin;
        int detailWidth = std::max(0, right - left);
        int y = margin;

        MoveWindow(_title, left, y, detailWidth, Scale(TITLE_HEIGHT), TRUE);
        y += Scale(TITLE_HEIGHT);
        MoveWindow(_path, left, y, detailWidth, Scale(LINE_HEIGHT), TRUE);
        y += Scale(LINE_HEIGHT);
        MoveWindow(_description, left, y, detailWidth, Scale(DESCRIPTION_HEIGHT), TRUE);
        y += Scale(DESCRIPTION_HEIGHT) + margin;

        int labelWidth = Scale(PRESET_LABEL_WIDTH);
        MoveWindow(_presetLabel, left, y, labelWidth, Scale(CONTROL_HEIGHT), TRUE);
        MoveWindow(_presets, left + labelWidth, y, std::max(0, detailWidth - labelWidth), Scale(COMBO_DROP_HEIGHT), TRUE);
        y += Scale(CONTROL_HEIGHT) + margin;

        int x = left;
        int buttonHeight = Scale(CONTROL_HEIGHT);
        MoveWindow(_runButton, x, y, Scale(BUTTON_WIDTH), buttonHeight, TRUE);
        x += Scale(BUTTON_WIDTH) + margin;
        MoveWindow(_stopButton, x, y, Scale(BUTTON_WIDTH), buttonHeight, TRUE);
        x += Scale(BUTTON_WIDTH) + margin;
        MoveWindow(_folderButton, x, y, Scale(WIDE_BUTTON_WIDTH), buttonHeight, TRUE);
        x += Scale(WIDE_BUTTON_WIDTH) + margin;
        MoveWindow(_reloadButton, x, y, Scale(WIDE_BUTTON_WIDTH), buttonHeight, TRUE);
        y += buttonHeight + margin * 2;

        MoveWindow(_outputLabel, left, y, detailWidth, Scale(LINE_HEIGHT), TRUE);
        y += Scale(LINE_HEIGHT);
        MoveWindow(_output, left, y, detailWidth, std::max(0, height - margin - y), TRUE);
    }

    int MainWindow::Scale(int value) const
    {
        return MulDiv(value, static_cast<int>(_dpi), USER_DEFAULT_SCREEN_DPI);
    }

    void MainWindow::LoadConfig()
    {
        const ScriptEntry* selected = SelectedEntry();
        std::string selectId = selected != nullptr ? selected->Id : std::string();

        _config = ScriptConfig();
        _configError.clear();
        if (_root.empty())
        {
            _configError = L"リポジトリが見つかりません。このツールは、リポジトリの中の ScriptLauncher フォルダに置いて使ってください（"
                + Utf8ToWide(SCRIPT_CONFIG_RELATIVE_PATH) + L" を上のフォルダへ順に探しています）。";
        }
        else
        {
            std::filesystem::path configPath = _root / SCRIPT_CONFIG_RELATIVE_PATH;
            std::error_code error;
            _configWriteTime = std::filesystem::last_write_time(configPath, error);

            std::string text;
            if (!ReadAllText(configPath, text))
            {
                _configError = L"登録ファイルを開けません: " + configPath.wstring();
            }
            else
            {
                ScriptConfigResult result = ParseScriptConfig(text);
                if (result.IsSuccess()) { _config = std::move(result.Config); }
                else { _configError = L"登録ファイルの書き方に誤りがあります: " + Utf8ToWide(result.Error); }
            }
        }

        PopulateList(selectId);
        UpdateStatus();
    }

    void MainWindow::ReloadConfigIfChanged()
    {
        if (_root.empty()) { return; }

        std::error_code error;
        std::filesystem::file_time_type writeTime = std::filesystem::last_write_time(_root / SCRIPT_CONFIG_RELATIVE_PATH, error);
        if (!error && writeTime != _configWriteTime) { LoadConfig(); }
    }

    void MainWindow::PopulateList(const std::string& selectId)
    {
        SendMessageW(_list, WM_SETREDRAW, FALSE, 0);
        _shownPresetsId.clear();
        ListView_DeleteAllItems(_list);
        ListView_RemoveAllGroups(_list);

        // 分類は、登録ファイルに最初に出てきた順に並べる。
        std::vector<std::string> categories;
        for (const ScriptEntry& entry : _config.Scripts)
        {
            if (std::find(categories.begin(), categories.end(), entry.Category) == categories.end())
            {
                categories.push_back(entry.Category);
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
        for (std::size_t i = 0; i < _config.Scripts.size(); ++i)
        {
            const ScriptEntry& entry = _config.Scripts[i];
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

        if (!_config.Scripts.empty())
        {
            ListView_SetItemState(_list, selectIndex, LVIS_SELECTED | LVIS_FOCUSED, LVIS_SELECTED | LVIS_FOCUSED);
            ListView_EnsureVisible(_list, selectIndex, FALSE);
        }

        UpdateDetails();
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

            std::size_t entryIndex = static_cast<std::size_t>(item.lParam);
            if (entryIndex >= _config.Scripts.size()) { continue; }

            std::wstring result = ResultText(_config.Scripts[entryIndex]);
            ListView_SetItemText(_list, index, COLUMN_RESULT, result.data());
        }
    }

    void MainWindow::UpdateDetails()
    {
        const ScriptEntry* entry = SelectedEntry();
        if (entry == nullptr)
        {
            SetWindowTextW(_title, _configError.empty() ? L"スクリプトを選んでください" : L"登録を読めませんでした");
            SetWindowTextW(_path, L"");
            SetWindowTextW(_description, _configError.c_str());
            SendMessageW(_presets, CB_RESETCONTENT, 0, 0);
            _shownPresetsId.clear();
            UpdateButtons();
            return;
        }

        SetWindowTextW(_title, Utf8ToWide(entry->Name).c_str());

        // 引数の組の一覧は、選んだスクリプトが変わったときだけ作り直す。
        int presetIndex = static_cast<int>(SendMessageW(_presets, CB_GETCURSEL, 0, 0));
        if (_shownPresetsId != entry->Id)
        {
            _shownPresetsId = entry->Id;
            SendMessageW(_presets, CB_RESETCONTENT, 0, 0);
            for (const ScriptPreset& preset : entry->Presets)
            {
                std::wstring label = Utf8ToWide(preset.Label);
                if (!preset.Args.empty()) { label += L"（" + Utf8ToWide(JoinArguments(preset.Args)) + L"）"; }

                SendMessageW(_presets, CB_ADDSTRING, 0, reinterpret_cast<LPARAM>(label.c_str()));
            }

            presetIndex = 0;
            SendMessageW(_presets, CB_SETCURSEL, 0, 0);
        }

        if (presetIndex < 0) { presetIndex = 0; }

        std::wstring path = Utf8ToWide(entry->WindowsPath) + L"　・　" +
            DescribeMode(entry->ModeFor(static_cast<std::size_t>(presetIndex)), entry->IsElevated);
        SetWindowTextW(_path, path.c_str());

        std::wstring description = ToControlNewlines(Utf8ToWide(entry->Description));
        if (!ScriptExists(*entry))
        {
            description = (entry->IsLocalOnly
                ? L"【この PC にはありません】" + Utf8ToWide(entry->WindowsPath) + L" を置くと使えます。\r\n"
                : L"【ファイルが見つかりません】ブランチを切り替えた・ファイルを消したなどで無くなっています。\r\n") + description;
        }

        SetWindowTextW(_description, description.c_str());
        UpdateButtons();
    }

    void MainWindow::UpdateButtons()
    {
        const ScriptEntry* entry = SelectedEntry();
        bool canRun = entry != nullptr && ScriptExists(*entry);
        if (canRun)
        {
            int presetIndex = std::max(0, static_cast<int>(SendMessageW(_presets, CB_GETCURSEL, 0, 0)));
            bool isCapture = !entry->IsElevated && entry->ModeFor(static_cast<std::size_t>(presetIndex)) == RunMode::Capture;
            if (isCapture && _runner->IsCapturing()) { canRun = false; }
        }

        EnableWindow(_runButton, canRun);
        EnableWindow(_presets, entry != nullptr);
        EnableWindow(_folderButton, entry != nullptr);
        EnableWindow(_stopButton, _runner != nullptr && _runner->IsCapturing());
        bool isShieldShown = entry != nullptr && entry->IsElevated && !_isSelfElevated;
        SendMessageW(_runButton, BCM_SETSHIELD, 0, isShieldShown ? TRUE : FALSE);
    }

    void MainWindow::UpdateStatus()
    {
        std::wstring text;
        if (_root.empty()) { text = L"リポジトリが見つかりません"; }
        else { text = L"リポジトリ: " + _root.wstring(); }

        if (_configError.empty()) { text += L"　・　登録 " + std::to_wstring(_config.Scripts.size()) + L" 本"; }
        else { text += L"　・　登録ファイルを読めません"; }

        if (_isSelfElevated)
        {
            text += L"　・　このツールが管理者で動いています。スクリプトもすべて管理者で動くので、普段は通常の権限で開いてください";
        }

        SendMessageW(_status, SB_SETTEXTW, 0, reinterpret_cast<LPARAM>(text.c_str()));
    }

    const ScriptEntry* MainWindow::SelectedEntry() const
    {
        if (_list == nullptr) { return nullptr; }

        int index = ListView_GetNextItem(_list, -1, LVNI_SELECTED);
        if (index < 0) { return nullptr; }

        LVITEMW item{};
        item.mask = LVIF_PARAM;
        item.iItem = index;
        if (!ListView_GetItem(_list, &item)) { return nullptr; }

        std::size_t entryIndex = static_cast<std::size_t>(item.lParam);
        return entryIndex < _config.Scripts.size() ? &_config.Scripts[entryIndex] : nullptr;
    }

    bool MainWindow::ScriptExists(const ScriptEntry& entry) const
    {
        if (_root.empty()) { return false; }

        std::error_code error;
        return std::filesystem::is_regular_file(_root / std::filesystem::path(Utf8ToWide(entry.WindowsPath)), error);
    }

    std::wstring MainWindow::ResultText(const ScriptEntry& entry) const
    {
        auto it = _results.find(entry.Id);
        if (it != _results.end()) { return it->second; }

        if (!ScriptExists(entry)) { return entry.IsLocalOnly ? L"この PC に無い" : L"ファイルが無い"; }

        return L"";
    }

    void MainWindow::RunSelected()
    {
        const ScriptEntry* selected = SelectedEntry();
        if (selected == nullptr || !IsWindowEnabled(_runButton)) { return; }

        // 確認の間に一覧が読み直されても困らないよう、写しを使う。
        ScriptEntry entry = *selected;
        int presetIndex = std::max(0, static_cast<int>(SendMessageW(_presets, CB_GETCURSEL, 0, 0)));

        if (!entry.Confirm.empty())
        {
            std::wstring confirm = ToControlNewlines(Utf8ToWide(entry.Confirm));
            int answer = MessageBoxW(_window, confirm.c_str(), Utf8ToWide(entry.Name).c_str(), MB_YESNO | MB_ICONWARNING | MB_DEFBUTTON2);
            if (answer != IDYES) { return; }
        }

        RunMode mode = entry.ModeFor(static_cast<std::size_t>(presetIndex));
        LaunchCommand command = BuildLaunchCommand(entry, static_cast<std::size_t>(presetIndex), WideToUtf8(_root.wstring()), SystemToolPaths());
        StartResult result = _runner->Start(command, mode, entry.IsElevated);
        if (!result.IsStarted)
        {
            if (!result.IsCancelled) { ShowError(L"「" + Utf8ToWide(entry.Name) + L"」を起動できませんでした。\r\n\r\n" + result.Error); }
            return;
        }

        std::wstring presetLabel = Utf8ToWide(entry.Presets[static_cast<std::size_t>(presetIndex)].Label);
        if (mode == RunMode::Capture && !entry.IsElevated)
        {
            SetWindowTextW(_output, L"");
            AppendOutput(L"> " + Utf8ToWide(entry.Name) + L"（" + presetLabel + L"）");
            AppendOutput(L"> " + Utf8ToWide(command.FullCommandLine()));
            AppendOutput(L"");
        }
        else
        {
            AppendOutput(L"――「" + Utf8ToWide(entry.Name) + L"（" + presetLabel + L"）」を" +
                (entry.IsElevated ? L"管理者として" : L"") + L"新しいウィンドウで開きました ――");
        }

        if (result.IsWatched)
        {
            _runs[result.RunId] = entry.Id;
            _results[entry.Id] = L"実行中";
        }
        else
        {
            _results[entry.Id] = L"起動した（終わりは追えない）";
        }

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
        const ScriptEntry* entry = SelectedEntry();
        if (entry == nullptr || _root.empty()) { return; }

        std::filesystem::path folder = (_root / std::filesystem::path(Utf8ToWide(entry->WindowsPath))).parent_path();
        std::error_code error;
        if (!std::filesystem::is_directory(folder, error)) { folder = _root; }

        ShellExecuteW(_window, L"open", folder.c_str(), nullptr, nullptr, SW_SHOWNORMAL);
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

        std::wstring result;
        if (runId == _stopRequestedRunId) { result = L"止めた"; }
        else if (exitCode == 0) { result = L"成功"; }
        else { result = L"失敗（終了コード " + std::to_wstring(static_cast<long>(exitCode)) + L"）"; }

        _results[it->second] = result;
        if (isCaptureRun)
        {
            AppendOutput(L"");
            AppendOutput(L"―― " + result + L" ――");
        }

        _runs.erase(it);
        if (runId == _stopRequestedRunId) { _stopRequestedRunId = 0; }

        UpdateListResults();
        UpdateButtons();
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
