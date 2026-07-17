using System;
using System.Collections.Generic;

namespace KillChord.Editor.SourceDataProvider
{
    /// <summary>
    ///     同一カテゴリ内のDataID重複とハッシュ衝突を検出します。
    /// </summary>
    internal static class DataIDCollisionDetector
    {
        /// <summary>
        ///     指定IDと登録済みIDの不整合を検出します。
        /// </summary>
        /// <param name="id"> 検証対象の文字列IDです。 </param>
        /// <param name="hashId"> 検証対象の数値IDです。 </param>
        /// <param name="options"> 同一カテゴリの登録済みID一覧です。 </param>
        /// <returns> 不整合がある場合は警告文、それ以外は空文字列です。 </returns>
        public static string FindWarning(
            string id,
            int hashId,
            IReadOnlyList<SourceDataIDOption> options)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return "IDが未設定です。";
            }

            int duplicateCount = 0;
            for (int i = 0; i < options.Count; i++)
            {
                SourceDataIDOption option = options[i];
                if (string.Equals(option.Id, id, StringComparison.Ordinal))
                {
                    duplicateCount++;
                    continue;
                }

                if (option.HashId == hashId)
                {
                    return $"異なるIDとのハッシュ衝突を検出しました。Other: {option.Id}";
                }
            }

            return duplicateCount > 1
                ? $"同じIDが{duplicateCount}件登録されています。"
                : string.Empty;
        }
    }
}
