using KillChord.Runtime.Utility.Identity;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.SourceDataProvider.Core
{
    /// <summary>
    ///     Collection要素の表示名、検索語、参照アセットを共通の規則で取得します。
    /// </summary>
    internal static class SourceCollectionElementDisplay
    {
        /// <summary>
        ///     定義側DataIDの文字列を、継承元を含めて取得します。
        /// </summary>
        public static string GetAuthoringId(UnityEngine.Object target, string collectionKey)
        {
            if (target == null) { return null; }
            using SerializedObject serialized = new(target);
            const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Public
                | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            for (Type type = target.GetType(); type != null && type != typeof(ScriptableObject); type = type.BaseType)
            {
                foreach (FieldInfo field in type.GetFields(FLAGS))
                {
                    if (field.FieldType != typeof(DataID)
                        || field.GetCustomAttribute<SourceDataCollectionAttribute>()?.CollectionKey != collectionKey)
                    {
                        continue;
                    }
                    string value = serialized.FindProperty(field.Name)?.FindPropertyRelative("_id")?.stringValue;
                    if (!string.IsNullOrWhiteSpace(value)) { return value; }
                }
            }
            return null;
        }

        /// <summary>
        ///     インライン要素のDataIDまたは識別用の文字列を取得します。
        /// </summary>
        public static string GetInlineKey(SerializedProperty element)
        {
            if (element.propertyType == SerializedPropertyType.String) { return element.stringValue; }
            SerializedProperty id = element.FindPropertyRelative("Id") ?? element.FindPropertyRelative("_stageId")
                ?? element.FindPropertyRelative("_skillId");
            SerializedProperty value = id?.FindPropertyRelative("_id") ?? element.FindPropertyRelative("_id");
            if (!string.IsNullOrWhiteSpace(value?.stringValue)) { return value.stringValue; }
            foreach (SerializedProperty child in GetChildren(element))
            {
                if (child.propertyType == SerializedPropertyType.Enum && child.enumValueIndex >= 0
                    && child.enumValueIndex < child.enumDisplayNames.Length)
                {
                    return child.enumDisplayNames[child.enumValueIndex];
                }
                if (child.propertyType == SerializedPropertyType.String && !string.IsNullOrWhiteSpace(child.stringValue))
                {
                    return child.stringValue;
                }
            }
            return null;
        }

        /// <summary>
        ///     要素を識別する表示名を取得します。
        /// </summary>
        public static string GetLabel(SerializedProperty element, int index)
        {
            if (element == null) { return $"Element {index + 1}"; }
            if (element.propertyType == SerializedPropertyType.ObjectReference)
            {
                return element.objectReferenceValue != null ? element.objectReferenceValue.name : $"Element {index + 1} (未設定)";
            }
            string key = GetInlineKey(element);
            SerializedProperty label = element.FindPropertyRelative("_selectorLabel");
            if (!string.IsNullOrWhiteSpace(label?.stringValue)) { return $"{key} / {label.stringValue}"; }
            return !string.IsNullOrWhiteSpace(key) ? key : $"Element {index + 1}";
        }

        /// <summary>
        ///     要素が直接またはインラインフィールド経由で参照するアセットを列挙します。
        /// </summary>
        public static IEnumerable<UnityEngine.Object> GetReferences(SerializedProperty element)
        {
            if (element.propertyType == SerializedPropertyType.ObjectReference)
            {
                if (element.objectReferenceValue != null) { yield return element.objectReferenceValue; }
                yield break;
            }
            foreach (SerializedProperty child in GetChildren(element))
            {
                if (child.propertyType == SerializedPropertyType.ObjectReference && child.objectReferenceValue != null)
                {
                    yield return child.objectReferenceValue;
                }
            }
        }

        /// <summary>
        ///     ID、本文、列挙値、参照名を含めて検索語と照合します。
        /// </summary>
        public static bool Matches(SerializedProperty element, string collectionKey, string query)
        {
            if (Contains(GetLabel(element, 0), query)) { return true; }
            if (element.propertyType == SerializedPropertyType.ObjectReference)
            {
                return Contains(GetAuthoringId(element.objectReferenceValue, collectionKey), query);
            }
            SerializedProperty iterator = element.Copy();
            SerializedProperty end = element.GetEndProperty();
            while (iterator.NextVisible(true) && !SerializedProperty.EqualContents(iterator, end))
            {
                if (iterator.propertyType == SerializedPropertyType.String && Contains(iterator.stringValue, query)) { return true; }
                if (iterator.propertyType == SerializedPropertyType.ObjectReference
                    && iterator.objectReferenceValue != null && Contains(iterator.objectReferenceValue.name, query)) { return true; }
                if (iterator.propertyType == SerializedPropertyType.Enum && iterator.enumValueIndex >= 0
                    && iterator.enumValueIndex < iterator.enumDisplayNames.Length
                    && Contains(iterator.enumDisplayNames[iterator.enumValueIndex], query)) { return true; }
            }
            return false;
        }

        /// <summary>
        ///     子階層を持つ要素の直下フィールドを列挙します。
        /// </summary>
        private static IEnumerable<SerializedProperty> GetChildren(SerializedProperty element)
        {
            if (!element.hasVisibleChildren) { yield break; }
            SerializedProperty iterator = element.Copy();
            SerializedProperty end = element.GetEndProperty();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
            {
                enterChildren = false;
                yield return iterator.Copy();
            }
        }

        /// <summary>
        ///     大文字小文字を区別せずに部分一致を判定します。
        /// </summary>
        private static bool Contains(string value, string query)
        {
            return !string.IsNullOrEmpty(value) && value.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
