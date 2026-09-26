using KillChord.Editor.SourceDataProvider.Core;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.Inspectors.SourceData
{
    /// <summary>
    ///     Wikiで選んだインライン要素を通常Inspectorの先頭で編集します。
    /// </summary>
    [InitializeOnLoad]
    internal static class SourceCollectionItemInspector
    {
        /// <summary>
        ///     標準Inspectorヘッダーの描画へ選択要素の表示を接続します。
        /// </summary>
        static SourceCollectionItemInspector()
        {
            UnityEditor.Editor.finishedDefaultHeaderGUI += DrawSelection;
            Undo.undoRedoPerformed += ClearSelection;
        }

        /// <summary>
        ///     指定した所有アセットと要素パスをInspectorへ引き渡します。
        /// </summary>
        public static void Select(ScriptableObject owner, SerializedProperty element)
        {
            _owner = owner;
            _propertyPath = element.propertyPath;
            int elementMarker = _propertyPath.LastIndexOf(".Array.data[", System.StringComparison.Ordinal);
            _collectionPath = elementMarker >= 0 ? _propertyPath.Substring(0, elementMarker) : null;
            _collectionSize = _collectionPath != null ? element.serializedObject.FindProperty(_collectionPath).arraySize : 0;
            element.isExpanded = true;
            Selection.activeObject = owner;
            EditorGUIUtility.PingObject(owner);
        }

        private static ScriptableObject _owner;
        private static string _propertyPath;
        private static string _collectionPath;
        private static int _collectionSize;
        private const float PREVIEW_SIZE = 64f;

        /// <summary>
        ///     選択中の所有アセットだけに要素編集欄と参照プレビューを表示します。
        /// </summary>
        private static void DrawSelection(UnityEditor.Editor editor)
        {
            if (_owner == null || editor.targets.Length != 1 || editor.target != _owner) { return; }
            SerializedObject serialized = editor.serializedObject;
            serialized.Update();
            if (_collectionPath == null || serialized.FindProperty(_collectionPath)?.arraySize != _collectionSize)
            {
                ClearSelection();
                return;
            }
            SerializedProperty property = serialized.FindProperty(_propertyPath);
            if (property == null) { ClearSelection(); return; }
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Plannerで選択中の項目", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(_propertyPath, EditorStyles.miniLabel);
            EditorGUILayout.PropertyField(property, new GUIContent(SourceCollectionElementDisplay.GetLabel(property, 0)), true);
            foreach (UnityEngine.Object reference in SourceCollectionElementDisplay.GetReferences(property))
            {
                Texture preview = AssetPreview.GetAssetPreview(reference) ?? AssetPreview.GetMiniThumbnail(reference);
                if (preview != null) { GUILayout.Label(preview, GUILayout.Width(PREVIEW_SIZE), GUILayout.Height(PREVIEW_SIZE)); }
            }
            EditorGUILayout.EndVertical();
            if (serialized.ApplyModifiedProperties()) { GUIUtility.ExitGUI(); }
        }

        /// <summary>
        ///     配列変更後に古いindexで別要素を編集しないよう、選択要求を破棄します。
        /// </summary>
        private static void ClearSelection()
        {
            _owner = null;
            _propertyPath = null;
        }
    }
}
