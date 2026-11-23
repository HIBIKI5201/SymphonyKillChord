#include "HostWindow.h"

#include "ComHandler.h"
#include "RepositoryRoot.h"
#include "TextConvert.h"

#include <WebView2.h>
#include <dwmapi.h>
#include <shellapi.h>
#include <shobjidl.h>

#include <nlohmann/json.hpp>

#include <cstdint>
#include <cstring>
#include <cwchar>
#include <optional>
#include <thread>
#include <vector>

namespace skc::conductor::gui
{
    namespace
    {
        using Json = nlohmann::json;
        using win32::Utf8ToWide;
        using win32::WideToUtf8;

        /// <summary>ウィンドウクラスの名前。</summary>
        constexpr wchar_t WINDOW_CLASS[] = L"KillChordConductorHost";

        /// <summary>ウィンドウの題。</summary>
        constexpr wchar_t WINDOW_TITLE[] = L"KillChord Conductor";

        /// <summary>CLI の exe 名（GUI と同じフォルダに置く）。</summary>
        constexpr wchar_t CONDUCTOR_EXE_NAME[] = L"conductor.exe";

        /// <summary>画面のファイルを置くフォルダ（リポジトリのルートから）。</summary>
        constexpr wchar_t UI_RELATIVE_PATH[] = L"Conductor\\ui";

        /// <summary>WebView2Loader.dll の置き場所（リポジトリのルートから）。</summary>
        constexpr wchar_t LOADER_RELATIVE_PATH[] = L"Conductor\\third_party\\webview2\\x64\\WebView2Loader.dll";

        /// <summary>画面のファイルを見せる仮のホスト名（ネットには出ない）。</summary>
        constexpr wchar_t UI_HOST[] = L"conductor.example";

        /// <summary>画面の入口。</summary>
        constexpr wchar_t UI_START_URL[] = L"https://conductor.example/index.html";

        /// <summary>WebView2 ランタイムを入れるページ。</summary>
        constexpr wchar_t RUNTIME_DOWNLOAD_URL[] = L"https://go.microsoft.com/fwlink/p/?LinkId=2124703";

        /// <summary>別スレッドから画面へ JSON を送る。lParam は new した std::string*。</summary>
        constexpr UINT WM_APP_POST_JSON = WM_APP + 20;

        /// <summary>ファイル・フォルダを選ぶ画面を出す。lParam は new した std::string*（頼みの JSON）。</summary>
        constexpr UINT WM_APP_PICK = WM_APP + 21;

        /// <summary>ウィンドウの最小の大きさ（96 DPI のとき）。</summary>
        constexpr int MIN_WIDTH = 760;
        constexpr int MIN_HEIGHT = 520;

        /// <summary>最初のウィンドウの大きさ（96 DPI のとき）。</summary>
        constexpr int START_WIDTH = 1180;
        constexpr int START_HEIGHT = 780;

        /// <summary>DWMWA_USE_IMMERSIVE_DARK_MODE（古い SDK に無いので数で書く）。</summary>
        constexpr DWORD DARK_MODE_ATTRIBUTE = 20;

        using CreateEnvironmentFunction = HRESULT(STDAPICALLTYPE*)(PCWSTR, PCWSTR, ICoreWebView2EnvironmentOptions*,
            ICoreWebView2CreateCoreWebView2EnvironmentCompletedHandler*);

        /// <summary>
        ///     この exe のあるフォルダを返す。
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
        ///     環境変数を読む。無ければ空。
        /// </summary>
        std::wstring GetEnvironment(const wchar_t* name)
        {
            std::vector<wchar_t> buffer(32768);
            DWORD length = GetEnvironmentVariableW(name, buffer.data(), static_cast<DWORD>(buffer.size()));
            return length > 0 && length < buffer.size() ? std::wstring(buffer.data(), length) : std::wstring();
        }

