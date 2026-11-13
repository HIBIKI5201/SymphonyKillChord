#pragma once

#include <string>

namespace skc::launcher::win32
{
    /// <summary>
    ///     UTF-8 を UTF-16 にする。
    /// </summary>
    /// <param name="text">UTF-8 の文字列。</param>
    /// <returns>UTF-16 の文字列。</returns>
    std::wstring Utf8ToWide(const std::string& text);

    /// <summary>
    ///     UTF-16 を UTF-8 にする。
    /// </summary>
    /// <param name="text">UTF-16 の文字列。</param>
    /// <returns>UTF-8 の文字列。</returns>
    std::string WideToUtf8(const std::wstring& text);

    /// <summary>
    ///     子プロセスが出した1行を UTF-16 にする。
    ///     正しい UTF-8 なら UTF-8 として、そうでなければ OEM コードページ（日本語の Windows なら CP932）として読む。
    /// </summary>
    /// <param name="line">1行分のバイト列（ANSI のエスケープは外したもの）。</param>
    /// <returns>UTF-16 の文字列。</returns>
    std::wstring DecodeOutputLine(const std::string& line);
}
