using System.Collections.Generic;
using KillChord.Editor.Debugger.LogicGraph;
using KillChord.Editor.Utility;
using SymphonyFrameWork.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Editor.Debugger
{
    /// <summary>
    ///     KillChordの各種デバッグ機能をまとめて表示するエディタウィンドウです。
    ///     今後、セーブデータ以外のデバッグ機能もこのウィンドウへ追加していきます。
    /// </summary>
    public sealed class KillChordDebuggerWindow : EditorWindow
    {
        private const string WINDOW_UXML_PATH =
            "Assets/Editor/Scripts/KillChordDebugger/KillChordDebuggerWindow.uxml";
        private const string STYLE_SHEET_PATH =
            "Assets/Editor/Scripts/KillChordDebugger/KillChordDebugger.uss";

        private const string SAVE_DATA_PANEL_KEY = "savedata";
        private const string LOGIC_GRAPH_PANEL_KEY = "logicgraph";

        /// <summary> KillChordデバッガーを開きます。 </summary>
        [MenuItem(EditorWindowPathConst.KILL_CHORD_DEBUGGER_PATH)]
        private static void Open()
        {
            KillChordDebuggerWindow window = GetWindow<KillChordDebuggerWindow>();
            window.titleContent = new GUIContent("KillChord Debugger");
            window.minSize = new Vector2(480, 360);
        }

        /// <summary> デバッガーのUIを構築します。 </summary>
        private void CreateGUI()
        {
            _saveDataView?.Dispose();
            _saveDataView = null;
            _logicGraphPanel?.Dispose();
            _logicGraphPanel = null;
            rootVisualElement.Clear();
            _panels.Clear();

            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(WINDOW_UXML_PATH);
            if (visualTree == null)
            {
                rootVisualElement.Add(new Label($"UXMLが見つかりません: {WINDOW_UXML_PATH}"));
                return;
            }
            visualTree.CloneTree(rootVisualElement);

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(STYLE_SHEET_PATH);
            if (styleSheet != null)
            {
                rootVisualElement.styleSheets.Add(styleSheet);
            }

            VisualElement content = rootVisualElement.Q<VisualElement>("kcd-content");
            Button saveDataNavButton = rootVisualElement.Q<Button>("kcd-nav-savedata");
            Button logicGraphNavButton = rootVisualElement.Q<Button>("kcd-nav-logicgraph");

            _saveDataView = new SaveDataWindow();
            content.Add(_saveDataView);
            _panels[SAVE_DATA_PANEL_KEY] = _saveDataView;

            _logicGraphPanel = new LogicGraphPanel();
            content.Add(_logicGraphPanel);
            _panels[LOGIC_GRAPH_PANEL_KEY] = _logicGraphPanel;

            saveDataNavButton?.RegisterCallback<ClickEvent>(_ => ShowPanel(SAVE_DATA_PANEL_KEY, saveDataNavButton));
            logicGraphNavButton?.RegisterCallback<ClickEvent>(_ => ShowPanel(LOGIC_GRAPH_PANEL_KEY, logicGraphNavButton));

            ShowPanel(SAVE_DATA_PANEL_KEY, saveDataNavButton);
        }

        /// <summary>
        ///     指定したキーのパネルのみを表示します。今後パネルが増えた場合はここに切り替え先を追加します。
        /// </summary>
        private void ShowPanel(string key, Button activeNavButton)
        {
            foreach (KeyValuePair<string, VisualElement> panel in _panels)
            {
                panel.Value.style.display = panel.Key == key
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            }

            VisualElement nav = rootVisualElement.Q<VisualElement>("kcd-nav");
            nav?.Query<Button>().ForEach(button =>
                button.EnableInClassList("kcd-nav__button--active", button == activeNavButton));
        }

        private SaveDataWindow _saveDataView;
        private LogicGraphPanel _logicGraphPanel;
        private readonly Dictionary<string, VisualElement> _panels = new();

        /// <summary> ウィンドウ無効化時にセーブデータパネルとロジックパネルを破棄します。 </summary>
        private void OnDisable()
        {
            _saveDataView?.Dispose();
            _saveDataView = null;
            _logicGraphPanel?.Dispose();
            _logicGraphPanel = null;
        }
    }
}
