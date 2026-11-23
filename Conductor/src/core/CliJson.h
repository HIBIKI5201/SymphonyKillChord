#pragma once

#include "HomeConfig.h"
#include "ProjectStatus.h"
#include "ScriptEntry.h"

#include <string>
#include <vector>

namespace skc::conductor
{
    /// <summary>
    ///     conductor scripts list の1行分。
    /// </summary>
    struct ListedScript
    {
        /// <summary>登録の中身。</summary>
        ScriptEntry Entry;

        /// <summary>動かすファイルがこの PC にあるか。</summary>
        bool Exists = false;
    };

    /// <summary>
    ///     conductor home が出すもの。
    /// </summary>
    struct HomeView
    {
        /// <summary>設定した役職の id。未設定なら空。</summary>
        std::string Role;

        /// <summary>選べる役職。</summary>
        std::vector<RoleEntry> Roles;

        /// <summary>役職に合わせて並べたボタン。</summary>
        std::vector<HomeTile> Tiles;
    };

    /// <summary>
    ///     スクリプトの一覧を JSON にする（--json の出力）。
    /// </summary>
    /// <param name="scripts">一覧。</param>
    /// <returns>JSON の本文。</returns>
    std::string ScriptListToJson(const std::vector<ListedScript>& scripts);

    /// <summary>
    ///     ScriptListToJson の出力を読む（GUI 用）。
    /// </summary>
    /// <param name="text">JSON の本文。</param>
    /// <param name="scripts">読んだ一覧。</param>
    /// <param name="error">読めなかった理由。</param>
    /// <returns>読めたら true。</returns>
    bool ScriptListFromJson(const std::string& text, std::vector<ListedScript>& scripts, std::string& error);

    /// <summary>
    ///     ホーム画面の中身を JSON にする（--json の出力）。
    /// </summary>
    /// <param name="view">中身。</param>
    /// <returns>JSON の本文。</returns>
    std::string HomeViewToJson(const HomeView& view);

    /// <summary>
    ///     HomeViewToJson の出力を読む（GUI 用）。
    /// </summary>
    /// <param name="text">JSON の本文。</param>
    /// <param name="view">読んだ中身。</param>
    /// <param name="error">読めなかった理由。</param>
    /// <returns>読めたら true。</returns>
    bool HomeViewFromJson(const std::string& text, HomeView& view, std::string& error);

    /// <summary>
    ///     プロジェクトの状態を JSON にする（--json の出力）。
    /// </summary>
    /// <param name="status">状態。</param>
    /// <returns>JSON の本文。</returns>
    std::string StatusToJson(const ProjectStatus& status);

    /// <summary>
    ///     StatusToJson の出力を読む（GUI 用）。
    /// </summary>
    /// <param name="text">JSON の本文。</param>
    /// <param name="status">読んだ状態。</param>
    /// <param name="error">読めなかった理由。</param>
    /// <returns>読めたら true。</returns>
    bool StatusFromJson(const std::string& text, ProjectStatus& status, std::string& error);

    /// <summary>
    ///     スクリプトを動かす CLI のコマンドを、画面に出す形にする（conductor scripts run id --preset 2）。
    /// </summary>
    /// <param name="scriptId">スクリプトの id。</param>
    /// <param name="presetIndex">引数の組の添字（0 から）。</param>
    /// <returns>コマンドの文字列。</returns>
    std::string FormatScriptRunCommand(const std::string& scriptId, std::size_t presetIndex);
}
