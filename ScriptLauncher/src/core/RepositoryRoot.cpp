#include "RepositoryRoot.h"

#include <system_error>

namespace skc::launcher
{
    std::optional<std::filesystem::path> FindRepositoryRoot(const std::filesystem::path& start)
    {
        std::error_code error;
        std::filesystem::path current = std::filesystem::absolute(start, error);
        if (error) { return std::nullopt; }

        while (true)
        {
            if (std::filesystem::is_regular_file(current / SCRIPT_CONFIG_RELATIVE_PATH, error))
            {
                return current;
            }

            std::filesystem::path parent = current.parent_path();
            if (parent.empty() || parent == current) { return std::nullopt; }

            current = parent;
        }
    }
}
