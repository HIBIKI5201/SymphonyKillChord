#pragma once

namespace skc::conductor
{
    // conductor の終了コード。scripts run だけは、スクリプトの終了コードをそのまま返す。

    /// <summary>成功。</summary>
    inline constexpr int EXIT_OK = 0;

    /// <summary>失敗（読めない・書けない・git が失敗したなど）。</summary>
    inline constexpr int EXIT_FAILED = 1;

    /// <summary>使い方の誤り（知らないコマンド・オプション、無い id）。</summary>
    inline constexpr int EXIT_USAGE = 2;

    /// <summary>前提が足りない（リポジトリ・ファイル・プログラムが見つからない）。</summary>
    inline constexpr int EXIT_PRECONDITION = 3;

    /// <summary>利用者が取り消した（確認で「いいえ」、UAC の確認で取り消し）。</summary>
    inline constexpr int EXIT_CANCELLED = 4;
}