        /// <summary>
        ///     JSON を文字列にする。UTF-8 として壊れたところは置き換える。
        /// </summary>
        std::string Dump(const Json& json)
        {
            return json.dump(-1, ' ', false, Json::error_handler_t::replace);
        }

        /// <summary>
        ///     Windows がダークモードかを返す（アプリの色の設定）。
        /// </summary>
        bool IsSystemDark()
        {
            DWORD value = 1;
            DWORD size = sizeof(value);
            LSTATUS status = RegGetValueW(HKEY_CURRENT_USER, L"Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize",
                L"AppsUseLightTheme", RRF_RT_REG_DWORD, nullptr, &value, &size);
            return status == ERROR_SUCCESS && value == 0;
        }

        /// <summary>
        ///     WebView2 の文字列（CoTaskMemAlloc されたもの）を受け取って解放する。
        /// </summary>
        std::string TakeString(LPWSTR text)
        {
            if (text == nullptr) { return std::string(); }

            std::string result = WideToUtf8(text);
            CoTaskMemFree(text);
            return result;
        }

        /// <summary>
        ///     HRESULT を短い文にする。
        /// </summary>
        std::wstring DescribeResult(HRESULT result)
        {
            wchar_t* buffer = nullptr;
            DWORD length = FormatMessageW(
                FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
                nullptr, static_cast<DWORD>(result), 0, reinterpret_cast<wchar_t*>(&buffer), 0, nullptr);
            std::wstring message = length > 0 ? std::wstring(buffer, length) : std::wstring();
            if (buffer != nullptr) { LocalFree(buffer); }

            while (!message.empty() && (message.back() == L'\r' || message.back() == L'\n')) { message.pop_back(); }

            wchar_t code[16];
            swprintf(code, 16, L"0x%08X", static_cast<unsigned int>(result));
            return message + L"（" + code + L"）";
        }
    }

    HostWindow::HostWindow() = default;

    HostWindow::~HostWindow()
    {
        if (_webView != nullptr) { _webView->Release(); }
        if (_controller != nullptr) { _controller->Release(); }
        if (_loader != nullptr) { FreeLibrary(_loader); }
    }

    bool HostWindow::Create(HINSTANCE instance, int showCommand)
    {
        WNDCLASSEXW windowClass{};
        windowClass.cbSize = sizeof(windowClass);
        windowClass.lpfnWndProc = WindowProcedure;
        windowClass.hInstance = instance;
        windowClass.hCursor = LoadCursorW(nullptr, IDC_ARROW);
        windowClass.hIcon = LoadIconW(instance, MAKEINTRESOURCEW(1));
        windowClass.hIconSm = windowClass.hIcon;
        windowClass.hbrBackground = CreateSolidBrush(IsSystemDark() ? RGB(17, 19, 24) : RGB(246, 247, 250));
        windowClass.lpszClassName = WINDOW_CLASS;
        if (RegisterClassExW(&windowClass) == 0) { return false; }

        std::filesystem::path exeDirectory = ExecutableDirectory();
        std::optional<std::filesystem::path> root = FindRepositoryRoot(exeDirectory);
        if (!root.has_value()) { root = FindRepositoryRoot(std::filesystem::current_path()); }
        if (root.has_value()) { _root = *root; }

        _cli = std::make_unique<CliClient>(exeDirectory / CONDUCTOR_EXE_NAME, _root);

        UINT dpi = GetDpiForSystem();
        _window = CreateWindowExW(0, WINDOW_CLASS, WINDOW_TITLE, WS_OVERLAPPEDWINDOW,
            CW_USEDEFAULT, CW_USEDEFAULT, MulDiv(START_WIDTH, dpi, 96), MulDiv(START_HEIGHT, dpi, 96),
            nullptr, nullptr, instance, this);
        if (_window == nullptr) { return false; }

        _runner = std::make_unique<ProcessRunner>(_window);
        ApplyTitleBarTheme();
        ShowWindow(_window, showCommand);
        UpdateWindow(_window);

        if (_root.empty())
        {
            ShowStartupError(L"リポジトリが見つかりません。KillChordConductor.exe は、リポジトリのルートに置いたまま使ってください。", false);
            return true;
        }

        _uiFolder = _root / UI_RELATIVE_PATH;
        StartWebView();
        return true;
    }

