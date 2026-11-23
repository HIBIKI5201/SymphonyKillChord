// KillChord Conductor（GUI）の入口。画面は Conductor/ui の HTML を WebView2 で出す。
#include "HostWindow.h"

#include <windows.h>
#include <objbase.h>

int WINAPI wWinMain(HINSTANCE instance, HINSTANCE, PWSTR, int showCommand)
{
    // WebView2 とファイルを選ぶ画面は、STA の COM を要る。
    HRESULT com = CoInitializeEx(nullptr, COINIT_APARTMENTTHREADED | COINIT_DISABLE_OLE1DDE);

    int exitCode = 1;
    {
        skc::conductor::gui::HostWindow window;
        if (window.Create(instance, showCommand))
        {
            MSG message{};
            while (GetMessageW(&message, nullptr, 0, 0) > 0)
            {
                TranslateMessage(&message);
                DispatchMessageW(&message);
            }

            exitCode = static_cast<int>(message.wParam);
        }
    }

    if (SUCCEEDED(com)) { CoUninitialize(); }

    return exitCode;
}
