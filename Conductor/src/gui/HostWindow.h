#pragma once

#include "CliClient.h"
#include "ProcessRunner.h"

#include <windows.h>

#include <filesystem>
#include <memory>
#include <string>

struct ICoreWebView2;
struct ICoreWebView2Controller;

namespace skc::conductor::gui
{
    /// <summary>
    ///     画面（Conductor/ui の HTML）を WebView2 で出すウィンドウ。
    ///     画面からの頼みは conductor.exe を呼んで答えるだけで、自分では何も実行しない。
    /// </summary>
    class HostWindow
    {
    public:
        HostWindow();
        ~HostWindow();

        HostWindow(const HostWindow&) = delete;
        HostWindow& operator=(const HostWindow&) = delete;

        /// <summary>
        ///     ウィンドウを作り、WebView2 の用意を始める。
        /// </summary>
        /// <param name="instance">このプログラムのインスタンス。</param>
        /// <param name="showCommand">ウィンドウの出し方。</param>
        /// <returns>作れたら true。</returns>
        bool Create(HINSTANCE instance, int showCommand);

    private:
        HWND _window = nullptr;
        HMODULE _loader = nullptr;
        ICoreWebView2Controller* _controller = nullptr;
        ICoreWebView2* _webView = nullptr;
        std::filesystem::path _root;
        std::filesystem::path _uiFolder;
        std::unique_ptr<CliClient> _cli;
        std::unique_ptr<ProcessRunner> _runner;

        static LRESULT CALLBACK WindowProcedure(HWND window, UINT message, WPARAM wParam, LPARAM lParam);
        LRESULT HandleMessage(UINT message, WPARAM wParam, LPARAM lParam);

        bool StartWebView();
        void OnWebViewReady(ICoreWebView2Controller* controller);
        void OnWebMessage(const std::string& json);
        void PostJson(const std::string& json);
        void PostJsonFromWorker(std::string json);
        void Reply(const std::string& id, const std::string& resultJson);
        void ShowPicker(const std::string& requestJson);
        void ResizeWebView();
        void ApplyTitleBarTheme();
        void ShowStartupError(const std::wstring& message, bool offerRuntime);
    };
}