    LRESULT CALLBACK HostWindow::WindowProcedure(HWND window, UINT message, WPARAM wParam, LPARAM lParam)
    {
        if (message == WM_NCCREATE)
        {
            auto* create = reinterpret_cast<CREATESTRUCTW*>(lParam);
            auto* self = static_cast<HostWindow*>(create->lpCreateParams);
            self->_window = window;
            SetWindowLongPtrW(window, GWLP_USERDATA, reinterpret_cast<LONG_PTR>(self));
        }

        auto* self = reinterpret_cast<HostWindow*>(GetWindowLongPtrW(window, GWLP_USERDATA));
        if (self == nullptr) { return DefWindowProcW(window, message, wParam, lParam); }

        return self->HandleMessage(message, wParam, lParam);
    }

    LRESULT HostWindow::HandleMessage(UINT message, WPARAM wParam, LPARAM lParam)
    {
        switch (message)
        {
            case WM_SIZE:
                ResizeWebView();
                return 0;

            case WM_DPICHANGED:
            {
                const RECT* suggested = reinterpret_cast<const RECT*>(lParam);
                SetWindowPos(_window, nullptr, suggested->left, suggested->top, suggested->right - suggested->left,
                    suggested->bottom - suggested->top, SWP_NOZORDER | SWP_NOACTIVATE);
                return 0;
            }

            case WM_GETMINMAXINFO:
            {
                UINT dpi = GetDpiForWindow(_window);
                auto* info = reinterpret_cast<MINMAXINFO*>(lParam);
                info->ptMinTrackSize.x = MulDiv(MIN_WIDTH, dpi, 96);
                info->ptMinTrackSize.y = MulDiv(MIN_HEIGHT, dpi, 96);
                return 0;
            }

            case WM_ACTIVATE:
                // 前に出てきたら、画面に状態を読み直してもらう（ほかで git やファイルを触ったかもしれない）。
                if (LOWORD(wParam) != WA_INACTIVE) { PostJson(R"({"type":"focus"})"); }
                if (_controller != nullptr && LOWORD(wParam) != WA_INACTIVE) { _controller->MoveFocus(COREWEBVIEW2_MOVE_FOCUS_REASON_PROGRAMMATIC); }
                return 0;

            case WM_SETTINGCHANGE:
                if (lParam != 0 && lstrcmpiW(reinterpret_cast<const wchar_t*>(lParam), L"ImmersiveColorSet") == 0) { ApplyTitleBarTheme(); }
                return 0;

            case WM_APP_RUN_OUTPUT:
            {
                std::unique_ptr<std::wstring> line(reinterpret_cast<std::wstring*>(lParam));
                PostJson(Dump(Json{ { "type", "output" }, { "runId", static_cast<std::uint64_t>(wParam) }, { "line", WideToUtf8(*line) } }));
                return 0;
            }

            case WM_APP_RUN_EXITED:
                _runner->OnExited(wParam);
                PostJson(Dump(Json{ { "type", "exited" }, { "runId", static_cast<std::uint64_t>(wParam) },
                    { "exitCode", static_cast<std::uint32_t>(lParam) } }));
                return 0;

            case WM_APP_POST_JSON:
            {
                std::unique_ptr<std::string> json(reinterpret_cast<std::string*>(lParam));
                PostJson(*json);
                return 0;
            }

            case WM_APP_PICK:
            {
                std::unique_ptr<std::string> request(reinterpret_cast<std::string*>(lParam));
                ShowPicker(*request);
                return 0;
            }

            case WM_DESTROY:
                if (_controller != nullptr) { _controller->Close(); }
                PostQuitMessage(0);
                return 0;

            default:
                return DefWindowProcW(_window, message, wParam, lParam);
        }
    }

