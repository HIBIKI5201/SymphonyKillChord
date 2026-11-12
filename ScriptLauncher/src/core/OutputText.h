#pragma once

#include <string>
#include <vector>

namespace skc::launcher
{
    /// <summary>
    ///     子プロセスの出力を、受け取った塊から行に切り分ける。
    ///     行ごとに文字コードを判定できるよう、行の途中で切れた分は次の塊まで持っておく。
    /// </summary>
    class OutputLineBuffer
    {
    public:
        /// <summary>
        ///     受け取った塊を足し、そろった行を返す（改行は含めない。行末の \r は外す）。
        /// </summary>
        /// <param name="chunk">受け取ったバイト列。</param>
        /// <returns>そろった行。</returns>
        std::vector<std::string> Feed(const std::string& chunk);

        /// <summary>
        ///     持っている行の途中の分を返して空にする（プロセスが終わったとき用）。
        /// </summary>
        /// <returns>残り。無ければ空。</returns>
        std::string Flush();

    private:
        std::string _pending;
    };

    /// <summary>
    ///     バイト列が正しい UTF-8 かを返す。
    ///     PowerShell は UTF-8 で出し、cmd.exe の pause などは OEM コードページ（日本語なら CP932）で出すため、行ごとに判定に使う。
    /// </summary>
    /// <param name="text">バイト列。</param>
    /// <returns>正しい UTF-8 なら true（ASCII だけでも true）。</returns>
    bool IsValidUtf8(const std::string& text);

    /// <summary>
    ///     ANSI のエスケープシーケンス（色の指定・カーソル移動・ウィンドウタイトル）を取り除く。
    /// </summary>
    /// <param name="line">1行分の文字列。</param>
    /// <returns>取り除いた文字列。</returns>
    std::string StripAnsi(const std::string& line);
}
