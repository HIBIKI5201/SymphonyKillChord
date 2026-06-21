using System;
using KillChord.Runtime.View;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "KillChord/Settings/Audio")]
public class AudioSetting : ScriptableObject
{
    [SerializeField] private SettingSlider _sliderPrefab;
    [SerializeField] private string[] _audioSettingTitle;

    public void Build(UIDocument document, AudioSettingData model)
    {
        for(int i = 0; i < model.Settings.Length; i++)
        {
            CreateSlider(document,
            _audioSettingTitle[i],
            () => model.Settings[i],
            value => model.Settings[i] = value);
        }
    }

    private void CreateSlider(
        UIDocument document,
        string title,
        Func<float> getter,
        Action<float> setter)
    {
        var slider = Instantiate(_sliderPrefab);

        slider.Create(document, Category.Audio,title);
        slider.Bind(getter, setter);
    }
}