    bool HostWindow::StartWebView()
    {
        std::error_code error;
        if (!std::filesystem::is_regular_file(_uiFolder / L"index.html", error))
        {
            ShowStartupError(L"画面のファイル（" + (_uiFolder / L"index.html").wstring() + L"）がありません。リポジトリを最新にしてください。", false);
            return false;
        }

        // 置き場所を決めて読む（exe の隣にあればそちらを先に使う）。
        std::filesystem::path besideExe = ExecutableDirectory() / L"WebView2Loader.dll";
        _loader = LoadLibraryExW(besideExe.c_str(), nullptr, LOAD_WITH_ALTERED_SEARCH_PATH);
        if (_loader == nullptr) { _loader = LoadLibraryExW((_root / LOADER_RELATIVE_PATH).c_str(), nullptr, LOAD_WITH_ALTERED_SEARCH_PATH); }
        if (_loader == nullptr)
        {
            ShowStartupError(L"WebView2Loader.dll が見つかりません（" + (_root / LOADER_RELATIVE_PATH).wstring() + L"）。リポジトリを最新にしてください。", false);
            return false;
        }

        auto createEnvironment = reinterpret_cast<CreateEnvironmentFunction>(
            reinterpret_cast<void*>(GetProcAddress(_loader, "CreateCoreWebView2EnvironmentWithOptions")));
        if (createEnvironment == nullptr)
        {
            ShowStartupError(L"WebView2Loader.dll が古いか壊れています。リポジトリを最新にしてください。", false);
            return false;
        }

        // 閲覧の記録などは、リポジトリでなくこの PC のアプリ用フォルダに置く。
        std::wstring localAppData = GetEnvironment(L"LOCALAPPDATA");
        std::filesystem::path dataFolder = localAppData.empty() ? std::filesystem::temp_directory_path() / L"KillChordConductor"
            : std::filesystem::path(localAppData) / L"KillChordConductor" / L"WebView2";

        auto* handler = new ComHandler<ICoreWebView2CreateCoreWebView2EnvironmentCompletedHandler, HRESULT, ICoreWebView2Environment*>(
            IID_ICoreWebView2CreateCoreWebView2EnvironmentCompletedHandler,
            [this](HRESULT result, ICoreWebView2Environment* environment) -> HRESULT
            {
                if (FAILED(result) || environment == nullptr)
                {
                    ShowStartupError(L"WebView2 ランタイムを使えません: " + DescribeResult(result), true);
                    return S_OK;
                }

                auto* controllerHandler = new ComHandler<ICoreWebView2CreateCoreWebView2ControllerCompletedHandler, HRESULT, ICoreWebView2Controller*>(
                    IID_ICoreWebView2CreateCoreWebView2ControllerCompletedHandler,
                    [this](HRESULT controllerResult, ICoreWebView2Controller* controller) -> HRESULT
                    {
                        if (FAILED(controllerResult) || controller == nullptr)
                        {
                            ShowStartupError(L"画面を作れませんでした: " + DescribeResult(controllerResult), true);
                            return S_OK;
                        }

                        OnWebViewReady(controller);
                        return S_OK;
                    });
                environment->CreateCoreWebView2Controller(_window, controllerHandler);
                controllerHandler->Release();
                return S_OK;
            });

        HRESULT result = createEnvironment(nullptr, dataFolder.c_str(), nullptr, handler);
        handler->Release();
        if (FAILED(result))
        {
            ShowStartupError(L"WebView2 ランタイムが見つかりません: " + DescribeResult(result), true);
            return false;
        }

        return true;
    }

