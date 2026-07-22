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
        [MenuItem(EditorWindowPathConst.PLANNER_MASTER_DATA_WINDOW_PATH)]
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
        private int _selectedCollectionItemIndex;
        private NavigationMode _navigationMode;
        private string _selectedSourceAssetKey = string.Empty;
        private string _selectedCollectionKey = string.Empty;
        private UnityEditor.Editor _cachedEditor;
        private UnityEngine.Object _cachedEditorTarget;

        /// <summary>
        ///     初期表示時に選択状態を補正します。
        /// </summary>
        private void OnEnable()
        {
            SourceDataProviderSettings.instance.RefreshSourceAssetsFromAddressables();
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
                _selectedCollectionItemIndex = 0;
                _selectedSourceAssetKey = string.Empty;
                _selectedCollectionKey = string.Empty;
                return;
            }

            _selectedPageIndex = Mathf.Clamp(_selectedPageIndex, 0, pages.Count - 1);
            PlannerMasterDataEditorSettings.PageDefinition page = pages[_selectedPageIndex];
            if (_navigationMode == NavigationMode.SourceAssets
                && page.SourceAssetAddressableKeys.Count == 0
                && page.CollectionCategories.Count > 0)
            {
                _navigationMode = NavigationMode.Collections;
            }
            else if (_navigationMode == NavigationMode.Collections
                && page.CollectionCategories.Count == 0
                && page.SourceAssetAddressableKeys.Count > 0)
            {
                _navigationMode = NavigationMode.SourceAssets;
            }

            if (_navigationMode == NavigationMode.SourceAssets)
            {
                if (!Contains(page.SourceAssetAddressableKeys, _selectedSourceAssetKey))
                {
                    _selectedSourceAssetKey = page.SourceAssetAddressableKeys.Count > 0
                        ? page.SourceAssetAddressableKeys[0]
                        : string.Empty;
                }
                _selectedCollectionKey = string.Empty;
                _selectedCollectionItemIndex = 0;
                return;
            }

            if (!Contains(page.CollectionCategories, _selectedCollectionKey))
            {
                _selectedCollectionKey = page.CollectionCategories.Count > 0
                    ? page.CollectionCategories[0]
                    : string.Empty;
            }
            _selectedSourceAssetKey = string.Empty;
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
                    _selectedCollectionItemIndex = 0;
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
                SourceDataProviderSettings.instance.RefreshSourceAssetsFromAddressables();
                ClearCachedEditor();
                Repaint();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(
                "Source Assetsではアセット全体を、Collectionsでは個別データを選択して編集できます。"
                + " 新しい型の追加は、SourceDataProvider設定とページ設定の更新だけで反映できます。",
                MessageType.Info);

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Source Assets: {page.SourceAssetAddressableKeys.Count}");
            EditorGUILayout.LabelField($"Collections: {page.CollectionCategories.Count}");
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        ///     SourceAsset / collection のナビゲーション列を描画します。
        /// </summary>
        /// <param name="page"> 選択中ページです。 </param>
        private void DrawNavigationColumn(PlannerMasterDataEditorSettings.PageDefinition page)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(NAVIGATION_COLUMN_WIDTH));
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            NavigationMode nextMode = (NavigationMode)GUILayout.Toolbar(
                (int)_navigationMode,
                NAVIGATION_MODE_LABELS);
            if (nextMode != _navigationMode)
            {
                _navigationMode = nextMode;
                _selectedSourceAssetKey = string.Empty;
                _selectedCollectionKey = string.Empty;
                _selectedCollectionItemIndex = 0;
                ClearCachedEditor();
                EnsureSelection();
            }

            using (EditorGUILayout.ScrollViewScope scope = new(_navigationScrollPosition))
            {
                _navigationScrollPosition = scope.scrollPosition;
                if (_navigationMode == NavigationMode.SourceAssets)
                {
                    DrawSourceAssetNavigation(page);
                }
                else
                {
                    DrawCollectionNavigation(page);
                }
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
                _selectedCollectionItemIndex = 0;
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
                string label = BuildCollectionLabel(collectionKey);
                bool isSelected = string.Equals(_selectedCollectionKey, collectionKey, StringComparison.Ordinal);
                if (GUILayout.Button(label, isSelected ? EditorStyles.miniButtonMid : EditorStyles.miniButton))
                {
                    _selectedCollectionKey = collectionKey;
                    _selectedSourceAssetKey = string.Empty;
                    _selectedCollectionItemIndex = 0;
                    ClearCachedEditor();
                }

                if (isSelected)
                {
                    DrawCollectionItemNavigation(collectionKey);
                }
            }
        }

        /// <summary>
        ///     選択中Collectionの個別要素一覧を描画します。
        /// </summary>
        /// <param name="collectionKey"> 対象CollectionKeyです。 </param>
        private void DrawCollectionItemNavigation(string collectionKey)
        {
            if (!TryResolveCollection(
                    collectionKey,
                    out _,
                    out _,
                    out _,
                    out SerializedProperty collectionProperty)
                || !collectionProperty.isArray)
            {
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("個別データ", EditorStyles.miniBoldLabel);
            if (collectionProperty.arraySize == 0)
            {
                EditorGUILayout.LabelField("データなし", EditorStyles.centeredGreyMiniLabel);
            }

            for (int i = 0; i < collectionProperty.arraySize; i++)
            {
                SerializedProperty element = collectionProperty.GetArrayElementAtIndex(i);
                string label = BuildCollectionItemLabel(element, i);
                bool isSelected = i == _selectedCollectionItemIndex;
                if (GUILayout.Button(
                    label,
                    isSelected ? EditorStyles.miniButtonMid : EditorStyles.miniButton))
                {
                    _selectedCollectionItemIndex = i;
                    ClearCachedEditor();
                }
            }
            EditorGUILayout.EndVertical();
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
                if (_navigationMode == NavigationMode.Collections
                    && !string.IsNullOrWhiteSpace(_selectedCollectionKey))
                {
                    DrawCollectionDetail(_selectedCollectionKey);
                }
                else if (_navigationMode == NavigationMode.SourceAssets
                    && !string.IsNullOrWhiteSpace(_selectedSourceAssetKey))
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
            if (!TryResolveCollection(
                    collectionKey,
                    out SourceDataProviderSettings.SourceCollectionMapping mapping,
                    out ScriptableObject sourceAsset,
                    out SerializedObject serializedObject,
                    out SerializedProperty property))
            {
                EditorGUILayout.HelpBox(
                    $"CollectionKey「{collectionKey}」のSourceAssetまたはProperty Pathを解決できません。",
                    MessageType.Error);
                return;
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField($"Collection [{collectionKey}]", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Source Assetを開く", EditorStyles.toolbarButton))
            {
                NavigateToSourceAsset(mapping.SourceAssetAddressableKey);
            }
            EditorGUILayout.EndHorizontal();

            DrawCollectionMetadata(mapping);
            if (!property.isArray)
            {
                EditorGUILayout.HelpBox(
                    "Collection対象が配列またはListではありません。",
                    MessageType.Error);
                return;
            }

            if (IsStageTreeCollection(collectionKey))
            {
                int selectedStageIndex = string.Equals(
                    collectionKey,
                    STAGE_ASSET_COLLECTION_KEY,
                    StringComparison.Ordinal)
                    ? _selectedCollectionItemIndex
                    : -1;
                int graphSelection = PlannerStageTreeGraphRenderer.Draw(sourceAsset, selectedStageIndex);
                if (selectedStageIndex >= 0 && graphSelection != selectedStageIndex)
                {
                    _selectedCollectionItemIndex = graphSelection;
                    ClearCachedEditor();
                }
            }

            DrawCollectionCommands(mapping, sourceAsset, property);
            if (property.arraySize == 0)
            {
                EditorGUILayout.HelpBox(
                    "Collectionにデータがありません。「データを追加」から作成できます。",
                    MessageType.Info);
                return;
            }

            _selectedCollectionItemIndex = Mathf.Clamp(
                _selectedCollectionItemIndex,
                0,
                property.arraySize - 1);
            SerializedProperty element = property.GetArrayElementAtIndex(_selectedCollectionItemIndex);
            DrawCollectionItemEditor(
                collectionKey,
                sourceAsset,
                serializedObject,
                element,
                _selectedCollectionItemIndex);
        }

        /// <summary>
        ///     Collectionの追加と登録解除操作を描画します。
        /// </summary>
        /// <param name="mapping"> Collection設定です。 </param>
        /// <param name="sourceAsset"> Collectionを保持するSourceAssetです。 </param>
        /// <param name="collectionProperty"> Collectionプロパティです。 </param>
        private void DrawCollectionCommands(
            SourceDataProviderSettings.SourceCollectionMapping mapping,
            ScriptableObject sourceAsset,
            SerializedProperty collectionProperty)
        {
            if (!SourceDataProviderRepositoryResolver.TryGetCollectionElementType(
                    sourceAsset,
                    mapping.PropertyPath,
                    out Type elementType))
            {
                EditorGUILayout.HelpBox(
                    "Collectionの要素型を取得できないため、データを追加できません。",
                    MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("データを追加", GUILayout.Height(COMMAND_BUTTON_HEIGHT)))
            {
                ShowCollectionCreationMenu(mapping, sourceAsset, elementType);
            }

            using (new EditorGUI.DisabledScope(collectionProperty.arraySize == 0))
            {
                if (GUILayout.Button(
                    "Collectionから外す",
                    GUILayout.Height(COMMAND_BUTTON_HEIGHT)))
                {
                    RemoveSelectedCollectionItem(sourceAsset, mapping);
                }
            }
            EditorGUILayout.EndHorizontal();

            if (typeof(ScriptableObject).IsAssignableFrom(elementType)
                && string.IsNullOrWhiteSpace(mapping.AssetCreationDirectory))
            {
                EditorGUILayout.HelpBox(
                    "新規アセットの生成先はSource Data Provider設定のAsset Creation Directoryで指定します。",
                    MessageType.Info);
            }
        }

        /// <summary>
        ///     Collection要素型に応じた作成メニューを表示します。
        /// </summary>
        /// <param name="mapping"> Collection設定です。 </param>
        /// <param name="sourceAsset"> Collectionを保持するSourceAssetです。 </param>
        /// <param name="elementType"> Collectionの要素型です。 </param>
        private void ShowCollectionCreationMenu(
            SourceDataProviderSettings.SourceCollectionMapping mapping,
            ScriptableObject sourceAsset,
            Type elementType)
        {
            IReadOnlyList<Type> assetTypes =
                PlannerCollectionItemCreator.GetCreatableAssetTypes(elementType);
            if (assetTypes.Count == 0)
            {
                AddInlineCollectionItem(mapping, sourceAsset, elementType);
                return;
            }

            if (assetTypes.Count == 1)
            {
                CreateCollectionAsset(mapping, sourceAsset, assetTypes[0]);
                return;
            }

            GenericMenu menu = new();
            for (int i = 0; i < assetTypes.Count; i++)
            {
                Type assetType = assetTypes[i];
                menu.AddItem(
                    new GUIContent(assetType.Name),
                    false,
                    () => CreateCollectionAsset(mapping, sourceAsset, assetType));
            }
            menu.ShowAsContext();
        }

        /// <summary>
        ///     指定型のScriptableObjectを生成してCollectionへ追加します。
        /// </summary>
        /// <param name="mapping"> Collection設定です。 </param>
        /// <param name="sourceAsset"> Collectionを保持するSourceAssetです。 </param>
        /// <param name="assetType"> 生成する型です。 </param>
        private void CreateCollectionAsset(
            SourceDataProviderSettings.SourceCollectionMapping mapping,
            ScriptableObject sourceAsset,
            Type assetType)
        {
            SerializedObject serializedObject = new(sourceAsset);
            SerializedProperty collectionProperty = serializedObject.FindProperty(mapping.PropertyPath);
            int newIndex = collectionProperty?.arraySize ?? 0;
            if (!PlannerCollectionItemCreator.TryCreateAsset(
                    sourceAsset,
                    mapping,
                    serializedObject,
                    collectionProperty,
                    assetType,
                    out ScriptableObject createdAsset,
                    out string errorMessage))
            {
                ShowNotification(new GUIContent(errorMessage));
                return;
            }

            _selectedCollectionItemIndex = newIndex;
            ClearCachedEditor();
            EditorGUIUtility.PingObject(createdAsset);
            Repaint();
        }

        /// <summary>
        ///     インラインCollectionへ新規要素を追加します。
        /// </summary>
        /// <param name="mapping"> Collection設定です。 </param>
        /// <param name="sourceAsset"> Collectionを保持するSourceAssetです。 </param>
        /// <param name="elementType"> Collectionの要素型です。 </param>
        private void AddInlineCollectionItem(
            SourceDataProviderSettings.SourceCollectionMapping mapping,
            ScriptableObject sourceAsset,
            Type elementType)
        {
            SerializedObject serializedObject = new(sourceAsset);
            SerializedProperty collectionProperty = serializedObject.FindProperty(mapping.PropertyPath);
            int newIndex = collectionProperty?.arraySize ?? 0;
            if (!PlannerCollectionItemCreator.TryAddInlineItem(
                    sourceAsset,
                    serializedObject,
                    collectionProperty,
                    elementType,
                    out string errorMessage))
            {
                ShowNotification(new GUIContent(errorMessage));
                return;
            }

            _selectedCollectionItemIndex = newIndex;
            ClearCachedEditor();
            Repaint();
        }

        /// <summary>
        ///     選択中要素をCollectionから登録解除します。
        /// </summary>
        /// <param name="sourceAsset"> Collectionを保持するSourceAssetです。 </param>
        /// <param name="mapping"> Collection設定です。 </param>
        private void RemoveSelectedCollectionItem(
            ScriptableObject sourceAsset,
            SourceDataProviderSettings.SourceCollectionMapping mapping)
        {
            if (!EditorUtility.DisplayDialog(
                    "Collectionから外す",
                    "選択中データをCollectionから外します。参照先アセット自体は削除しません。",
                    "外す",
                    "キャンセル"))
            {
                return;
            }

            SerializedObject serializedObject = new(sourceAsset);
            SerializedProperty collectionProperty = serializedObject.FindProperty(mapping.PropertyPath);
            if (collectionProperty == null
                || !collectionProperty.isArray
                || collectionProperty.arraySize == 0)
            {
                return;
            }

            int removeIndex = Mathf.Clamp(
                _selectedCollectionItemIndex,
                0,
                collectionProperty.arraySize - 1);
            Undo.RecordObject(sourceAsset, "Collectionからデータを外す");
            int previousSize = collectionProperty.arraySize;
            collectionProperty.DeleteArrayElementAtIndex(removeIndex);
            if (collectionProperty.arraySize == previousSize)
            {
                collectionProperty.DeleteArrayElementAtIndex(removeIndex);
            }
            serializedObject.ApplyModifiedProperties();
            SaveAssetIfDirty(sourceAsset);
            _selectedCollectionItemIndex = Mathf.Max(0, removeIndex - 1);
            ClearCachedEditor();
        }

        /// <summary>
        ///     Collectionの選択中要素を編集します。
        /// </summary>
        /// <param name="collectionKey"> 対象CollectionKeyです。 </param>
        /// <param name="sourceAsset"> Collectionを保持するSourceAssetです。 </param>
        /// <param name="serializedObject"> SourceAssetのSerializedObjectです。 </param>
        /// <param name="element"> 選択中要素です。 </param>
        /// <param name="elementIndex"> 選択中インデックスです。 </param>
        private void DrawCollectionItemEditor(
            string collectionKey,
            ScriptableObject sourceAsset,
            SerializedObject serializedObject,
            SerializedProperty element,
            int elementIndex)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                $"個別データ {elementIndex + 1}: {BuildCollectionItemLabel(element, elementIndex)}",
                EditorStyles.boldLabel);

            if (element.propertyType == SerializedPropertyType.ObjectReference)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(element, new GUIContent("Asset"));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    SaveAssetIfDirty(sourceAsset);
                    ClearCachedEditor();
                }

                if (element.objectReferenceValue != null)
                {
                    DrawInspector(element.objectReferenceValue);
                    EditorGUILayout.Space();
                    DrawCollectionObjectReferencePreview(
                        collectionKey,
                        element.objectReferenceValue);
                }
                return;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(element, INCLUDE_CHILDREN);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                SaveAssetIfDirty(sourceAsset);
            }

            EditorGUILayout.Space();
            DrawCollectionElementPreview(element);
        }

        /// <summary>
        ///     Collection設定と対象プロパティをまとめて解決します。
        /// </summary>
        /// <param name="collectionKey"> CollectionKeyです。 </param>
        /// <param name="mapping"> Collection設定です。 </param>
        /// <param name="sourceAsset"> SourceAssetです。 </param>
        /// <param name="serializedObject"> SourceAssetのSerializedObjectです。 </param>
        /// <param name="collectionProperty"> Collectionプロパティです。 </param>
        /// <returns> 全て解決できた場合はtrueです。 </returns>
        private static bool TryResolveCollection(
            string collectionKey,
            out SourceDataProviderSettings.SourceCollectionMapping mapping,
            out ScriptableObject sourceAsset,
            out SerializedObject serializedObject,
            out SerializedProperty collectionProperty)
        {
            mapping = null;
            sourceAsset = null;
            serializedObject = null;
            collectionProperty = null;
            if (!SourceDataProviderSettings.instance.TryGetCollectionMapping(collectionKey, out mapping)
                || !SourceDataProviderRepositoryResolver.TryResolveAsset(
                    mapping.SourceAssetAddressableKey,
                    out sourceAsset))
            {
                return false;
            }

            serializedObject = new SerializedObject(sourceAsset);
            collectionProperty = serializedObject.FindProperty(mapping.PropertyPath);
            return collectionProperty != null;
        }

        /// <summary>
        ///     指定Addressableキーを含むページのSource Assets画面へ移動します。
        /// </summary>
        /// <param name="addressableKey"> 移動先SourceAssetのAddressableキーです。 </param>
        private void NavigateToSourceAsset(string addressableKey)
        {
            IReadOnlyList<PlannerMasterDataEditorSettings.PageDefinition> pages =
                PlannerMasterDataEditorSettings.instance.Pages;
            for (int i = 0; i < pages.Count; i++)
            {
                if (!Contains(pages[i].SourceAssetAddressableKeys, addressableKey))
                {
                    continue;
                }

                _selectedPageIndex = i;
                _navigationMode = NavigationMode.SourceAssets;
                _selectedSourceAssetKey = addressableKey;
                _selectedCollectionKey = string.Empty;
                _selectedCollectionItemIndex = 0;
                ClearCachedEditor();
                return;
            }

            ShowNotification(new GUIContent(
                $"SourceAsset「{addressableKey}」を表示するページが設定されていません。"));
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
            EditorGUILayout.LabelField("全項目編集", EditorStyles.boldLabel);
            UnityEditor.Editor editor = GetOrCreateEditor(target);
            if (editor == null)
            {
                EditorGUILayout.HelpBox("Inspectorの生成に失敗しました。", MessageType.Warning);
                return;
            }

            EditorGUI.BeginChangeCheck();
            editor.DrawDefaultInspector();
            if (EditorGUI.EndChangeCheck())
            {
                SaveAssetIfDirty(target);
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
                PlannerStageTreeGraphRenderer.Draw(sourceAsset, -1);
                return;
            }

            bool hasSpecialPreview = PlannerEnemyStatusPreview.CanDraw(addressableKey, sourceAsset);
            if (hasSpecialPreview)
            {
                PlannerEnemyStatusPreview.Draw(sourceAsset);
            }

            IReadOnlyList<SourceDataProviderSettings.SourceCollectionMapping> mappings =
                SourceDataProviderSettings.instance.GetCollectionMappingsByAddressableKey(addressableKey);
            if (mappings.Count == 0)
            {
                if (!hasSpecialPreview)
                {
                    EditorGUILayout.HelpBox("このSourceAssetには専用プレビューがありません。", MessageType.None);
                }
                return;
            }

            SerializedObject serializedObject = new(sourceAsset);
            for (int i = 0; i < mappings.Count; i++)
            {
                SourceDataProviderSettings.SourceCollectionMapping mapping = mappings[i];
                SerializedProperty property = serializedObject.FindProperty(mapping.PropertyPath);
                if (property == null)
                {
                    continue;
                }

                DrawCollectionPreview(mapping.CollectionKey, property);
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

            if (string.Equals(label, WAVE_COLLECTION_KEY, StringComparison.Ordinal))
            {
                PlannerEnemyWavePreview.DrawRepository(property, PREVIEW_ELEMENT_LIMIT);
                return;
            }

            if (IsScenarioCatalogCollection(label))
            {
                DrawScenarioCatalogPreview(property);
                return;
            }

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

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(element, INCLUDE_CHILDREN);
            }
        }

        /// <summary>
        ///     編集されたアセットのみを保存します。
        /// </summary>
        /// <param name="target"> 保存対象です。 </param>
        private static void SaveAssetIfDirty(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssetIfDirty(target);
        }

        /// <summary>
        ///     collection設定の補足情報を描画します。
        /// </summary>
        /// <param name="mapping"> 対象のcollection設定です。 </param>
        private static void DrawCollectionMetadata(
            SourceDataProviderSettings.SourceCollectionMapping mapping)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField("Collection Key", mapping.CollectionKey);
                EditorGUILayout.TextField("Property Path", mapping.PropertyPath);
                EditorGUILayout.TextField(
                    "Asset Creation Directory",
                    string.IsNullOrWhiteSpace(mapping.AssetCreationDirectory)
                        ? "<未設定>"
                        : mapping.AssetCreationDirectory);
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        ///     CollectionKeyに応じた参照アセットのプレビューを描画します。
        /// </summary>
        /// <param name="collectionKey"> 対象CollectionKeyです。 </param>
        /// <param name="target"> 参照先アセットです。 </param>
        private static void DrawCollectionObjectReferencePreview(
            string collectionKey,
            UnityEngine.Object target)
        {
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            if (string.Equals(collectionKey, WAVE_COLLECTION_KEY, StringComparison.Ordinal))
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                PlannerEnemyWavePreview.Draw(target, PREVIEW_ELEMENT_LIMIT);
                EditorGUILayout.EndVertical();
                return;
            }

            DrawObjectReferencePreview(target);
        }

        /// <summary>
        ///     Scenario catalog向けの専用プレビューを描画します。
        /// </summary>
        /// <param name="property"> カタログ配列プロパティです。 </param>
        private static void DrawScenarioCatalogPreview(SerializedProperty property)
        {
            for (int i = 0; i < Mathf.Min(property.arraySize, PREVIEW_ELEMENT_LIMIT); i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                SerializedProperty idProperty = element.FindPropertyRelative(COLLECTION_ID_PROPERTY_NAME);
                SerializedProperty idValueProperty = idProperty?.FindPropertyRelative(SOURCE_DATA_ID_PROPERTY_NAME);
                SerializedProperty assetKeyProperty = element.FindPropertyRelative(CATALOG_ASSET_KEY_PROPERTY_NAME);
                SerializedProperty assetProperty = element.FindPropertyRelative(COLLECTION_ASSET_PROPERTY_NAME);

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                if (assetProperty != null && assetProperty.objectReferenceValue != null)
                {
                    Texture preview = AssetPreview.GetAssetPreview(assetProperty.objectReferenceValue)
                        ?? AssetPreview.GetMiniThumbnail(assetProperty.objectReferenceValue);
                    GUILayout.Label(preview, GUILayout.Width(64f), GUILayout.Height(64f));
                }

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField($"ID: {idValueProperty?.stringValue ?? "<未設定>"}");
                EditorGUILayout.LabelField($"Key: {assetKeyProperty?.stringValue ?? "<未設定>"}");
                if (assetProperty != null && assetProperty.objectReferenceValue != null)
                {
                    EditorGUILayout.ObjectField("Asset", assetProperty.objectReferenceValue, typeof(UnityEngine.Object), false);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            if (property.arraySize > PREVIEW_ELEMENT_LIMIT)
            {
                EditorGUILayout.HelpBox(
                    $"残り {property.arraySize - PREVIEW_ELEMENT_LIMIT} 件はInspector側で確認してください。",
                    MessageType.None);
            }
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
                int collectionCount =
                    SourceDataProviderSettings.instance.GetCollectionMappingsByAddressableKey(addressableKey).Count;
                return $"{sourceAsset.name} ({sourceAsset.GetType().Name}) [{collectionCount}]";
            }

            return addressableKey;
        }

        /// <summary>
        ///     CollectionKeyナビゲーション用ラベルを生成します。
        /// </summary>
        /// <param name="collectionKey"> CollectionKeyです。 </param>
        /// <returns> 表示ラベルです。 </returns>
        private static string BuildCollectionLabel(string collectionKey)
        {
            if (!SourceDataProviderSettings.instance.TryGetCollectionMapping(
                collectionKey,
                out SourceDataProviderSettings.SourceCollectionMapping mapping)
                || !SourceDataProviderRepositoryResolver.TryResolveAsset(
                    mapping.SourceAssetAddressableKey,
                    out ScriptableObject sourceAsset))
            {
                return collectionKey;
            }

            SerializedObject serializedObject = new(sourceAsset);
            SerializedProperty property = serializedObject.FindProperty(mapping.PropertyPath);
            int count = property != null && property.isArray ? property.arraySize : 0;
            return $"{collectionKey} ({count})";
        }

        /// <summary>
        ///     Collection内の個別データ表示名を生成します。
        /// </summary>
        /// <param name="element"> Collection要素です。 </param>
        /// <param name="index"> Collection内のインデックスです。 </param>
        /// <returns> 個別データ表示名です。 </returns>
        private static string BuildCollectionItemLabel(SerializedProperty element, int index)
        {
            if (element?.objectReferenceValue != null)
            {
                if (PlannerEnemyWavePreview.TryBuildItemLabel(
                    element.objectReferenceValue,
                    out string label))
                {
                    return label;
                }

                return element.objectReferenceValue.name;
            }

            SerializedProperty dataIdProperty = element?.FindPropertyRelative(COLLECTION_ID_PROPERTY_NAME)
                ?? element?.FindPropertyRelative(STAGE_ID_PROPERTY_NAME);
            SerializedProperty idValueProperty = dataIdProperty?.FindPropertyRelative(SOURCE_DATA_ID_PROPERTY_NAME)
                ?? element?.FindPropertyRelative(SOURCE_DATA_ID_PROPERTY_NAME);
            if (!string.IsNullOrWhiteSpace(idValueProperty?.stringValue))
            {
                return idValueProperty.stringValue;
            }

            return $"Element {index + 1}";
        }

        /// <summary>
        ///     StageTreeの構成Collectionか判定します。
        /// </summary>
        /// <param name="collectionKey"> CollectionKeyです。 </param>
        /// <returns> StageまたはBind Collectionの場合はtrueです。 </returns>
        private static bool IsStageTreeCollection(string collectionKey)
        {
            return string.Equals(collectionKey, STAGE_ASSET_COLLECTION_KEY, StringComparison.Ordinal)
                || string.Equals(collectionKey, STAGE_BIND_COLLECTION_KEY, StringComparison.Ordinal);
        }

        /// <summary>
        ///     Scenario catalog系のCollectionKeyか判定します。
        /// </summary>
        /// <param name="collectionKey"> CollectionKeyです。 </param>
        /// <returns> Scenario catalog系の場合はtrueです。 </returns>
        private static bool IsScenarioCatalogCollection(string collectionKey)
        {
            return string.Equals(collectionKey, SCENARIO_BACKGROUND_COLLECTION_KEY, StringComparison.Ordinal)
                || string.Equals(collectionKey, SCENARIO_ANIMATION_COLLECTION_KEY, StringComparison.Ordinal)
                || string.Equals(collectionKey, SCENARIO_PORTRAIT_COLLECTION_KEY, StringComparison.Ordinal);
        }

        private const float PAGE_SIDEBAR_WIDTH = 180f;
        private const float NAVIGATION_COLUMN_WIDTH = 280f;
        private const float COMMAND_BUTTON_HEIGHT = 28f;
        private const int PREVIEW_ELEMENT_LIMIT = 8;
        private const bool INCLUDE_CHILDREN = true;
        private const string COLLECTION_ASSET_PROPERTY_NAME = "Asset";
        private const string COLLECTION_ID_PROPERTY_NAME = "Id";
        private const string CATALOG_ASSET_KEY_PROPERTY_NAME = "AssetKey";
        private const string SOURCE_DATA_ID_PROPERTY_NAME = "_id";
        private const string STAGE_ID_PROPERTY_NAME = "_stageId";
        private const string STAGE_ASSET_COLLECTION_KEY = "StageAsset";
        private const string STAGE_BIND_COLLECTION_KEY = "StageBind";
        private const string WAVE_COLLECTION_KEY = "Wave";
        private const string SCENARIO_BACKGROUND_COLLECTION_KEY = "ScenarioBackground";
        private const string SCENARIO_ANIMATION_COLLECTION_KEY = "ScenarioAnimation";
        private const string SCENARIO_PORTRAIT_COLLECTION_KEY = "ScenarioPortrait";

        private static readonly string[] NAVIGATION_MODE_LABELS = { "Source Assets", "Collections" };

        /// <summary>
        ///     ナビゲーション列へ表示するデータ階層です。
        /// </summary>
        private enum NavigationMode
        {
            SourceAssets,
            Collections,
        }
    }
}
