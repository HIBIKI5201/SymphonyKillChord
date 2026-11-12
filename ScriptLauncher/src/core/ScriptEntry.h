#pragma once

#include <optional>
#include <string>
#include <vector>

namespace skc::launcher
{
    /// <summary>
    ///     スクリプトをどう動かすか。
    /// </summary>
    enum class RunMode
    {
        /// <summary>新しいコンソールウィンドウで開く。入力を待つもの・常駐するもの用。</summary>
        Console,

        /// <summary>出力をツールの中に表示する。入力を待たないもの用。</summary>
        Capture,
    };

    /// <summary>
    ///     スクリプトに渡す引数の組（「確認だけ」など）。
    /// </summary>
    struct ScriptPreset
    {
        /// <summary>画面に出す名前。</summary>
        std::string Label;

        /// <summary>引数。1要素が1引数。</summary>
        std::vector<std::string> Args;

        /// <summary>この組だけ動かし方を変えるとき、その動かし方。</summary>
        std::optional<RunMode> Mode;
    };

    /// <summary>
    ///     ツールに登録したスクリプト1つ分。launcher-scripts.json の1要素に当たる。
    /// </summary>
    struct ScriptEntry
    {
        /// <summary>重複しない識別子。</summary>
        std::string Id;

        /// <summary>画面に出す名前。</summary>
        std::string Name;

        /// <summary>一覧での分類。</summary>
        std::string Category;

        /// <summary>Windows で動かすファイルの、リポジトリのルートからのパス（/ 区切り）。</summary>
        std::string WindowsPath;

        /// <summary>mac で動かすファイルのパス。無ければ空。</summary>
        std::string MacPath;

        /// <summary>説明。</summary>
        std::string Description;

        /// <summary>動かす前に出す確認の文。空なら確認しない。</summary>
        std::string Confirm;

        /// <summary>既定の動かし方。</summary>
        RunMode Mode = RunMode::Console;

        /// <summary>管理者として動かすか（実行のたびに UAC の確認が出る）。</summary>
        bool IsElevated = false;

        /// <summary>リポジトリに無く、PC にだけ置くファイルか（Builds/ など .gitignore されている場所）。</summary>
        bool IsLocalOnly = false;

        /// <summary>引数の組。1つ以上ある（無指定なら「引数なし」を1つ入れる）。</summary>
        std::vector<ScriptPreset> Presets;

        /// <summary>
        ///     引数の組を使ったときの動かし方を返す。
        /// </summary>
        /// <param name="presetIndex">引数の組の添字。</param>
        /// <returns>動かし方。</returns>
        RunMode ModeFor(std::size_t presetIndex) const
        {
            if (presetIndex < Presets.size() && Presets[presetIndex].Mode.has_value())
            {
                return *Presets[presetIndex].Mode;
            }

            return Mode;
        }
    };
}