    void HostWindow::OnWebViewReady(ICoreWebView2Controller* controller)
    {
        _controller = controller;
        _controller->AddRef();
        _controller->get_CoreWebView2(&_webView);

        // 読み込み中に白く光らないよう、背景を画面の色に合わせる。
        ICoreWebView2Controller2* controller2 = nullptr;
        if (SUCCEEDED(_controller->QueryInterface(IID_ICoreWebView2Controller2, reinterpret_cast<void**>(&controller2))))
        {
            bool isDark = IsSystemDark();
            controller2->put_DefaultBackgroundColor(COREWEBVIEW2_COLOR{ 255, static_cast<BYTE>(isDark ? 17 : 246),
                static_cast<BYTE>(isDark ? 19 : 247), static_cast<BYTE>(isDark ? 24 : 250) });
            controller2->Release();
        }

        ICoreWebView2Settings* settings = nullptr;
        if (SUCCEEDED(_webView->get_Settings(&settings)))
        {
            bool isDebug = GetEnvironment(L"KILLCHORD_CONDUCTOR_DEVTOOLS") == L"1";
            settings->put_AreDevToolsEnabled(isDebug);
            settings->put_AreDefaultContextMenusEnabled(isDebug);
            settings->put_IsStatusBarEnabled(FALSE);
            settings->put_IsZoomControlEnabled(FALSE);
            settings->put_IsWebMessageEnabled(TRUE);
            settings->Release();
        }

        // 画面のファイルは、仮のホスト名で Conductor/ui から読む（file:// より制限が少なく、外へは出ない）。
        ICoreWebView2_3* webView3 = nullptr;
        if (FAILED(_webView->QueryInterface(IID_ICoreWebView2_3, reinterpret_cast<void**>(&webView3))))
        {
            ShowStartupError(L"WebView2 ランタイムが古すぎます。Windows Update か、WebView2 ランタイムを入れ直してください。", true);
            return;
        }

        webView3->SetVirtualHostNameToFolderMapping(UI_HOST, _uiFolder.c_str(), COREWEBVIEW2_HOST_RESOURCE_ACCESS_KIND_DENY_CORS);
        webView3->Release();

        EventRegistrationToken token{};
        _webView->add_WebMessageReceived(
            new ComHandler<ICoreWebView2WebMessageReceivedEventHandler, ICoreWebView2*, ICoreWebView2WebMessageReceivedEventArgs*>(
                IID_ICoreWebView2WebMessageReceivedEventHandler,
                [this](ICoreWebView2*, ICoreWebView2WebMessageReceivedEventArgs* args) -> HRESULT
                {
                    LPWSTR text = nullptr;
                    if (SUCCEEDED(args->get_WebMessageAsJson(&text))) { OnWebMessage(TakeString(text)); }

                    return S_OK;
                }),
            &token);

        // 画面の外へは移らない（リンクは CLI の docs open でブラウザに開く）。
        _webView->add_NavigationStarting(
            new ComHandler<ICoreWebView2NavigationStartingEventHandler, ICoreWebView2*, ICoreWebView2NavigationStartingEventArgs*>(
                IID_ICoreWebView2NavigationStartingEventHandler,
                [](ICoreWebView2*, ICoreWebView2NavigationStartingEventArgs* args) -> HRESULT
                {
                    LPWSTR uri = nullptr;
                    if (SUCCEEDED(args->get_Uri(&uri)))
                    {
                        std::string text = TakeString(uri);
                        if (text.rfind("https://conductor.example/", 0) != 0) { args->put_Cancel(TRUE); }
                    }

                    return S_OK;
                }),
            &token);

        _webView->add_NewWindowRequested(
            new ComHandler<ICoreWebView2NewWindowRequestedEventHandler, ICoreWebView2*, ICoreWebView2NewWindowRequestedEventArgs*>(
                IID_ICoreWebView2NewWindowRequestedEventHandler,
                [](ICoreWebView2*, ICoreWebView2NewWindowRequestedEventArgs* args) -> HRESULT
                {
                    args->put_Handled(TRUE);
                    return S_OK;
                }),
            &token);

        ResizeWebView();
        _webView->Navigate(UI_START_URL);
        _controller->MoveFocus(COREWEBVIEW2_MOVE_FOCUS_REASON_PROGRAMMATIC);
    }

