using Cysharp.Threading.Tasks;
using KillChord.Editor.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Editor.SpecViewer
{
    /// <summary>
    ///     プロジェクト ドキュメントの写し（Docs/NotionSpecifications）を、エディタの中で読むウィンドウ。
    ///     左にページのツリーと検索結果、右にページの本文を出す。写しは読み取り専用で、直すのは Notion である。
    /// </summary>
    public sealed class SpecViewerWindow : EditorWindow
    {
        /// <summary>
        ///     仕様書ビューアを開く。
        /// </summary>
        [MenuItem(EditorWindowPathConst.SPEC_VIEWER_PATH)]
        private static void Open()
        {
            SpecViewerWindow window = GetWindow<SpecViewerWindow>();
            window.titleContent = new GUIContent("仕様書ビューア");
            window.minSize = new Vector2(MIN_WIDTH, MIN_HEIGHT);
        }

        private const float MIN_WIDTH = 720f;
        private const float MIN_HEIGHT = 400f;
        private const float TREE_PANE_WIDTH = 280f;
        private const float CONTENT_PADDING = 10f;
        private const int MAX_SEARCH_RESULTS = 300;
        private const long SEARCH_DELAY_MILLISECONDS = 300;
        private const int MAX_CACHED_TEXTURES = 200;
        private const float RESULT_ITEM_HEIGHT = 36f;
        private const string DEFAULT_PAGE_PATH = "Symphony Kill Chord.md";
        private static readonly string[] LOADABLE_IMAGE_EXTENSIONS = { ".png", ".jpg", ".jpeg" };

        [SerializeField, Tooltip("最後に開いていたページ（写しのルートからの相対パス）")]
        private string _lastPagePath = string.Empty;

        private readonly SpecDocumentParser _parser = new();
        private readonly SpecTreeBuilder _treeBuilder = new();
        private readonly SpecPageHistory _history = new();
        private readonly Dictionary<string, Texture2D> _textures = new(StringComparer.Ordinal);
        private readonly List<SpecSearchHit> _results = new();
        private SpecMirror _mirror;
        private SpecCatalog _catalog;
        private SpecLinkResolver _resolver;
        private SpecInlineFormatter _formatter;
        private SpecDocumentRenderer _renderer;
        private CancellationTokenSource _searchCancellation;
        private IVisualElementScheduledItem _pendingSearch;
        private string _currentPath = string.Empty;

        private HelpBox _warningBox;
        private Label _statusLabel;
        private Button _backButton;
        private Button _forwardButton;
        private Button _updateButton;
        private Button _notionButton;
        private ToolbarSearchField _searchField;
        private Toggle _fullTextToggle;
        private TreeView _treeView;
        private ListView _resultList;
        private ScrollView _pageScroll;

        /// <summary>
        ///     ウィンドウを閉じるときに、読み込んだ画像を破棄し、検索を止める。
        /// </summary>
        private void OnDisable()
        {
            _searchCancellation?.Cancel();
            ClearTextures();
        }

        /// <summary>
        ///     ウィンドウの UI を組み立て、写しを読み込む。
        /// </summary>
        private void CreateGUI()
        {
            string repositoryRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            _mirror = new SpecMirror(repositoryRoot);
            _formatter = new SpecInlineFormatter(SpecViewerStyles.LinkColorHex, SpecViewerStyles.InlineCodeBackgroundHex);
            _formatter.SetPageTitleResolver(ResolvePageTitle);
            _renderer = new SpecDocumentRenderer(_formatter, HandleLinkClicked, LoadImage, element => _pageScroll.ScrollTo(element));

            rootVisualElement.Clear();
            rootVisualElement.Add(CreateToolbar());
            _warningBox = new HelpBox(string.Empty, HelpBoxMessageType.Warning) { style = { display = DisplayStyle.None } };
            rootVisualElement.Add(_warningBox);

            TwoPaneSplitView split = new(0, TREE_PANE_WIDTH, TwoPaneSplitViewOrientation.Horizontal) { style = { flexGrow = 1 } };
            VisualElement leftPane = new();
            _treeView = CreateTreeView();
            _resultList = CreateResultList();
            leftPane.Add(_treeView);
            leftPane.Add(_resultList);
            split.Add(leftPane);
            _pageScroll = new ScrollView(ScrollViewMode.Vertical);
            _pageScroll.contentContainer.style.paddingLeft = CONTENT_PADDING;
            _pageScroll.contentContainer.style.paddingRight = CONTENT_PADDING;
            _pageScroll.contentContainer.style.paddingBottom = CONTENT_PADDING;
            split.Add(_pageScroll);
            rootVisualElement.Add(split);

            ReloadAsync(_lastPagePath).Forget();
        }

        /// <summary>
        ///     ページのリンク・画像・添付ファイルが押されたときに、行き先に合わせて開く。
        /// </summary>
        /// <param name="href">ページに書かれたリンク先。</param>
        private void HandleLinkClicked(string href)
        {
            if (_resolver == null) { return; }

            SpecLinkTarget target = _resolver.Resolve(_currentPath, href);
            switch (target.Kind)
            {
                case SpecLinkTarget.TargetKind.Page:
                    OpenPage(target.Value, true);
                    break;
                case SpecLinkTarget.TargetKind.File when File.Exists(target.Value):
                    EditorUtility.OpenWithDefaultApp(target.Value);
                    break;
                case SpecLinkTarget.TargetKind.File:
                    ShowWarning($"リンク先のファイルが写しにありません: {target.Value}");
                    break;
                case SpecLinkTarget.TargetKind.External:
                    Application.OpenURL(target.Value);
                    break;
            }
        }

        /// <summary>
        ///     上部のツールバー（戻る・進む・検索・更新・Notion で開く）を作る。
        /// </summary>
        /// <returns>ツールバー。</returns>
        private VisualElement CreateToolbar()
        {
            Toolbar toolbar = new();
            _backButton = new ToolbarButton(() => OpenFromHistory(_history.GoBack())) { text = "←", tooltip = "戻る" };
            _forwardButton = new ToolbarButton(() => OpenFromHistory(_history.GoForward())) { text = "→", tooltip = "進む" };
            _searchField = new ToolbarSearchField { style = { flexGrow = 1, flexShrink = 1 } };
            _searchField.RegisterValueChangedCallback(_ => ScheduleSearch());
            _fullTextToggle = new ToolbarToggle { text = "本文も検索", tooltip = "タイトル・見出し・プロパティに加えて、本文も検索します（少し時間がかかります）" };
            _fullTextToggle.RegisterValueChangedCallback(_ => ScheduleSearch());
            _statusLabel = new Label { style = { unityTextAlign = TextAnchor.MiddleLeft, marginLeft = 6, marginRight = 6 } };
            _updateButton = new ToolbarButton(() => UpdateMirrorAsync().Forget()) { text = "更新", tooltip = "仕様書リポジトリから写しを最新にします" };
            _notionButton = new ToolbarButton(OpenCurrentInNotion) { text = "Notionで開く" };

            toolbar.Add(_backButton);
            toolbar.Add(_forwardButton);
            toolbar.Add(_searchField);
            toolbar.Add(_fullTextToggle);
            toolbar.Add(_statusLabel);
            toolbar.Add(_updateButton);
            toolbar.Add(_notionButton);
            RefreshNavigationButtons();
            return toolbar;
        }

        /// <summary>
        ///     ページのツリーを作る。項目を選ぶとそのページを開く。
        /// </summary>
        /// <returns>ツリー。</returns>
        private TreeView CreateTreeView()
        {
            TreeView treeView = new() { style = { flexGrow = 1 } };
            treeView.makeItem = () => new Label { style = { unityTextAlign = TextAnchor.MiddleLeft } };
            treeView.bindItem = (element, index) => ((Label)element).text = treeView.GetItemDataForIndex<SpecTreeNode>(index).DisplayName;
            treeView.selectedIndicesChanged += indices =>
            {
                foreach (int index in indices)
                {
                    SpecCatalogEntry entry = treeView.GetItemDataForIndex<SpecTreeNode>(index).Entry;
                    if (entry != null) { OpenPage(entry.Path, true); }
                    return;
                }
            };
            return treeView;
        }

        /// <summary>
        ///     検索結果の一覧を作る。項目を選ぶとそのページを開く。
        /// </summary>
        /// <returns>一覧。</returns>
        private ListView CreateResultList()
        {
            ListView list = new(_results, RESULT_ITEM_HEIGHT, MakeResultItem, BindResultItem)
            {
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                style = { flexGrow = 1, display = DisplayStyle.None }
            };
            list.selectedIndicesChanged += indices =>
            {
                foreach (int index in indices)
                {
                    OpenPage(_results[index].Entry.Path, true);
                    return;
                }
            };
            return list;
        }

        /// <summary>
        ///     検索結果の1項目の要素を作る。
        /// </summary>
        /// <returns>要素。</returns>
        private static VisualElement MakeResultItem()
        {
            VisualElement item = new() { style = { paddingTop = 2, paddingBottom = 2 } };
            item.Add(new Label { name = "title", style = { unityFontStyleAndWeight = FontStyle.Bold } });
            item.Add(new Label { name = "detail", style = { whiteSpace = WhiteSpace.Normal, opacity = 0.7f } });
            return item;
        }

        /// <summary>
        ///     検索結果の1項目に内容を入れる。
        /// </summary>
        /// <param name="element">要素。</param>
        /// <param name="index">結果の添字。</param>
        private void BindResultItem(VisualElement element, int index)
        {
            SpecSearchHit hit = _results[index];
            element.Q<Label>("title").text = SpecInlineFormatter.Escape(hit.Entry.Title);
            element.Q<Label>("detail").text = SpecInlineFormatter.Escape(string.IsNullOrEmpty(hit.Snippet) ? hit.Entry.Path : hit.Snippet);
        }

        /// <summary>
        ///     写しの状態を確かめ、ページの一覧を読み直してツリーを作る。
        /// </summary>
        /// <param name="pageToOpen">読み直した後に開くページ。無ければ空文字。</param>
        /// <returns>非同期処理。</returns>
        private async UniTaskVoid ReloadAsync(string pageToOpen)
        {
            HideWarning();
            if (!_mirror.IsAvailable)
            {
                _catalog = null;
                _resolver = null;
                _treeView.SetRootItems(new List<TreeViewItemData<SpecTreeNode>>());
                _treeView.Rebuild();
                _pageScroll.Clear();
                _statusLabel.text = "写しなし";
                ShowWarning(_mirror.UnavailableMessage);
                Debug.LogWarning($"[{nameof(SpecViewerWindow)}] {_mirror.UnavailableMessage}");
                return;
            }

            _statusLabel.text = "読み込み中…";
            string root = _mirror.RootDirectory;
            SpecCatalog catalog = await UniTask.RunOnThreadPool(() => SpecCatalog.Load(root));
            string updatedText = await UniTask.RunOnThreadPool(() => _mirror.ReadLastUpdatedText());
            if (this == null) { return; }

            _catalog = catalog;
            _resolver = new SpecLinkResolver(catalog, root);
            _treeView.SetRootItems(_treeBuilder.Build(catalog.Entries));
            _treeView.Rebuild();
            _statusLabel.text = string.IsNullOrEmpty(updatedText) ? $"{catalog.Entries.Count}ページ" : $"写し: {updatedText}";
            if (!string.IsNullOrEmpty(catalog.Warning)) { ShowWarning(catalog.Warning); }

            if (!string.IsNullOrEmpty(pageToOpen) && catalog.TryGetByPath(pageToOpen, out _)) { OpenPage(pageToOpen, true); }
            else if (catalog.Entries.Count > 0 && catalog.TryGetByPath(DEFAULT_PAGE_PATH, out _)) { OpenPage(DEFAULT_PAGE_PATH, true); }
        }

        /// <summary>
        ///     写しを仕様書リポジトリから最新にして、読み直す。
        /// </summary>
        /// <returns>非同期処理。</returns>
        private async UniTaskVoid UpdateMirrorAsync()
        {
            _updateButton.SetEnabled(false);
            _statusLabel.text = "更新中…";
            string error = await UniTask.RunOnThreadPool(() => _mirror.Pull());
            if (this == null) { return; }

            _updateButton.SetEnabled(true);
            ReloadAsync(_currentPath).Forget();
            if (!string.IsNullOrEmpty(error))
            {
                // 読み直しで警告が消えないよう、少し後に出す。
                rootVisualElement.schedule.Execute(() => ShowWarning(error));
                Debug.LogWarning($"[{nameof(SpecViewerWindow)}] {error}");
            }
        }

        /// <summary>
        ///     ページを開いて本文を描く。
        /// </summary>
        /// <param name="path">ページの相対パス。</param>
        /// <param name="addToHistory">履歴に積むか。</param>
        private void OpenPage(string path, bool addToHistory)
        {
            if (_catalog == null || !_catalog.TryGetByPath(path, out SpecCatalogEntry entry)) { return; }

            string markdown;
            try
            {
                markdown = File.ReadAllText(_resolver.ToAbsolutePath(path));
            }
            catch (IOException exception)
            {
                ShowWarning($"ページを読めませんでした: {exception.Message}");
                return;
            }

            if (_textures.Count > MAX_CACHED_TEXTURES) { ClearTextures(); }
            _currentPath = path;
            _lastPagePath = path;
            if (addToHistory) { _history.Push(path); }

            _pageScroll.Clear();
            _pageScroll.Add(CreatePageHeader(entry));
            _pageScroll.Add(_renderer.Render(_parser.Parse(markdown)));
            _pageScroll.scrollOffset = Vector2.zero;
            SelectInTree(path);
            RefreshNavigationButtons();
        }

        /// <summary>
        ///     ページの見出し部分（パス・最終更新・プロパティ）を作る。
        /// </summary>
        /// <param name="entry">ページ。</param>
        /// <returns>要素。</returns>
        private static VisualElement CreatePageHeader(SpecCatalogEntry entry)
        {
            VisualElement header = new() { style = { marginTop = CONTENT_PADDING, marginBottom = CONTENT_PADDING / 2f, opacity = 0.7f } };
            header.Add(new Label(SpecInlineFormatter.Escape(entry.Path)) { style = { whiteSpace = WhiteSpace.Normal } });
            if (!string.IsNullOrEmpty(entry.Database)) { header.Add(new Label($"データベース: {SpecInlineFormatter.Escape(entry.Database)}")); }
            header.Add(new Label("この画面は Notion の写しです。内容を直すときは Notion を編集してください。"));
            return header;
        }

        /// <summary>
        ///     履歴から取り出したページを開く。
        /// </summary>
        /// <param name="path">ページの相対パス。無ければ何もしない。</param>
        private void OpenFromHistory(string path)
        {
            if (!string.IsNullOrEmpty(path)) { OpenPage(path, false); }
        }

        /// <summary>
        ///     ツリーで開いたページを選んだ状態にする（選択の通知は出さない）。
        /// </summary>
        /// <param name="path">ページの相対パス。</param>
        private void SelectInTree(string path)
        {
            if (!_treeBuilder.IdsByPath.TryGetValue(path, out int id)) { return; }

            _treeView.SetSelectionByIdWithoutNotify(new[] { id });
            _treeView.ScrollToItemById(id);
        }

        /// <summary>
        ///     検索語が変わってから少し待って検索する（打ち込み中に何度も検索しないため）。
        /// </summary>
        private void ScheduleSearch()
        {
            _pendingSearch?.Pause();
            _pendingSearch = rootVisualElement.schedule.Execute(() => SearchAsync().Forget()).StartingIn(SEARCH_DELAY_MILLISECONDS);
        }

        /// <summary>
        ///     目録で検索し、本文も検索するときは続けて全文を検索する。検索語が空ならツリーに戻す。
        /// </summary>
        /// <returns>非同期処理。</returns>
        private async UniTaskVoid SearchAsync()
        {
            _searchCancellation?.Cancel();
            _searchCancellation = new CancellationTokenSource();
            CancellationToken token = _searchCancellation.Token;
            string query = _searchField.value;
            bool isSearching = _catalog != null && SpecCatalog.SplitTerms(query).Length > 0;
            _treeView.style.display = isSearching ? DisplayStyle.None : DisplayStyle.Flex;
            _resultList.style.display = isSearching ? DisplayStyle.Flex : DisplayStyle.None;
            _results.Clear();
            if (!isSearching)
            {
                _resultList.Rebuild();
                return;
            }

            foreach (SpecCatalogEntry entry in _catalog.Search(query, MAX_SEARCH_RESULTS)) { _results.Add(new SpecSearchHit(entry, string.Empty)); }
            _resultList.Rebuild();
            if (!_fullTextToggle.value) { return; }

            SpecFullTextSearcher searcher = new(_resolver);
            IReadOnlyList<SpecCatalogEntry> entries = _catalog.Entries;
            IReadOnlyList<SpecSearchHit> hits;
            try
            {
                hits = await UniTask.RunOnThreadPool(() => searcher.Search(entries, query, MAX_SEARCH_RESULTS, token), cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (this == null || token.IsCancellationRequested) { return; }

            // 目録で見つかったものの後に、本文だけで見つかったものを足す。
            HashSet<string> listed = new(StringComparer.Ordinal);
            foreach (SpecSearchHit result in _results) { listed.Add(result.Entry.Path); }
            foreach (SpecSearchHit hit in hits)
            {
                if (listed.Add(hit.Entry.Path) && _results.Count < MAX_SEARCH_RESULTS) { _results.Add(hit); }
            }

            _resultList.Rebuild();
        }

        /// <summary>
        ///     NotionのページIDから、写しにあるページのタイトルを引く。
        /// </summary>
        /// <param name="pageId">ページID。</param>
        /// <returns>タイトル。写しに無ければ null。</returns>
        private string ResolvePageTitle(string pageId)
        {
            return _catalog != null && _catalog.TryGetByPageId(pageId, out SpecCatalogEntry entry) ? entry.Title : null;
        }

        /// <summary>
        ///     ページに書かれた画像のパスから画像を読む。読めない形式や、写しに無いファイルなら null を返す。
        /// </summary>
        /// <param name="href">ページに書かれた画像のパス。</param>
        /// <returns>画像。</returns>
        private Texture2D LoadImage(string href)
        {
            if (_resolver == null) { return null; }

            SpecLinkTarget target = _resolver.Resolve(_currentPath, href);
            if (target.Kind != SpecLinkTarget.TargetKind.File || !File.Exists(target.Value)) { return null; }
            if (Array.IndexOf(LOADABLE_IMAGE_EXTENSIONS, Path.GetExtension(target.Value).ToLowerInvariant()) < 0) { return null; }
            if (_textures.TryGetValue(target.Value, out Texture2D cached) && cached != null) { return cached; }

            Texture2D texture = new(2, 2) { hideFlags = HideFlags.HideAndDontSave };
            if (!texture.LoadImage(File.ReadAllBytes(target.Value)))
            {
                DestroyImmediate(texture);
                return null;
            }

            _textures[target.Value] = texture;
            return texture;
        }

        /// <summary>
        ///     読み込んだ画像を破棄する。
        /// </summary>
        private void ClearTextures()
        {
            foreach (Texture2D texture in _textures.Values)
            {
                if (texture != null) { DestroyImmediate(texture); }
            }

            _textures.Clear();
        }

        /// <summary>
        ///     開いているページを Notion で開く。
        /// </summary>
        private void OpenCurrentInNotion()
        {
            if (_catalog != null && _catalog.TryGetByPath(_currentPath, out SpecCatalogEntry entry) && !string.IsNullOrEmpty(entry.Url))
            {
                Application.OpenURL(entry.Url);
            }
        }

        /// <summary>
        ///     戻る・進む・Notion で開くボタンの押せる状態を更新する。
        /// </summary>
        private void RefreshNavigationButtons()
        {
            _backButton.SetEnabled(_history.HasPrevious);
            _forwardButton.SetEnabled(_history.HasNext);
            bool hasUrl = _catalog != null && _catalog.TryGetByPath(_currentPath, out SpecCatalogEntry entry) && !string.IsNullOrEmpty(entry.Url);
            _notionButton.SetEnabled(hasUrl);
        }

        /// <summary>
        ///     ウィンドウ上部に警告を出す。
        /// </summary>
        /// <param name="message">警告文。</param>
        private void ShowWarning(string message)
        {
            _warningBox.text = message;
            _warningBox.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        ///     ウィンドウ上部の警告を消す。
        /// </summary>
        private void HideWarning()
        {
            _warningBox.style.display = DisplayStyle.None;
        }
    }
}
