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

    std::vector<SubmoduleState> ParseSubmoduleStatus(const std::string& output)
    {
        std::vector<SubmoduleState> submodules;
        std::istringstream stream(output);
        std::string line;
        while (std::getline(stream, line))
        {
            line = TrimRight(line);
            if (line.size() < 2) { continue; }

            // 形式: <状態の1文字><コミット> <パス> [(<説明>)]
            char mark = line[0];
            std::istringstream fields(line.substr(1));
            std::string commit;
            std::string path;
            fields >> commit >> path;
            if (path.empty()) { continue; }

            std::string state = "ok";
            if (mark == '-') { state = "missing"; }
            else if (mark == '+') { state = "modified"; }
            else if (mark == 'U') { state = "conflict"; }

            submodules.push_back(SubmoduleState{ path, state });
        }

        return submodules;
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
}
