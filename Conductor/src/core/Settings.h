#pragma once

#include <map>
#include <string>
#include <vector>

namespace skc::conductor
{
    /// <summary>
    ///     この PC だけの設定（git の管理外に置く）。キーと値はどちらも文字列。
    /// </summary>
    struct Settings
    {
        /// <summary>設定の値（キー → 値）。</summary>
        std::map<std::string, std::string> Values;

        /// <summary>
        ///     値を返す。無ければ空。
        /// </summary>
        /// <param name="key">キー。</param>
        /// <returns>値。</returns>
        std::string Get(const std::string& key) const;
    };

    /// <summary>
    ///     設定のキー1つ分の説明。
    /// </summary>
    struct SettingKey
    {
        /// <summary>キー。</summary>
        const char* Key;

        /// <summary>説明。</summary>
        const char* Description;
    };

    /// <summary>役職のキー。</summary>
    inline constexpr const char* SETTING_ROLE = "role";

    /// <summary>
    ///     書けるキーの一覧を返す。
    /// </summary>
    /// <returns>キーの一覧。</returns>
    const std::vector<SettingKey>& KnownSettingKeys();

    /// <summary>
    ///     書けるキーかを返す。
    /// </summary>
    /// <param name="key">キー。</param>
    /// <returns>書けるなら true。</returns>
    bool IsKnownSettingKey(const std::string& key);

    /// <summary>
    ///     設定ファイルの本文（UTF-8 の JSON）を読む。空やファイルが無いときは空の設定にする。
    ///     知らないキーは残す（新しい版で足したキーを、古い版で消さないため）。
    /// </summary>
    /// <param name="jsonText">本文。</param>
    /// <param name="error">読めなかった理由。読めたら空。</param>
    /// <returns>設定。読めなければ空の設定。</returns>
    Settings ParseSettings(const std::string& jsonText, std::string& error);

    /// <summary>
    ///     設定を JSON の本文にする。
    /// </summary>
    /// <param name="settings">設定。</param>
    /// <returns>本文（末尾に改行）。</returns>
    std::string SerializeSettings(const Settings& settings);
}
