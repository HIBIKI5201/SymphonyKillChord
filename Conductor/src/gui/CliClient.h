#pragma once

#include "CommandLine.h"

#include <windows.h>

#include <filesystem>
#include <string>
#include <vector>

namespace skc::conductor::gui
{
    /// <summary>
    ///     CLI を動かして出力を受け取った結果。
    /// </summary>
    struct CliOutput
    {
        /// <summary>起動できたか。</summary>
        bool IsStarted = false;

        /// <summary>終了コード。</summary>
        DWORD ExitCode = 0;

        /// <summary>標準出力（UTF-8）。</summary>
        std::string Output;

        /// <summary>標準エラー（UTF-8）。起動できなかったときは、その理由。</summary>
        std::string Error;

        /// <summary>
        ///     起動でき、終了コードが 0 だったかを返す。
        /// </summary>
        /// <returns>成功なら true。</returns>
        bool IsSuccess() const { return IsStarted && ExitCode == 0; }
    };

    /// <summary>
    ///     GUI から CLI（conductor.exe）を呼ぶ役。GUI は自分では何も実行せず、すべてこれを通す。
    /// </summary>
    class CliClient
    {
    public:
        /// <summary>
        ///     CLI の場所とリポジトリのルートを決める。
        /// </summary>
        /// <param name="conductorPath">conductor.exe の場所。</param>
        /// <param name="repositoryRoot">リポジトリのルート。</param>
        CliClient(std::filesystem::path conductorPath, std::filesystem::path repositoryRoot);

        /// <summary>
        ///     conductor.exe があるかを返す。
        /// </summary>
        /// <returns>あれば true。</returns>
        bool Exists() const;

        /// <summary>
        ///     conductor.exe の場所を返す。
        /// </summary>
        /// <returns>場所。</returns>
        const std::filesystem::path& ConductorPath() const { return _conductorPath; }

        /// <summary>
        ///     CLI を起動するコマンドを組み立てる（--root を足す）。ProcessRunner に渡す用。
        /// </summary>
        /// <param name="args">conductor に渡す引数（UTF-8）。</param>
        /// <returns>起動するコマンド。</returns>
        LaunchCommand BuildCommand(const std::vector<std::string>& args) const;

        /// <summary>
        ///     CLI をウィンドウを出さずに動かし、終わるまで待って出力を受け取る（--json の読み取り用）。
        ///     呼んだスレッドを止めるので、時間のかかるもの（status）は別のスレッドから呼ぶ。
        /// </summary>
        /// <param name="args">conductor に渡す引数（UTF-8）。</param>
        /// <returns>結果。</returns>
        CliOutput Run(const std::vector<std::string>& args) const;

    private:
        std::filesystem::path _conductorPath;
        std::filesystem::path _repositoryRoot;
    };
}
