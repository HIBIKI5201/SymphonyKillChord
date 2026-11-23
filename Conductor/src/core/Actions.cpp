#include "Actions.h"

#include <algorithm>

namespace skc::conductor
{
    const std::vector<BuiltinAction>& BuiltinActions()
    {
        static const std::vector<BuiltinAction> ACTIONS = {
            { "unity-open", { "unity", "open" } },
            { "unity-install", { "unity", "install" } },
            { "cri-open", { "cri", "open" } },
            { "assets-open", { "assets", "open" } },
            { "assets-import", { "assets", "import" } },
            { "docs-viewer", { "docs", "viewer" } },
            { "repo-open", { "repo", "open" } },
        };
        return ACTIONS;
    }

    const BuiltinAction* FindBuiltinAction(const std::string& id)
    {
        const std::vector<BuiltinAction>& actions = BuiltinActions();
        auto it = std::find_if(actions.begin(), actions.end(), [&](const BuiltinAction& action) { return action.Id == id; });
        return it == actions.end() ? nullptr : &*it;
    }

    std::string FormatConductorCommand(const std::vector<std::string>& args)
    {
        std::string text = "conductor";
        for (const std::string& arg : args)
        {
            bool needsQuote = arg.empty() || arg.find_first_of(" \t") != std::string::npos;
            text += needsQuote ? " \"" + arg + "\"" : " " + arg;
        }

        return text;
    }

    std::string BuildMenuItemCode(const std::string& menuPath)
    {
        if (menuPath.empty() || menuPath.find_first_of("\"\\\r\n") != std::string::npos) { return std::string(); }

        return "UnityEditor.EditorApplication.ExecuteMenuItem(\"" + menuPath + "\")";
    }
}
