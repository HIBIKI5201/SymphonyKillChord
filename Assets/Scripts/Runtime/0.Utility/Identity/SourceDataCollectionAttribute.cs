using System;
using UnityEngine;

namespace KillChord.Runtime.Utility.Identity
{
    /// <summary>
    ///     DataIDフィールドが属するSourceDataProviderのCollectionKeyを指定します。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SourceDataCollectionAttribute : PropertyAttribute
    {
        /// <summary>
        ///     CollectionKeyを指定して属性を初期化します。
        /// </summary>
        /// <param name="collectionKey"> SourceDataProviderへ登録するCollectionKeyです。 </param>
        public SourceDataCollectionAttribute(string collectionKey)
        {
            CollectionKey = collectionKey;
        }

        /// <summary> SourceDataProviderへ登録されたCollectionKeyです。 </summary>
        public string CollectionKey { get; }
    }
}
