using UnityEngine;

namespace KillChord.Runtime.Utility.Identity
{
    /// <summary>
    ///     CollectionKeyと文字列IDから実行時用の数値IDを生成します。
    /// </summary>
    public static class DataIDHasher
    {
        /// <summary>
        ///     CollectionKeyと文字列IDを組み合わせて安定した数値IDを生成します。
        /// </summary>
        /// <param name="collectionKey"> IDのCollectionKeyです。 </param>
        /// <param name="id"> 人間可読な文字列IDです。 </param>
        /// <returns> 実行時に使用する数値IDです。入力が空の場合は0です。 </returns>
        public static int Compute(string collectionKey, string id)
        {
            if (string.IsNullOrWhiteSpace(collectionKey) || string.IsNullOrWhiteSpace(id))
            {
                return 0;
            }

            return Animator.StringToHash(collectionKey + COLLECTION_KEY_SEPARATOR + id);
        }

        private const string COLLECTION_KEY_SEPARATOR = ":";
    }
}
