using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.SourceDataProvider.Core
{
    /// <summary>
    ///     個別ScriptableObjectのSourceData登録状態をInspectorヘッダーへ表示します。
    /// </summary>
    [InitializeOnLoad]
    internal static class SourceDataRegistrationHeader
    {
        /// <summary>
        ///     Inspectorヘッダー描画イベントを購読します。
        /// </summary>
        static SourceDataRegistrationHeader()
        {
            UnityEditor.Editor.finishedDefaultHeaderGUI -= DrawRegistrationHeader;
            UnityEditor.Editor.finishedDefaultHeaderGUI += DrawRegistrationHeader;
        }

        /// <summary>
        ///     対象アセットに対応する登録UIを描画します。
        /// </summary>
        /// <param name="editor"> 対象Inspectorです。 </param>
        private static void DrawRegistrationHeader(UnityEditor.Editor editor)
        {
            if (editor.targets.Length != 1
                || editor.target is not ScriptableObject target)
            {
                return;
            }

            IReadOnlyList<SourceDataProviderSettings.SourceCollectionMapping> mappings =
                SourceDataProviderSettings.instance.SourceCollectionMappings;
            for (int i = 0; i < mappings.Count; i++)
            {
                SourceDataProviderSettings.SourceCollectionMapping mapping = mappings[i];
                if (!TryGetCompatibleArray(mapping, target, out UnityEngine.Object dataAsset, out SerializedProperty array))
                {
                    continue;
                }

                DrawMapping(mapping, dataAsset, array, target);
            }
        }

        /// <summary>
        ///     対象アセットを格納できる配列を取得します。
        /// </summary>
        /// <param name="mapping"> collection設定です。 </param>
        /// <param name="target"> 登録対象アセットです。 </param>
        /// <param name="dataAsset"> 解決したDataAssetです。 </param>
        /// <param name="array"> 解決した配列プロパティです。 </param>
        /// <returns> 対応する配列を取得できた場合はtrueです。 </returns>
        private static bool TryGetCompatibleArray(
            SourceDataProviderSettings.SourceCollectionMapping mapping,
            ScriptableObject target,
            out UnityEngine.Object dataAsset,
            out SerializedProperty array)
        {
            dataAsset = null;
            array = null;
            if (mapping == null
                || string.IsNullOrWhiteSpace(mapping.PropertyPath)
                || !SourceDataProviderRepositoryResolver.TryResolveAsset(mapping.DataAssetAddressableKey, out ScriptableObject resolvedDataAsset)
                || resolvedDataAsset == target
                || !SerializedPropertyFieldResolver.TryResolve(
                    resolvedDataAsset.GetType(),
                    mapping.PropertyPath,
                    out FieldInfo fieldInfo))
            {
                return false;
            }
            dataAsset = resolvedDataAsset;

            Type elementType = GetElementType(fieldInfo.FieldType);
            if (elementType == null || !elementType.IsAssignableFrom(target.GetType()))
            {
                return false;
            }

            SerializedObject serializedDataAsset = new(dataAsset);
            array = serializedDataAsset.FindProperty(mapping.PropertyPath);
            return array != null && array.isArray;
        }

        /// <summary>
        ///     配列またはList型から要素型を取得します。
        /// </summary>
        /// <param name="collectionType"> 配列またはList型です。 </param>
        /// <returns> 要素型。取得できない場合はnullです。 </returns>
        private static Type GetElementType(Type collectionType)
        {
            if (collectionType.IsArray)
            {
                return collectionType.GetElementType();
            }

            return collectionType.IsGenericType
                ? collectionType.GetGenericArguments()[0]
                : null;
        }

        /// <summary>
        ///     1件分の登録状態と操作ボタンを描画します。
        /// </summary>
        /// <param name="mapping"> collection設定です。 </param>
        /// <param name="dataAsset"> 対象DataAssetです。 </param>
        /// <param name="array"> 登録先配列です。 </param>
        /// <param name="target"> 登録対象アセットです。 </param>
        private static void DrawMapping(
            SourceDataProviderSettings.SourceCollectionMapping mapping,
            UnityEngine.Object dataAsset,
            SerializedProperty array,
            ScriptableObject target)
        {
            int registeredIndex = FindRegisteredIndex(array, target);
            bool isRegistered = registeredIndex >= 0;
            string labelCollectionKey = string.IsNullOrWhiteSpace(mapping.CollectionKey)
                ? dataAsset.GetType().Name
                : mapping.CollectionKey;

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField(
                $"SourceDataProvider [{labelCollectionKey}]",
                isRegistered ? "登録済み" : "未登録");
            if (GUILayout.Button("Ping", GUILayout.Width(48f)))
            {
                EditorGUIUtility.PingObject(dataAsset);
            }

            string buttonLabel = isRegistered ? "登録解除" : "登録";
            if (GUILayout.Button(buttonLabel, GUILayout.Width(64f)))
            {
                SetRegistration(dataAsset, array.propertyPath, target, registeredIndex);
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        ///     対象アセットが登録されている配列位置を取得します。
        /// </summary>
        /// <param name="array"> 検索対象配列です。 </param>
        /// <param name="target"> 検索対象アセットです。 </param>
        /// <returns> 登録位置。未登録の場合は-1です。 </returns>
        private static int FindRegisteredIndex(SerializedProperty array, ScriptableObject target)
        {
            for (int i = 0; i < array.arraySize; i++)
            {
                SerializedProperty element = array.GetArrayElementAtIndex(i);
                if (element.propertyType == SerializedPropertyType.ObjectReference
                    && element.objectReferenceValue == target)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        ///     配列への登録または登録解除を実行します。
        /// </summary>
        /// <param name="dataAsset"> 対象DataAssetです。 </param>
        /// <param name="arrayPropertyPath"> 登録先配列のプロパティパスです。 </param>
        /// <param name="target"> 登録対象アセットです。 </param>
        /// <param name="registeredIndex"> 現在の登録位置です。 </param>
        private static void SetRegistration(
            UnityEngine.Object dataAsset,
            string arrayPropertyPath,
            ScriptableObject target,
            int registeredIndex)
        {
            Undo.RecordObject(dataAsset, "Change Source Data Registration");
            SerializedObject serializedDataAsset = new(dataAsset);
            SerializedProperty array = serializedDataAsset.FindProperty(arrayPropertyPath);
            if (registeredIndex >= 0)
            {
                int previousSize = array.arraySize;
                array.DeleteArrayElementAtIndex(registeredIndex);
                if (array.arraySize == previousSize)
                {
                    array.DeleteArrayElementAtIndex(registeredIndex);
                }
            }
            else
            {
                int newIndex = array.arraySize;
                array.InsertArrayElementAtIndex(newIndex);
                array.GetArrayElementAtIndex(newIndex).objectReferenceValue = target;
            }

            serializedDataAsset.ApplyModifiedProperties();
            EditorUtility.SetDirty(dataAsset);
            AssetDatabase.SaveAssets();
        }
    }
}
