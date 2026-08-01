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
        /// <param name="isSceneScoped">
        ///     シーン内で完結し、SourceDataProviderへ登録しないCollectionKeyの場合はtrueです。
        /// </param>
        public SourceDataCollectionAttribute(string collectionKey, bool isSceneScoped = false)
        {
            CollectionKey = collectionKey;
            IsSceneScoped = isSceneScoped;
        }

        /// <summary> SourceDataProviderへ登録されたCollectionKeyです。 </summary>
        public string CollectionKey { get; }

        /// <summary> SourceDataProviderへ登録しないシーン内完結のCollectionKeyであるかです。 </summary>
        public bool IsSceneScoped { get; }
    }
}
