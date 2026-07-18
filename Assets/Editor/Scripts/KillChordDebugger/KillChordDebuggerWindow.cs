using System.Collections.Generic;
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

        [MenuItem("KillChord/Debugger")]
        private static void Open()
        {
            KillChordDebuggerWindow window = GetWindow<KillChordDebuggerWindow>();
            window.titleContent = new GUIContent("KillChord Debugger");
            window.minSize = new Vector2(480, 360);
        }

        private void CreateGUI()
        {
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

            _saveDataView = new SaveDataDebugView();
            content.Add(_saveDataView.Root);
            _panels[SAVE_DATA_PANEL_KEY] = _saveDataView.Root;

            saveDataNavButton?.RegisterCallback<ClickEvent>(_ => ShowPanel(SAVE_DATA_PANEL_KEY, saveDataNavButton));

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

        private SaveDataDebugView _saveDataView;
        private readonly Dictionary<string, VisualElement> _panels = new();
    }
}
