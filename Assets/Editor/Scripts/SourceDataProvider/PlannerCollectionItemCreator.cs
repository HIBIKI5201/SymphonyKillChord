using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.SourceDataProvider
{
    /// <summary>
    ///     Planner Master Data画面からCollection要素を作成します。
    /// </summary>
    internal static class PlannerCollectionItemCreator
    {
        /// <summary>
        ///     Collectionへ作成可能なScriptableObject型一覧を取得します。
        /// </summary>
        /// <param name="elementType"> Collectionの要素型です。 </param>
        /// <returns> 作成可能な具象ScriptableObject型一覧です。 </returns>
        public static IReadOnlyList<Type> GetCreatableAssetTypes(Type elementType)
        {
            List<Type> results = new();
            if (elementType == null || !typeof(ScriptableObject).IsAssignableFrom(elementType))
            {
                return results;
            }

            if (!elementType.IsAbstract && !elementType.IsGenericTypeDefinition)
            {
                results.Add(elementType);
            }

            TypeCache.TypeCollection derivedTypes = TypeCache.GetTypesDerivedFrom(elementType);
            for (int i = 0; i < derivedTypes.Count; i++)
            {
                Type derivedType = derivedTypes[i];
                if (derivedType.IsAbstract
                    || derivedType.IsGenericTypeDefinition
                    || !typeof(ScriptableObject).IsAssignableFrom(derivedType))
                {
                    continue;
                }

                results.Add(derivedType);
            }

            results.Sort((left, right) =>
                string.Compare(left.Name, right.Name, StringComparison.Ordinal));
            return results;
        }

        /// <summary>
        ///     ScriptableObjectを生成してCollectionへ登録します。
        /// </summary>
        /// <param name="sourceAsset"> Collectionを保持するSourceAssetです。 </param>
        /// <param name="mapping"> Collection設定です。 </param>
        /// <param name="serializedObject"> SourceAssetのSerializedObjectです。 </param>
        /// <param name="collectionProperty"> Collectionプロパティです。 </param>
        /// <param name="assetType"> 生成するScriptableObject型です。 </param>
        /// <param name="createdAsset"> 生成したアセットです。 </param>
        /// <param name="errorMessage"> 生成できなかった理由です。 </param>
        /// <returns> 作成と登録に成功した場合はtrueです。 </returns>
        public static bool TryCreateAsset(
            ScriptableObject sourceAsset,
            SourceDataProviderSettings.SourceCollectionMapping mapping,
            SerializedObject serializedObject,
            SerializedProperty collectionProperty,
            Type assetType,
            out ScriptableObject createdAsset,
            out string errorMessage)
        {
            createdAsset = null;
            if (!ValidateAssetCreation(mapping, collectionProperty, assetType, out errorMessage))
            {
                return false;
            }

            string directory = mapping.AssetCreationDirectory.Replace('\\', '/').TrimEnd('/');
            string assetPath = AssetDatabase.GenerateUniqueAssetPath(
                $"{directory}/{assetType.Name}.asset");
            createdAsset = ScriptableObject.CreateInstance(assetType);
            createdAsset.name = System.IO.Path.GetFileNameWithoutExtension(assetPath);

            Undo.RecordObject(sourceAsset, "Collectionへデータを追加");
            AssetDatabase.CreateAsset(createdAsset, assetPath);
            Undo.RegisterCreatedObjectUndo(createdAsset, "Collectionデータを作成");

            int newIndex = collectionProperty.arraySize;
            collectionProperty.InsertArrayElementAtIndex(newIndex);
            SerializedProperty newElement = collectionProperty.GetArrayElementAtIndex(newIndex);
            newElement.objectReferenceValue = createdAsset;
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(sourceAsset);
            AssetDatabase.SaveAssetIfDirty(sourceAsset);
            AssetDatabase.SaveAssetIfDirty(createdAsset);
            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        ///     インラインCollectionへ既定値の要素を追加します。
        /// </summary>
        /// <param name="sourceAsset"> Collectionを保持するSourceAssetです。 </param>
        /// <param name="serializedObject"> SourceAssetのSerializedObjectです。 </param>
        /// <param name="collectionProperty"> Collectionプロパティです。 </param>
        /// <param name="elementType"> Collectionの要素型です。 </param>
        /// <param name="errorMessage"> 追加できなかった理由です。 </param>
        /// <returns> 追加に成功した場合はtrueです。 </returns>
        public static bool TryAddInlineItem(
            ScriptableObject sourceAsset,
            SerializedObject serializedObject,
            SerializedProperty collectionProperty,
            Type elementType,
            out string errorMessage)
        {
            if (sourceAsset == null || collectionProperty == null || !collectionProperty.isArray)
            {
                errorMessage = "Collectionプロパティを解決できません。";
                return false;
            }

            Undo.RecordObject(sourceAsset, "Collectionへデータを追加");
            int newIndex = collectionProperty.arraySize;
            collectionProperty.InsertArrayElementAtIndex(newIndex);
            SerializedProperty newElement = collectionProperty.GetArrayElementAtIndex(newIndex);
            TryResetInlineValue(newElement, elementType);
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(sourceAsset);
            AssetDatabase.SaveAssetIfDirty(sourceAsset);
            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        ///     ScriptableObject生成に必要な設定を検証します。
        /// </summary>
        /// <param name="mapping"> Collection設定です。 </param>
        /// <param name="collectionProperty"> Collectionプロパティです。 </param>
        /// <param name="assetType"> 生成対象型です。 </param>
        /// <param name="errorMessage"> 検証エラーです。 </param>
        /// <returns> 生成可能な場合はtrueです。 </returns>
        private static bool ValidateAssetCreation(
            SourceDataProviderSettings.SourceCollectionMapping mapping,
            SerializedProperty collectionProperty,
            Type assetType,
            out string errorMessage)
        {
            if (collectionProperty == null || !collectionProperty.isArray)
            {
                errorMessage = "Collectionプロパティを解決できません。";
                return false;
            }

            if (assetType == null
                || assetType.IsAbstract
                || !typeof(ScriptableObject).IsAssignableFrom(assetType))
            {
                errorMessage = "作成可能なScriptableObject型ではありません。";
                return false;
            }

            string directory = mapping.AssetCreationDirectory?.Replace('\\', '/').TrimEnd('/');
            if (string.IsNullOrWhiteSpace(directory))
            {
                errorMessage = "Source Data Provider設定でAsset Creation Directoryを指定してください。";
                return false;
            }

            if (!directory.StartsWith("Assets/", StringComparison.Ordinal)
                || !AssetDatabase.IsValidFolder(directory))
            {
                errorMessage = "生成先には存在するAssets配下のフォルダを指定してください。";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        ///     追加したインライン要素を型の既定値へ戻します。
        /// </summary>
        /// <param name="property"> 追加した要素です。 </param>
        /// <param name="elementType"> 要素型です。 </param>
        private static void TryResetInlineValue(SerializedProperty property, Type elementType)
        {
            if (property == null || elementType == null)
            {
                return;
            }

            try
            {
                property.boxedValue = elementType == typeof(string)
                    ? string.Empty
                    : Activator.CreateInstance(elementType);
            }
            catch (Exception exception) when (exception is InvalidOperationException
                or ArgumentException
                or NotSupportedException
                or MissingMethodException)
            {
                ResetPropertyValue(property);
            }
        }

        /// <summary>
        ///     SerializedPropertyを型に応じた既定値へ戻します。
        /// </summary>
        /// <param name="property"> 既定値へ戻すプロパティです。 </param>
        private static void ResetPropertyValue(SerializedProperty property)
        {
            if (property == null)
            {
                return;
            }

            if (property.isArray && property.propertyType != SerializedPropertyType.String)
            {
                property.ClearArray();
                return;
            }

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.Character:
                    property.longValue = 0;
                    return;
                case SerializedPropertyType.Boolean:
                    property.boolValue = false;
                    return;
                case SerializedPropertyType.Float:
                    property.doubleValue = 0d;
                    return;
                case SerializedPropertyType.String:
                    property.stringValue = string.Empty;
                    return;
                case SerializedPropertyType.Color:
                    property.colorValue = Color.clear;
                    return;
                case SerializedPropertyType.ObjectReference:
                case SerializedPropertyType.ExposedReference:
                    property.objectReferenceValue = null;
                    return;
                case SerializedPropertyType.Enum:
                    property.enumValueIndex = 0;
                    return;
                case SerializedPropertyType.Vector2:
                    property.vector2Value = Vector2.zero;
                    return;
                case SerializedPropertyType.Vector3:
                    property.vector3Value = Vector3.zero;
                    return;
                case SerializedPropertyType.Vector4:
                    property.vector4Value = Vector4.zero;
                    return;
                case SerializedPropertyType.Rect:
                    property.rectValue = Rect.zero;
                    return;
                case SerializedPropertyType.AnimationCurve:
                    property.animationCurveValue = new AnimationCurve();
                    return;
                case SerializedPropertyType.Bounds:
                    property.boundsValue = new Bounds();
                    return;
                case SerializedPropertyType.Quaternion:
                    property.quaternionValue = Quaternion.identity;
                    return;
                case SerializedPropertyType.Vector2Int:
                    property.vector2IntValue = Vector2Int.zero;
                    return;
                case SerializedPropertyType.Vector3Int:
                    property.vector3IntValue = Vector3Int.zero;
                    return;
                case SerializedPropertyType.RectInt:
                    property.rectIntValue = new RectInt();
                    return;
                case SerializedPropertyType.BoundsInt:
                    property.boundsIntValue = new BoundsInt();
                    return;
                case SerializedPropertyType.ManagedReference:
                    property.managedReferenceValue = null;
                    return;
                case SerializedPropertyType.Hash128:
                    property.hash128Value = new Hash128();
                    return;
                case SerializedPropertyType.Generic:
                    ResetGenericChildren(property);
                    return;
            }
        }

        /// <summary>
        ///     Genericプロパティ直下の各フィールドを再帰的に既定値へ戻します。
        /// </summary>
        /// <param name="property"> Genericプロパティです。 </param>
        private static void ResetGenericChildren(SerializedProperty property)
        {
            int rootDepth = property.depth;
            SerializedProperty iterator = property.Copy();
            SerializedProperty endProperty = iterator.GetEndProperty();
            if (!iterator.NextVisible(true))
            {
                return;
            }

            while (!SerializedProperty.EqualContents(iterator, endProperty)
                && iterator.depth > rootDepth)
            {
                if (iterator.depth == rootDepth + 1)
                {
                    ResetPropertyValue(iterator.Copy());
                }

                if (!iterator.NextVisible(false))
                {
                    break;
                }
            }
        }
    }
}
