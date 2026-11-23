#pragma once

#include "ScriptConfig.h"

#include <optional>
#include <string>
#include <vector>

namespace skc::conductor
{
    /// <summary>
    ///     役職（ホーム画面のボタンの並び順を決める）。
    /// </summary>
    struct RoleEntry
    {
        /// <summary>識別子（programmer など）。設定の role に入れる値。</summary>
        std::string Id;

        /// <summary>画面に出す名前。</summary>
        std::string Label;
    };

    /// <summary>
    ///     ホーム画面のボタン1つ分。home.json の tiles の1要素に当たる。
    ///     スクリプト（launcher-scripts.json の id と引数の組）か、GUI のページのどちらかを開く。
    /// </summary>
    struct HomeTile
    {
        /// <summary>重複しない識別子。</summary>
        std::string Id;

        /// <summary>ボタンの題。</summary>
        std::string Title;

        /// <summary>ボタンの下に出す説明。</summary>
        std::string Note;

        /// <summary>動かすスクリプトの id。ページを開くボタンなら空。</summary>
        std::string ScriptId;

        /// <summary>スクリプトの引数の組の名前。空なら最初の組。</summary>
        std::string PresetLabel;

        /// <summary>引数の組の添字（PresetLabel から読むときに決める）。</summary>
        std::size_t PresetIndex = 0;

        /// <summary>組み込みの操作の名前（unity-open など）。スクリプト・ページのボタンなら空。</summary>
        std::string Action;

        /// <summary>開くページ（scripts・settings）。スクリプトを動かすボタンなら空。</summary>
        std::string Page;

        /// <summary>このボタンをよく使う役職。前に並べるのに使う（隠しはしない）。</summary>
        std::vector<std::string> Roles;
    };

    /// <summary>
    ///     ホーム画面の「ドキュメント」に並べるリンク1つ分（Notion のページなど）。
    /// </summary>
    struct HomeLink
    {
        /// <summary>重複しない識別子（conductor docs open &lt;id&gt; で使う）。</summary>
        std::string Id;

        /// <summary>画面に出す名前。</summary>
        std::string Title;

        /// <summary>説明。</summary>
        std::string Note;

        /// <summary>開く URL（https:// で始まる）。</summary>
        std::string Url;
    };

    /// <summary>
    ///     home.json の中身。
    /// </summary>
    struct HomeConfig
    {
        /// <summary>このツールが読める形式の版。</summary>
        static constexpr int SUPPORTED_VERSION = 1;

        /// <summary>役職（ファイルに書いた順）。</summary>
        std::vector<RoleEntry> Roles;

        /// <summary>ボタン（ファイルに書いた順）。</summary>
        std::vector<HomeTile> Tiles;

        /// <summary>ドキュメントのリンク（ファイルに書いた順）。</summary>
        std::vector<HomeLink> Links;

        /// <summary>
        ///     役職を id で探す。
        /// </summary>
        /// <param name="id">役職の id。</param>
        /// <returns>見つかった役職。無ければ null。</returns>
        const RoleEntry* FindRole(const std::string& id) const;
    };

    /// <summary>
    ///     home.json を読んだ結果。失敗したときは Error に理由が入る。
    /// </summary>
    struct HomeConfigResult
    {
        /// <summary>読めた設定。</summary>
        HomeConfig Config;

        /// <summary>読めなかった理由。読めたら空。</summary>
        std::string Error;

        /// <summary>
        ///     読めたかを返す。
        /// </summary>
        /// <returns>読めたら true。</returns>
        bool IsSuccess() const { return Error.empty(); }
    };

    /// <summary>
    ///     home.json の本文（UTF-8）を読む。
    ///     スクリプトの登録と照らし合わせ、無いスクリプト・無い引数の組・無い役職を指していれば失敗にする。
    /// </summary>
    /// <param name="jsonText">JSON の本文。</param>
    /// <param name="scripts">launcher-scripts.json の中身。</param>
    /// <returns>読んだ結果。</returns>
    HomeConfigResult ParseHomeConfig(const std::string& jsonText, const ScriptConfig& scripts);

    /// <summary>
    ///     引数の組を、名前か 1 から数えた番号で探す。
    /// </summary>
    /// <param name="entry">スクリプト。</param>
    /// <param name="preset">組の名前か番号（空なら最初の組）。</param>
    /// <returns>組の添字。見つからなければ空。</returns>
    std::optional<std::size_t> FindPresetIndex(const ScriptEntry& entry, const std::string& preset);

    /// <summary>
    ///     役職に合わせてボタンを並べ替える。その役職がよく使うものを前に、残りを後ろに、それぞれファイルの順で並べる。
    ///     役職が空なら、ファイルの順のまま返す。
    /// </summary>
    /// <param name="tiles">ボタン。</param>
    /// <param name="role">役職の id。</param>
    /// <returns>並べ替えたボタン。</returns>
    std::vector<HomeTile> OrderTilesForRole(const std::vector<HomeTile>& tiles, const std::string& role);
}
