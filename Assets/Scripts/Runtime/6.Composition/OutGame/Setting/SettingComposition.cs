using KillChord.Runtime.View.OutGame.Screen;
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
    [SerializeField] private GameObject _parent;
     private AudioSettingData _audioModel;
     private ScreenSettingData _screenModel;
     private bool _isShowScrren = false;

    private void Start()
    {
        var seManager = ServiceLocator.GetInstance<SoundEffectVolumeManager>();
        var voiceManager = ServiceLocator.GetInstance<VoiceVolumeManager>();
        var outGameUiEvent = ServiceLocator.GetInstance<OutGameUIEvent>();
        var bgmManager = ServiceLocator.GetInstance<MusicPlayer>();
        _audioModel = new AudioSettingData(master : 1f, bgm : bgmManager.GetVolume(), se : seManager.GetVolume(),voice : voiceManager.GetVolume());
        // outGameUiEvent.OnShownSettingScreen += () =>
        // {
        //     _isShowScrren = !_isShowScrren;
        //    _parent.SetActive(_isShowScrren);  
        // };
        _audioModel.BGMVolume += bgmManager.SetVolume;
        _audioModel.SEVolume += seManager.SetVolume;
        _audioModel.VoiceVolume += voiceManager.SetVolume;
        _audioSetting.Build(_uiDocument, _audioModel,_parent.transform);
        _screenSetting.Build(_uiDocument, _screenModel);
    }
}
