#pragma once

#include "ScriptEntry.h"

#include <string>
#include <vector>

namespace skc::launcher
{
    /// <summary>
    ///     スクリプトを動かすのに使うプログラムの場所。
    /// </summary>
    struct ToolPaths
    {
        /// <summary>cmd.exe のフルパス。</summary>
        std::string CmdExe = "cmd.exe";

        /// <summary>Windows PowerShell（powershell.exe）のフルパス。</summary>
        std::string PowerShellExe = "powershell.exe";
    };

    /// <summary>
    ///     起動するプログラムと、そこに渡すコマンドライン。
    /// </summary>
    struct LaunchCommand
    {
        /// <summary>起動するプログラムのフルパス。</summary>
        std::string Application;

        /// <summary>プログラム名を除いた引数の部分（ShellExecuteEx の lpParameters にそのまま渡せる形）。</summary>
        std::string Arguments;

        /// <summary>作業フォルダ（スクリプトのあるフォルダ。ダブルクリックしたときと同じ）。</summary>
        std::string WorkingDirectory;

        /// <summary>
        ///     CreateProcess に渡すコマンドライン全体（プログラム名を含む）を返す。
        /// </summary>
        /// <returns>コマンドライン。</returns>
        std::string FullCommandLine() const;
    };

    /// <summary>
    ///     引数1つを、CommandLineToArgvW が元の文字列に戻せる形で囲む。
    ///     空白・タブ・ダブルクォートを含まなければそのまま返す。
    /// </summary>
    /// <param name="argument">引数。</param>
    /// <returns>囲んだ引数。</returns>
    std::string QuoteArgument(const std::string& argument);

    /// <summary>
    ///     引数を囲んで、空白でつなぐ。
    /// </summary>
    /// <param name="arguments">引数。</param>
    /// <returns>つないだ文字列。</returns>
    std::string JoinArguments(const std::vector<std::string>& arguments);

    /// <summary>
    ///     PowerShell の単一引用符の文字列リテラルにする（' は '' にする）。
    /// </summary>
    /// <param name="text">文字列。</param>
    /// <returns>リテラル。</returns>
    std::string QuotePowerShellLiteral(const std::string& text);

    /// <summary>
    ///     リポジトリのルートと、/ 区切りの相対パスから、Windows のフルパスを作る。
    /// </summary>
    /// <param name="repositoryRoot">リポジトリのルート（Windows のパス）。</param>
    /// <param name="relativePath">/ 区切りの相対パス。</param>
    /// <returns>\ 区切りのフルパス。</returns>
    std::string ToWindowsPath(const std::string& repositoryRoot, const std::string& relativePath);

    /// <summary>
    ///     スクリプトと引数の組から、起動するコマンドを組み立てる。
    ///     bat は cmd.exe /d /s /c で、ps1 は powershell.exe で、exe は直接動かす。
    ///     コンソールで開く ps1 は、終わったあとウィンドウがすぐ閉じないよう、Enter を待ってから終了コードを返す。
    /// </summary>
    /// <param name="entry">スクリプト。</param>
    /// <param name="presetIndex">引数の組の添字。</param>
    /// <param name="repositoryRoot">リポジトリのルート（Windows のパス）。</param>
    /// <param name="tools">cmd.exe・powershell.exe の場所。</param>
    /// <returns>起動するコマンド。</returns>
    LaunchCommand BuildLaunchCommand(
        const ScriptEntry& entry,
        std::size_t presetIndex,
        const std::string& repositoryRoot,
        const ToolPaths& tools);
}
