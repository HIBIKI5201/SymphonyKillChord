#include "ScriptConfig.h"

#include <nlohmann/json.hpp>

#include <algorithm>
#include <cctype>
#include <set>
#include <stdexcept>
#include <string>

namespace skc::launcher
{
    namespace
    {
        using Json = nlohmann::json;

        /// <summary>cmd.exe が特別に扱うため、bat に渡す引数に含められない文字。</summary>
        constexpr const char* BATCH_UNSAFE_CHARACTERS = "\"&|<>^%!()";

        /// <summary>Windows で動かせるファイルの拡張子。</summary>
        constexpr const char* WINDOWS_EXTENSIONS[] = { ".bat", ".cmd", ".ps1", ".exe" };

        /// <summary>
        ///     文字列を小文字にする（ASCII のみ）。
        /// </summary>
        std::string ToLowerAscii(std::string text)
        {
            std::transform(text.begin(), text.end(), text.begin(),
                [](unsigned char c) { return static_cast<char>(std::tolower(c)); });
            return text;
        }

        /// <summary>
        ///     パスが指定の拡張子で終わるかを返す（大文字小文字を区別しない）。
        /// </summary>
        bool HasExtension(const std::string& path, const std::string& extension)
        {
            if (path.size() < extension.size()) { return false; }

            return ToLowerAscii(path.substr(path.size() - extension.size())) == extension;
        }

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
        ///     任意の文字列の欄を読む。無ければ空、null も空として扱う。
        /// </summary>
        std::string OptionalString(const Json& node, const char* key, const std::string& where)
        {
            auto it = node.find(key);
            if (it == node.end() || it->is_null()) { return std::string(); }
            if (!it->is_string()) { throw std::runtime_error(where + " の \"" + key + "\" は文字列で書いてください。"); }

            return it->get<std::string>();
        }

        /// <summary>
        ///     任意の真偽値の欄を読む。無ければ false。
        /// </summary>
        bool OptionalBool(const Json& node, const char* key, const std::string& where)
        {
            auto it = node.find(key);
            if (it == node.end()) { return false; }
            if (!it->is_boolean()) { throw std::runtime_error(where + " の \"" + key + "\" は true か false で書いてください。"); }

            return it->get<bool>();
        }

        /// <summary>
        ///     動かし方の文字列を読む。
        /// </summary>
        RunMode ParseMode(const std::string& text, const std::string& where)
        {
            if (text == "console") { return RunMode::Console; }
            if (text == "capture") { return RunMode::Capture; }

            throw std::runtime_error(where + " の \"mode\" は \"console\" か \"capture\" です（\"" + text + "\" と書かれています）。");
        }

        /// <summary>
        ///     リポジトリのルートからの相対パスとして正しいかを調べる。
        /// </summary>
        void ValidateRelativePath(const std::string& path, const std::string& where)
        {
            bool isAbsolute = !path.empty() && (path[0] == '/' || path[0] == '\\' || path.find(':') != std::string::npos);
            if (isAbsolute)
            {
                throw std::runtime_error(where + " のパス \"" + path + "\" は、リポジトリのルートからの相対パスで書いてください。");
            }

            if (path.find('\\') != std::string::npos)
            {
                throw std::runtime_error(where + " のパス \"" + path + "\" は / 区切りで書いてください。");
            }

            std::size_t start = 0;
            while (start <= path.size())
            {
                std::size_t end = path.find('/', start);
                if (end == std::string::npos) { end = path.size(); }
                if (path.compare(start, end - start, "..") == 0 && end - start == 2)
                {
                    throw std::runtime_error(where + " のパス \"" + path + "\" に .. は使えません。");
                }

                start = end + 1;
            }
        }

        /// <summary>
        ///     引数に制御文字や、bat に渡せない文字が入っていないかを調べる。
        /// </summary>
        void ValidateArgument(const std::string& argument, bool isBatch, const std::string& where)
        {
            for (char c : argument)
            {
                if (static_cast<unsigned char>(c) < 0x20)
                {
                    throw std::runtime_error(where + " の引数に改行などの制御文字は使えません。");
                }

                if (isBatch && std::char_traits<char>::find(BATCH_UNSAFE_CHARACTERS, std::char_traits<char>::length(BATCH_UNSAFE_CHARACTERS), c) != nullptr)
                {
                    throw std::runtime_error(where + " の引数 \"" + argument + "\" に、bat へ渡せない文字（\" & | < > ^ % ! ( )）が入っています。");
                }
            }
        }

