#include "HomeConfig.h"

#include "Actions.h"

#include <nlohmann/json.hpp>

#include <algorithm>
#include <set>
#include <stdexcept>

namespace skc::conductor
{
    namespace
    {
        using Json = nlohmann::json;

        /// <summary>home.json の page に書けるページ。</summary>
        constexpr const char* KNOWN_PAGES[] = { "scripts", "settings" };

        /// <summary>
        ///     必須の文字列の欄を読む。無いか文字列でなければ例外にする。
        /// </summary>
        std::string RequireString(const Json& node, const char* key, const std::string& where)
        {
            auto it = node.find(key);
            if (it == node.end() || !it->is_string() || it->get<std::string>().empty())
            {
                throw std::runtime_error(where + " に \"" + key + "\"（空でない文字列）がありません。");
            }

            return it->get<std::string>();
        }

        /// <summary>
        ///     任意の文字列の欄を読む。無ければ空。
        /// </summary>
        std::string OptionalString(const Json& node, const char* key, const std::string& where)
        {
            auto it = node.find(key);
            if (it == node.end() || it->is_null()) { return std::string(); }
            if (!it->is_string()) { throw std::runtime_error(where + " の \"" + key + "\" は文字列で書いてください。"); }

            return it->get<std::string>();
        }

        /// <summary>
        ///     役職を読む。
        /// </summary>
        std::vector<RoleEntry> ParseRoles(const Json& root)
        {
            auto roles = root.find("roles");
            if (roles == root.end() || !roles->is_array() || roles->empty())
            {
                throw std::runtime_error("\"roles\"（1つ以上の配列）がありません。");
            }

            std::vector<RoleEntry> result;
            std::set<std::string> ids;
            for (std::size_t i = 0; i < roles->size(); ++i)
            {
                const Json& node = (*roles)[i];
                std::string where = "roles[" + std::to_string(i) + "]";
                if (!node.is_object()) { throw std::runtime_error(where + " はオブジェクトで書いてください。"); }

                RoleEntry role{ RequireString(node, "id", where), RequireString(node, "label", where) };
                if (!ids.insert(role.Id).second) { throw std::runtime_error("役職の id \"" + role.Id + "\" が2回以上使われています。"); }

                result.push_back(std::move(role));
            }

            return result;
        }

        /// <summary>
        ///     ボタン1つ分を読み、スクリプトの登録・役職と照らし合わせる。
        /// </summary>
        HomeTile ParseTile(const Json& node, std::size_t index, const ScriptConfig& scripts, const std::vector<RoleEntry>& roles)
        {
            std::string where = "tiles[" + std::to_string(index) + "]";
            if (!node.is_object()) { throw std::runtime_error(where + " はオブジェクトで書いてください。"); }

            HomeTile tile;
            tile.Id = RequireString(node, "id", where);
            where += "（" + tile.Id + "）";
            tile.Title = RequireString(node, "title", where);
            tile.Note = OptionalString(node, "note", where);
            tile.ScriptId = OptionalString(node, "script", where);
            tile.PresetLabel = OptionalString(node, "preset", where);
            tile.Page = OptionalString(node, "page", where);
            tile.Action = OptionalString(node, "action", where);

            int kinds = (tile.ScriptId.empty() ? 0 : 1) + (tile.Page.empty() ? 0 : 1) + (tile.Action.empty() ? 0 : 1);
            if (kinds != 1)
            {
                throw std::runtime_error(where + " には \"script\"・\"page\"・\"action\" のどれか1つだけを書いてください。");
            }

            if (!tile.Action.empty())
            {
                if (FindBuiltinAction(tile.Action) == nullptr)
                {
                    std::string names;
                    for (const BuiltinAction& action : BuiltinActions()) { names += (names.empty() ? "" : "・") + action.Id; }

                    throw std::runtime_error(where + " の \"action\" \"" + tile.Action + "\" はありません（" + names + " のどれか）。");
                }

                if (!tile.PresetLabel.empty()) { throw std::runtime_error(where + " は組み込みの操作なので \"preset\" は書けません。"); }
            }
            else if (!tile.Page.empty())
            {
                bool isKnown = std::any_of(std::begin(KNOWN_PAGES), std::end(KNOWN_PAGES), [&](const char* page) { return tile.Page == page; });
                if (!isKnown) { throw std::runtime_error(where + " の \"page\" は \"scripts\" か \"settings\" です（\"" + tile.Page + "\" と書かれています）。"); }
                if (!tile.PresetLabel.empty()) { throw std::runtime_error(where + " はページを開くボタンなので \"preset\" は書けません。"); }
            }
            else
            {
                auto entry = std::find_if(scripts.Scripts.begin(), scripts.Scripts.end(),
                    [&](const ScriptEntry& script) { return script.Id == tile.ScriptId; });
                if (entry == scripts.Scripts.end())
                {
                    throw std::runtime_error(where + " の \"script\" \"" + tile.ScriptId + "\" が launcher-scripts.json にありません。");
                }

                std::optional<std::size_t> presetIndex = FindPresetIndex(*entry, tile.PresetLabel);
                if (!presetIndex.has_value())
                {
                    throw std::runtime_error(where + " の \"preset\" \"" + tile.PresetLabel + "\" が、スクリプト \"" + tile.ScriptId + "\" の引数の組にありません。");
                }

                tile.PresetIndex = *presetIndex;
            }

            auto tileRoles = node.find("roles");
            if (tileRoles != node.end() && !tileRoles->is_null())
            {
                if (!tileRoles->is_array()) { throw std::runtime_error(where + " の \"roles\" は文字列の配列で書いてください。"); }

                for (const Json& role : *tileRoles)
                {
                    if (!role.is_string()) { throw std::runtime_error(where + " の \"roles\" は文字列の配列で書いてください。"); }

                    std::string id = role.get<std::string>();
                    bool isKnown = std::any_of(roles.begin(), roles.end(), [&](const RoleEntry& known) { return known.Id == id; });
                    if (!isKnown) { throw std::runtime_error(where + " の役職 \"" + id + "\" が \"roles\" にありません。"); }

                    tile.Roles.push_back(std::move(id));
                }
            }

            return tile;
        }
    }

