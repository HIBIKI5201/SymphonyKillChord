using KillChord.Editor.Utility;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Editor.SinfoniaOperator
{
    /// <summary>
    ///     SinfoniaOperatorの設定をProjectSettingsに表示するプロバイダー。
    /// </summary>
    public class SinfoniaOperatorSettingsProvider : SettingsProvider
    {
        /// <summary>
        ///     SinfoniaOperator の設定プロバイダーを生成する。
        /// </summary>
        private SinfoniaOperatorSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) :
            base(path, scopes, keywords)
        {
        }

        /// <summary>
        ///     Project Settings に SinfoniaOperator の設定ページを登録する。
        /// </summary>
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new SinfoniaOperatorSettingsProvider(SETTINGS_PATH, SettingsScope.Project);
        }

        /// <summary>
        ///     設定ページを開いたときに、設定アセットを編集できる状態にしてインスペクターを生成する。
        /// </summary>
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            var settings = SinfoniaOperatorSettings.instance;
            settings.hideFlags = HideFlags.HideAndDontSave & ~HideFlags.NotEditable;
            UnityEditor.Editor.CreateCachedEditor(settings, null, ref _editor);
        }

        /// <summary>
        ///     設定のインスペクターを描画し、変更があれば保存する。
        /// </summary>
        public override void OnGUI(string searchContext)
        {
            EditorGUI.BeginChangeCheck();
            _editor.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                SinfoniaOperatorSettings.instance.Save();
            }
        }

        private const string SETTINGS_PATH = ProviderConst.PROJECT_PATH + "SinfoniaOperator";

        private UnityEditor.Editor _editor;
    }
}