    void HostWindow::OnWebMessage(const std::string& text)
    {
        Json message;
        try { message = Json::parse(text); }
        catch (const std::exception&) { return; }

        if (!message.is_object()) { return; }

        std::string type = message.value("type", std::string());
        std::string id = message.value("id", std::string());
        std::vector<std::string> args;
        if (auto found = message.find("args"); found != message.end() && found->is_array())
        {
            for (const Json& arg : *found)
            {
                if (arg.is_string()) { args.push_back(arg.get<std::string>()); }
            }
        }

        if (type == "hello")
        {
            Reply(id, Dump(Json{
                { "root", WideToUtf8(_root.wstring()) },
                { "cli", WideToUtf8(_cli->ConductorPath().wstring()) },
                { "hasCli", _cli->Exists() },
                { "isDark", IsSystemDark() },
            }));
            return;
        }

        if (type == "cli")
        {
            if (!_cli->Exists())
            {
                Reply(id, Dump(Json{ { "started", false }, { "exitCode", -1 }, { "stdout", "" },
                    { "stderr", "conductor.exe が見つかりません（" + WideToUtf8(_cli->ConductorPath().wstring()) + "）。" } }));
                return;
            }

            // git などを呼ぶので時間がかかることがある。画面を止めないよう別のスレッドで動かす。
            CliClient client = *_cli;
            HWND window = _window;
            std::thread([client, window, id, args]()
            {
                CliOutput output = client.Run(args);
                Json reply = {
                    { "type", "reply" },
                    { "id", id },
                    { "result", {
                        { "started", output.IsStarted },
                        { "exitCode", output.IsStarted ? static_cast<int>(output.ExitCode) : -1 },
                        { "stdout", output.Output },
                        { "stderr", output.Error },
                    } },
                };
                auto* json = new std::string(Dump(reply));
                if (!PostMessageW(window, WM_APP_POST_JSON, 0, reinterpret_cast<LPARAM>(json))) { delete json; }
            }).detach();
            return;
        }

        if (type == "run")
        {
            std::string kindName = message.value("kind", std::string("console"));
            LaunchKind kind = kindName == "capture" ? LaunchKind::Capture : kindName == "hidden" ? LaunchKind::Hidden : LaunchKind::Console;
            StartResult started = _cli->Exists() ? _runner->Start(_cli->BuildCommand(args), kind) : StartResult{};
            if (!_cli->Exists()) { started.Error = L"conductor.exe が見つかりません。"; }

            Reply(id, Dump(Json{
                { "started", started.IsStarted },
                { "runId", static_cast<std::uint64_t>(started.RunId) },
                { "error", WideToUtf8(started.Error) },
            }));
            return;
        }

        if (type == "stop")
        {
            _runner->StopCapture();
            Reply(id, R"({"stopped":true})");
            return;
        }

        if (type == "pick")
        {
            // 選ぶ画面はモーダルなので、この受け口の中では出さず、ウィンドウのメッセージで出す。
            auto* request = new std::string(text);
            if (!PostMessageW(_window, WM_APP_PICK, 0, reinterpret_cast<LPARAM>(request))) { delete request; }
            return;
        }

        if (type == "copy")
        {
            // クリップボードへ写す（画面の navigator.clipboard は仮のホストだと断られることがあるため）。
            std::wstring wide = Utf8ToWide(message.value("text", std::string()));
            bool isCopied = false;
            if (OpenClipboard(_window))
            {
                EmptyClipboard();
                HGLOBAL memory = GlobalAlloc(GMEM_MOVEABLE, (wide.size() + 1) * sizeof(wchar_t));
                if (memory != nullptr)
                {
                    memcpy(GlobalLock(memory), wide.c_str(), (wide.size() + 1) * sizeof(wchar_t));
                    GlobalUnlock(memory);
                    isCopied = SetClipboardData(CF_UNICODETEXT, memory) != nullptr;
                    if (!isCopied) { GlobalFree(memory); }
                }

                CloseClipboard();
            }

            Reply(id, Dump(Json{ { "copied", isCopied } }));
            return;
        }

        Reply(id, Dump(Json{ { "error", "知らない頼みです: " + type } }));
    }

