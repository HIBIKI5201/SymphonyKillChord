#pragma once

#include "ScriptEntry.h"

#include <filesystem>
#include <optional>
#include <string>
#include <vector>

namespace skc::conductor::cli
{
    /// <summary>
    ///     出力を受けて動かした結果。
    /// </summary>
    struct CaptureResult
    {
        /// <summary>起動できたか。</summary>
        bool IsStarted = false;

        /// <summary>終了コード。</summary>
        int ExitCode = 0;

        /// <summary>標準出力（UTF-8 のつもりで読む）。</summary>
        std::string Output;

        /// <summary>起動できなかった理由。</summary>
        std::string Error;
    };

    /// <summary>
    ///     スクリプトを動かした結果。
    /// </summary>
    struct ScriptRunResult
    {
        /// <summary>起動できたか。</summary>
        bool IsStarted = false;

        /// <summary>利用者が UAC の確認で取り消したか。</summary>
        bool IsCancelled = false;

        /// <summary>スクリプトの終了コード。</summary>
        int ExitCode = 0;

        /// <summary>起動できなかった理由。</summary>
        std::string Error;
    };

    /// <summary>
    ///     パスを UTF-8 の文字列にする。
    /// </summary>
    /// <param name="path">パス。</param>
    /// <returns>UTF-8 の文字列。</returns>
    std::string PathToUtf8(const std::filesystem::path& path);

    /// <summary>
    ///     UTF-8 の文字列をパスにする。
    /// </summary>
    /// <param name="text">UTF-8 の文字列。</param>
    /// <returns>パス。</returns>
    std::filesystem::path Utf8ToPath(const std::string& text);

    /// <summary>
    ///     この exe のあるフォルダを返す。
    /// </summary>
    /// <returns>フォルダ。</returns>
    std::filesystem::path ExecutableDirectory();

    /// <summary>
    ///     この PC だけの設定ファイルの場所を返す。
    ///     Windows は %LOCALAPPDATA%\KillChordConductor\settings.json、ほかは ~/.config/killchord-conductor/settings.json。
    ///     環境変数 KILLCHORD_CONDUCTOR_SETTINGS があれば、そのパスを使う（テスト用）。
    /// </summary>
    /// <returns>ファイルの場所。決められなければ空。</returns>
    std::filesystem::path SettingsFilePath();

    /// <summary>
    ///     PATH からプログラムを探す。
    /// </summary>
    /// <param name="name">プログラム名（拡張子なし）。</param>
    /// <returns>見つかった場所（UTF-8）。無ければ空。</returns>
    std::optional<std::string> FindOnPath(const std::string& name);

    /// <summary>
    ///     プログラムを動かし、標準出力を受け取る。標準エラーは捨てる。ウィンドウは出さない。
    /// </summary>
    /// <param name="argv">プログラム名（PATH から探す）と引数（UTF-8）。</param>
    /// <param name="workingDirectory">作業フォルダ。</param>
    /// <returns>結果。</returns>
    CaptureResult RunAndCapture(const std::vector<std::string>& argv, const std::filesystem::path& workingDirectory);

    /// <summary>
    ///     スクリプトを、このプロセスの標準入出力をそのまま渡して動かし、終わるまで待つ。
    ///     Ctrl+C はスクリプトに任せ、こちらは終わるのを待つ。
    ///     Windows は windows の欄のファイル、ほかの OS は mac の欄のファイルを bash で動かす。
    /// </summary>
    /// <param name="entry">スクリプト。</param>
    /// <param name="presetIndex">引数の組の添字。</param>
    /// <param name="repositoryRoot">リポジトリのルート。</param>
    /// <param name="mode">動かし方（capture なら入力を待たせない形で動かす）。</param>
    /// <param name="holdWindow">ps1 が終わったあと Enter を待つか（新しいウィンドウで開いたとき用）。</param>
    /// <returns>結果。</returns>
    ScriptRunResult RunScript(const ScriptEntry& entry, std::size_t presetIndex,
        const std::filesystem::path& repositoryRoot, RunMode mode, bool holdWindow);

    /// <summary>
    ///     このOS で動かすスクリプトのパス（リポジトリのルートからの / 区切り）を返す。
    /// </summary>
    /// <param name="entry">スクリプト。</param>
    /// <returns>パス。この OS 用が無ければ空。</returns>
    std::string ScriptPathForThisOs(const ScriptEntry& entry);

    /// <summary>
    ///     ファイル・フォルダ・URL を、OS の既定のアプリで開く（Windows は ShellExecute、mac は open）。
    /// </summary>
    /// <param name="target">開くもの（UTF-8）。</param>
    /// <param name="error">開けなかった理由。</param>
    /// <returns>開けたら true。</returns>
    bool OpenWithShell(const std::string& target, std::string& error);

    /// <summary>
    ///     プログラムを起動し、終わるのを待たない（Unity・CRI Atom Craft など）。
    /// </summary>
    /// <param name="program">プログラムの場所（UTF-8）。</param>
    /// <param name="args">引数。</param>
    /// <param name="workingDirectory">作業フォルダ。</param>
    /// <param name="error">起動できなかった理由。</param>
    /// <returns>起動できたら true。</returns>
    bool LaunchDetached(const std::string& program, const std::vector<std::string>& args,
        const std::filesystem::path& workingDirectory, std::string& error);

    /// <summary>
    ///     Unity Hub が入れたエディタの、ありそうな場所を返す（存在は確かめない）。
    /// </summary>
    /// <param name="version">Unity の版。</param>
    /// <returns>候補。</returns>
    std::vector<std::filesystem::path> UnityEditorCandidates(const std::string& version);

    /// <summary>
    ///     CRI Atom Craft の exe を、よくある場所から探す。
    /// </summary>
    /// <returns>見つかった場所。無ければ空。</returns>
    std::optional<std::filesystem::path> FindCriAtomCraft();

    /// <summary>
    ///     Google Drive（パソコン版）の SymphonyKillChord フォルダの、ありそうな場所を返す（存在は確かめない）。
    /// </summary>
    /// <returns>候補。</returns>
    std::vector<std::filesystem::path> DriveFolderCandidates();

    /// <summary>
    ///     標準出力へ UTF-8 の文字列を書く（コンソールなら文字化けしないよう UTF-16 で書く）。
    /// </summary>
    /// <param name="text">文字列。</param>
    void WriteOut(const std::string& text);

    /// <summary>
    ///     標準エラーへ UTF-8 の文字列を書く。
    /// </summary>
    /// <param name="text">文字列。</param>
    void WriteError(const std::string& text);

    /// <summary>
    ///     標準入力から1行読む。
    /// </summary>
    /// <param name="line">読んだ行（UTF-8）。</param>
    /// <returns>読めたら true（入力の終わりなら false）。</returns>
    bool ReadLine(std::string& line);
}