    const RoleEntry* HomeConfig::FindRole(const std::string& id) const
    {
        auto it = std::find_if(Roles.begin(), Roles.end(), [&](const RoleEntry& role) { return role.Id == id; });
        return it == Roles.end() ? nullptr : &*it;
    }

    HomeConfigResult ParseHomeConfig(const std::string& jsonText, const ScriptConfig& scripts)
    {
        HomeConfigResult result;
        try
        {
            Json root = Json::parse(jsonText);
            if (!root.is_object()) { throw std::runtime_error("ファイルの一番外側はオブジェクトにしてください。"); }

            auto version = root.find("version");
            if (version == root.end() || !version->is_number_integer()) { throw std::runtime_error("\"version\"（整数）がありません。"); }
            if (version->get<int>() != HomeConfig::SUPPORTED_VERSION)
            {
                throw std::runtime_error("\"version\" が " + std::to_string(version->get<int>()) +
                    " です。このツールは " + std::to_string(HomeConfig::SUPPORTED_VERSION) + " だけ読めます。ツールを新しくしてください。");
            }

            result.Config.Roles = ParseRoles(root);

            auto tiles = root.find("tiles");
            if (tiles == root.end() || !tiles->is_array()) { throw std::runtime_error("\"tiles\"（配列）がありません。"); }

            auto links = root.find("links");
            if (links != root.end() && !links->is_null())
            {
                if (!links->is_array()) { throw std::runtime_error("\"links\" は配列で書いてください。"); }

                std::set<std::string> linkIds;
                for (std::size_t i = 0; i < links->size(); ++i)
                {
                    const Json& node = (*links)[i];
                    std::string where = "links[" + std::to_string(i) + "]";
                    if (!node.is_object()) { throw std::runtime_error(where + " はオブジェクトで書いてください。"); }

                    HomeLink link{ RequireString(node, "id", where), RequireString(node, "title", where),
                        OptionalString(node, "note", where), RequireString(node, "url", where) };
                    if (link.Url.rfind("https://", 0) != 0) { throw std::runtime_error(where + " の \"url\" は https:// で始めてください。"); }
                    if (!linkIds.insert(link.Id).second) { throw std::runtime_error("リンクの id \"" + link.Id + "\" が2回以上使われています。"); }

                    result.Config.Links.push_back(std::move(link));
                }
            }

            std::set<std::string> ids;
            for (std::size_t i = 0; i < tiles->size(); ++i)
            {
                HomeTile tile = ParseTile((*tiles)[i], i, scripts, result.Config.Roles);
                if (!ids.insert(tile.Id).second) { throw std::runtime_error("ボタンの id \"" + tile.Id + "\" が2回以上使われています。"); }

                result.Config.Tiles.push_back(std::move(tile));
            }
        }
        catch (const nlohmann::json::exception& exception)
        {
            result.Config = HomeConfig();
            result.Error = std::string("JSON として読めません: ") + exception.what();
        }
        catch (const std::exception& exception)
        {
            result.Config = HomeConfig();
            result.Error = exception.what();
        }

        return result;
    }

    std::optional<std::size_t> FindPresetIndex(const ScriptEntry& entry, const std::string& preset)
    {
        if (preset.empty()) { return entry.Presets.empty() ? std::nullopt : std::optional<std::size_t>(0); }

        for (std::size_t i = 0; i < entry.Presets.size(); ++i)
        {
            if (entry.Presets[i].Label == preset) { return i; }
        }

        // 名前に当たらなければ、1 から数えた番号として読む。
        bool isNumber = std::all_of(preset.begin(), preset.end(), [](char c) { return c >= '0' && c <= '9'; });
        if (isNumber && preset.size() <= 4)
        {
            std::size_t number = static_cast<std::size_t>(std::stoul(preset));
            if (number >= 1 && number <= entry.Presets.size()) { return number - 1; }
        }

        return std::nullopt;
    }

    std::vector<HomeTile> OrderTilesForRole(const std::vector<HomeTile>& tiles, const std::string& role)
    {
        std::vector<HomeTile> ordered = tiles;
        if (role.empty()) { return ordered; }

        std::stable_partition(ordered.begin(), ordered.end(), [&](const HomeTile& tile)
        {
            return std::find(tile.Roles.begin(), tile.Roles.end(), role) != tile.Roles.end();
        });
        return ordered;
    }
}
