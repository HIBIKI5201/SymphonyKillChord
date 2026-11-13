#include "TextConvert.h"

#include "OutputText.h"

#include <windows.h>

namespace skc::launcher::win32
{
    namespace
    {
        /// <summary>
        ///     指定のコードページのバイト列を UTF-16 にする。
        /// </summary>
        std::wstring ToWide(const std::string& text, UINT codePage)
        {
            if (text.empty()) { return std::wstring(); }

            int length = MultiByteToWideChar(codePage, 0, text.data(), static_cast<int>(text.size()), nullptr, 0);
            if (length <= 0) { return std::wstring(); }

            std::wstring wide(static_cast<std::size_t>(length), L'\0');
            MultiByteToWideChar(codePage, 0, text.data(), static_cast<int>(text.size()), wide.data(), length);
            return wide;
        }
    }

    std::wstring Utf8ToWide(const std::string& text)
    {
        return ToWide(text, CP_UTF8);
    }

    std::string WideToUtf8(const std::wstring& text)
    {
        if (text.empty()) { return std::string(); }

        int length = WideCharToMultiByte(CP_UTF8, 0, text.data(), static_cast<int>(text.size()), nullptr, 0, nullptr, nullptr);
        if (length <= 0) { return std::string(); }

        std::string utf8(static_cast<std::size_t>(length), '\0');
        WideCharToMultiByte(CP_UTF8, 0, text.data(), static_cast<int>(text.size()), utf8.data(), length, nullptr, nullptr);
        return utf8;
    }

    std::wstring DecodeOutputLine(const std::string& line)
    {
        return ToWide(line, IsValidUtf8(line) ? CP_UTF8 : GetOEMCP());
    }
}
