using System;
using KillChord.Runtime.View;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "KillChord/Settings/Audio")]
public class AudioSetting : ScriptableObject
{
    [SerializeField] private SettingSlider _sliderPrefab;

    public void Build(UIDocument document, AudioSettingData model)
    {
        CreateSlider(document,
            "Master Volume",
            () => model.MasterVolume,
            value => model.MasterVolume = value);

        CreateSlider(document,
            "BGM Volume",
            () => model.BgmVolume,
            value => model.BgmVolume = value);

        CreateSlider(document,
            "SE Volume",
            () => model.SeVolume,
            value => model.SeVolume = value);
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