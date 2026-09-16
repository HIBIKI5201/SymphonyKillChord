using KillChord.Editor.ProjectWindow;
using KillChord.Editor.Inspectors.SourceData;
using KillChord.Editor.SourceDataProvider.Core;
using KillChord.Editor.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.SourceDataProvider.Wiki
{
    /// <summary>
    ///     SourceDataProvider登録済みのマスターデータを一覧・検索するための閲覧専用wiki画面です。
    ///     値編集は一切行わず、選択したデータの実体をProject/Inspectorへ橋渡しする役割に専念します。
    /// </summary>
    public sealed class PlannerMasterDataWindow : EditorWindow
    {
        /// <summary>
        ///     CoreのPropertyDrawer等がWiki型を直接参照せずジャンプできるよう、
        ///     <see cref="PlannerNavigationHub"/>へ実処理を登録します(WikiからCoreへの依存に限定するため)。
        /// </summary>
        [InitializeOnLoadMethod]
        private static void RegisterNavigationHub()
        {
            PlannerNavigationHub.NavigateToSourceAsset = addressableKey =>
            {
                if (!TryGetOrOpenWindow(out PlannerMasterDataWindow window))
                {
                    return false;
                }

                window.NavigateToSourceAsset(addressableKey);
                return true;
            };

            PlannerNavigationHub.NavigateToCollectionItem = (collectionKey, dataId) =>
            {
                if (!TryGetOrOpenWindow(out PlannerMasterDataWindow window))
                {
                    return false;
                }

                window.NavigateToCollectionItem(collectionKey, dataId);
                return true;
            };
        }

        /// <summary>
        ///     ウィンドウを開きます。
        /// </summary>
        [MenuItem(EditorWindowPathConst.PLANNER_MASTER_DATA_WINDOW_PATH)]
        public static void ShowWindow()
        {
            PlannerMasterDataWindow window = GetWindow<PlannerMasterDataWindow>();
            window.titleContent = new GUIContent("Planner Master Data");
            window.minSize = new Vector2(960f, 560f);
        }

        /// <summary>
        ///     開いているウィンドウを取得し、無ければ新規に開きます。
        /// </summary>
        /// <param name="window"> 取得したウィンドウです。 </param>
        /// <returns> ウィンドウを取得できた場合はtrueです。 </returns>
        public static bool TryGetOrOpenWindow(out PlannerMasterDataWindow window)
        {
            window = Resources.FindObjectsOfTypeAll<PlannerMasterDataWindow>().FirstOrDefault();
            if (window == null)
            {
                ShowWindow();
                window = Resources.FindObjectsOfTypeAll<PlannerMasterDataWindow>().FirstOrDefault();
            }

            return window != null;
        }

        [SerializeField, Tooltip("選択中ページのIndexです。ドメインリロード間で表示状態を保持するために使用します。")]
        private int _selectedPageIndex;
        [SerializeField, Tooltip("Collection内で選択中の要素Indexです。ドメインリロード間で表示状態を保持するために使用します。")]
        private int _selectedCollectionItemIndex;
        [SerializeField, Tooltip("ナビゲーション列に表示する階層(SourceAssets/Collections)です。")]
        private NavigationMode _navigationMode;
        [SerializeField, Tooltip("選択中SourceAssetのAddressableキーです。")]
        private string _selectedSourceAssetKey = string.Empty;
        [SerializeField, Tooltip("選択中CollectionKeyです。")]
        private string _selectedCollectionKey = string.Empty;
        [SerializeField, Tooltip("検索ボックスへ入力中の検索クエリです。")]
        private string _searchQuery = string.Empty;
        [SerializeField, Tooltip("Collection項目一覧の表示ソート順です。")]
        private CollectionSortMode _collectionSortMode;
        private Vector2 _pageScrollPosition;
        private Vector2 _navigationScrollPosition;
        private Vector2 _itemsScrollPosition;
        private Vector2 _searchScrollPosition;
        private string _lastIndexedSearchQuery;
        private bool _showCollectionMetadata;
        private readonly List<SearchResult> _searchResults = new();

        /// <summary>
        ///     初期表示時に選択状態を補正します。
        /// </summary>
        private void OnEnable()
        {
            SourceDataProviderSettings.instance.RefreshSourceAssetsFromAddressables();
            EditorApplication.projectChanged += InvalidateSearch;
            Undo.undoRedoPerformed += InvalidateSearch;
            SourceDataProviderSettings.OnChanged += InvalidateSearch;
            InvalidateSearch();
            EnsureSelection();
        }

        /// <summary>
        ///     ウィンドウを閉じるときに変更通知を解除します。
        /// </summary>
        private void OnDisable()
        {
            EditorApplication.projectChanged -= InvalidateSearch;
            Undo.undoRedoPerformed -= InvalidateSearch;
            SourceDataProviderSettings.OnChanged -= InvalidateSearch;
        }

        /// <summary>
        ///     Inspectorで編集して戻った際に検索結果を更新します。
        /// </summary>
        private void OnFocus()
        {
            InvalidateSearch();
        }

        /// <summary>
        ///     次のLayoutで検索結果を再構築します。
        /// </summary>
        private void InvalidateSearch()
        {
            _lastIndexedSearchQuery = null;
            Repaint();
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
            DrawToolbar();

            if (!string.IsNullOrWhiteSpace(_searchQuery))
            {
                DrawSearchResults();
                return;
            }

            EditorGUILayout.BeginHorizontal();
            DrawPageSidebar(pages);
            DrawNavigationColumn(pages[_selectedPageIndex]);
            DrawItemsColumn();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        ///     検索ボックスとRefreshボタンを持つToolbarを描画します。
        /// </summary>
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Variant", GUILayout.Width(46f));
            GameDataVariant currentVariant = GameDataVariantEditorState.SelectedVariant;
            GameDataVariant nextVariant = (GameDataVariant)EditorGUILayout.EnumPopup(
                currentVariant,
                EditorStyles.toolbarPopup,
                GUILayout.Width(80f));
            if (nextVariant != currentVariant)
            {
                GameDataVariantEditorState.SetSelectedVariant(nextVariant);
                OnVariantChanged();
            }

            GUILayout.Space(8f);
            GUILayout.Label("Search", GUILayout.Width(46f));
            GUI.SetNextControlName(SEARCH_FIELD_CONTROL_NAME);
            string nextQuery = EditorGUILayout.TextField(
                _searchQuery,
                EditorStyles.toolbarSearchField,
                GUILayout.MinWidth(200f));
            if (nextQuery != _searchQuery)
            {
                SetSearchQuery(nextQuery);
            }

            if (!string.IsNullOrEmpty(_searchQuery) && GUILayout.Button("×", EditorStyles.toolbarButton, GUILayout.Width(20f)))
            {
                SetSearchQuery(string.Empty);
                GUI.FocusControl(null);
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(72f)))
            {
                SourceDataProviderSettings.instance.RefreshSourceAssetsFromAddressables();
                _lastIndexedSearchQuery = null;
                Repaint();
                GUIUtility.ExitGUI();
            }

            if (GUILayout.Button("Page Settings", EditorStyles.toolbarButton, GUILayout.Width(96f)))
            {
                SettingsService.OpenProjectSettings("Project/KillChord/Planner Master Data");
            }

            if (GUILayout.Button("SourceData Settings", EditorStyles.toolbarButton, GUILayout.Width(128f)))
            {
                SettingsService.OpenProjectSettings("Project/KillChord/Source Data Provider");
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        ///     検索クエリを更新します。検索結果や表示先の変更でGUILayoutの構造が食い違わないよう、
        ///     入力イベントを打ち切り、次のLayoutで新しい状態を描画します。
        /// </summary>
        /// <param name="nextQuery"> 更新後の検索クエリです。 </param>
        private void SetSearchQuery(string nextQuery)
        {
            bool wasEmpty = string.IsNullOrWhiteSpace(_searchQuery);
            _searchQuery = nextQuery;
            bool isEmptyNow = string.IsNullOrWhiteSpace(_searchQuery);
            if (wasEmpty != isEmptyNow && Event.current != null)
            {
                Repaint();
                GUIUtility.ExitGUI();
            }
            if (Event.current != null && Event.current.type != EventType.Layout)
            {
                Repaint();
                GUIUtility.ExitGUI();
            }
        }

        /// <summary>
        ///     表示対象のゲームデータ種別(Demo/Release)が切り替わった際、関連キャッシュを破棄し選択状態を補正します。
        /// </summary>
        private void OnVariantChanged()
        {
            BattleSceneDataReader.ClearCache();
            SourceDataAssetIndex.Invalidate();
            _lastIndexedSearchQuery = null;
            SourceDataProviderSettings.instance.RefreshSourceAssetsFromAddressables();
            EnsureSelection();
            Repaint();
            GUIUtility.ExitGUI();
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
            List<string> visibleSourceAssetKeys = GetVisibleSourceAssetKeys(page);
            if (_navigationMode == NavigationMode.SourceAssets
                && visibleSourceAssetKeys.Count == 0
                && page.CollectionCategories.Count > 0)
            {
                _navigationMode = NavigationMode.Collections;
            }
            else if (_navigationMode == NavigationMode.Collections
                && page.CollectionCategories.Count == 0
                && visibleSourceAssetKeys.Count > 0)
            {
                _navigationMode = NavigationMode.SourceAssets;
            }

            if (_navigationMode == NavigationMode.SourceAssets)
            {
                if (!Contains(page.SourceAssetAddressableKeys, _selectedSourceAssetKey)
                    || !visibleSourceAssetKeys.Contains(_selectedSourceAssetKey))
                {
                    _selectedSourceAssetKey = visibleSourceAssetKeys.Count > 0
                        ? visibleSourceAssetKeys[0]
                        : string.Empty;
                }
                _selectedCollectionKey = string.Empty;
                return;
            }

            if (!Contains(page.CollectionCategories, _selectedCollectionKey))
            {
                _selectedCollectionKey = page.CollectionCategories.Count > 0
                    ? page.CollectionCategories[0]
                    : string.Empty;
                _selectedCollectionItemIndex = 0;
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
                    if (!GUILayout.Button(pages[i].DisplayName, style, GUILayout.Height(32f)))
                    {
                        continue;
                    }

                    _selectedPageIndex = i;
                    _selectedCollectionItemIndex = 0;
                    _selectedSourceAssetKey = string.Empty;
                    _selectedCollectionKey = string.Empty;
                    EnsureSelection();
                    GUIUtility.ExitGUI();
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        ///     SourceAsset / collection のナビゲーション列を描画します(閲覧一覧: (a))。
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
                EnsureSelection();
                GUIUtility.ExitGUI();
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
            List<string> visibleKeys = GetVisibleSourceAssetKeys(page);
            if (visibleKeys.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    page.SourceAssetAddressableKeys.Count == 0
                        ? "このページにはSourceAssetが設定されていません。"
                        : "このページのSourceAssetは単一CollectionのRepositoryのみのため、Collectionsタブから参照してください。",
                    MessageType.None);
                return;
            }

            for (int i = 0; i < visibleKeys.Count; i++)
            {
                string addressableKey = visibleKeys[i];
                string label = BuildSourceAssetLabel(addressableKey);
                bool isSelected = string.Equals(_selectedSourceAssetKey, addressableKey, StringComparison.Ordinal);
                if (!GUILayout.Button(label, isSelected ? EditorStyles.miniButtonMid : EditorStyles.miniButton))
                {
                    continue;
                }

                NavigateToSourceAsset(addressableKey);
                GUIUtility.ExitGUI();
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
                if (!GUILayout.Button(label, isSelected ? EditorStyles.miniButtonMid : EditorStyles.miniButton))
                {
                    continue;
                }

                _selectedCollectionKey = collectionKey;
                _selectedSourceAssetKey = string.Empty;
                _selectedCollectionItemIndex = 0;
                GUIUtility.ExitGUI();
            }
        }

        /// <summary>
        ///     選択中のSourceAssetまたはCollectionが持つデータ項目一覧を描画します(閲覧詳細: (b))。
        ///     行をクリックすると実体を選択・Pingして、実際の編集はInspector側へ委ねます。
        /// </summary>
        private void DrawItemsColumn()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            using (EditorGUILayout.ScrollViewScope scope = new(_itemsScrollPosition))
            {
                _itemsScrollPosition = scope.scrollPosition;
                if (_navigationMode == NavigationMode.Collections
                    && !string.IsNullOrWhiteSpace(_selectedCollectionKey))
                {
                    DrawCollectionItems(_selectedCollectionKey);
                }
                else if (_navigationMode == NavigationMode.SourceAssets
                    && !string.IsNullOrWhiteSpace(_selectedSourceAssetKey))
                {
                    DrawSourceAssetItems(_selectedSourceAssetKey);
                }
                else
                {
                    EditorGUILayout.HelpBox("左の一覧から閲覧対象を選択してください。", MessageType.Info);
                }
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        ///     SourceAssetの概要と、そのSourceAssetが持つCollection一覧を描画します。
        /// </summary>
        /// <param name="addressableKey"> 対象のAddressableキーです。 </param>
        private void DrawSourceAssetItems(string addressableKey)
        {
            if (!SourceDataProviderRepositoryResolver.TryResolveAsset(
                    addressableKey,
                    out ScriptableObject sourceAsset))
            {
                EditorGUILayout.HelpBox(
                    $"SourceAsset「{addressableKey}」を解決できません。Addressablesの登録状況を確認してください。",
                    MessageType.Error);
                return;
            }

            DrawObjectHeaderRow(addressableKey, sourceAsset.GetType().Name, sourceAsset);

            IReadOnlyList<SourceDataProviderSettings.SourceCollectionMapping> mappings =
                SourceDataProviderSettings.instance.GetCollectionMappingsByAddressableKey(addressableKey);
            if (mappings.Count == 0)
            {
                EditorGUILayout.HelpBox("このSourceAssetにはCollectionが登録されていません。", MessageType.None);
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Collections", EditorStyles.boldLabel);
            for (int i = 0; i < mappings.Count; i++)
            {
                SourceDataProviderSettings.SourceCollectionMapping mapping = mappings[i];
                if (GUILayout.Button(BuildCollectionLabel(mapping.CollectionKey), EditorStyles.miniButton))
                {
                    // 現在のページにこのCollectionKeyが含まれるかを確認せず直接切り替えると、次のOnGUIの
                    // EnsureSelection()が「現在ページに無いCollection」と判定して無言で別Collectionへ
                    // 戻してしまう。NavigateToCollectionItemは対応するページへ移動する処理を含むため、
                    // これを再利用してページ・ナビゲーション状態を一貫させる。
                    NavigateToCollectionItem(mapping.CollectionKey, null);
                    GUIUtility.ExitGUI();
                }
            }
        }

        /// <summary>
        ///     選択中Collectionが持つ個別データ項目を一覧表示します。行クリックで実体を選択・Pingします。
        /// </summary>
        /// <param name="collectionKey"> 対象CollectionKeyです。 </param>
        private void DrawCollectionItems(string collectionKey)
        {
            if (!TryResolveCollection(
                    collectionKey,
                    out SourceDataProviderSettings.SourceCollectionMapping mapping,
                    out ScriptableObject sourceAsset,
                    out SerializedProperty collectionProperty))
            {
                EditorGUILayout.HelpBox(
                    $"CollectionKey「{collectionKey}」のSourceAssetまたはProperty Pathを解決できません。",
                    MessageType.Error);
                return;
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField($"Collection [{collectionKey}]", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.Label("Sort", GUILayout.Width(28f));
            _collectionSortMode = (CollectionSortMode)EditorGUILayout.EnumPopup(
                _collectionSortMode,
                EditorStyles.toolbarPopup,
                GUILayout.Width(110f));
            if (GUILayout.Button("Source Assetを開く", EditorStyles.toolbarButton))
            {
                NavigateToSourceAsset(mapping.SourceAssetAddressableKey);
            }
            EditorGUILayout.EndHorizontal();

            DrawCollectionMetadata(mapping);

            if (!collectionProperty.isArray)
            {
                EditorGUILayout.HelpBox("Collection対象が配列またはListではありません。", MessageType.Error);
                return;
            }

            DrawCollectionCommands(mapping, sourceAsset, collectionProperty);

            EditorGUILayout.Space();
            if (IsRepositoryOnlySourceAsset(mapping.SourceAssetAddressableKey))
            {
                DrawRepositoryHeadRow(mapping.SourceAssetAddressableKey, sourceAsset);
            }

            if (collectionProperty.arraySize == 0)
            {
                EditorGUILayout.HelpBox("Collectionにデータがありません。「データを追加」から作成できます。", MessageType.Info);
                return;
            }

            List<int> order = BuildItemDisplayOrder(collectionProperty, collectionKey, _collectionSortMode);
            for (int position = 0; position < order.Count; position++)
            {
                int i = order[position];
                SerializedProperty element = collectionProperty.GetArrayElementAtIndex(i);
                string label = $"{position + 1}. {BuildSortAwareItemLabel(element, i, collectionKey, _collectionSortMode)}";
                bool isSelected = i == _selectedCollectionItemIndex;
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(label, isSelected ? EditorStyles.miniButtonMid : EditorStyles.miniButton))
                {
                    _selectedCollectionItemIndex = i;
                    SelectAndPingElement(sourceAsset, element);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        ///     Collectionへのデータ追加・登録解除を描画します。配列/Listの要素構成を変更する「構造管理」操作であり、
        ///     個別データの値編集ではないため(値編集はInspector側のCustomEditorが担う)、wiki側に残しています。
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
                EditorGUILayout.HelpBox("Collectionの要素型を取得できないため、データを追加できません。", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("データを追加", GUILayout.Height(COMMAND_BUTTON_HEIGHT)))
            {
                ShowCollectionCreationMenu(mapping, sourceAsset, elementType);
            }

            using (new EditorGUI.DisabledScope(_selectedCollectionItemIndex < 0
                || _selectedCollectionItemIndex >= collectionProperty.arraySize))
            {
                if (GUILayout.Button("Collectionから外す", GUILayout.Height(COMMAND_BUTTON_HEIGHT)))
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
            IReadOnlyList<Type> assetTypes = PlannerCollectionItemCreator.GetCreatableAssetTypes(elementType);
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
                menu.AddItem(new GUIContent(assetType.Name), false, () => CreateCollectionAsset(mapping, sourceAsset, assetType));
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
            EditorGUIUtility.PingObject(createdAsset);
            Selection.activeObject = createdAsset;
            InvalidateSearch();
            if (Event.current != null) { GUIUtility.ExitGUI(); }
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
            SourceCollectionItemInspector.Select(sourceAsset, collectionProperty.GetArrayElementAtIndex(newIndex));
            InvalidateSearch();
            GUIUtility.ExitGUI();
        }

        /// <summary>
        ///     選択中要素をCollectionから登録解除します。参照先アセットファイル自体は削除しません。
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
            if (collectionProperty == null || !collectionProperty.isArray || collectionProperty.arraySize == 0)
            {
                return;
            }

            int removeIndex = Mathf.Clamp(_selectedCollectionItemIndex, 0, collectionProperty.arraySize - 1);
            Undo.RecordObject(sourceAsset, "Collectionからデータを外す");
            int previousSize = collectionProperty.arraySize;
            collectionProperty.DeleteArrayElementAtIndex(removeIndex);
            if (collectionProperty.arraySize == previousSize)
            {
                // ObjectReference要素はUnityの仕様上、1回目のDeleteArrayElementAtIndexで参照をnull化するだけの
                // 場合があるため、サイズが変わっていなければ再度呼び出して実際に取り除く。
                collectionProperty.DeleteArrayElementAtIndex(removeIndex);
            }
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(sourceAsset);
            AssetDatabase.SaveAssetIfDirty(sourceAsset);
            _selectedCollectionItemIndex = Mathf.Max(0, removeIndex - 1);
            InvalidateSearch();
            GUIUtility.ExitGUI();
        }

        /// <summary>
        ///     Collection要素を実体としてSelection/Pingします。ObjectReferenceならその参照先を、
        ///     インライン構造体(参照アセットを持たない値)ならCollectionを保持するSourceAsset自体を対象にします。
        /// </summary>
        /// <param name="owningSourceAsset"> Collectionを保持するSourceAssetです。 </param>
        /// <param name="element"> 対象要素です。 </param>
        private static void SelectAndPingElement(ScriptableObject owningSourceAsset, SerializedProperty element)
        {
            if (element.propertyType != SerializedPropertyType.ObjectReference)
            {
                SourceCollectionItemInspector.Select(owningSourceAsset, element);
                return;
            }
            UnityEngine.Object target = element.objectReferenceValue;
            target ??= owningSourceAsset;

            Selection.activeObject = target;
            EditorGUIUtility.PingObject(target);
        }

        /// <summary>
        ///     Collectionの先頭に表示する、Collectionを保持するSourceAsset自体(Repository)の行を描画します。
        ///     クリックすると実体をSelection/Pingします。値編集はInspector側のCustomEditorへ委ねます。
        /// </summary>
        /// <param name="addressableKey"> Repository SourceAssetのAddressableキーです。 </param>
        /// <param name="sourceAsset"> Repository SourceAsset自体です。 </param>
        private void DrawRepositoryHeadRow(string addressableKey, ScriptableObject sourceAsset)
        {
            string label = $"📦 {sourceAsset.name} ({sourceAsset.GetType().Name}) [Repository]";
            if (GUILayout.Button(label, EditorStyles.miniButton))
            {
                _selectedCollectionItemIndex = -1;
                Selection.activeObject = sourceAsset;
                EditorGUIUtility.PingObject(sourceAsset);
            }
        }

        /// <summary>
        ///     指定AddressableキーのSourceAssetが、単一のCollectionのみを保持する「Repository」であるか判定します。
        ///     Repositoryは一覧が冗長になるためSource Assetsタブには表示せず、対応するCollectionの先頭行として表示します。
        /// </summary>
        /// <param name="addressableKey"> 判定対象のAddressableキーです。 </param>
        /// <returns> 単一Collectionのみを保持するRepositoryの場合はtrueです。 </returns>
        private static bool IsRepositoryOnlySourceAsset(string addressableKey)
        {
            if (string.IsNullOrWhiteSpace(addressableKey))
            {
                return false;
            }

            IReadOnlyList<SourceDataProviderSettings.SourceCollectionMapping> mappings =
                SourceDataProviderSettings.instance.GetCollectionMappingsByAddressableKey(addressableKey);
            if (mappings.Count != 1)
            {
                return false;
            }

            // 「登録Collectionが1件」だけでは、Player(_attackDifinitionsの1件を持ちつつ体力等の単体設定も
            // 持つCharacterDefinitionAsset)のような非Repositoryアセットも誤って隠してしまう。
            // Collection配列自体(とm_Script)以外に実質的なフィールドが無いことも合わせて確認する。
            if (!SourceDataProviderRepositoryResolver.TryResolveAsset(addressableKey, out ScriptableObject sourceAsset))
            {
                // 解決できない場合は判定材料が無いため、安全側(Source Assetsタブに表示する)に倒す。
                return false;
            }

            return !HasSubstantialFieldsOutsideCollection(sourceAsset, mappings[0].PropertyPath);
        }

        /// <summary>
        ///     指定Collectionプロパティ以外に、実質的な(トリビアルでない)トップレベルフィールドを
        ///     一定数より多く持つか判定します。Repository判定の補助に使用します。
        /// </summary>
        /// <param name="sourceAsset"> 判定対象のSourceAssetです。 </param>
        /// <param name="collectionPropertyPath"> 除外するCollectionプロパティのパスです。 </param>
        /// <returns> Collection以外に実質的なフィールドが一定数を超えて存在する場合はtrueです。 </returns>
        private static bool HasSubstantialFieldsOutsideCollection(
            ScriptableObject sourceAsset,
            string collectionPropertyPath)
        {
            SerializedObject serializedObject = new(sourceAsset);
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            int otherFieldCount = 0;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (string.Equals(iterator.propertyPath, SCRIPT_PROPERTY_NAME, StringComparison.Ordinal)
                    || string.Equals(iterator.propertyPath, collectionPropertyPath, StringComparison.Ordinal))
                {
                    continue;
                }

                otherFieldCount++;
                if (otherFieldCount > REPOSITORY_EXTRA_FIELD_TOLERANCE)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     ページのSourceAssetキーのうち、Repository(単一Collectionのみ保持)ではないものだけを抽出します。
        /// </summary>
        /// <param name="page"> 対象ページです。 </param>
        /// <returns> Source Assetsタブに表示すべきAddressableキー一覧です。 </returns>
        private static List<string> GetVisibleSourceAssetKeys(PlannerMasterDataEditorSettings.PageDefinition page)
        {
            List<string> result = new();
            for (int i = 0; i < page.SourceAssetAddressableKeys.Count; i++)
            {
                string key = page.SourceAssetAddressableKeys[i];
                if (!IsRepositoryOnlySourceAsset(key)
                    || !Contains(page.CollectionCategories, SourceDataProviderSettings.instance
                        .GetCollectionMappingsByAddressableKey(key)[0].CollectionKey))
                {
                    result.Add(key);
                }
            }

            return result;
        }

        /// <summary>
        ///     Collection設定と対象プロパティをまとめて解決します。
        /// </summary>
        /// <param name="collectionKey"> CollectionKeyです。 </param>
        /// <param name="mapping"> Collection設定です。 </param>
        /// <param name="sourceAsset"> SourceAssetです。 </param>
        /// <param name="collectionProperty"> Collectionプロパティです。 </param>
        /// <returns> 全て解決できた場合はtrueです。 </returns>
        private static bool TryResolveCollection(
            string collectionKey,
            out SourceDataProviderSettings.SourceCollectionMapping mapping,
            out ScriptableObject sourceAsset,
            out SerializedProperty collectionProperty)
        {
            mapping = null;
            sourceAsset = null;
            collectionProperty = null;
            if (!SourceDataProviderSettings.instance.TryGetCollectionMapping(collectionKey, out mapping)
                || !SourceDataProviderRepositoryResolver.TryResolveAsset(
                    mapping.SourceAssetAddressableKey,
                    out sourceAsset))
            {
                return false;
            }

            SerializedObject serializedObject = new(sourceAsset);
            collectionProperty = serializedObject.FindProperty(mapping.PropertyPath);
            return collectionProperty != null;
        }

        /// <summary>
        ///     対象SourceAssetの実体をSelect/Pingします。対応するページがあれば、そのページへも移動します。
        ///     実体の解決・選択は、ページ割当の有無に関わらず常に行います(検索が全登録SourceAssetを
        ///     対象にしている一方、ページに割り当てられていないSourceAssetも存在するため)。
        /// </summary>
        /// <param name="addressableKey"> 移動先SourceAssetのAddressableキーです。 </param>
        public void NavigateToSourceAsset(string addressableKey)
        {
            bool resolved = SourceDataProviderRepositoryResolver.TryResolveAsset(
                addressableKey,
                out ScriptableObject sourceAsset);
            if (resolved)
            {
                Selection.activeObject = sourceAsset;
                EditorGUIUtility.PingObject(sourceAsset);
            }

            IReadOnlyList<PlannerMasterDataEditorSettings.PageDefinition> pages =
                PlannerMasterDataEditorSettings.instance.Pages;
            bool pageFound = false;
            for (int i = 0; i < pages.Count; i++)
            {
                int pageIndex = (Mathf.Clamp(_selectedPageIndex, 0, pages.Count - 1) + i) % pages.Count;
                if (!Contains(pages[pageIndex].SourceAssetAddressableKeys, addressableKey))
                {
                    continue;
                }

                _selectedPageIndex = pageIndex;
                _navigationMode = NavigationMode.SourceAssets;
                _selectedSourceAssetKey = addressableKey;
                _selectedCollectionKey = string.Empty;
                _selectedCollectionItemIndex = 0;
                if (IsRepositoryOnlySourceAsset(addressableKey))
                {
                    string collectionKey = SourceDataProviderSettings.instance
                        .GetCollectionMappingsByAddressableKey(addressableKey)[0].CollectionKey;
                    if (Contains(pages[pageIndex].CollectionCategories, collectionKey))
                    {
                        _navigationMode = NavigationMode.Collections;
                        _selectedCollectionKey = collectionKey;
                        _selectedSourceAssetKey = string.Empty;
                        _selectedCollectionItemIndex = -1;
                    }
                }
                pageFound = true;
                break;
            }

            if (!resolved)
            {
                ShowNotification(new GUIContent(
                    $"SourceAsset「{addressableKey}」を解決できません。Addressablesの登録状況を確認してください。"));
            }
            else if (!pageFound)
            {
                ShowNotification(new GUIContent(
                    $"SourceAsset「{addressableKey}」を表示するページが設定されていないため、実体のみ選択しました。"));
            }

            Repaint();
            Focus();
            SetSearchQuery(string.Empty);
        }

        /// <summary>
        ///     指定CollectionKeyとDataIDに対応するデータの実体をSelect/Pingします。対応するページがあれば、
        ///     そのページへも移動します。実体の解決・選択は、ページ割当の有無に関わらず常に行います。
        /// </summary>
        /// <param name="collectionKey"> 移動先CollectionKeyです。 </param>
        /// <param name="dataId"> 移動先の個別データIDです。 </param>
        public void NavigateToCollectionItem(string collectionKey, string dataId)
        {
            bool collectionResolved = TryResolveCollection(
                    collectionKey,
                    out _,
                    out ScriptableObject sourceAsset,
                    out SerializedProperty collectionProperty)
                && collectionProperty.isArray;

            int itemIndex = 0;
            bool itemFound = false;
            if (collectionResolved && !string.IsNullOrWhiteSpace(dataId))
            {
                for (int i = 0; i < collectionProperty.arraySize; i++)
                {
                    if (!ElementMatchesDataId(
                        collectionProperty.GetArrayElementAtIndex(i),
                        dataId,
                        collectionKey))
                    {
                        continue;
                    }

                    itemIndex = i;
                    itemFound = true;
                    SelectAndPingElement(sourceAsset, collectionProperty.GetArrayElementAtIndex(i));
                    break;
                }
            }

            int pageIndex = FindCollectionPage(collectionKey);

            if (pageIndex >= 0)
            {
                _selectedPageIndex = pageIndex;
                _navigationMode = NavigationMode.Collections;
                _selectedCollectionKey = collectionKey;
                _selectedSourceAssetKey = string.Empty;
                _selectedCollectionItemIndex = itemIndex;
            }

            if (!collectionResolved)
            {
                ShowNotification(new GUIContent(
                    $"CollectionKey「{collectionKey}」のSourceAssetまたはProperty Pathを解決できません。"));
            }
            else if (!string.IsNullOrWhiteSpace(dataId) && !itemFound)
            {
                ShowNotification(new GUIContent(
                    $"CollectionKey「{collectionKey}」内にID「{dataId}」のデータが見つかりません。"));
            }
            else if (pageIndex < 0)
            {
                ShowNotification(new GUIContent(
                    $"Collection「{collectionKey}」を表示するページが設定されていないため、実体のみ選択しました。"));
            }

            Repaint();
            Focus();
            SetSearchQuery(string.Empty);
        }

        /// <summary>
        ///     Collection要素が指定DataIDに一致するか判定します。
        /// </summary>
        /// <param name="element"> 対象要素です。 </param>
        /// <param name="dataId"> 検索するDataIDです。 </param>
        /// <param name="collectionKey"> 対象CollectionKeyです。 </param>
        /// <returns> 一致する場合はtrueです。 </returns>
        private static bool ElementMatchesDataId(
            SerializedProperty element,
            string dataId,
            string collectionKey)
        {
            if (element == null)
            {
                return false;
            }

            if (element.propertyType == SerializedPropertyType.ObjectReference)
            {
                return element.objectReferenceValue != null
                    && ObjectHasMatchingAuthoringId(element.objectReferenceValue, collectionKey, dataId);
            }

            // PropertyDrawerから渡されたIDを表示と同じ規則で照合します。
            return string.Equals(ExtractDataId(element), dataId, StringComparison.Ordinal);
        }

        /// <summary>
        ///     対象オブジェクトが持つ直下のDataIDフィールド(参照ではなく定義側)が指定IDと一致するか判定します。
        /// </summary>
        /// <param name="target"> 対象オブジェクトです。 </param>
        /// <param name="collectionKey"> 対象CollectionKeyです。 </param>
        /// <param name="dataId"> 検索するDataIDです。 </param>
        /// <returns> 一致する場合はtrueです。 </returns>
        private static bool ObjectHasMatchingAuthoringId(UnityEngine.Object target, string collectionKey, string dataId)
        {
            return string.Equals(SourceCollectionElementDisplay.GetAuthoringId(target, collectionKey), dataId, StringComparison.Ordinal);
        }

        /// <summary>
        ///     選択中オブジェクトのヘッダー(種別・Addressableキー・Pingボタン)を描画します。
        /// </summary>
        /// <param name="addressableKey"> Addressableキーです。 </param>
        /// <param name="typeName"> 型名です。 </param>
        /// <param name="target"> 対象オブジェクトです。 </param>
        private static void DrawObjectHeaderRow(string addressableKey, string typeName, UnityEngine.Object target)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField($"{typeName}  ({addressableKey})", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(AssetDatabase.GetAssetPath(target), EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Select & Ping", GUILayout.Width(96f), GUILayout.Height(32f)))
            {
                Selection.activeObject = target;
                EditorGUIUtility.PingObject(target);
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        ///     collection設定の補足情報を描画します。
        /// </summary>
        /// <param name="mapping"> 対象のcollection設定です。 </param>
        private void DrawCollectionMetadata(
            SourceDataProviderSettings.SourceCollectionMapping mapping)
        {
            bool expanded = EditorGUILayout.Foldout(_showCollectionMetadata, "Collection設定の詳細", true);
            if (expanded != _showCollectionMetadata)
            {
                _showCollectionMetadata = expanded;
                GUIUtility.ExitGUI();
            }
            if (!_showCollectionMetadata) { return; }
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField("Collection Key", mapping.CollectionKey);
                EditorGUILayout.TextField("Property Path", mapping.PropertyPath);
            }
            EditorGUILayout.EndVertical();
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
            return SourceCollectionElementDisplay.GetLabel(element, index);
        }

        /// <summary>
        ///     指定ソートモードに従い、Collection項目の表示順(元配列のIndex列)を構築します。
        ///     実データの並び順は変更しません。
        /// </summary>
        /// <param name="collectionProperty"> Collection配列プロパティです。 </param>
        /// <param name="collectionKey"> 対象CollectionKeyです。 </param>
        /// <param name="sortMode"> 表示ソートモードです。 </param>
        /// <returns> 表示順に並んだ元配列Indexの一覧です。 </returns>
        private static List<int> BuildItemDisplayOrder(
            SerializedProperty collectionProperty,
            string collectionKey,
            CollectionSortMode sortMode)
        {
            List<int> order = new(collectionProperty.arraySize);
            for (int i = 0; i < collectionProperty.arraySize; i++)
            {
                order.Add(i);
            }

            if (sortMode == CollectionSortMode.Default)
            {
                return order;
            }

            List<(int index, string key)> keyed = new(order.Count);
            for (int i = 0; i < order.Count; i++)
            {
                SerializedProperty element = collectionProperty.GetArrayElementAtIndex(order[i]);
                string key = sortMode == CollectionSortMode.AssetName
                    ? GetElementAssetDisplayName(element)
                    : GetElementDataIdDisplayName(element, collectionKey);
                keyed.Add((order[i], key));
            }

            keyed.Sort((a, b) =>
            {
                bool aHasKey = !string.IsNullOrWhiteSpace(a.key);
                bool bHasKey = !string.IsNullOrWhiteSpace(b.key);
                if (aHasKey != bHasKey)
                {
                    // キーを取得できない項目は末尾へ集約します。
                    return aHasKey ? -1 : 1;
                }

                if (!aHasKey)
                {
                    return a.index.CompareTo(b.index);
                }

                int comparison = string.Compare(a.key, b.key, StringComparison.OrdinalIgnoreCase);
                return comparison != 0 ? comparison : a.index.CompareTo(b.index);
            });

            List<int> result = new(keyed.Count);
            for (int i = 0; i < keyed.Count; i++)
            {
                result.Add(keyed[i].index);
            }

            return result;
        }

        /// <summary>
        ///     ソートモードに応じたCollection項目の表示名を生成します。対応するキーを取得できない場合は
        ///     既定の表示名(<see cref="BuildCollectionItemLabel"/>)へフォールバックします。
        /// </summary>
        /// <param name="element"> Collection要素です。 </param>
        /// <param name="index"> Collection内のインデックスです。 </param>
        /// <param name="collectionKey"> 対象CollectionKeyです。 </param>
        /// <param name="sortMode"> 表示ソートモードです。 </param>
        /// <returns> 表示名です。 </returns>
        private static string BuildSortAwareItemLabel(
            SerializedProperty element,
            int index,
            string collectionKey,
            CollectionSortMode sortMode)
        {
            string key = sortMode switch
            {
                CollectionSortMode.AssetName => GetElementAssetDisplayName(element),
                CollectionSortMode.DataId => GetElementDataIdDisplayName(element, collectionKey),
                _ => null,
            };

            return !string.IsNullOrWhiteSpace(key) ? key : BuildCollectionItemLabel(element, index);
        }

        /// <summary>
        ///     Collection要素が参照する実アセットの名前を取得します。ObjectReference要素はその参照先、
        ///     インライン構造体は"Asset"という名前のObjectReferenceフィールドを探し、
        ///     見つからない場合はStatusBonusEffectIcon/SkillGenreIconの"Icon"のような、
        ///     最初に見つかったObjectReferenceフィールドにフォールバックします。
        /// </summary>
        /// <param name="element"> Collection要素です。 </param>
        /// <returns> 取得できた場合はアセット名、それ以外はnullです。 </returns>
        private static string GetElementAssetDisplayName(SerializedProperty element)
        {
            return SourceCollectionElementDisplay.GetReferences(element).FirstOrDefault()?.name;
        }

        /// <summary>
        ///     Collection要素のDataID文字列を取得します。ObjectReference要素は参照先アセットが持つ、
        ///     このCollectionKeyに紐づく定義側DataIDフィールドを探します。
        /// </summary>
        /// <param name="element"> Collection要素です。 </param>
        /// <param name="collectionKey"> 対象CollectionKeyです。 </param>
        /// <returns> 取得できた場合はDataID文字列、それ以外はnullです。 </returns>
        private static string GetElementDataIdDisplayName(SerializedProperty element, string collectionKey)
        {
            if (element.propertyType == SerializedPropertyType.ObjectReference)
            {
                return element.objectReferenceValue == null
                    ? null
                    : TryGetAuthoringDataIdValue(element.objectReferenceValue, collectionKey, out string dataId)
                        ? dataId
                        : null;
            }

            return ExtractDataId(element);
        }

        /// <summary>
        ///     対象オブジェクトが持つ直下のDataIDフィールド(参照ではなく定義側)から、指定CollectionKeyに
        ///     対応するID文字列を取得します。
        /// </summary>
        /// <param name="target"> 対象オブジェクトです。 </param>
        /// <param name="collectionKey"> 対象CollectionKeyです。 </param>
        /// <param name="dataId"> 取得できたID文字列です。 </param>
        /// <returns> 取得できた場合はtrueです。 </returns>
        private static bool TryGetAuthoringDataIdValue(UnityEngine.Object target, string collectionKey, out string dataId)
        {
            dataId = SourceCollectionElementDisplay.GetAuthoringId(target, collectionKey);
            return !string.IsNullOrWhiteSpace(dataId);
        }

        //
        // --- 検索 --------------------------------------------------------
        //

        /// <summary>
        ///     検索結果一覧を描画します。SourceAsset・CollectionKey・個別データを横断して検索します。
        /// </summary>
        private void DrawSearchResults()
        {
            if (Event.current.type == EventType.Layout
                && !string.Equals(_lastIndexedSearchQuery, _searchQuery, StringComparison.Ordinal))
            {
                RebuildSearchResults(_searchQuery);
                _lastIndexedSearchQuery = _searchQuery;
            }

            EditorGUILayout.LabelField($"検索結果: {_searchResults.Count} 件", EditorStyles.boldLabel);
            using (EditorGUILayout.ScrollViewScope scope = new(_searchScrollPosition))
            {
                _searchScrollPosition = scope.scrollPosition;
                if (_searchResults.Count == 0)
                {
                    EditorGUILayout.HelpBox("一致するSourceAsset・Collection・データが見つかりません。", MessageType.None);
                    return;
                }

                for (int i = 0; i < _searchResults.Count; i++)
                {
                    DrawSearchResultRow(_searchResults[i]);
                }
            }
        }

        /// <summary>
        ///     検索結果1件を描画します。クリックすると該当データへジャンプします。
        /// </summary>
        /// <param name="result"> 描画する検索結果です。 </param>
        private void DrawSearchResultRow(SearchResult result)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField(result.KindLabel, EditorStyles.miniLabel, GUILayout.Width(84f));
            EditorGUILayout.LabelField(result.DisplayLabel);
            if (GUILayout.Button("開く", GUILayout.Width(56f)))
            {
                switch (result.Kind)
                {
                    case SearchResultKind.SourceAsset:
                        NavigateToSourceAsset(result.AddressableKey);
                        break;
                    case SearchResultKind.Collection:
                        NavigateToCollectionItem(result.CollectionKey, null);
                        break;
                    case SearchResultKind.CollectionItem:
                        NavigateToSearchResult(result);
                        break;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        ///     現在のページを優先してCollectionの表示先を探します。
        /// </summary>
        private int FindCollectionPage(string collectionKey)
        {
            IReadOnlyList<PlannerMasterDataEditorSettings.PageDefinition> pages = PlannerMasterDataEditorSettings.instance.Pages;
            for (int i = 0; i < pages.Count; i++)
            {
                int index = (Mathf.Clamp(_selectedPageIndex, 0, pages.Count - 1) + i) % pages.Count;
                if (Contains(pages[index].CollectionCategories, collectionKey)) { return index; }
            }
            return -1;
        }

        /// <summary>
        ///     IDの重複や空値によらず、検索した所有アセット内の正確な要素を選択します。
        /// </summary>
        private void NavigateToSearchResult(SearchResult result)
        {
            if (_lastIndexedSearchQuery != _searchQuery
                || !TryResolveCollection(result.CollectionKey, out _, out ScriptableObject owner, out SerializedProperty collection)
                || !collection.isArray
                || owner != result.Owner)
            {
                InvalidateSearch();
                GUIUtility.ExitGUI();
                return;
            }
            for (int i = 0; i < collection.arraySize; i++)
            {
                SerializedProperty element = collection.GetArrayElementAtIndex(i);
                if (element.propertyPath != result.PropertyPath) { continue; }
                int page = FindCollectionPage(result.CollectionKey);
                if (page >= 0)
                {
                    _selectedPageIndex = page;
                    _navigationMode = NavigationMode.Collections;
                    _selectedCollectionKey = result.CollectionKey;
                    _selectedSourceAssetKey = string.Empty;
                    _selectedCollectionItemIndex = i;
                }
                SelectAndPingElement(owner, element);
                SetSearchQuery(string.Empty);
                return;
            }
            ShowNotification(new GUIContent("検索対象が変更されています。検索結果を更新します。"));
            InvalidateSearch();
        }

        /// <summary>
        ///     検索クエリに応じて登録済みSourceAsset・Collection・個別データを走査し、検索結果を再構築します。
        /// </summary>
        /// <param name="query"> 検索クエリです。 </param>
        private void RebuildSearchResults(string query)
        {
            _searchResults.Clear();
            if (string.IsNullOrWhiteSpace(query))
            {
                return;
            }

            IReadOnlyList<SourceDataProviderSettings.SourceAssetMapping> sourceAssetMappings =
                SourceDataProviderSettings.instance.SourceAssetMappings;
            for (int i = 0; i < sourceAssetMappings.Count; i++)
            {
                string addressableKey = sourceAssetMappings[i]?.AddressableKey;
                if (string.IsNullOrWhiteSpace(addressableKey))
                {
                    continue;
                }

                string label = BuildSourceAssetLabel(addressableKey);
                if (MatchesQuery(label, query) || MatchesQuery(addressableKey, query))
                {
                    _searchResults.Add(SearchResult.ForSourceAsset(addressableKey, label));
                }
            }

            IReadOnlyList<SourceDataProviderSettings.SourceCollectionMapping> collectionMappings =
                SourceDataProviderSettings.instance.SourceCollectionMappings;
            for (int i = 0; i < collectionMappings.Count; i++)
            {
                SourceDataProviderSettings.SourceCollectionMapping mapping = collectionMappings[i];
                if (mapping == null)
                {
                    continue;
                }

                if (MatchesQuery(mapping.CollectionKey, query))
                {
                    _searchResults.Add(SearchResult.ForCollection(
                        mapping.CollectionKey,
                        BuildCollectionLabel(mapping.CollectionKey)));
                }

                if (!SourceDataProviderRepositoryResolver.TryResolveAsset(
                        mapping.SourceAssetAddressableKey,
                        out ScriptableObject sourceAsset))
                {
                    continue;
                }

                SerializedObject serializedObject = new(sourceAsset);
                SerializedProperty property = serializedObject.FindProperty(mapping.PropertyPath);
                if (property == null || !property.isArray)
                {
                    continue;
                }

                for (int elementIndex = 0; elementIndex < property.arraySize; elementIndex++)
                {
                    SerializedProperty element = property.GetArrayElementAtIndex(elementIndex);
                    string itemLabel = BuildCollectionItemLabel(element, elementIndex);
                    if (!SourceCollectionElementDisplay.Matches(element, mapping.CollectionKey, query))
                    {
                        continue;
                    }

                    _searchResults.Add(SearchResult.ForCollectionItem(
                        mapping.CollectionKey,
                        element.propertyPath,
                        $"{mapping.CollectionKey} / {itemLabel}",
                        sourceAsset));
                }
            }
        }

        /// <summary>
        ///     インラインCollection要素のDataIDまたは表示用キーを抽出します。
        /// </summary>
        /// <param name="element"> 対象要素です。 </param>
        /// <returns> DataID相当の文字列です。 </returns>
        private static string ExtractDataId(SerializedProperty element)
        {
            return element.propertyType == SerializedPropertyType.ObjectReference
                ? null
                : SourceCollectionElementDisplay.GetInlineKey(element);
        }

        /// <summary>
        ///     大文字小文字を無視した部分一致で検索クエリに一致するか判定します。
        /// </summary>
        /// <param name="candidate"> 判定対象の文字列です。 </param>
        /// <param name="query"> 検索クエリです。 </param>
        /// <returns> 一致する場合はtrueです。 </returns>
        private static bool MatchesQuery(string candidate, string query)
        {
            return !string.IsNullOrEmpty(candidate)
                && candidate.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private const float PAGE_SIDEBAR_WIDTH = 160f;
        private const float NAVIGATION_COLUMN_WIDTH = 260f;
        private const float COMMAND_BUTTON_HEIGHT = 28f;
        private const string SEARCH_FIELD_CONTROL_NAME = "PlannerMasterDataWindow.SearchField";
        private const string SCRIPT_PROPERTY_NAME = "m_Script";
        private const int REPOSITORY_EXTRA_FIELD_TOLERANCE = 0;

        private static readonly string[] NAVIGATION_MODE_LABELS = { "Source Assets", "Collections" };

        /// <summary>
        ///     ナビゲーション列へ表示するデータ階層です。
        /// </summary>
        private enum NavigationMode
        {
            SourceAssets,
            Collections,
        }

        /// <summary>
        ///     Collection項目一覧の表示ソート順です。実データの配列順は変更せず、表示のみを並べ替えます。
        /// </summary>
        private enum CollectionSortMode
        {
            /// <summary> 配列に格納されている順序のままです。 </summary>
            Default,
            /// <summary> 参照アセット名(ObjectReferenceの場合は参照先、インライン構造体は"Asset"フィールド)の昇順です。 </summary>
            AssetName,
            /// <summary> DataID文字列の昇順です。 </summary>
            DataId,
        }

        /// <summary>
        ///     検索結果の種別です。
        /// </summary>
        private enum SearchResultKind
        {
            SourceAsset,
            Collection,
            CollectionItem,
        }

        /// <summary>
        ///     検索結果1件分の情報を保持します。
        /// </summary>
        private readonly struct SearchResult
        {
            private SearchResult(
                SearchResultKind kind,
                string addressableKey,
                string collectionKey,
                string propertyPath,
                string displayLabel,
                ScriptableObject owner = null)
            {
                Kind = kind;
                AddressableKey = addressableKey;
                CollectionKey = collectionKey;
                PropertyPath = propertyPath;
                DisplayLabel = displayLabel;
                Owner = owner;
            }

            public readonly SearchResultKind Kind;
            public readonly string AddressableKey;
            public readonly string CollectionKey;
            public readonly string PropertyPath;
            public readonly ScriptableObject Owner;
            public readonly string DisplayLabel;

            /// <summary> 種別を表す短いラベルです。 </summary>
            public string KindLabel => Kind switch
            {
                SearchResultKind.SourceAsset => "SourceAsset",
                SearchResultKind.Collection => "Collection",
                _ => "Item",
            };

            public static SearchResult ForSourceAsset(string addressableKey, string label) =>
                new(SearchResultKind.SourceAsset, addressableKey, null, null, label);

            public static SearchResult ForCollection(string collectionKey, string label) =>
                new(SearchResultKind.Collection, null, collectionKey, null, label);

            public static SearchResult ForCollectionItem(string collectionKey, string propertyPath, string label, ScriptableObject owner) =>
                new(SearchResultKind.CollectionItem, null, collectionKey, propertyPath, label, owner);
        }
    }
}
