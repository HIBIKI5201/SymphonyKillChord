#pragma once

#include <string>
#include <vector>

namespace skc::conductor
{
    /// <summary>
    ///     ホーム画面のボタンから呼べる、conductor の組み込みの操作（スクリプトでないもの）。
    /// </summary>
    struct BuiltinAction
    {
        /// <summary>home.json の "action" に書く名前。</summary>
        std::string Id;

        /// <summary>conductor に渡す引数（conductor を除く）。</summary>
        std::vector<std::string> Args;
    };

    /// <summary>
    ///     組み込みの操作の一覧を返す。
    /// </summary>
    /// <returns>一覧。</returns>
    const std::vector<BuiltinAction>& BuiltinActions();

    /// <summary>
    ///     組み込みの操作を名前で探す。
    /// </summary>
    /// <param name="id">名前。</param>
    /// <returns>見つかった操作。無ければ nullptr。</returns>
    const BuiltinAction* FindBuiltinAction(const std::string& id);

    /// <summary>
    ///     conductor のコマンドを、人が打てる1行にする（空白を含む引数は " で囲む）。
    /// </summary>
    /// <param name="args">conductor に渡す引数。</param>
    /// <returns>「conductor ...」の1行。</returns>
    std::string FormatConductorCommand(const std::vector<std::string>& args);

    /// <summary>
    ///     Unity のメニューを動かす C# の式を作る（Unity CLI の script eval に渡す）。
    /// </summary>
    /// <param name="menuPath">メニューのパス（例: Tools/KillChord/DriveImportSync/Sync Now）。</param>
    /// <returns>式。" や \ や改行を含むパスは受け付けず、空を返す。</returns>
    std::string BuildMenuItemCode(const std::string& menuPath);
}
