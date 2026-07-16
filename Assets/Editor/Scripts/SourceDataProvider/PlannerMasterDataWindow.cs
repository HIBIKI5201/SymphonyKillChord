using KillChord.Editor.Utility;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.SourceDataProvider
{
    /// <summary>
    ///     プランナー向けにSourceDataProvider登録済みのマスターデータを系統単位で編集する画面です。
    /// </summary>
    public sealed class PlannerMasterDataWindow : EditorWindow
    {
        /// <summary>
        ///     ウィンドウを開きます。
        /// </summary>
        [MenuItem(ToolConst.TOOLS_PATH + "Source Data Provider/Planner Master Data Window")]
        public static void ShowWindow()
        {
            PlannerMasterDataWindow window = GetWindow<PlannerMasterDataWindow>();
            window.titleContent = new GUIContent("Planner Master Data");
            window.minSize = new Vector2(1080f, 640f);
        }

        private Vector2 _pageScrollPosition;
        private Vector2 _navigationScrollPosition;
        private Vector2 _contentScrollPosition;
        private int _selectedPageIndex;
        private string _selectedSourceAssetKey = string.Empty;
        private string _selectedCollectionKey = string.Empty;
        private UnityEditor.Editor _cachedEditor;
        private UnityEngine.Object _cachedEditorTarget;

        /// <summary>
        ///     初期表示時に選択状態を補正します。
        /// </summary>
        private void OnEnable()
        {
            EnsureSelection();
        }

        /// <summary>
        ///     キャッシュ済みEditorを破棄します。
        /// </summary>
        private void OnDisable()
        {
            ClearCachedEditor();
        }

        /// <summary>
        ///     ウィンドウ全体を描画します。
        /// </summary>
        private void OnGUI()
        {
            PlannerMasterDataEditorSettings settings = PlannerMasterDataEditorSettings.instance;
            IReadOnlyList<PlannerMasterDataEditorSettings.PageDefinition> pages = settings.Pages;
            if (pages.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "ページ定義がありません。Project Settings の Planner Master Data から設定してください。",
                    MessageType.Warning);
                return;
            }

            EnsureSelection();

            EditorGUILayout.BeginHorizontal();
            DrawPageSidebar(pages);
            DrawPageContent(pages[_selectedPageIndex]);
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        ///     選択状態が現在の設定に対して有効になるよう補正します。
        /// </summary>
        private void EnsureSelection()
        {
            IReadOnlyList<PlannerMasterDataEditorSettings.PageDefinition> pages =
                PlannerMasterDataEditorSettings.instance.Pages;
            if (pages.Count == 0)
            {
                _selectedPageIndex = 0;
                _selectedSourceAssetKey = string.Empty;
                _selectedCollectionKey = string.Empty;
                return;
            }

            _selectedPageIndex = Mathf.Clamp(_selectedPageIndex, 0, pages.Count - 1);
            PlannerMasterDataEditorSettings.PageDefinition page = pages[_selectedPageIndex];
            if (!Contains(page.SourceAssetAddressableKeys, _selectedSourceAssetKey))
            {
                _selectedSourceAssetKey = page.SourceAssetAddressableKeys.Count > 0
                    ? page.SourceAssetAddressableKeys[0]
                    : string.Empty;
            }

            if (!Contains(page.CollectionCategories, _selectedCollectionKey))
            {
                _selectedCollectionKey = string.Empty;
            }
        }

        /// <summary>
        ///     ページ切り替えサイドバーを描画します。
        /// </summary>
        /// <param name="pages"> ページ一覧です。 </param>
        private void DrawPageSidebar(IReadOnlyList<PlannerMasterDataEditorSettings.PageDefinition> pages)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(PAGE_SIDEBAR_WIDTH));
            EditorGUILayout.LabelField("Pages", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            using (EditorGUILayout.ScrollViewScope scope = new(_pageScrollPosition))
            {
                _pageScrollPosition = scope.scrollPosition;
                for (int i = 0; i < pages.Count; i++)
                {
                    bool isSelected = i == _selectedPageIndex;
                    GUIStyle style = isSelected ? EditorStyles.miniButtonMid : EditorStyles.miniButton;
                    if (!GUILayout.Button(pages[i].DisplayName, style, GUILayout.Height(36f)))
                    {
                        continue;
                    }

                    _selectedPageIndex = i;
                    _selectedSourceAssetKey = string.Empty;
                    _selectedCollectionKey = string.Empty;
                    ClearCachedEditor();
                    EnsureSelection();
                }
            }
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("Page Settings"))
            {
                SettingsService.OpenProjectSettings("Project/KillChord/Planner Master Data");
            }

            if (GUILayout.Button("SourceData Settings"))
            {
                SettingsService.OpenProjectSettings("Project/KillChord/Source Data Provider");
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        ///     選択中ページの内容を描画します。
        /// </summary>
        /// <param name="page"> 描画対象ページです。 </param>
        private void DrawPageContent(PlannerMasterDataEditorSettings.PageDefinition page)
        {
            EditorGUILayout.BeginVertical();
            DrawPageHeader(page);

            EditorGUILayout.BeginHorizontal();
            DrawNavigationColumn(page);
            DrawDetailColumn();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        ///     ページヘッダーを描画します。
        /// </summary>
        /// <param name="page"> 描画対象ページです。 </param>
        private void DrawPageHeader(PlannerMasterDataEditorSettings.PageDefinition page)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label(page.DisplayName, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(72f)))
            {
                ClearCachedEditor();
                Repaint();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(
                "左の一覧からSourceAssetまたはcollectionを選ぶと、右側で編集とプレビューを行えます。"
                + " 新しい型の追加は、SourceDataProvider設定とページ設定の更新だけで反映できます。",
                MessageType.Info);
        }

        /// <summary>
        ///     SourceAsset / collection のナビゲーション列を描画します。
        /// </summary>
        /// <param name="page"> 選択中ページです。 </param>
        private void DrawNavigationColumn(PlannerMasterDataEditorSettings.PageDefinition page)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(NAVIGATION_COLUMN_WIDTH));
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            using (EditorGUILayout.ScrollViewScope scope = new(_navigationScrollPosition))
            {
                _navigationScrollPosition = scope.scrollPosition;
                DrawSourceAssetNavigation(page);
                EditorGUILayout.Space();
                DrawCollectionNavigation(page);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        ///     SourceAsset一覧を描画します。
        /// </summary>
        /// <param name="page"> 選択中ページです。 </param>
        private void DrawSourceAssetNavigation(PlannerMasterDataEditorSettings.PageDefinition page)
        {
            EditorGUILayout.LabelField("Source Assets", EditorStyles.boldLabel);
            if (page.SourceAssetAddressableKeys.Count == 0)
            {
                EditorGUILayout.HelpBox("このページにはSourceAssetが設定されていません。", MessageType.None);
                return;
            }

            for (int i = 0; i < page.SourceAssetAddressableKeys.Count; i++)
            {
                string addressableKey = page.SourceAssetAddressableKeys[i];
                string label = BuildSourceAssetLabel(addressableKey);
                bool isSelected = string.Equals(_selectedSourceAssetKey, addressableKey, StringComparison.Ordinal)
                    && string.IsNullOrWhiteSpace(_selectedCollectionKey);
                if (!GUILayout.Button(label, isSelected ? EditorStyles.miniButtonMid : EditorStyles.miniButton))
                {
                    continue;
                }

                _selectedSourceAssetKey = addressableKey;
                _selectedCollectionKey = string.Empty;
                ClearCachedEditor();
            }
        }

        /// <summary>
        ///     collection一覧を描画します。
        /// </summary>
        /// <param name="page"> 選択中ページです。 </param>
        private void DrawCollectionNavigation(PlannerMasterDataEditorSettings.PageDefinition page)
        {
            EditorGUILayout.LabelField("Collections", EditorStyles.boldLabel);
            if (page.CollectionCategories.Count == 0)
            {
                EditorGUILayout.HelpBox("このページにはcollectionが設定されていません。", MessageType.None);
                return;
            }

            for (int i = 0; i < page.CollectionCategories.Count; i++)
            {
                string collectionKey = page.CollectionCategories[i];
                bool isSelected = string.Equals(_selectedCollectionKey, collectionKey, StringComparison.Ordinal);
                if (!GUILayout.Button(collectionKey, isSelected ? EditorStyles.miniButtonMid : EditorStyles.miniButton))
                {
                    continue;
                }

                _selectedCollectionKey = collectionKey;
                ClearCachedEditor();
            }
        }

        /// <summary>
        ///     詳細表示列を描画します。
        /// </summary>
        private void DrawDetailColumn()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            using (EditorGUILayout.ScrollViewScope scope = new(_contentScrollPosition))
            {
                _contentScrollPosition = scope.scrollPosition;
                if (!string.IsNullOrWhiteSpace(_selectedCollectionKey))
                {
                    DrawCollectionDetail(_selectedCollectionKey);
                }
                else if (!string.IsNullOrWhiteSpace(_selectedSourceAssetKey))
                {
                    DrawSourceAssetDetail(_selectedSourceAssetKey);
                }
                else
                {
                    EditorGUILayout.HelpBox("左の一覧から編集対象を選択してください。", MessageType.Info);
                }
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        ///     SourceAsset詳細を描画します。
        /// </summary>
        /// <param name="addressableKey"> 対象のAddressableキーです。 </param>
        private void DrawSourceAssetDetail(string addressableKey)
        {
            if (!SourceDataProviderRepositoryResolver.TryResolveAsset(addressableKey, out ScriptableObject sourceAsset))
            {
                EditorGUILayout.HelpBox(
                    $"Addressableキー「{addressableKey}」からSourceAssetを解決できません。",
                    MessageType.Error);
                return;
            }

            DrawObjectHeader("Source Asset", addressableKey, sourceAsset);
            DrawInspector(sourceAsset);
            EditorGUILayout.Space();
            DrawSourceAssetPreview(addressableKey, sourceAsset);
        }

        /// <summary>
        ///     collection詳細を描画します。
        /// </summary>
        /// <param name="collectionKey"> 対象CollectionKeyです。 </param>
        private void DrawCollectionDetail(string collectionKey)
        {
            if (!SourceDataProviderSettings.instance.TryGetCollectionMapping(
                collectionKey,
                out SourceDataProviderSettings.SourceCollectionMapping mapping))
            {
                EditorGUILayout.HelpBox(
                    $"CollectionKey「{collectionKey}」がSourceDataProviderへ登録されていません。",
                    MessageType.Error);
                return;
            }

            if (!SourceDataProviderRepositoryResolver.TryResolveAsset(
                mapping.SourceAssetAddressableKey,
                out ScriptableObject sourceAsset))
            {
                EditorGUILayout.HelpBox(
                    $"Addressableキー「{mapping.SourceAssetAddressableKey}」からSourceAssetを解決できません。",
                    MessageType.Error);
                return;
            }

            DrawObjectHeader($"Collection [{collectionKey}]", mapping.SourceAssetAddressableKey, sourceAsset);

            SerializedObject serializedObject = new(sourceAsset);
            SerializedProperty property = string.IsNullOrWhiteSpace(mapping.PropertyPath)
                ? null
                : serializedObject.FindProperty(mapping.PropertyPath);
            if (property == null)
            {
                EditorGUILayout.HelpBox(
                    "このcollectionはルートScriptableObjectをそのまま扱います。ルートのInspectorを表示します。",
                    MessageType.None);
                DrawInspector(sourceAsset);
                return;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(property, INCLUDE_CHILDREN);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(sourceAsset);
                AssetDatabase.SaveAssets();
            }

            EditorGUILayout.Space();
            DrawCollectionPreview(collectionKey, property);
        }

        /// <summary>
        ///     オブジェクトヘッダーを描画します。
        /// </summary>
        /// <param name="title"> セクションタイトルです。 </param>
        /// <param name="addressableKey"> Addressableキーです。 </param>
        /// <param name="target"> 対象オブジェクトです。 </param>
        private static void DrawObjectHeader(string title, string addressableKey, UnityEngine.Object target)
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField("Addressable Key", addressableKey);
                EditorGUILayout.ObjectField("Object", target, target.GetType(), false);
            }

            if (GUILayout.Button("Ping", GUILayout.Width(72f)))
            {
                EditorGUIUtility.PingObject(target);
                Selection.activeObject = target;
            }
        }

        /// <summary>
        ///     対象オブジェクトのInspectorを描画します。
        /// </summary>
        /// <param name="target"> 描画対象オブジェクトです。 </param>
        private void DrawInspector(UnityEngine.Object target)
        {
            EditorGUILayout.LabelField("Inspector", EditorStyles.boldLabel);
            UnityEditor.Editor editor = GetOrCreateEditor(target);
            if (editor == null)
            {
                EditorGUILayout.HelpBox("Inspectorの生成に失敗しました。", MessageType.Warning);
                return;
            }

            EditorGUI.BeginChangeCheck();
            editor.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        ///     SourceAsset向けの簡易プレビューを描画します。
        /// </summary>
        /// <param name="addressableKey"> Addressableキーです。 </param>
        /// <param name="sourceAsset"> 対象SourceAssetです。 </param>
        private void DrawSourceAssetPreview(string addressableKey, ScriptableObject sourceAsset)
        {
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

            if (string.Equals(addressableKey, "StageTreeAsset", StringComparison.Ordinal))
            {
                DrawStageTreePreview(sourceAsset);
                return;
            }

            string[] configuredPaths =
                SourceDataProviderRepositoryResolver.GetConfiguredCollectionPropertyPaths(addressableKey);
            if (configuredPaths.Length == 0)
            {
                EditorGUILayout.HelpBox("このSourceAssetにはcollection設定がありません。", MessageType.None);
                return;
            }

            SerializedObject serializedObject = new(sourceAsset);
            for (int i = 0; i < configuredPaths.Length; i++)
            {
                SerializedProperty property = serializedObject.FindProperty(configuredPaths[i]);
                if (property == null)
                {
                    continue;
                }

                DrawCollectionPreview(configuredPaths[i], property);
            }
        }

        /// <summary>
        ///     StageTreeAsset向けの可視化プレビューを描画します。
        /// </summary>
        /// <param name="sourceAsset"> 対象SourceAssetです。 </param>
        private static void DrawStageTreePreview(ScriptableObject sourceAsset)
        {
            SerializedObject serializedObject = new(sourceAsset);
            SerializedProperty nodeAssetsProperty = serializedObject.FindProperty(STAGE_NODE_ASSETS_PROPERTY_NAME);
            SerializedProperty connectionsProperty = serializedObject.FindProperty(STAGE_CONNECTIONS_PROPERTY_NAME);

            if (nodeAssetsProperty == null)
            {
                EditorGUILayout.HelpBox("ステージノード一覧を取得できません。", MessageType.Warning);
                return;
            }

            EditorGUILayout.HelpBox(
                $"ノード数: {nodeAssetsProperty.arraySize} / 接続数: {connectionsProperty?.arraySize ?? 0}",
                MessageType.None);

            EditorGUILayout.LabelField("Stage Nodes", EditorStyles.miniBoldLabel);
            for (int i = 0; i < Mathf.Min(nodeAssetsProperty.arraySize, PREVIEW_ELEMENT_LIMIT); i++)
            {
                SerializedProperty element = nodeAssetsProperty.GetArrayElementAtIndex(i);
                if (element.objectReferenceValue is not ScriptableObject nodeAsset)
                {
                    continue;
                }

                Texture previewTexture = AssetPreview.GetMiniThumbnail(nodeAsset);
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Label(previewTexture, GUILayout.Width(32f), GUILayout.Height(32f));
                EditorGUILayout.LabelField(nodeAsset.name, GUILayout.Height(32f));
                if (GUILayout.Button("Ping", GUILayout.Width(56f), GUILayout.Height(28f)))
                {
                    EditorGUIUtility.PingObject(nodeAsset);
                    Selection.activeObject = nodeAsset;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (nodeAssetsProperty.arraySize > PREVIEW_ELEMENT_LIMIT)
            {
                EditorGUILayout.HelpBox(
                    $"残り {nodeAssetsProperty.arraySize - PREVIEW_ELEMENT_LIMIT} 件はInspector側で確認してください。",
                    MessageType.None);
            }
        }

        /// <summary>
        ///     collection向けの簡易プレビューを描画します。
        /// </summary>
        /// <param name="label"> プレビュー表示名です。 </param>
        /// <param name="property"> 対象collectionプロパティです。 </param>
        private static void DrawCollectionPreview(string label, SerializedProperty property)
        {
            EditorGUILayout.LabelField($"Preview: {label}", EditorStyles.boldLabel);
            if (!property.isArray)
            {
                EditorGUILayout.HelpBox("collectionとして扱う対象が配列またはListではありません。", MessageType.Warning);
                return;
            }

            EditorGUILayout.HelpBox($"要素数: {property.arraySize}", MessageType.None);

            for (int i = 0; i < Mathf.Min(property.arraySize, PREVIEW_ELEMENT_LIMIT); i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"Element {i + 1}", EditorStyles.miniBoldLabel);
                DrawCollectionElementPreview(element);
                EditorGUILayout.EndVertical();
            }

            if (property.arraySize > PREVIEW_ELEMENT_LIMIT)
            {
                EditorGUILayout.HelpBox(
                    $"残り {property.arraySize - PREVIEW_ELEMENT_LIMIT} 件はInspector側で確認してください。",
                    MessageType.None);
            }
        }

        /// <summary>
        ///     collection要素の簡易プレビューを描画します。
        /// </summary>
        /// <param name="element"> 対象要素です。 </param>
        private static void DrawCollectionElementPreview(SerializedProperty element)
        {
            if (element.propertyType == SerializedPropertyType.ObjectReference)
            {
                DrawObjectReferencePreview(element.objectReferenceValue);
                return;
            }

            SerializedProperty assetProperty = element.FindPropertyRelative(COLLECTION_ASSET_PROPERTY_NAME);
            SerializedProperty idProperty = element.FindPropertyRelative(COLLECTION_ID_PROPERTY_NAME);
            if (idProperty != null)
            {
                SerializedProperty idValueProperty = idProperty.FindPropertyRelative(SOURCE_DATA_ID_PROPERTY_NAME);
                if (idValueProperty != null)
                {
                    EditorGUILayout.TextField("ID", idValueProperty.stringValue);
                }
            }

            if (assetProperty != null && assetProperty.propertyType == SerializedPropertyType.ObjectReference)
            {
                DrawObjectReferencePreview(assetProperty.objectReferenceValue);
            }

            EditorGUILayout.PropertyField(element, INCLUDE_CHILDREN);
        }

        /// <summary>
        ///     オブジェクト参照のプレビューを描画します。
        /// </summary>
        /// <param name="target"> 対象オブジェクトです。 </param>
        private static void DrawObjectReferencePreview(UnityEngine.Object target)
        {
            if (target == null)
            {
                EditorGUILayout.HelpBox("参照先が未設定です。", MessageType.None);
                return;
            }

            Texture preview = AssetPreview.GetAssetPreview(target) ?? AssetPreview.GetMiniThumbnail(target);
            if (preview != null)
            {
                GUILayout.Label(preview, GUILayout.Width(96f), GUILayout.Height(96f));
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField("Asset", target, target.GetType(), false);
            }
        }

        /// <summary>
        ///     対象オブジェクトに対応するEditorを取得します。
        /// </summary>
        /// <param name="target"> 対象オブジェクトです。 </param>
        /// <returns> 対応するEditorです。 </returns>
        private UnityEditor.Editor GetOrCreateEditor(UnityEngine.Object target)
        {
            if (_cachedEditorTarget == target && _cachedEditor != null)
            {
                return _cachedEditor;
            }

            ClearCachedEditor();
            _cachedEditorTarget = target;
            _cachedEditor = UnityEditor.Editor.CreateEditor(target);
            return _cachedEditor;
        }

        /// <summary>
        ///     キャッシュ済みEditorを破棄します。
        /// </summary>
        private void ClearCachedEditor()
        {
            if (_cachedEditor != null)
            {
                DestroyImmediate(_cachedEditor);
            }

            _cachedEditor = null;
            _cachedEditorTarget = null;
        }

        /// <summary>
        ///     一覧に指定要素が含まれるか判定します。
        /// </summary>
        /// <param name="values"> 対象一覧です。 </param>
        /// <param name="target"> 検索する文字列です。 </param>
        /// <returns> 含まれる場合はtrueです。 </returns>
        private static bool Contains(IReadOnlyList<string> values, string target)
        {
            if (values == null || string.IsNullOrWhiteSpace(target))
            {
                return false;
            }

            for (int i = 0; i < values.Count; i++)
            {
                if (string.Equals(values[i], target, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     SourceAssetナビゲーション用ラベルを生成します。
        /// </summary>
        /// <param name="addressableKey"> Addressableキーです。 </param>
        /// <returns> 表示ラベルです。 </returns>
        private static string BuildSourceAssetLabel(string addressableKey)
        {
            if (SourceDataProviderRepositoryResolver.TryResolveAsset(addressableKey, out ScriptableObject sourceAsset))
            {
                return $"{sourceAsset.GetType().Name}\n{addressableKey}";
            }

            return addressableKey;
        }

        private const float PAGE_SIDEBAR_WIDTH = 180f;
        private const float NAVIGATION_COLUMN_WIDTH = 280f;
        private const int PREVIEW_ELEMENT_LIMIT = 8;
        private const bool INCLUDE_CHILDREN = true;
        private const string STAGE_NODE_ASSETS_PROPERTY_NAME = "_nodeAssets";
        private const string STAGE_CONNECTIONS_PROPERTY_NAME = "_connections";
        private const string COLLECTION_ASSET_PROPERTY_NAME = "Asset";
        private const string COLLECTION_ID_PROPERTY_NAME = "Id";
        private const string SOURCE_DATA_ID_PROPERTY_NAME = "_id";
    }
}
