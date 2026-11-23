#include "CliJson.h"

#include <nlohmann/json.hpp>

namespace skc::conductor
{
    namespace
    {
        using Json = nlohmann::json;

        /// <summary>
        ///     動かし方を文字列にする。
        /// </summary>
        const char* ModeName(RunMode mode)
        {
            return mode == RunMode::Capture ? "capture" : "console";
        }

        /// <summary>
        ///     文字列から動かし方を読む。
        /// </summary>
        RunMode ModeFromName(const std::string& name)
        {
            return name == "capture" ? RunMode::Capture : RunMode::Console;
        }

        /// <summary>
        ///     文字列の欄を読む。無ければ空。
        /// </summary>
        std::string StringOr(const Json& node, const char* key)
        {
            auto it = node.find(key);
            return it != node.end() && it->is_string() ? it->get<std::string>() : std::string();
        }

        /// <summary>
        ///     JSON を読み、例外を理由の文に直す。
        /// </summary>
        template <typename Reader>
        bool ReadJson(const std::string& text, std::string& error, Reader reader)
        {
            error.clear();
            try
            {
                reader(Json::parse(text));
                return true;
            }
            catch (const std::exception& exception)
            {
                error = std::string("CLI の出力を読めません: ") + exception.what();
                return false;
            }
        }
    }

    std::string ScriptListToJson(const std::vector<ListedScript>& scripts)
    {
        Json list = Json::array();
        for (const ListedScript& listed : scripts)
        {
            const ScriptEntry& entry = listed.Entry;
            Json presets = Json::array();
            for (std::size_t i = 0; i < entry.Presets.size(); ++i)
            {
                presets.push_back({
                    { "label", entry.Presets[i].Label },
                    { "args", entry.Presets[i].Args },
                    { "mode", ModeName(entry.ModeFor(i)) },
                });
            }

            list.push_back({
                { "id", entry.Id },
                { "name", entry.Name },
                { "category", entry.Category },
                { "windows", entry.WindowsPath },
                { "mac", entry.MacPath },
                { "description", entry.Description },
                { "confirm", entry.Confirm },
                { "mode", ModeName(entry.Mode) },
                { "elevate", entry.IsElevated },
                { "localOnly", entry.IsLocalOnly },
                { "exists", listed.Exists },
                { "presets", presets },
            });
        }

        return Json{ { "scripts", list } }.dump(2);
    }

    bool ScriptListFromJson(const std::string& text, std::vector<ListedScript>& scripts, std::string& error)
    {
        scripts.clear();
        return ReadJson(text, error, [&](const Json& root)
        {
            for (const Json& node : root.at("scripts"))
            {
                ListedScript listed;
                ScriptEntry& entry = listed.Entry;
                entry.Id = node.at("id").get<std::string>();
                entry.Name = StringOr(node, "name");
                entry.Category = StringOr(node, "category");
                entry.WindowsPath = StringOr(node, "windows");
                entry.MacPath = StringOr(node, "mac");
                entry.Description = StringOr(node, "description");
                entry.Confirm = StringOr(node, "confirm");
                entry.Mode = ModeFromName(StringOr(node, "mode"));
                entry.IsElevated = node.value("elevate", false);
                entry.IsLocalOnly = node.value("localOnly", false);
                listed.Exists = node.value("exists", false);
                for (const Json& presetNode : node.at("presets"))
                {
                    ScriptPreset preset;
                    preset.Label = StringOr(presetNode, "label");
                    preset.Args = presetNode.value("args", std::vector<std::string>());
                    preset.Mode = ModeFromName(StringOr(presetNode, "mode"));
                    entry.Presets.push_back(std::move(preset));
                }

                scripts.push_back(std::move(listed));
            }
        });
    }

    std::string HomeViewToJson(const HomeView& view)
    {
        Json roles = Json::array();
        for (const RoleEntry& role : view.Roles) { roles.push_back({ { "id", role.Id }, { "label", role.Label } }); }

        Json tiles = Json::array();
        for (const HomeTile& tile : view.Tiles)
        {
            Json node = {
                { "id", tile.Id },
                { "title", tile.Title },
                { "note", tile.Note },
                { "roles", tile.Roles },
            };
            if (tile.Page.empty())
            {
                node["script"] = tile.ScriptId;
                node["presetIndex"] = tile.PresetIndex;
                node["command"] = FormatScriptRunCommand(tile.ScriptId, tile.PresetIndex);
            }
            else
            {
                node["page"] = tile.Page;
            }

            tiles.push_back(std::move(node));
        }

        return Json{ { "role", view.Role }, { "roles", roles }, { "tiles", tiles } }.dump(2);
    }

    bool HomeViewFromJson(const std::string& text, HomeView& view, std::string& error)
    {
        view = HomeView();
        return ReadJson(text, error, [&](const Json& root)
        {
            view.Role = StringOr(root, "role");
            for (const Json& node : root.at("roles")) { view.Roles.push_back(RoleEntry{ StringOr(node, "id"), StringOr(node, "label") }); }

            for (const Json& node : root.at("tiles"))
            {
                HomeTile tile;
                tile.Id = StringOr(node, "id");
                tile.Title = StringOr(node, "title");
                tile.Note = StringOr(node, "note");
                tile.ScriptId = StringOr(node, "script");
                tile.PresetIndex = node.value("presetIndex", static_cast<std::size_t>(0));
                tile.Page = StringOr(node, "page");
                tile.Roles = node.value("roles", std::vector<std::string>());
                view.Tiles.push_back(std::move(tile));
            }
        });
    }

    std::string StatusToJson(const ProjectStatus& status)
    {
        Json submodules = Json::array();
        for (const SubmoduleState& submodule : status.Submodules)
        {
            submodules.push_back({ { "path", submodule.Path }, { "state", submodule.State } });
        }

        return Json{
            { "root", status.Root },
            { "git", {
                { "error", status.GitError },
                { "branch", status.Branch },
                { "upstream", status.Upstream },
                { "ahead", status.Ahead },
                { "behind", status.Behind },
                { "changes", status.Changes },
            } },
            { "submodules", submodules },
            { "unity", {
                { "projectVersion", status.UnityVersion },
                { "cli", status.UnityCliPath },
            } },
        }.dump(2);
    }

    bool StatusFromJson(const std::string& text, ProjectStatus& status, std::string& error)
    {
        status = ProjectStatus();
        return ReadJson(text, error, [&](const Json& root)
        {
            status.Root = StringOr(root, "root");
            const Json& git = root.at("git");
            status.GitError = StringOr(git, "error");
            status.Branch = StringOr(git, "branch");
            status.Upstream = StringOr(git, "upstream");
            status.Ahead = git.value("ahead", 0);
            status.Behind = git.value("behind", 0);
            status.Changes = git.value("changes", 0);
            for (const Json& node : root.at("submodules"))
            {
                status.Submodules.push_back(SubmoduleState{ StringOr(node, "path"), StringOr(node, "state") });
            }

            const Json& unity = root.at("unity");
            status.UnityVersion = StringOr(unity, "projectVersion");
            status.UnityCliPath = StringOr(unity, "cli");
        });
    }

    std::string FormatScriptRunCommand(const std::string& scriptId, std::size_t presetIndex)
    {
        return "conductor scripts run " + scriptId + " --preset " + std::to_string(presetIndex + 1);
    }
}
