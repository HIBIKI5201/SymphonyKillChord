using KillChord.Runtime.View.Persistent.Music;
using System;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Title
{
    /// <summary>
    ///     音量設定タブの View クラス。
    /// </summary>
    public sealed class VolumeSettingsTabView : IDisposable
    {
        /// <summary>
        ///    音量設定タブの UI 要素を初期化します。
        /// </summary>
        /// <param name="rootElement"></param>
        /// <param name="musicPlayer"></param>
        /// <param name="sePlayer"></param>
        /// <exception cref="NullReferenceException"></exception>
        public VolumeSettingsTabView(VisualElement rootElement, MusicPlayer musicPlayer, SoundEffectVolumeManager sePlayer)
        {
            _musicPlayer = musicPlayer;
            _sePlayer = sePlayer;

            _bgmVolumeSlider = rootElement.Q<VisualElement>(BGM_SLIDER_NAME)
                ?? throw new NullReferenceException($"{BGM_SLIDER_NAME} が見つかりません。");
            _seVolumeSlider = rootElement.Q<VisualElement>(SE_SLIDER_NAME)
                ?? throw new NullReferenceException($"{SE_SLIDER_NAME} が見つかりません。");

            // 音量スライダーの参照を取得
            _bgmSlider = _bgmVolumeSlider.Q<Slider>()
                ?? throw new NullReferenceException($"{BGM_SLIDER_NAME} のスライダーが見つかりません。");
            _seSlider = _seVolumeSlider.Q<Slider>()
                ?? throw new NullReferenceException($"{SE_SLIDER_NAME} のスライダーが見つかりません。");

            // 音量スライダーの初期値を設定
            _bgmSlider.value = musicPlayer.GetVolume();
            _seSlider.value = sePlayer.GetVolume();

            // 名前での検索ではないのは、スライダーエレメント上には単一のラベルしか存在しないため。
            var bgmLabel = _bgmVolumeSlider.Q<Label>()
                ?? throw new NullReferenceException($"{BGM_SLIDER_NAME} のラベルが見つかりません。");
            var seLabel = _seVolumeSlider.Q<Label>()
                ?? throw new NullReferenceException($"{SE_SLIDER_NAME} のラベルが見つかりません。");

            bgmLabel.text = "BGM";
            seLabel.text = "SE";

            RegisterCallbacks();
        }

        /// <summary>
        ///   音量設定タブの View のリソースを解放します。
        /// </summary>
        public void Dispose()
        {
            UnregisterCallbacks();
        }

        private const string BGM_SLIDER_NAME = "BGMVolumeSlider";
        private const string SE_SLIDER_NAME = "SEVolumeSlider";

        private VisualElement _bgmVolumeSlider;
        private VisualElement _seVolumeSlider;

        private Slider _bgmSlider;
        private Slider _seSlider;

        private MusicPlayer _musicPlayer;
        private SoundEffectVolumeManager _sePlayer;

        /// <summary>
        ///     音量スライダーのコールバックを登録します。
        /// </summary>
        private void RegisterCallbacks()
        {
            _bgmSlider.RegisterValueChangedCallback(UpdateBGMVolume);
            _seSlider.RegisterValueChangedCallback(UpdateSEVolume);
        }

        /// <summary>
        ///     音量スライダーのコールバックを解除します。
        /// </summary>
        private void UnregisterCallbacks()
        {
            _bgmSlider.UnregisterValueChangedCallback(UpdateBGMVolume);
            _seSlider.UnregisterValueChangedCallback(UpdateSEVolume);
        }

        /// <summary>
        ///     BGM 音量スライダーの値が変更されたときに呼び出されるコールバック。
        /// </summary>
        /// <param name="evt"></param>
        private void UpdateBGMVolume(ChangeEvent<float> evt)
        {
            _musicPlayer.SetVolume(evt.newValue);
        }

        /// <summary>
        ///     SE 音量スライダーの値が変更されたときに呼び出されるコールバック。
        /// </summary>
        /// <param name="evt"></param>
        private void UpdateSEVolume(ChangeEvent<float> evt)
        {
            _sePlayer.SetVolume(evt.newValue);
        }
    }
}
