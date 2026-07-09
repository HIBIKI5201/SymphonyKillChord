using System;
using KillChord.Runtime.View;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Setting
{
    [CreateAssetMenu(menuName = "KillChord/Settings/Audio")]
    public class AudioSetting : ScriptableObject
    {
        [SerializeField] private SettingSlider _sliderPrefab; // MonoBehaviour の Prefab
        [SerializeField] private string[] _audioSettingTitle;

        public void Build(UIDocument document, AudioSettingData model, Transform parent)
        {
            for (int i = 0; i < model.Settings.Length; i++)
            {
                int index = i;
                CreateSlider(document, parent,
                    _audioSettingTitle[index], //実際の設定項目順番と、名前を一致させる必要がある。
                    () => model.Get(index),
                    value => model.Set(index, value));
            }
        }

        private void CreateSlider(
            UIDocument document,
            Transform parent,
            string title,
            Func<float> getter,
            Action<float> setter)
        {
            var slider = Instantiate(_sliderPrefab, parent);

            slider.Create(document, Category.Audio, title);
            slider.Bind(getter, setter);
        }
    }
}
