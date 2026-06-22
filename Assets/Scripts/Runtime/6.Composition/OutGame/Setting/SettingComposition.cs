using KillChord.Runtime.View.Persistent.Music;
using KillChord.Runtime.View.Persistent.Voice;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingComposition : MonoBehaviour
{
    [SerializeField] private AudioSetting _audioSetting;
    [SerializeField] private ScreenSetting _screenSetting;

    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private Transform _parent;
     private AudioSettingData _audioModel;
     private ScreenSettingData _screenModel;

    private void Start()
    {
        var seManager = ServiceLocator.GetInstance<SoundEffectVolumeManager>();
        var voiceManager = ServiceLocator.GetInstance<VoiceVolumeManager>();


        _audioModel = new AudioSettingData(master : 1f, bgm : 1f, se : seManager.GetVolume(),voice : voiceManager.GetVolume());

        _audioModel.SEVolume += seManager.SetVolume;
        _audioModel.VoiceVolume += voiceManager.SetVolume;
        _audioSetting.Build(_uiDocument, _audioModel,_parent);
        _screenSetting.Build(_uiDocument, _screenModel);
    }
}
