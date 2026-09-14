using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using KillChord.Editor.SourceDataProvider.Core;

namespace KillChord.Editor.ProjectWindow
{
    /// <summary>
    ///     Project Windowへ表示するSourceData関連のアセット状態です。
    /// </summary>
    [Flags]
    internal enum SourceDataAssetFlags
    {
        /// <summary> 対象となる状態がありません。 </summary>
        None = 0,

        /// <summary> Addressablesへ登録されています。 </summary>
        Addressable = 1 << 0,

        /// <summary> SourceDataProviderのcollection要素として登録されています。 </summary>
        CollectionItem = 1 << 1,
    }

    /// <summary>
    ///     SourceData関連のアセット状態をGUID単位でキャッシュします。
    /// </summary>
    internal static class SourceDataAssetIndex
    {
        private static Dictionary<string, SourceDataAssetFlags> _cache;
        private static bool _isDirty = true;

        /// <summary>
        ///     指定GUIDのSourceData関連状態を取得します。
        /// </summary>
        /// <param name="guid"> 確認対象のアセットGUIDです。 </param>
        /// <returns> 対象アセットの状態です。 </returns>
        internal static SourceDataAssetFlags GetFlags(string guid)
        {
            if (_isDirty)
            {
                Rebuild();
                _isDirty = false;
            }

            return !string.IsNullOrEmpty(guid)
                && _cache.TryGetValue(guid, out SourceDataAssetFlags flags)
                    ? flags
                    : SourceDataAssetFlags.None;
        }

        /// <summary>
        ///     次回参照時にインデックスを再構築するよう予約します。
        /// </summary>
        internal static void Invalidate()
        {
            _isDirty = true;
        }

        /// <summary>
        ///     Addressables設定とSourceDataProvider設定からインデックスを再構築します。
        /// </summary>
        private static void Rebuild()
        {
            Dictionary<string, SourceDataAssetFlags> cache = new();
            AddAddressableFlags(cache);
            AddCollectionItemFlags(cache);
            _cache = cache;
        }

        /// <summary>
        ///     Addressablesへ登録されている全アセットをインデックスへ追加します。
        /// </summary>
        /// <param name="cache"> 構築中のインデックスです。 </param>
        private static void AddAddressableFlags(Dictionary<string, SourceDataAssetFlags> cache)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings?.groups == null)
            {
                return;
            }

            for (int groupIndex = 0; groupIndex < settings.groups.Count; groupIndex++)
            {
                AddressableAssetGroup group = settings.groups[groupIndex];
                if (group?.entries == null)
                {
                    continue;
                }

                foreach (AddressableAssetEntry entry in group.entries)
                {
                    if (entry != null && !string.IsNullOrEmpty(entry.guid))
                    {
                        AddFlag(cache, entry.guid, SourceDataAssetFlags.Addressable);
                    }
                }
            }
        }

        /// <summary>
        ///     SourceDataProviderのcollection要素をインデックスへ追加します。
        /// </summary>
        /// <param name="cache"> 構築中のインデックスです。 </param>
        private static void AddCollectionItemFlags(Dictionary<string, SourceDataAssetFlags> cache)
        {
            IReadOnlyList<SourceDataProviderSettings.SourceCollectionMapping> mappings;
            try
            {
                mappings = SourceDataProviderSettings.instance.SourceCollectionMappings;
            }
            catch (Exception)
            {
                return;
            }

            if (mappings == null)
            {
                return;
            }

            for (int mappingIndex = 0; mappingIndex < mappings.Count; mappingIndex++)
            {
                try
                {
                    AddCollectionItemFlags(cache, mappings[mappingIndex]);
                }
                catch (Exception)
                {
                    // 破損した設定があっても、ほかのcollectionのインデックス構築を継続する。
                }
            }
        }

        /// <summary>
        ///     1つのcollection設定が参照するアセットをインデックスへ追加します。
        /// </summary>
        /// <param name="cache"> 構築中のインデックスです。 </param>
        /// <param name="mapping"> 走査するcollection設定です。 </param>
        private static void AddCollectionItemFlags(
            Dictionary<string, SourceDataAssetFlags> cache,
            SourceDataProviderSettings.SourceCollectionMapping mapping)
        {
            if (mapping == null
                || !SourceDataProviderRepositoryResolver.TryResolveAsset(
                    mapping.SourceAssetAddressableKey,
                    out ScriptableObject sourceAsset)
                || sourceAsset == null)
            {
                return;
            }

            SerializedObject serializedObject = new(sourceAsset);
            SerializedProperty collectionProperty = serializedObject.FindProperty(mapping.PropertyPath);
            if (collectionProperty == null || !collectionProperty.isArray)
            {
                return;
            }

            for (int elementIndex = 0; elementIndex < collectionProperty.arraySize; elementIndex++)
            {
                SerializedProperty element = collectionProperty.GetArrayElementAtIndex(elementIndex);
                if (element.propertyType != SerializedPropertyType.ObjectReference
                    || element.objectReferenceValue == null
                    || !AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                        element.objectReferenceValue,
                        out string guid,
                        out long _))
                {
                    continue;
                }

                AddFlag(cache, guid, SourceDataAssetFlags.CollectionItem);
            }
        }

        /// <summary>
        ///     指定GUIDへ状態フラグを追加します。
        /// </summary>
        /// <param name="cache"> 構築中のインデックスです。 </param>
        /// <param name="guid"> 状態を追加するアセットGUIDです。 </param>
        /// <param name="flag"> 追加する状態です。 </param>
        private static void AddFlag(
            Dictionary<string, SourceDataAssetFlags> cache,
            string guid,
            SourceDataAssetFlags flag)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return;
            }

            cache[guid] = cache.TryGetValue(guid, out SourceDataAssetFlags currentFlags)
                ? currentFlags | flag
                : flag;
        }
    }

    /// <summary>
    ///     Addressables設定の変更時にSourceDataアセットインデックスを無効化します。
    /// </summary>
    [InitializeOnLoad]
    internal static class SourceDataAssetIndexInvalidationTrigger
    {
        /// <summary>
        ///     Addressables設定変更イベントを購読します。
        /// </summary>
        static SourceDataAssetIndexInvalidationTrigger()
        {
            AddressableAssetSettings.OnModificationGlobal +=
                (_, _, _) => SourceDataAssetIndex.Invalidate();
        }
    }
}
