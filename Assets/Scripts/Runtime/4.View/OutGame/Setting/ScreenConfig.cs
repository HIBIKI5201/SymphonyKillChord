using System;
using KillChord.Runtime.View;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Setting
{
    /// <summary>
    ///     画面設定の項目を生成するための設定データ。
    /// </summary>
    [CreateAssetMenu(menuName = "KillChord/Settings/Screen")]
    public class ScreenConfig : ScriptableObject
    {
        [SerializeField, Tooltip("ドロップダウン項目のプレハブ。")] private SettingDropDown _dropDownPrefab;
        [SerializeField, Tooltip("トグル項目のプレハブ。")] private SettingToggle _togglePrefab;

        /// <summary>
        ///     解像度・画面モード・垂直同期の設定項目を生成する。
        /// </summary>
        public void Build(UIDocument document, ScreenSettingData model)
        {
            CreateDropDown(document,
                "Resolution",
                () => model.ResolutionIndex,
                value => model.ResolutionIndex = value);

            CreateDropDown(document,
                "Screen Mode",
                () => model.ScreenModeIndex,
                value => model.ScreenModeIndex = value);

            CreateToggle(document,
                "VSync",
                () => model.IsVSync,
                value => model.IsVSync = value);
        }

        /// <summary>
        ///     ドロップダウンの設定項目を生成して値と結びつける。
        /// </summary>
        private void CreateDropDown(
            UIDocument document,
            string title,
            Func<int> getter,
            Action<int> setter)
        {
            var dropDown = Instantiate(_dropDownPrefab);

            dropDown.Create(document, Category.Screen, title);
            dropDown.Bind(getter, setter);
        }

        /// <summary>
        ///     トグルの設定項目を生成して値と結びつける。
        /// </summary>
        private void CreateToggle(
            UIDocument document,
            string title,
            Func<bool> getter,
            Action<bool> setter)
        {
            var toggle = Instantiate(_togglePrefab);

            toggle.Create(document, Category.Screen, title);
            toggle.Bind(getter, setter);
        }
    }
}

