// スクリプト管理ツールの入口。
#include "MainWindow.h"

#include <windows.h>
#include <commctrl.h>
#include <objbase.h>

/// <summary>
///     ウィンドウを作り、メッセージを回す。
/// </summary>
int WINAPI wWinMain(HINSTANCE instance, HINSTANCE, PWSTR, int showCommand)
{
    INITCOMMONCONTROLSEX controls{ sizeof(INITCOMMONCONTROLSEX), ICC_LISTVIEW_CLASSES | ICC_BAR_CLASSES | ICC_STANDARD_CLASSES };
    InitCommonControlsEx(&controls);

    // ShellExecuteEx（管理者での起動・フォルダを開く）のために COM を初期化する。
    HRESULT com = CoInitializeEx(nullptr, COINIT_APARTMENTTHREADED | COINIT_DISABLE_OLE1DDE);

    int exitCode = 1;
    skc::launcher::win32::MainWindow window;
    if (skc::launcher::win32::MainWindow::RegisterWindowClass(instance) && window.Create(instance, showCommand))
    {
        MSG message{};
        while (GetMessageW(&message, nullptr, 0, 0) > 0)
        {
            // Tab でのフォーカス移動と Enter での実行を、ダイアログと同じように効かせる。
            if (IsDialogMessageW(window.Handle(), &message)) { continue; }

            TranslateMessage(&message);
            DispatchMessageW(&message);
        }

        exitCode = static_cast<int>(message.wParam);
    }

    if (SUCCEEDED(com)) { CoUninitialize(); }

    return exitCode;
}
