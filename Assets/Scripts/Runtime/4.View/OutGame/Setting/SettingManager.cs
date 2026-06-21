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
        _audioSetting.Build(_uiDocument, _audioModel);
        _screenSetting.Build(_uiDocument, _screenModel);
    }
}
