#pragma once

#include <filesystem>
#include <optional>

namespace skc::launcher
{
    /// <summary>リポジトリのルートから見た、スクリプト登録ファイルの場所。</summary>
    inline constexpr const char* SCRIPT_CONFIG_RELATIVE_PATH = "ScriptLauncher/launcher-scripts.json";

    /// <summary>
    ///     指定のフォルダから上へたどり、スクリプト登録ファイルのあるリポジトリのルートを探す。
    /// </summary>
    /// <param name="start">探し始めるフォルダ（ツールの exe のあるフォルダ）。</param>
    /// <returns>見つかったルート。無ければ空。</returns>
    std::optional<std::filesystem::path> FindRepositoryRoot(const std::filesystem::path& start);
}
