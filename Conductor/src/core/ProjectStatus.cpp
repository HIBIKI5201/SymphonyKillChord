#include "ProjectStatus.h"

#include <cstdlib>
#include <sstream>

namespace skc::conductor
{
    namespace
    {
        /// <summary>
        ///     行末の \r と空白を外す。
        /// </summary>
        std::string TrimRight(std::string text)
        {
            while (!text.empty() && (text.back() == '\r' || text.back() == ' ' || text.back() == '\t')) { text.pop_back(); }

            return text;
        }

        /// <summary>
        ///     文字列が接頭辞で始まるかを返す。
        /// </summary>
        bool StartsWith(const std::string& text, const char* prefix)
        {
            return text.rfind(prefix, 0) == 0;
        }

        /// <summary>
        ///     "+3" "-2" のような数を読む。読めなければ 0。
        /// </summary>
        int ParseSignedCount(const std::string& text)
        {
            if (text.size() < 2) { return 0; }

            try
            {
                return std::abs(std::stoi(text.substr(1)));
            }
            catch (...)
            {
                return 0;
            }
        }
    }

    void ParseGitStatusPorcelain(const std::string& output, ProjectStatus& status)
    {
        std::istringstream stream(output);
        std::string line;
        while (std::getline(stream, line))
        {
            line = TrimRight(line);
            if (line.empty()) { continue; }

            if (StartsWith(line, "# branch.head "))
            {
                std::string head = line.substr(14);
                status.Branch = head;
            }
            else if (StartsWith(line, "# branch.upstream "))
            {
                status.Upstream = line.substr(18);
            }
            else if (StartsWith(line, "# branch.ab "))
            {
                std::istringstream counts(line.substr(12));
                std::string ahead;
                std::string behind;
                counts >> ahead >> behind;
                status.Ahead = ParseSignedCount(ahead);
                status.Behind = ParseSignedCount(behind);
            }
            else if (line[0] != '#')
            {
                // 1・2（変更）、u（衝突）、?（追跡外）の行。! は無視されたファイルなので数えない。
                if (line[0] != '!') { ++status.Changes; }
            }
        }
    }

    std::vector<std::string> ParseGitmodulesPaths(const std::string& output)
    {
        std::vector<std::string> paths;
        std::istringstream stream(output);
        std::string line;
        while (std::getline(stream, line))
        {
            line = TrimRight(line);
            std::size_t space = line.find(' ');
            if (space == std::string::npos || space + 1 >= line.size()) { continue; }

            paths.push_back(line.substr(space + 1));
        }

        return paths;
    }

    std::vector<GitlinkEntry> ParseGitlinkEntries(const std::string& output)
    {
        std::vector<GitlinkEntry> entries;
        std::istringstream stream(output);
        std::string line;
        while (std::getline(stream, line))
        {
            line = TrimRight(line);
            std::size_t tab = line.find('\t');
            if (tab == std::string::npos) { continue; }

            std::istringstream fields(line.substr(0, tab));
            std::string mode;
            GitlinkEntry entry;
            fields >> mode >> entry.Commit >> entry.Stage;
            if (mode != "160000" || entry.Commit.empty()) { continue; }

            entry.Path = line.substr(tab + 1);
            entries.push_back(entry);
        }

        return entries;
    }

    std::string DecideSubmoduleState(const std::string& path, const std::vector<GitlinkEntry>& entries,
        const std::optional<std::string>& checkedOutCommit)
    {
        const GitlinkEntry* recorded = nullptr;
        for (const GitlinkEntry& entry : entries)
        {
            if (entry.Path != path) { continue; }
            if (entry.Stage != 0) { return "conflict"; }

            recorded = &entry;
        }

        if (!checkedOutCommit.has_value() || checkedOutCommit->empty()) { return "missing"; }
        if (recorded == nullptr) { return "missing"; }

        return *checkedOutCommit == recorded->Commit ? "ok" : "modified";
    }

    std::string ParseUnityProjectVersion(const std::string& text)
    {
        std::istringstream stream(text);
        std::string line;
        while (std::getline(stream, line))
        {
            line = TrimRight(line);
            if (StartsWith(line, "m_EditorVersion: ")) { return line.substr(17); }
        }

        return std::string();
    }

    std::string ParseUnityRevision(const std::string& text)
    {
        std::istringstream stream(text);
        std::string line;
        while (std::getline(stream, line))
        {
            line = TrimRight(line);
            if (!StartsWith(line, "m_EditorVersionWithRevision: ")) { continue; }

            std::size_t open = line.find('(');
            std::size_t close = line.find(')', open == std::string::npos ? 0 : open);
            if (open == std::string::npos || close == std::string::npos) { return std::string(); }

            return line.substr(open + 1, close - open - 1);
        }

        return std::string();
    }

    std::string UnityHubInstallUrl(const std::string& version, const std::string& revision)
    {
        if (version.empty() || revision.empty()) { return std::string(); }

        return "unityhub://" + version + "/" + revision;
    }
}
