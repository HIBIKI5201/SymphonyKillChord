using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Editor.Debugger.LogicGraph
{
    /// <summary>
    ///     KillChord Debugger の「ロジック」パネル。
    ///     <para> Play中に攻撃パイプラインの実行を記録し、左の一覧で選んだ1回分をグラフで表示する。 </para>
    ///     <para> 記録はエディタ側だけで行い、ランタイムのコードには手を入れない。 </para>
    /// </summary>
    public sealed class LogicGraphPanel : VisualElement, IDisposable
    {
        /// <summary>
        ///     パネルの見た目を組み立て、記録の取り込みを始める。
        /// </summary>
        public LogicGraphPanel()
        {
            AddToClassList("lgd-panel");

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(STYLE_SHEET_PATH);
            if (styleSheet != null)
            {
                styleSheets.Add(styleSheet);
            }
            else
            {
                Debug.LogWarning($"[{nameof(LogicGraphPanel)}] スタイルシートが見つかりません: {STYLE_SHEET_PATH}");
            }

            Add(CreateToolbar());

            VisualElement body = new VisualElement();
            body.AddToClassList("lgd-body");
            Add(body);

            _recordList = CreateRecordList();
            body.Add(_recordList);

            _graphView = new AttackPipelineGraphView();
            _graphView.AddToClassList("lgd-graph-host");
            body.Add(_graphView);

            _installer = new AttackPipelineTraceInstaller(_recorder);
            _refreshItem = schedule.Execute(Refresh).Every(REFRESH_INTERVAL_MS);
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
            AssemblyReloadEvents.beforeAssemblyReload += HandleBeforeAssemblyReload;
            EditorApplication.quitting += HandleEditorQuitting;
            LogicGraphAssetSaveGuard.OnWillSave += HandleWillSaveAssets;
            Refresh();
        }

        /// <summary>
        ///     記録の取り込みを止め、包んだ攻撃ステップを元に戻す。
        /// </summary>
        public void Dispose()
        {
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= HandleBeforeAssemblyReload;
            EditorApplication.quitting -= HandleEditorQuitting;
            LogicGraphAssetSaveGuard.OnWillSave -= HandleWillSaveAssets;
            _refreshItem?.Pause();
            _refreshItem = null;
            _installer.Uninstall();
        }

        private const string STYLE_SHEET_PATH =
            "Assets/Editor/Scripts/KillChordDebugger/LogicGraph/LogicGraph.uss";
        private const long REFRESH_INTERVAL_MS = 250;
        private const float RECORD_ITEM_HEIGHT = 20f;
        private const string DAMAGE_FORMAT = "F1";

        private readonly AttackPipelineTraceRecorder _recorder = new();
        private readonly AttackPipelineTraceInstaller _installer;
        private readonly List<AttackTraceRecord> _listItems = new();
        private readonly AttackPipelineGraphView _graphView;
        private readonly ListView _recordList;
        private Label _statusLabel;
        private IVisualElementScheduledItem _refreshItem;
        private AttackTraceRecord _shownRecord;
        private int _shownVersion = -1;
        private bool _isFollowingLatest = true;

        /// <summary>
        ///     Playの開始・終了に合わせて、攻撃ステップを包む・戻す。
        /// </summary>
        /// <param name="change"> Playモードの変化。 </param>
        private void HandlePlayModeStateChanged(PlayModeStateChange change)
        {
            // Playを抜ける前に元へ戻し、アセットに記録用のステップを残さない。
            if (change == PlayModeStateChange.ExitingPlayMode)
            {
                _installer.Uninstall();
            }

            Refresh();
        }

        /// <summary>
        ///     再コンパイルの前に、包んだ攻撃ステップを元に戻す。
        /// </summary>
        private void HandleBeforeAssemblyReload()
        {
            _installer.Uninstall();
        }

        /// <summary>
        ///     エディタ終了の前に、包んだ攻撃ステップを元に戻す。
        /// </summary>
        private void HandleEditorQuitting()
        {
            _installer.Uninstall();
        }

        /// <summary>
        ///     アセット保存の直前に元へ戻し、保存が終わってから包み直す。
        /// </summary>
        private void HandleWillSaveAssets()
        {
            if (!_installer.IsInstalled) { return; }

            _installer.Uninstall();
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlaying && _refreshItem != null)
                {
                    _installer.Install();
                }
            };
        }

        /// <summary>
        ///     上部のツールバーを作る。
        /// </summary>
        /// <returns> ツールバーの要素。 </returns>
        private VisualElement CreateToolbar()
        {
            VisualElement toolbar = new VisualElement();
            toolbar.AddToClassList("lgd-toolbar");

            Label title = new Label("攻撃パイプライン");
            title.AddToClassList("lgd-toolbar__title");
            toolbar.Add(title);

            Toggle followToggle = new Toggle("最新を追う") { value = _isFollowingLatest };
            followToggle.AddToClassList("lgd-toolbar__item");
            followToggle.RegisterValueChangedCallback(evt =>
            {
                _isFollowingLatest = evt.newValue;
                _shownVersion = -1;
                Refresh();
            });
            toolbar.Add(followToggle);

            Toggle pauseToggle = new Toggle("記録を止める") { value = _recorder.IsPaused };
            pauseToggle.AddToClassList("lgd-toolbar__item");
            pauseToggle.RegisterValueChangedCallback(evt => _recorder.SetPaused(evt.newValue));
            toolbar.Add(pauseToggle);

            Button clearButton = new Button(() =>
            {
                _recorder.Clear();
                _shownRecord = null;
                _graphView.ClearGraph();
                Refresh();
            }) { text = "消去" };
            clearButton.AddToClassList("lgd-toolbar__item");
            toolbar.Add(clearButton);

            _statusLabel = new Label();
            _statusLabel.AddToClassList("lgd-toolbar__status");
            toolbar.Add(_statusLabel);

            return toolbar;
        }

        /// <summary>
        ///     記録の一覧（新しい順）を作る。
        /// </summary>
        /// <returns> 一覧の要素。 </returns>
        private ListView CreateRecordList()
        {
            ListView listView = new ListView
            {
                itemsSource = _listItems,
                fixedItemHeight = RECORD_ITEM_HEIGHT,
                selectionType = SelectionType.Single,
                makeItem = () =>
                {
                    Label label = new Label();
                    label.AddToClassList("lgd-log__item");
                    return label;
                },
                bindItem = (element, index) =>
                {
                    if (element is Label label && index >= 0 && index < _listItems.Count)
                    {
                        label.text = FormatRecordLine(_listItems[index]);
                    }
                }
            };
            listView.AddToClassList("lgd-log");
            listView.selectionChanged += selection =>
            {
                foreach (object item in selection)
                {
                    if (item is AttackTraceRecord record)
                    {
                        ShowRecord(record);
                    }
                }
            };
            return listView;
        }

        /// <summary>
        ///     登録状態・一覧・グラフを最新の記録に合わせる。
        /// </summary>
        private void Refresh()
        {
            // Play中で、Playを抜ける途中でなければ攻撃ステップを包む。
            if (EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode
                &&!_installer.IsInstalled)
            {
                _installer.Install();
            }

            _statusLabel.text = EditorApplication.isPlaying
                ? $"記録中 {_recorder.Records.Count} 件（パイプライン {_installer.InstalledAssetCount} 個）{(_recorder.IsPaused ? "（停止中）" : string.Empty)}"
                : $"Play中に記録します（{_recorder.Records.Count} 件）";

            if (_shownVersion == _recorder.Version) { return; }
            _shownVersion = _recorder.Version;

            // 一覧は新しい記録を上に並べる。
            _listItems.Clear();
            IReadOnlyList<AttackTraceRecord> records = _recorder.Records;
            for (int i = records.Count - 1; i >= 0; i--)
            {
                _listItems.Add(records[i]);
            }
            _recordList.RefreshItems();

            if (_isFollowingLatest && _listItems.Count > 0)
            {
                _recordList.SetSelectionWithoutNotify(new[] { 0 });
                ShowRecord(_listItems[0]);
            }
        }

        /// <summary>
        ///     記録をグラフに表示する。同じ記録なら描き直さない。
        /// </summary>
        /// <param name="record"> 表示する記録。 </param>
        private void ShowRecord(AttackTraceRecord record)
        {
            if (ReferenceEquals(_shownRecord, record)) { return; }

            _shownRecord = record;
            _graphView.ShowRecord(record);
        }

        /// <summary>
        ///     一覧の1行分の文字列を作る。
        /// </summary>
        /// <param name="record"> 対象の記録。 </param>
        /// <returns> 一覧に出す文字列。 </returns>
        private static string FormatRecordLine(AttackTraceRecord record)
        {
            string state = record.IsCompleted ? record.FinalDamage.ToString(DAMAGE_FORMAT) : "未完了";
            string just = record.IsJustHit ? " Just" : string.Empty;
            return $"{record.Time:F2}s #{record.SequenceNumber} {record.AttackName} → {state}{just}";
        }
    }
}
