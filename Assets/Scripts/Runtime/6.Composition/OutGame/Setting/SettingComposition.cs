using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.OutGame.Setting;
using KillChord.Runtime.View.Persistent.Music;
using KillChord.Runtime.View.Persistent.Voice;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.Composition.OutGame.Setting
{
    /// <summary>
    ///     設定画面を初期化するモジュールです。
    /// </summary>
    public sealed class SettingComposition : OutGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(SettingComposition);

        /// <summary> 実行順です。 </summary>
        public override int Order => 140;

        [SerializeField] private AudioConfig _audioSetting;
        [SerializeField] private ScreenConfig _screenSetting;

        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private GameObject _parent;
        private AudioSettingData _audioModel;
        private ScreenSettingData _screenModel;

        /// <summary>
        ///     設定画面を初期化します。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            SoundEffectVolumeManager seManager = ServiceLocator.GetInstance<SoundEffectVolumeManager>();
            VoiceVolumeManager voiceManager = ServiceLocator.GetInstance<VoiceVolumeManager>();
            OutGameUIEvent outGameUiEvent = ServiceLocator.GetInstance<OutGameUIEvent>();
            MusicPlayer bgmManager = ServiceLocator.GetInstance<MusicPlayer>();
            if (seManager == null || voiceManager == null || outGameUiEvent == null || bgmManager == null)
            {
                return false;
            }

            _audioModel = new AudioSettingData(
                master : 1f,
                bgm : bgmManager.GetVolume(),
                se : seManager.GetVolume(),
                voice : voiceManager.GetVolume());
            _audioModel.BGMVolume += bgmManager.SetVolume;
            _audioModel.SEVolume += seManager.SetVolume;
            _audioModel.VoiceVolume += voiceManager.SetVolume;
            _audioSetting.Build(_uiDocument, _audioModel, _parent.transform);
            _screenSetting.Build(_uiDocument, _screenModel);
            return true;
        }
    }
}
