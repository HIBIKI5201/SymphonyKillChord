using UnityEngine;

namespace KillChord.Runtime.Utility.Identity
{
    /// <summary>
    ///     カテゴリと文字列IDから実行時用の数値IDを生成します。
    /// </summary>
    public static class DataIDHasher
    {
        /// <summary>
        ///     カテゴリと文字列IDを組み合わせて安定した数値IDを生成します。
        /// </summary>
        /// <param name="category"> IDのカテゴリ名です。 </param>
        /// <param name="id"> 人間可読な文字列IDです。 </param>
        /// <returns> 実行時に使用する数値IDです。入力が空の場合は0です。 </returns>
        public static int Compute(string category, string id)
        {
            if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(id))
            {
                return 0;
            }

            return Animator.StringToHash(category + CATEGORY_SEPARATOR + id);
        }

        private const string CATEGORY_SEPARATOR = ":";
    }
}
