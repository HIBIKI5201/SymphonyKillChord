#include "Settings.h"

#include <nlohmann/json.hpp>

#include <algorithm>

namespace skc::conductor
{
    std::string Settings::Get(const std::string& key) const
    {
        auto it = Values.find(key);
        return it == Values.end() ? std::string() : it->second;
    }

    const std::vector<SettingKey>& KnownSettingKeys()
    {
        static const std::vector<SettingKey> KEYS = {
            { SETTING_ROLE, "役職。ホーム画面で、この役職がよく使うボタンを前に並べる（conductor home で役職の一覧が出る）" },
            { SETTING_CRI_APP, "CRI Atom Craft の exe の場所。空なら、よくある場所を探し、無ければプロジェクトのファイルを関連付けで開く" },
            { SETTING_DRIVE_FOLDER, "Google Drive（パソコン版）の SymphonyKillChord フォルダの場所。空なら G:\\マイドライブ などを探す" },
            { SETTING_UNITY_EDITOR, "Unity エディタ（Unity.exe）の場所。空なら Unity CLI か Unity Hub の場所を使う" },
        };
        return KEYS;
    }

    bool IsKnownSettingKey(const std::string& key)
    {
        const std::vector<SettingKey>& keys = KnownSettingKeys();
        return std::any_of(keys.begin(), keys.end(), [&](const SettingKey& known) { return key == known.Key; });
    }

    Settings ParseSettings(const std::string& jsonText, std::string& error)
    {
        error.clear();
        Settings settings;
        bool isBlank = std::all_of(jsonText.begin(), jsonText.end(), [](char c) { return c == ' ' || c == '\t' || c == '\r' || c == '\n'; });
        if (isBlank) { return settings; }

        try
        {
            nlohmann::json root = nlohmann::json::parse(jsonText);
            if (!root.is_object())
            {
                error = "設定ファイルの一番外側がオブジェクトではありません。";
                return settings;
            }

            for (auto it = root.begin(); it != root.end(); ++it)
            {
                if (it.value().is_string()) { settings.Values[it.key()] = it.value().get<std::string>(); }
            }
        }
        catch (const nlohmann::json::exception& exception)
        {
            error = std::string("設定ファイルを JSON として読めません: ") + exception.what();
            settings = Settings();
        }

        return settings;
    }

    std::string SerializeSettings(const Settings& settings)
    {
        nlohmann::json root = nlohmann::json::object();
        for (const auto& [key, value] : settings.Values) { root[key] = value; }

        return root.dump(2) + "\n";
    }
}
