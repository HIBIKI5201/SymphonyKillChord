using KillChord.Runtime.Adaptor.Persistent.Music;
using R3;
using System;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Setting
{
    /// <summary>
    ///     Home設定画面の音量ゲージを管理するView。
    /// </summary>
    public sealed class AudioSettingsView : IDisposable
    {
        /// <summary>
        ///     UI要素を取得し、共通音量設定へバインドする。
        /// </summary>
        public AudioSettingsView(
            VisualElement rootElement,
            IAudioSettingsViewModel audioSettingsViewModel,
            IAudioSettingsCommand audioSettingsCommand)
        {
            _audioSettingsViewModel = audioSettingsViewModel
                ?? throw new ArgumentNullException(nameof(audioSettingsViewModel));
            _audioSettingsCommand = audioSettingsCommand
                ?? throw new ArgumentNullException(nameof(audioSettingsCommand));
            _bgmSlider = Require<SliderInt>(rootElement, BGM_SLIDER_NAME);
            _soundEffectSlider = Require<SliderInt>(rootElement, SOUND_EFFECT_SLIDER_NAME);
            _voiceSlider = Require<SliderInt>(rootElement, VOICE_SLIDER_NAME);
            _bgmValueLabel = Require<Label>(rootElement, BGM_VALUE_LABEL_NAME);
            _soundEffectValueLabel = Require<Label>(rootElement, SOUND_EFFECT_VALUE_LABEL_NAME);
            _voiceValueLabel = Require<Label>(rootElement, VOICE_VALUE_LABEL_NAME);
            _subscriptions = new CompositeDisposable();

            RegisterCallbacks();
            SubscribeViewModel();
        }

        /// <summary>
        ///     登録済みコールバックを解除する。
        /// </summary>
        public void Dispose()
        {
            _bgmSlider.UnregisterValueChangedCallback(HandleBgmVolumeChanged);
            _soundEffectSlider.UnregisterValueChangedCallback(HandleSoundEffectVolumeChanged);
            _voiceSlider.UnregisterValueChangedCallback(HandleVoiceVolumeChanged);
            _subscriptions.Dispose();
        }

        private const string BGM_SLIDER_NAME = "BgmVolumeSlider";
        private const string SOUND_EFFECT_SLIDER_NAME = "SoundEffectVolumeSlider";
        private const string VOICE_SLIDER_NAME = "VoiceVolumeSlider";
        private const string BGM_VALUE_LABEL_NAME = "BgmValueLabel";
        private const string SOUND_EFFECT_VALUE_LABEL_NAME = "SoundEffectValueLabel";
        private const string VOICE_VALUE_LABEL_NAME = "VoiceValueLabel";

        private readonly IAudioSettingsViewModel _audioSettingsViewModel;
        private readonly IAudioSettingsCommand _audioSettingsCommand;
        private readonly SliderInt _bgmSlider;
        private readonly SliderInt _soundEffectSlider;
        private readonly SliderInt _voiceSlider;
        private readonly Label _bgmValueLabel;
        private readonly Label _soundEffectValueLabel;
        private readonly Label _voiceValueLabel;
        private readonly CompositeDisposable _subscriptions;

        /// <summary>
        ///     UIと共通音量設定のコールバックを登録する。
        /// </summary>
        private void RegisterCallbacks()
        {
            _bgmSlider.RegisterValueChangedCallback(HandleBgmVolumeChanged);
            _soundEffectSlider.RegisterValueChangedCallback(HandleSoundEffectVolumeChanged);
            _voiceSlider.RegisterValueChangedCallback(HandleVoiceVolumeChanged);
        }

        /// <summary>
        ///     共通音量設定の変更を購読する。
        /// </summary>
        private void SubscribeViewModel()
        {
            _audioSettingsViewModel.BgmVolume
                .Subscribe(HandleBgmVolumePublished)
                .AddTo(_subscriptions);
            _audioSettingsViewModel.SoundEffectVolume
                .Subscribe(HandleSoundEffectVolumePublished)
                .AddTo(_subscriptions);
            _audioSettingsViewModel.VoiceVolume
                .Subscribe(HandleVoiceVolumePublished)
                .AddTo(_subscriptions);
        }

        /// <summary>
        ///     BGMゲージの変更を共通音量設定へ渡す。
        /// </summary>
        private void HandleBgmVolumeChanged(ChangeEvent<int> changeEvent)
        {
            _audioSettingsCommand.SetBgmVolume(changeEvent.newValue);
        }

        /// <summary>
        ///     効果音ゲージの変更を共通音量設定へ渡す。
        /// </summary>
        private void HandleSoundEffectVolumeChanged(ChangeEvent<int> changeEvent)
        {
            _audioSettingsCommand.SetSoundEffectVolume(changeEvent.newValue);
        }

        /// <summary>
        ///     ボイスゲージの変更を共通音量設定へ渡す。
        /// </summary>
        private void HandleVoiceVolumeChanged(ChangeEvent<int> changeEvent)
        {
            _audioSettingsCommand.SetVoiceVolume(changeEvent.newValue);
        }

        /// <summary>
        ///     BGM音量をゲージと数値表示へ反映する。
        /// </summary>
        private void HandleBgmVolumePublished(int volume)
        {
            SetValue(_bgmSlider, _bgmValueLabel, volume);
        }

        /// <summary>
        ///     効果音音量をゲージと数値表示へ反映する。
        /// </summary>
        private void HandleSoundEffectVolumePublished(int volume)
        {
            SetValue(_soundEffectSlider, _soundEffectValueLabel, volume);
        }

        /// <summary>
        ///     ボイス音量をゲージと数値表示へ反映する。
        /// </summary>
        private void HandleVoiceVolumePublished(int volume)
        {
            SetValue(_voiceSlider, _voiceValueLabel, volume);
        }

        /// <summary>
        ///     ゲージと数値ラベルへ同じ値を設定する。
        /// </summary>
        private static void SetValue(SliderInt slider, Label valueLabel, int value)
        {
            slider.SetValueWithoutNotify(value);
            valueLabel.text = value.ToString();
        }

        /// <summary>
        ///     必須UI要素を取得する。
        /// </summary>
        private static T Require<T>(VisualElement rootElement, string elementName)
            where T : VisualElement
        {
            return rootElement.Q<T>(elementName)
                ?? throw new InvalidOperationException(
                    $"[{nameof(AudioSettingsView)}] {elementName} が見つかりませんでした。");
        }
    }
}
