using System;
using KillChord.Runtime.View;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Setting
{
    [CreateAssetMenu(menuName = "KillChord/Settings/Screen")]
    public class ScreenConfig : ScriptableObject
    {
        [SerializeField] private SettingDropDown _dropDownPrefab;
        [SerializeField] private SettingToggle _togglePrefab;

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