        /// <summary>
        ///     引数の組を読む。
        /// </summary>
        std::vector<ScriptPreset> ParsePresets(const Json& node, bool isBatch, const std::string& where)
        {
            std::vector<ScriptPreset> presets;
            auto it = node.find("presets");
            if (it != node.end() && !it->is_null())
            {
                if (!it->is_array()) { throw std::runtime_error(where + " の \"presets\" は配列で書いてください。"); }

                for (std::size_t i = 0; i < it->size(); ++i)
                {
                    const Json& presetNode = (*it)[i];
                    std::string presetWhere = where + " の presets[" + std::to_string(i) + "]";
                    if (!presetNode.is_object()) { throw std::runtime_error(presetWhere + " はオブジェクトで書いてください。"); }

                    ScriptPreset preset;
                    preset.Label = RequireString(presetNode, "label", presetWhere);
                    auto args = presetNode.find("args");
                    if (args != presetNode.end() && !args->is_null())
                    {
                        if (!args->is_array()) { throw std::runtime_error(presetWhere + " の \"args\" は文字列の配列で書いてください。"); }

                        for (const Json& arg : *args)
                        {
                            if (!arg.is_string()) { throw std::runtime_error(presetWhere + " の \"args\" は文字列の配列で書いてください。"); }

                            ValidateArgument(arg.get<std::string>(), isBatch, presetWhere);
                            preset.Args.push_back(arg.get<std::string>());
                        }
                    }

                    std::string mode = OptionalString(presetNode, "mode", presetWhere);
                    if (!mode.empty()) { preset.Mode = ParseMode(mode, presetWhere); }

                    presets.push_back(std::move(preset));
                }
            }

            if (presets.empty())
            {
                presets.push_back(ScriptPreset{ "引数なし", {}, std::nullopt });
            }

            return presets;
        }

        /// <summary>
        ///     スクリプト1つ分を読む。
        /// </summary>
        ScriptEntry ParseEntry(const Json& node, std::size_t index)
        {
            std::string where = "scripts[" + std::to_string(index) + "]";
            if (!node.is_object()) { throw std::runtime_error(where + " はオブジェクトで書いてください。"); }

            ScriptEntry entry;
            entry.Id = RequireString(node, "id", where);
            where += "（" + entry.Id + "）";
            entry.Name = RequireString(node, "name", where);
            entry.Category = RequireString(node, "category", where);
            entry.WindowsPath = RequireString(node, "windows", where);
            entry.MacPath = OptionalString(node, "mac", where);
            entry.Description = OptionalString(node, "description", where);
            entry.Confirm = OptionalString(node, "confirm", where);
            entry.IsElevated = OptionalBool(node, "elevate", where);
            entry.IsLocalOnly = OptionalBool(node, "localOnly", where);

            std::string mode = OptionalString(node, "mode", where);
            entry.Mode = mode.empty() ? RunMode::Console : ParseMode(mode, where);

            ValidateRelativePath(entry.WindowsPath, where);
            if (!entry.MacPath.empty()) { ValidateRelativePath(entry.MacPath, where); }

            bool isKnownExtension = std::any_of(std::begin(WINDOWS_EXTENSIONS), std::end(WINDOWS_EXTENSIONS),
                [&](const char* extension) { return HasExtension(entry.WindowsPath, extension); });
            if (!isKnownExtension)
            {
                throw std::runtime_error(where + " の \"windows\" は .bat / .cmd / .ps1 / .exe のどれかにしてください。");
            }

            entry.Presets = ParsePresets(node, IsBatchFile(entry.WindowsPath), where);

            // 管理者で動かすものは ShellExecuteEx で起動するため、出力を受け取れない。
            if (entry.IsElevated)
            {
                for (std::size_t i = 0; i < entry.Presets.size(); ++i)
                {
                    if (entry.ModeFor(i) == RunMode::Capture)
                    {
                        throw std::runtime_error(where + " は \"elevate\": true なので、\"mode\": \"capture\" にはできません（管理者で動かすと出力を受け取れません）。");
                    }
                }
            }

            return entry;
        }
    }

    ScriptConfigResult ParseScriptConfig(const std::string& jsonText)
    {
        ScriptConfigResult result;
        try
        {
            Json root = Json::parse(jsonText);
            if (!root.is_object()) { throw std::runtime_error("ファイルの一番外側はオブジェクトにしてください。"); }

            auto version = root.find("version");
            if (version == root.end() || !version->is_number_integer())
            {
                throw std::runtime_error("\"version\"（整数）がありません。");
            }

            if (version->get<int>() != ScriptConfig::SUPPORTED_VERSION)
            {
                throw std::runtime_error("\"version\" が " + std::to_string(version->get<int>()) +
                    " です。このツールは " + std::to_string(ScriptConfig::SUPPORTED_VERSION) + " だけ読めます。ツールを新しくしてください。");
            }

            auto scripts = root.find("scripts");
            if (scripts == root.end() || !scripts->is_array())
            {
                throw std::runtime_error("\"scripts\"（配列）がありません。");
            }

            std::set<std::string> ids;
            for (std::size_t i = 0; i < scripts->size(); ++i)
            {
                ScriptEntry entry = ParseEntry((*scripts)[i], i);
                if (!ids.insert(entry.Id).second)
                {
                    throw std::runtime_error("id \"" + entry.Id + "\" が2回以上使われています。");
                }

                result.Config.Scripts.push_back(std::move(entry));
            }
        }
        catch (const nlohmann::json::exception& exception)
        {
            result.Config = ScriptConfig();
            result.Error = std::string("JSON として読めません: ") + exception.what();
        }
        catch (const std::exception& exception)
        {
            result.Config = ScriptConfig();
            result.Error = exception.what();
        }

        return result;
    }

    bool IsBatchFile(const std::string& path)
    {
        return HasExtension(path, ".bat") || HasExtension(path, ".cmd");
    }

    bool IsPowerShellFile(const std::string& path)
    {
        return HasExtension(path, ".ps1");
    }
}
