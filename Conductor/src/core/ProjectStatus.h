#pragma once

#include <optional>
#include <string>
#include <vector>

namespace skc::conductor
{
    /// <summary>
    ///     サブモジュール1つ分の状態。
    /// </summary>
    struct SubmoduleState
    {
        /// <summary>リポジトリのルートからのパス。</summary>
        std::string Path;

        /// <summary>状態（ok・missing・modified・conflict）。</summary>
        std::string State;
    };

    /// <summary>
    ///     プロジェクトの状態（conductor status が出すもの）。
    /// </summary>
    struct ProjectStatus
    {
        /// <summary>リポジトリのルート。</summary>
        std::string Root;

        /// <summary>git で調べられなかった理由。調べられたら空。</summary>
        std::string GitError;

        /// <summary>今のブランチ（切り離されていれば "(detached)"）。</summary>
        std::string Branch;

        /// <summary>追いかけているリモートのブランチ。無ければ空。</summary>
        std::string Upstream;

        /// <summary>リモートより進んでいるコミットの数（最後に fetch した時点）。</summary>
        int Ahead = 0;

        /// <summary>リモートより遅れているコミットの数（最後に fetch した時点）。</summary>
        int Behind = 0;

        /// <summary>コミットしていない変更（追跡外を含む）のファイル数。</summary>
        int Changes = 0;

        /// <summary>サブモジュール。</summary>
        std::vector<SubmoduleState> Submodules;

        /// <summary>このプロジェクトの Unity の版（ProjectSettings/ProjectVersion.txt）。</summary>
        std::string UnityVersion;

        /// <summary>Unity CLI（unity）の場所。無ければ空。</summary>
        std::string UnityCliPath;

        /// <summary>このプロジェクトの Unity の版のリビジョン（Unity Hub で入れるときに使う）。</summary>
        std::string UnityRevision;

        /// <summary>この版の Unity エディタの場所。見つからなければ空。</summary>
        std::string UnityEditorPath;

        /// <summary>Unity でこのプロジェクトを開いているか（Temp/UnityLockfile があるか）。</summary>
        bool IsUnityOpen = false;

        /// <summary>CRI Atom Craft のプロジェクト（.atmcproject）の場所。サブモジュールが無ければ空。</summary>
        std::string CriProjectPath;

        /// <summary>CRI Atom Craft の exe の場所。見つからなければ空（そのときはファイルの関連付けで開く）。</summary>
        std::string CriAppPath;

        /// <summary>Google Drive（パソコン版）の SymphonyKillChord フォルダの場所。見つからなければ空。</summary>
        std::string DriveFolder;

        /// <summary>仕様書の写し（Docs/NotionSpecifications）を取ってあるか。</summary>
        bool HasSpecMirror = false;
    };

    /// <summary>
    ///     git status --porcelain=v2 --branch の出力を読み、ブランチ・進み遅れ・変更の数を入れる。
    /// </summary>
    /// <param name="output">出力。</param>
    /// <param name="status">入れる先。</param>
    void ParseGitStatusPorcelain(const std::string& output, ProjectStatus& status);

    /// <summary>
    ///     git submodule status の出力を読む。
    /// </summary>
    /// <param name="output">出力。</param>
    /// <returns>サブモジュールの状態。</returns>
    std::vector<SubmoduleState> ParseSubmoduleStatus(const std::string& output);

    /// <summary>
    ///     ProjectVersion.txt から Unity の版を読む。
    /// </summary>
    /// <param name="text">ファイルの中身。</param>
    /// <returns>版。読めなければ空。</returns>
    std::string ParseUnityProjectVersion(const std::string& text);

    /// <summary>
    ///     ProjectVersion.txt から Unity の版のリビジョン（m_EditorVersionWithRevision の括弧の中）を読む。
    /// </summary>
    /// <param name="text">ファイルの中身。</param>
    /// <returns>リビジョン。読めなければ空。</returns>
    std::string ParseUnityRevision(const std::string& text);

    /// <summary>
    ///     Unity Hub でこの版を入れる画面を開く URL を作る（unityhub://版/リビジョン）。
    /// </summary>
    /// <param name="version">版。</param>
    /// <param name="revision">リビジョン。</param>
    /// <returns>URL。どちらかが空なら空。</returns>
    std::string UnityHubInstallUrl(const std::string& version, const std::string& revision);
}