    void HostWindow::ShowPicker(const std::string& requestJson)
    {
        Json request = Json::parse(requestJson, nullptr, false);
        std::string id = request.is_object() ? request.value("id", std::string()) : std::string();
        bool isFolder = request.is_object() && request.value("kind", std::string()) == "folder";
        std::wstring title = Utf8ToWide(request.is_object() ? request.value("title", std::string()) : std::string());

        std::string picked;
        IFileOpenDialog* dialog = nullptr;
        if (SUCCEEDED(CoCreateInstance(CLSID_FileOpenDialog, nullptr, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&dialog))))
        {
            FILEOPENDIALOGOPTIONS options = 0;
            dialog->GetOptions(&options);
            dialog->SetOptions(options | FOS_FORCEFILESYSTEM | (isFolder ? FOS_PICKFOLDERS : FOS_FILEMUSTEXIST));
            if (!title.empty()) { dialog->SetTitle(title.c_str()); }
            if (!isFolder)
            {
                COMDLG_FILTERSPEC filters[] = { { L"プログラム (*.exe)", L"*.exe" }, { L"すべてのファイル", L"*.*" } };
                dialog->SetFileTypes(2, filters);
            }

            if (SUCCEEDED(dialog->Show(_window)))
            {
                IShellItem* item = nullptr;
                if (SUCCEEDED(dialog->GetResult(&item)))
                {
                    LPWSTR path = nullptr;
                    if (SUCCEEDED(item->GetDisplayName(SIGDN_FILESYSPATH, &path))) { picked = TakeString(path); }

                    item->Release();
                }
            }

            dialog->Release();
        }

        Reply(id, Dump(Json{ { "path", picked } }));
    }

    void HostWindow::PostJson(const std::string& json)
    {
        if (_webView == nullptr) { return; }

        std::wstring wide = Utf8ToWide(json);
        _webView->PostWebMessageAsJson(wide.c_str());
    }

    void HostWindow::Reply(const std::string& id, const std::string& resultJson)
    {
        Json result = Json::parse(resultJson, nullptr, false);
        PostJson(Dump(Json{ { "type", "reply" }, { "id", id }, { "result", result } }));
    }

    void HostWindow::ResizeWebView()
    {
        if (_controller == nullptr) { return; }

        RECT bounds{};
        GetClientRect(_window, &bounds);
        _controller->put_Bounds(bounds);
    }

    void HostWindow::ApplyTitleBarTheme()
    {
        BOOL isDark = IsSystemDark() ? TRUE : FALSE;
        DwmSetWindowAttribute(_window, DARK_MODE_ATTRIBUTE, &isDark, sizeof(isDark));
    }

    void HostWindow::ShowStartupError(const std::wstring& message, bool offerRuntime)
    {
        std::wstring text = message + L"\r\n\r\n同じことは、CLI（リポジトリのルートの conductor.exe）でもできます。conductor --help で使い方が出ます。";
        if (offerRuntime)
        {
            text += L"\r\n\r\nWebView2 ランタイムを入れるページを開きますか？";
            if (MessageBoxW(_window, text.c_str(), WINDOW_TITLE, MB_YESNO | MB_ICONERROR) == IDYES)
            {
                ShellExecuteW(_window, L"open", RUNTIME_DOWNLOAD_URL, nullptr, nullptr, SW_SHOWNORMAL);
            }
        }
        else
        {
            MessageBoxW(_window, text.c_str(), WINDOW_TITLE, MB_OK | MB_ICONERROR);
        }

        DestroyWindow(_window);
    }
}
