#pragma once

#include "ScriptEntry.h"

#include <string>
#include <vector>

namespace skc::launcher
{
    /// <summary>
    ///     launcher-scripts.json の中身。
    /// </summary>
    struct ScriptConfig
    {
        /// <summary>このツールが読める形式の版。</summary>
        static constexpr int SUPPORTED_VERSION = 1;

        /// <summary>登録されたスクリプト（ファイルに書いた順）。</summary>
        std::vector<ScriptEntry> Scripts;
    };

    /// <summary>
    ///     設定を読んだ結果。失敗したときは Error に理由が入る。
    /// </summary>
    struct ScriptConfigResult
    {
        /// <summary>読めた設定。</summary>
        ScriptConfig Config;

        /// <summary>読めなかった理由。読めたら空。</summary>
        std::string Error;

        /// <summary>
        ///     読めたかを返す。
        /// </summary>
        /// <returns>読めたら true。</returns>
        bool IsSuccess() const { return Error.empty(); }
    };

    /// <summary>
    ///     launcher-scripts.json の本文（UTF-8）を読む。
    ///     形式の誤り（必須の欄が無い・id の重複・bat に渡せない文字を含む引数・管理者で出力を受ける組など）は失敗にする。
    /// </summary>
    /// <param name="jsonText">JSON の本文。</param>
    /// <returns>読んだ結果。</returns>
    ScriptConfigResult ParseScriptConfig(const std::string& jsonText);

    /// <summary>
    ///     ファイルの種類から、cmd.exe 経由で動かすファイルかを返す（.bat / .cmd）。
    /// </summary>
    /// <param name="path">ファイルのパス。</param>
    /// <returns>cmd.exe 経由なら true。</returns>
    bool IsBatchFile(const std::string& path);

    /// <summary>
    ///     ファイルの種類から、PowerShell で動かすファイルかを返す（.ps1）。
    /// </summary>
    /// <param name="path">ファイルのパス。</param>
    /// <returns>PowerShell なら true。</returns>
    bool IsPowerShellFile(const std::string& path);
}
