using KillChord.Runtime.Adaptor.Persistent.Environment;
using KillChord.Runtime.Adaptor.Persistent.Music;
using R3;
using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace KillChord.Runtime.View.InGame.UI
{
    /// <summary>
    ///     ポーズ画面から開く設定パネルのView。
    ///     BGM・SE・ボイスの音量と判定調整を、設定画面と同じViewModel・Commandで変更する。
    /// </summary>
    [AddComponentMenu("KillChord/View/UI/PauseOptionPanelView")]
    public sealed class PauseOptionPanelView : MonoBehaviour
    {
        /// <summary> パネルを閉じた時に発火するイベント。 </summary>
        public event Action OnClosed;

        /// <summary> パネルを開いている間はtrue。 </summary>
        public bool IsOpen => _isOpen;

        /// <summary>
        ///     設定のViewModel・Commandと、文言のViewModelを結びつける。
        /// </summary>
        /// <param name="labelViewModel"> 項目名を持つポーズ画面のViewModel。 </param>
        /// <param name="audioSettingsViewModel"> 音量設定のViewModel。 </param>
        /// <param name="audioSettingsCommand"> 音量設定のCommand。 </param>
        /// <param name="environmentSettingsViewModel"> 判定調整を持つ環境設定のViewModel。 </param>
        /// <param name="environmentSettingsCommand"> 判定調整を変更・保存する環境設定のCommand。 </param>
        public void Bind(
            PauseWindowViewModel labelViewModel,
            IAudioSettingsViewModel audioSettingsViewModel,
            IAudioSettingsCommand audioSettingsCommand,
            IEnvironmentSettingsViewModel environmentSettingsViewModel,
            IEnvironmentSettingsCommand environmentSettingsCommand)
        {
            Unbind();
            _audioSettingsCommand = audioSettingsCommand ?? throw new ArgumentNullException(nameof(audioSettingsCommand));
            _environmentSettingsCommand = environmentSettingsCommand ?? throw new ArgumentNullException(nameof(environmentSettingsCommand));
            if (labelViewModel == null) { throw new ArgumentNullException(nameof(labelViewModel)); }
            if (audioSettingsViewModel == null) { throw new ArgumentNullException(nameof(audioSettingsViewModel)); }
            if (environmentSettingsViewModel == null) { throw new ArgumentNullException(nameof(environmentSettingsViewModel)); }

            _subscriptions = new CompositeDisposable();
            SubscribeLabel(labelViewModel.OptionLabel, _titleText);
            SubscribeLabel(labelViewModel.BgmVolumeLabel, _bgmLabelText);
            SubscribeLabel(labelViewModel.SoundEffectVolumeLabel, _soundEffectLabelText);
            SubscribeLabel(labelViewModel.VoiceVolumeLabel, _voiceLabelText);
            SubscribeLabel(labelViewModel.RhythmOffsetLabel, _rhythmOffsetLabelText);
            SubscribeLabel(labelViewModel.CloseLabel, _closeButtonText);

            SubscribeVolume(audioSettingsViewModel.BgmVolume, _bgmSlider, _bgmValueText);
            SubscribeVolume(audioSettingsViewModel.SoundEffectVolume, _soundEffectSlider, _soundEffectValueText);
            SubscribeVolume(audioSettingsViewModel.VoiceVolume, _voiceSlider, _voiceValueText);
            environmentSettingsViewModel.RhythmOffsetStep
                .Subscribe(step => SetSliderValue(_rhythmOffsetSlider, step))
                .AddTo(_subscriptions);
            if (_rhythmOffsetValueText != null)
            {
                environmentSettingsViewModel.RhythmOffsetLabel
                    .Subscribe(label => _rhythmOffsetValueText.text = label)
                    .AddTo(_subscriptions);
            }

            AddSliderListener(_bgmSlider, HandleBgmSliderChanged);
            AddSliderListener(_soundEffectSlider, HandleSoundEffectSliderChanged);
            AddSliderListener(_voiceSlider, HandleVoiceSliderChanged);
            AddSliderListener(_rhythmOffsetSlider, HandleRhythmOffsetSliderChanged);
            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(Close);
            }
        }

        /// <summary>
        ///     既存UI入力モジュールの取消操作を購読し、開いている間はパネルを閉じる操作として扱う。
        /// </summary>
        /// <param name="inputModule"> 決定・取消を管理する既存UI入力モジュールです。 </param>
        public void BindCancelInput(InputSystemUIInputModule inputModule)
        {
            UnbindCancelInput();
            _cancelAction = inputModule != null ? inputModule.cancel?.action : null;
            if (_cancelAction != null)
            {
                _cancelAction.performed += HandleCancelInputHandler;
            }
        }

        /// <summary>
        ///     購読と入力の結びつきを解除する。
        /// </summary>
        public void Unbind()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            RemoveSliderListener(_bgmSlider, HandleBgmSliderChanged);
            RemoveSliderListener(_soundEffectSlider, HandleSoundEffectSliderChanged);
            RemoveSliderListener(_voiceSlider, HandleVoiceSliderChanged);
            RemoveSliderListener(_rhythmOffsetSlider, HandleRhythmOffsetSliderChanged);
            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(Close);
            }

            _audioSettingsCommand = null;
            _environmentSettingsCommand = null;
        }

        /// <summary>
        ///     取消操作の購読を解除する。
        /// </summary>
        public void UnbindCancelInput()
        {
            if (_cancelAction != null)
            {
                _cancelAction.performed -= HandleCancelInputHandler;
            }

            _cancelAction = null;
        }

        /// <summary>
        ///     パネルを開き、最初の項目を選択する。
        /// </summary>
        public void Open()
        {
            _isOpen = true;
            _isCancelPending = false;
            _openedFrame = Time.frameCount;
            gameObject.SetActive(true);
            SelectFirst();
        }

        /// <summary>
        ///     判定調整の変更を保存してパネルを閉じる。音量は変更した時点で保存されている。
        /// </summary>
        public void Close()
        {
            if (!_isOpen)
            {
                return;
            }

            _isOpen = false;
            _isCancelPending = false;
            // 判定調整は環境設定のプレビューとして変わるため、閉じる時に設定画面と同じ保存先へ確定する。
            _environmentSettingsCommand?.ConfirmChanges();
            gameObject.SetActive(false);
            OnClosed?.Invoke();
        }

        [SerializeField, Tooltip("パネルの見出しのテキストです。")]
        private TMP_Text _titleText;

        [SerializeField, Tooltip("BGM音量の項目名のテキストです。")]
        private TMP_Text _bgmLabelText;

        [SerializeField, Tooltip("BGM音量を変えるスライダーです。")]
        private Slider _bgmSlider;

        [SerializeField, Tooltip("BGM音量の値のテキストです。")]
        private TMP_Text _bgmValueText;

        [SerializeField, Tooltip("SE音量の項目名のテキストです。")]
        private TMP_Text _soundEffectLabelText;

        [SerializeField, Tooltip("SE音量を変えるスライダーです。")]
        private Slider _soundEffectSlider;

        [SerializeField, Tooltip("SE音量の値のテキストです。")]
        private TMP_Text _soundEffectValueText;

        [SerializeField, Tooltip("ボイス音量の項目名のテキストです。")]
        private TMP_Text _voiceLabelText;

        [SerializeField, Tooltip("ボイス音量を変えるスライダーです。")]
        private Slider _voiceSlider;

        [SerializeField, Tooltip("ボイス音量の値のテキストです。")]
        private TMP_Text _voiceValueText;

        [SerializeField, Tooltip("判定調整の項目名のテキストです。")]
        private TMP_Text _rhythmOffsetLabelText;

        [SerializeField, Tooltip("判定調整の段階を変えるスライダーです。")]
        private Slider _rhythmOffsetSlider;

        [SerializeField, Tooltip("判定調整の値のテキストです。")]
        private TMP_Text _rhythmOffsetValueText;

        [SerializeField, Tooltip("パネルを閉じるボタンです。")]
        private Button _closeButton;

        [SerializeField, Tooltip("パネルを閉じるボタンの文言のテキストです。")]
        private TMP_Text _closeButtonText;

        private IAudioSettingsCommand _audioSettingsCommand;
        private IEnvironmentSettingsCommand _environmentSettingsCommand;
        private CompositeDisposable _subscriptions;
        private InputAction _cancelAction;
        private bool _isOpen;
        private bool _isCancelPending;
        private int _openedFrame = -1;

        /// <summary>
        ///     開いた時と同じ取消操作では閉じないようにし、次のLateUpdateで閉じる。
        /// </summary>
        private void LateUpdate()
        {
            if (!_isCancelPending)
            {
                return;
            }

            _isCancelPending = false;
            if (_isOpen)
            {
                Close();
            }
        }

        /// <summary> 破棄時に購読を解除します。 </summary>
        private void OnDestroy()
        {
            UnbindCancelInput();
            Unbind();
        }

        /// <summary>
        ///     取消操作を受け取り、開いている間だけ閉じる予約をする。
        /// </summary>
        /// <param name="context"> UI取消操作の通知です。 </param>
        private void HandleCancelInputHandler(InputAction.CallbackContext context)
        {
            if (_isOpen && _openedFrame != Time.frameCount)
            {
                _isCancelPending = true;
            }
        }

        /// <summary> BGM音量のスライダー操作を保存する。 </summary>
        /// <param name="value"> スライダーの値。 </param>
        private void HandleBgmSliderChanged(float value)
        {
            _audioSettingsCommand?.SetBgmVolume(Mathf.RoundToInt(value));
        }

        /// <summary> SE音量のスライダー操作を保存する。 </summary>
        /// <param name="value"> スライダーの値。 </param>
        private void HandleSoundEffectSliderChanged(float value)
        {
            _audioSettingsCommand?.SetSoundEffectVolume(Mathf.RoundToInt(value));
        }

        /// <summary> ボイス音量のスライダー操作を保存する。 </summary>
        /// <param name="value"> スライダーの値。 </param>
        private void HandleVoiceSliderChanged(float value)
        {
            _audioSettingsCommand?.SetVoiceVolume(Mathf.RoundToInt(value));
        }

        /// <summary> 判定調整のスライダー操作を反映する。保存はパネルを閉じる時に行う。 </summary>
        /// <param name="value"> スライダーの値。 </param>
        private void HandleRhythmOffsetSliderChanged(float value)
        {
            _environmentSettingsCommand?.SetRhythmOffsetStep(Mathf.RoundToInt(value));
        }

        /// <summary>
        ///     最初の項目へフォーカスを移す。
        /// </summary>
        private void SelectFirst()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null || _bgmSlider == null)
            {
                return;
            }

            eventSystem.SetSelectedGameObject(null);
            eventSystem.SetSelectedGameObject(_bgmSlider.gameObject);
        }

        /// <summary>
        ///     音量の変化をスライダーと値のテキストへ反映する。
        /// </summary>
        /// <param name="volume"> 音量の通知元。 </param>
        /// <param name="slider"> 反映先のスライダー。 </param>
        /// <param name="valueText"> 反映先の値のテキスト。 </param>
        private void SubscribeVolume(ReadOnlyReactiveProperty<int> volume, Slider slider, TMP_Text valueText)
        {
            volume.Subscribe(value =>
            {
                SetSliderValue(slider, value);
                if (valueText != null)
                {
                    valueText.text = value.ToString(CultureInfo.InvariantCulture);
                }
            }).AddTo(_subscriptions);
        }

        /// <summary>
        ///     文言の変化をテキストへ反映する。
        /// </summary>
        /// <param name="label"> 文言の通知元。 </param>
        /// <param name="text"> 反映先のテキスト。 </param>
        private void SubscribeLabel(ReadOnlyReactiveProperty<string> label, TMP_Text text)
        {
            if (text == null)
            {
                return;
            }

            label.Subscribe(value => text.text = value).AddTo(_subscriptions);
        }

        /// <summary>
        ///     操作の通知を出さずにスライダーの値を変える。
        /// </summary>
        /// <param name="slider"> 対象のスライダー。 </param>
        /// <param name="value"> 設定する値。 </param>
        private static void SetSliderValue(Slider slider, int value)
        {
            if (slider != null)
            {
                slider.SetValueWithoutNotify(value);
            }
        }

        /// <summary>
        ///     スライダーの値の変化を購読する。
        /// </summary>
        /// <param name="slider"> 対象のスライダー。 </param>
        /// <param name="listener"> 値が変わった時の処理。 </param>
        private static void AddSliderListener(Slider slider, UnityEngine.Events.UnityAction<float> listener)
        {
            if (slider != null)
            {
                slider.onValueChanged.AddListener(listener);
            }
        }

        /// <summary>
        ///     スライダーの値の変化の購読を解除する。
        /// </summary>
        /// <param name="slider"> 対象のスライダー。 </param>
        /// <param name="listener"> 解除する処理。 </param>
        private static void RemoveSliderListener(Slider slider, UnityEngine.Events.UnityAction<float> listener)
        {
            if (slider != null)
            {
                slider.onValueChanged.RemoveListener(listener);
            }
        }
    }
}
