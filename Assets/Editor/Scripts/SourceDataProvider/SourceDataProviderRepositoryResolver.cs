using KillChord.Runtime.Utility.Identity;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace KillChord.Editor.SourceDataProvider
{
    /// <summary>
    ///     SourceDataProvider設定からAddressable ScriptableObjectと登録済みDataIDを解決します。
    /// </summary>
    internal static class SourceDataProviderRepositoryResolver
    {
        /// <summary>
        ///     AddressableキーからSourceAssetを取得します。
        /// </summary>
        /// <param name="addressableKey"> SourceAssetのAddressableキーです。 </param>
        /// <param name="sourceAsset"> 解決したSourceAssetです。 </param>
        /// <returns> SourceAssetを解決できた場合はtrueです。 </returns>
        public static bool TryResolveAsset(string addressableKey, out ScriptableObject sourceAsset)
        {
            sourceAsset = null;
            if (string.IsNullOrWhiteSpace(addressableKey))
            {
                return false;
            }

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                return false;
            }

            foreach (AddressableAssetGroup group in settings.groups)
            {
                if (group == null)
                {
                    continue;
                }

                foreach (AddressableAssetEntry entry in group.entries)
                {
                    if (entry == null
                        || !string.Equals(entry.address, addressableKey, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    sourceAsset = AssetDatabase.LoadMainAssetAtPath(entry.AssetPath) as ScriptableObject;
                    return sourceAsset != null;
                }
            }

            return false;
        }

        /// <summary>
        ///     Addressableキーから互換用のオブジェクトを取得します。
        /// </summary>
        /// <param name="addressableKey"> Addressableキーです。 </param>
        /// <param name="repository"> 解決したオブジェクトです。 </param>
        /// <returns> 解決できた場合はtrueです。 </returns>
        public static bool TryResolveRepository(string addressableKey, out UnityEngine.Object repository)
        {
            repository = null;
            if (!TryResolveAsset(addressableKey, out ScriptableObject sourceAsset))
            {
                return false;
            }

            repository = sourceAsset;
            return true;
        }

        /// <summary>
        ///     指定カテゴリがcollection設定に登録済みか判定します。
        /// </summary>
        /// <param name="collectionKey"> 確認するCollectionKeyです。 </param>
        /// <returns> 登録済みの場合はtrueです。 </returns>
        public static bool ContainsCollectionKey(string collectionKey)
        {
            return SourceDataProviderSettings.instance.ContainsCollectionKey(collectionKey);
        }

        /// <summary>
        ///     SourceAssetが持つ配列プロパティのパス一覧を取得します。
        /// </summary>
        /// <param name="repository"> 対象SourceAssetです。 </param>
        /// <returns> 配列プロパティのパス一覧です。 </returns>
        public static string[] GetArrayPropertyPaths(UnityEngine.Object repository)
        {
            return GetCollectionPropertyPaths(repository);
        }

        /// <summary>
        ///     SourceAssetが持つ配列プロパティのパス一覧を取得します。
        /// </summary>
        /// <param name="sourceAsset"> 対象SourceAssetです。 </param>
        /// <returns> 配列プロパティのパス一覧です。 </returns>
        public static string[] GetCollectionPropertyPaths(UnityEngine.Object sourceAsset)
        {
            if (sourceAsset == null)
            {
                return Array.Empty<string>();
            }

            List<string> paths = new();
            foreach (FieldInfo fieldInfo in GetSerializableFields(sourceAsset.GetType()))
            {
                if (IsCollectionField(fieldInfo.FieldType))
                {
                    paths.Add(fieldInfo.Name);
                }
            }

            paths.Sort(StringComparer.Ordinal);
            return paths.ToArray();
        }

        /// <summary>
        ///     指定SourceAssetに紐づく有効なcollectionプロパティパス一覧を取得します。
        /// </summary>
        /// <param name="addressableKey"> SourceAssetのAddressableキーです。 </param>
        /// <returns> 有効なcollectionプロパティパス一覧です。 </returns>
        public static string[] GetConfiguredCollectionPropertyPaths(string addressableKey)
        {
            IReadOnlyList<SourceDataProviderSettings.SourceCollectionMapping> mappings =
                SourceDataProviderSettings.instance.GetCollectionMappingsByAddressableKey(addressableKey);
            if (mappings.Count == 0)
            {
                return Array.Empty<string>();
            }

            HashSet<string> paths = new(StringComparer.Ordinal);
            for (int i = 0; i < mappings.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(mappings[i].PropertyPath))
                {
                    paths.Add(mappings[i].PropertyPath);
                }
            }

            string[] results = new string[paths.Count];
            paths.CopyTo(results);
            Array.Sort(results, StringComparer.Ordinal);
            return results;
        }

        /// <summary>
        ///     指定カテゴリへ登録済みのDataID一覧を取得します。
        /// </summary>
        /// <param name="collectionKey"> 取得するCollectionKeyです。 </param>
        /// <returns> 登録済みのDataID一覧です。 </returns>
        public static IReadOnlyList<SourceDataIDOption> GetOptions(string collectionKey)
        {
            List<SourceDataIDOption> options = new();
            if (!SourceDataProviderSettings.instance.TryGetCollectionMapping(collectionKey, out SourceDataProviderSettings.SourceCollectionMapping mapping)
                || !TryResolveAsset(mapping.SourceAssetAddressableKey, out ScriptableObject sourceAsset))
            {
                return options;
            }

            HashSet<int> visitedInstanceIds = new();
            SerializedObject serializedObject = new(sourceAsset);
            SerializedProperty rootProperty = string.IsNullOrWhiteSpace(mapping.PropertyPath)
                ? null
                : serializedObject.FindProperty(mapping.PropertyPath);

            if (rootProperty == null)
            {
                CollectFromObject(sourceAsset, collectionKey, options, visitedInstanceIds);
            }
            else
            {
                CollectFromProperty(
                    serializedObject,
                    rootProperty,
                    collectionKey,
                    options,
                    visitedInstanceIds);
            }

            options.Sort((left, right) => string.Compare(left.Id, right.Id, StringComparison.Ordinal));
            return options;
        }

        /// <summary>
        ///     対象DataIDが生成側フィールドか判定します。
        /// </summary>
        /// <param name="collectionKey"> DataIDのCollectionKeyです。 </param>
        /// <param name="target"> DataIDを保持するUnityオブジェクトです。 </param>
        /// <param name="propertyPath"> DataIDのプロパティパスです。 </param>
        /// <returns> 生成側フィールドの場合はtrueです。 </returns>
        public static bool IsAuthoringProperty(
            string collectionKey,
            UnityEngine.Object target,
            string propertyPath)
        {
            if (!SourceDataProviderSettings.instance.TryGetCollectionMapping(collectionKey, out SourceDataProviderSettings.SourceCollectionMapping mapping)
                || !TryResolveAsset(mapping.SourceAssetAddressableKey, out ScriptableObject sourceAsset))
            {
                return true;
            }

            if (target == sourceAsset)
            {
                return string.IsNullOrWhiteSpace(mapping.PropertyPath)
                    || propertyPath.StartsWith(mapping.PropertyPath, StringComparison.Ordinal);
            }

            if (string.IsNullOrWhiteSpace(mapping.PropertyPath))
            {
                return false;
            }

            SerializedObject serializedObject = new(sourceAsset);
            SerializedProperty collectionProperty = serializedObject.FindProperty(mapping.PropertyPath);
            if (collectionProperty == null || !collectionProperty.isArray)
            {
                return false;
            }

            for (int i = 0; i < collectionProperty.arraySize; i++)
            {
                SerializedProperty element = collectionProperty.GetArrayElementAtIndex(i);
                if (element.propertyType == SerializedPropertyType.ObjectReference
                    && element.objectReferenceValue == target)
                {
                    return !propertyPath.Contains(".Array.data[", StringComparison.Ordinal);
                }
            }

            return false;
        }

        /// <summary>
        ///     Unityオブジェクト内から指定カテゴリのDataIDを収集します。
        /// </summary>
        /// <param name="target"> 走査対象のUnityオブジェクトです。 </param>
        /// <param name="collectionKey"> 収集するCollectionKeyです。 </param>
        /// <param name="options"> 収集結果です。 </param>
        /// <param name="visitedInstanceIds"> 走査済みオブジェクトのInstanceID一覧です。 </param>
        private static void CollectFromObject(
            UnityEngine.Object target,
            string collectionKey,
            List<SourceDataIDOption> options,
            HashSet<int> visitedInstanceIds)
        {
            if (target == null || !visitedInstanceIds.Add(target.GetInstanceID()))
            {
                return;
            }

            SerializedObject serializedObject = new(target);
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = true;
                CollectCurrentProperty(
                    serializedObject,
                    iterator,
                    collectionKey,
                    options,
                    visitedInstanceIds);
            }
        }

        /// <summary>
        ///     指定プロパティとその子要素からDataIDを収集します。
        /// </summary>
        /// <param name="serializedObject"> プロパティを所有するSerializedObjectです。 </param>
        /// <param name="rootProperty"> 走査起点のプロパティです。 </param>
        /// <param name="collectionKey"> 収集するCollectionKeyです。 </param>
        /// <param name="options"> 収集結果です。 </param>
        /// <param name="visitedInstanceIds"> 走査済みオブジェクトのInstanceID一覧です。 </param>
        private static void CollectFromProperty(
            SerializedObject serializedObject,
            SerializedProperty rootProperty,
            string collectionKey,
            List<SourceDataIDOption> options,
            HashSet<int> visitedInstanceIds)
        {
            SerializedProperty iterator = rootProperty.Copy();
            SerializedProperty endProperty = iterator.GetEndProperty();
            CollectCurrentProperty(serializedObject, iterator, collectionKey, options, visitedInstanceIds);
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren)
                && !SerializedProperty.EqualContents(iterator, endProperty))
            {
                enterChildren = true;
                CollectCurrentProperty(
                    serializedObject,
                    iterator,
                    collectionKey,
                    options,
                    visitedInstanceIds);
            }
        }

        /// <summary>
        ///     現在のSerializedPropertyからDataIDまたは参照先オブジェクトを収集します。
        /// </summary>
        /// <param name="serializedObject"> プロパティを所有するSerializedObjectです。 </param>
        /// <param name="property"> 走査対象のプロパティです。 </param>
        /// <param name="collectionKey"> 収集するCollectionKeyです。 </param>
        /// <param name="options"> 収集結果です。 </param>
        /// <param name="visitedInstanceIds"> 走査済みオブジェクトのInstanceID一覧です。 </param>
        private static void CollectCurrentProperty(
            SerializedObject serializedObject,
            SerializedProperty property,
            string collectionKey,
            List<SourceDataIDOption> options,
            HashSet<int> visitedInstanceIds)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                if (property.objectReferenceValue is ScriptableObject scriptableObject)
                {
                    CollectFromObject(scriptableObject, collectionKey, options, visitedInstanceIds);
                }
                return;
            }

            if (!string.Equals(property.type, nameof(DataID), StringComparison.Ordinal)
                || !SerializedPropertyFieldResolver.TryResolve(
                    serializedObject.targetObject.GetType(),
                    property.propertyPath,
                    out FieldInfo fieldInfo))
            {
                return;
            }

            SourceDataCollectionAttribute attribute =
                fieldInfo.GetCustomAttribute<SourceDataCollectionAttribute>();
            if (attribute == null
                || !string.Equals(attribute.CollectionKey, collectionKey, StringComparison.Ordinal))
            {
                return;
            }

            SerializedProperty idProperty = property.FindPropertyRelative(ID_PROPERTY_NAME);
            SerializedProperty hashProperty = property.FindPropertyRelative(HASH_PROPERTY_NAME);
            if (idProperty == null
                || hashProperty == null
                || string.IsNullOrWhiteSpace(idProperty.stringValue))
            {
                return;
            }

            options.Add(new SourceDataIDOption(
                idProperty.stringValue,
                hashProperty.intValue,
                serializedObject.targetObject));
        }

        /// <summary>
        ///     型に定義された直列化対象フィールド一覧を取得します。
        /// </summary>
        /// <param name="type"> 走査対象の型です。 </param>
        /// <returns> 直列化対象フィールド一覧です。 </returns>
        private static IEnumerable<FieldInfo> GetSerializableFields(Type type)
        {
            const BindingFlags BINDING_FLAGS =
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            for (Type current = type; current != null && current != typeof(ScriptableObject); current = current.BaseType)
            {
                FieldInfo[] fields = current.GetFields(BINDING_FLAGS);
                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo fieldInfo = fields[i];
                    if (fieldInfo.IsStatic
                        || Attribute.IsDefined(fieldInfo, typeof(NonSerializedAttribute))
                        || Attribute.IsDefined(fieldInfo, typeof(HideInInspector)))
                    {
                        continue;
                    }

                    if (fieldInfo.IsPublic || Attribute.IsDefined(fieldInfo, typeof(SerializeField)))
                    {
                        yield return fieldInfo;
                    }
                }
            }
        }

        /// <summary>
        ///     フィールド型がcollection候補か判定します。
        /// </summary>
        /// <param name="fieldType"> 判定対象のフィールド型です。 </param>
        /// <returns> collection候補の場合はtrueです。 </returns>
        private static bool IsCollectionField(Type fieldType)
        {
            if (fieldType == typeof(string))
            {
                return false;
            }

            if (fieldType.IsArray)
            {
                return true;
            }

            return fieldType.IsGenericType
                && fieldType.GetGenericTypeDefinition() == typeof(List<>);
        }

        internal const string ID_PROPERTY_NAME = "_id";
        internal const string HASH_PROPERTY_NAME = "_hashId";
    }
}
