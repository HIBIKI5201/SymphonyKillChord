using UnityEngine;
using UnityEngine.UIElements;

public class SettingManager : MonoBehaviour
{
    [SerializeField] private AudioSetting _audioSetting;
    [SerializeField] private ScreenSetting _screenSetting;

    [SerializeField] private UIDocument _uiDocument;

     private AudioSettingData _audioModel;
     private ScreenSettingData _screenModel;

    private void Start()
    {
        _audioModel = new AudioSettingData(master : 1f, bgm : 1f, se : 1f);
        _audioSetting.Build(_uiDocument, _audioModel);
        _screenSetting.Build(_uiDocument, _screenModel);
    }
}
