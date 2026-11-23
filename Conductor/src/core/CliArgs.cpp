#include "CliArgs.h"

#include <algorithm>

namespace skc::conductor
{
    namespace
    {
        /// <summary>値を取らないオプション。</summary>
        constexpr const char* FLAG_OPTIONS[] = { "json", "yes", "hold", "help", "version" };

        /// <summary>値を取るオプション。</summary>
        constexpr const char* VALUE_OPTIONS[] = { "preset", "mode", "role", "root" };

        /// <summary>
        ///     一覧に名前があるかを返す。
        /// </summary>
        template <std::size_t N>
        bool Contains(const char* const (&names)[N], const std::string& name)
        {
            return std::any_of(std::begin(names), std::end(names), [&](const char* known) { return name == known; });
        }
    }

    std::string CliArgs::Value(const std::string& name) const
    {
        auto it = Values.find(name);
        return it == Values.end() ? std::string() : it->second;
    }

    std::string CliArgs::Positional(std::size_t index) const
    {
        return index < Positionals.size() ? Positionals[index] : std::string();
    }

    CliArgs ParseCliArgs(const std::vector<std::string>& args)
    {
        CliArgs result;
        for (std::size_t i = 0; i < args.size(); ++i)
        {
            std::string arg = args[i];
            if (arg == "-h") { arg = "--help"; }
            if (arg == "-y") { arg = "--yes"; }

            if (arg.size() < 3 || arg.compare(0, 2, "--") != 0)
            {
                if (arg.size() >= 2 && arg[0] == '-')
                {
                    result.Error = "知らないオプションです: " + arg;
                    return result;
                }

                result.Positionals.push_back(arg);
                continue;
            }

            std::string name = arg.substr(2);
            std::string value;
            bool hasInlineValue = false;
            std::size_t equals = name.find('=');
            if (equals != std::string::npos)
            {
                value = name.substr(equals + 1);
                name = name.substr(0, equals);
                hasInlineValue = true;
            }

            if (Contains(FLAG_OPTIONS, name))
            {
                if (hasInlineValue)
                {
                    result.Error = "--" + name + " は値を取りません。";
                    return result;
                }

                result.Flags.insert(name);
            }
            else if (Contains(VALUE_OPTIONS, name))
            {
                if (!hasInlineValue)
                {
                    if (i + 1 >= args.size())
                    {
                        result.Error = "--" + name + " の値がありません。";
                        return result;
                    }

                    value = args[++i];
                }

                result.Values[name] = value;
            }
            else
            {
                result.Error = "知らないオプションです: --" + name;
                return result;
            }
        }

        return result;
    }
}
