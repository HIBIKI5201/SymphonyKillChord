#pragma once

#include <map>
#include <set>
#include <string>
#include <vector>

namespace skc::conductor
{
    /// <summary>
    ///     CLI の引数を分けた結果。
    /// </summary>
    struct CliArgs
    {
        /// <summary>オプション以外の引数（コマンド名を含む）。</summary>
        std::vector<std::string> Positionals;

        /// <summary>値を取らないオプション（--json など。先頭の -- は外す）。</summary>
        std::set<std::string> Flags;

        /// <summary>値を取るオプション（--preset 2 など。先頭の -- は外す）。</summary>
        std::map<std::string, std::string> Values;

        /// <summary>分けられなかった理由。分けられたら空。</summary>
        std::string Error;

        /// <summary>
        ///     値を取らないオプションが付いているかを返す。
        /// </summary>
        /// <param name="name">オプション名（-- を除く）。</param>
        /// <returns>付いていれば true。</returns>
        bool Has(const std::string& name) const { return Flags.count(name) > 0; }

        /// <summary>
        ///     値を取るオプションの値を返す。
        /// </summary>
        /// <param name="name">オプション名（-- を除く）。</param>
        /// <returns>値。付いていなければ空。</returns>
        std::string Value(const std::string& name) const;

        /// <summary>
        ///     n 番目（0 から）のオプション以外の引数を返す。
        /// </summary>
        /// <param name="index">番号。</param>
        /// <returns>引数。無ければ空。</returns>
        std::string Positional(std::size_t index) const;
    };

    /// <summary>
    ///     CLI の引数を分ける。知らないオプションと、値の無い値オプションは失敗にする。
    ///     -h は --help、-y は --yes と同じ。--name=value の書き方もできる。
    /// </summary>
    /// <param name="args">プログラム名を除いた引数（UTF-8）。</param>
    /// <returns>分けた結果。</returns>
    CliArgs ParseCliArgs(const std::vector<std::string>& args);
}
