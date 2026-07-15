using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.SourceDataProvider
{
    /// <summary>
    ///     個別ScriptableObjectのリポジトリ登録状態をInspectorヘッダーへ表示します。
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
        ///     対象アセットに対応するリポジトリ登録UIを描画します。
        /// </summary>
        /// <param name="editor"> 対象Inspectorです。 </param>
        private static void DrawRegistrationHeader(UnityEditor.Editor editor)
        {
            if (editor.targets.Length != 1
                || editor.target is not ScriptableObject target)
            {
                return;
            }

            foreach (SourceDataProviderSettings.RepositoryMapping mapping
                in SourceDataProviderSettings.instance.RepositoryMappings)
            {
                if (!TryGetCompatibleArray(mapping, target, out UnityEngine.Object repository, out SerializedProperty array))
                {
                    continue;
                }

                DrawMapping(mapping, repository, array, target);
            }
        }

        /// <summary>
        ///     対象アセットを格納できるリポジトリ配列を取得します。
        /// </summary>
        /// <param name="mapping"> リポジトリ設定です。 </param>
        /// <param name="target"> 登録対象アセットです。 </param>
        /// <param name="repository"> 解決したリポジトリです。 </param>
        /// <param name="array"> 解決した配列プロパティです。 </param>
        /// <returns> 対応する配列を取得できた場合はtrueです。 </returns>
        private static bool TryGetCompatibleArray(
            SourceDataProviderSettings.RepositoryMapping mapping,
            ScriptableObject target,
            out UnityEngine.Object repository,
            out SerializedProperty array)
        {
            repository = null;
            array = null;
            if (string.IsNullOrWhiteSpace(mapping.ArrayPropertyPath)
                || !SourceDataProviderRepositoryResolver.TryResolveRepository(mapping.AddressableKey, out repository)
                || repository == target
                || !SerializedPropertyFieldResolver.TryResolve(
                    repository.GetType(),
                    mapping.ArrayPropertyPath,
                    out FieldInfo fieldInfo))
            {
                return false;
            }

            Type elementType = GetElementType(fieldInfo.FieldType);
            if (elementType == null || !elementType.IsAssignableFrom(target.GetType()))
            {
                return false;
            }

            SerializedObject serializedRepository = new(repository);
            array = serializedRepository.FindProperty(mapping.ArrayPropertyPath);
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
        ///     1件分のリポジトリ登録状態と操作ボタンを描画します。
        /// </summary>
        /// <param name="mapping"> リポジトリ設定です。 </param>
        /// <param name="repository"> 対象リポジトリです。 </param>
        /// <param name="array"> 登録先配列です。 </param>
        /// <param name="target"> 登録対象アセットです。 </param>
        private static void DrawMapping(
            SourceDataProviderSettings.RepositoryMapping mapping,
            UnityEngine.Object repository,
            SerializedProperty array,
            ScriptableObject target)
        {
            int registeredIndex = FindRegisteredIndex(array, target);
            bool isRegistered = registeredIndex >= 0;

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField(
                $"SourceDataProvider [{mapping.Category}]",
                isRegistered ? "登録済み" : "未登録");
            if (GUILayout.Button("Ping", GUILayout.Width(48f)))
            {
                EditorGUIUtility.PingObject(repository);
            }

            string buttonLabel = isRegistered ? "登録解除" : "登録";
            if (GUILayout.Button(buttonLabel, GUILayout.Width(64f)))
            {
                SetRegistration(repository, array.propertyPath, target, registeredIndex);
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
        ///     リポジトリ配列への登録または登録解除を実行します。
        /// </summary>
        /// <param name="repository"> 対象リポジトリです。 </param>
        /// <param name="arrayPropertyPath"> 登録先配列のプロパティパスです。 </param>
        /// <param name="target"> 登録対象アセットです。 </param>
        /// <param name="registeredIndex"> 現在の登録位置です。 </param>
        private static void SetRegistration(
            UnityEngine.Object repository,
            string arrayPropertyPath,
            ScriptableObject target,
            int registeredIndex)
        {
            Undo.RecordObject(repository, "Change Source Data Registration");
            SerializedObject serializedRepository = new(repository);
            SerializedProperty array = serializedRepository.FindProperty(arrayPropertyPath);
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

            serializedRepository.ApplyModifiedProperties();
            EditorUtility.SetDirty(repository);
            AssetDatabase.SaveAssets();
        }
    }
}
