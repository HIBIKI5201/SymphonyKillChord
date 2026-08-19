using KillChord.Runtime.Adaptor.InGame.Skill;
using LitMotion;
using LitMotion.Extensions;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View.InGame.Skill
{
    /// <summary>
    ///     スキル一覧UIの1スキル分の行View。
    ///     コマンドの全拍を並べて表示し、入力済みの拍を点灯させる。
    ///     クールダウン中はゲージで進捗を示す。
    /// </summary>
    public sealed class SkillListRowView : MonoBehaviour, ISkillInputProgressRowView
    {
        /// <summary> StepViewを並べる親Transform。 </summary>
        public Transform StepRoot => _stepRoot;

        /// <summary>
        ///     行全体のアニメーション設定とスキルアイコンを初期化する。
        /// </summary>
        /// <param name="animationSetting"> アニメーション設定。 </param>
        /// <param name="skillIcon"> この行が担当するスキルのアイコン。未設定の場合はnull。 </param>
        public void Initialize(SkillInputProgressAnimationSetting animationSetting, Sprite skillIcon)
        {
            _animationSetting = animationSetting ?? throw new ArgumentNullException(nameof(animationSetting));
            _stepRootRectTransform = _stepRoot as RectTransform;

            if (_stepRootRectTransform == null)
            {
                throw new InvalidOperationException($"{nameof(_stepRoot)} にはRectTransformが必要です。");
            }

            _stepRootBaseAnchoredPositionX = _stepRootRectTransform.anchoredPosition.x;

            // スキルアイコンは入力進捗によらず不変のため、初期化時に一度だけ適用する。
            ApplySkillIcon(skillIcon);
            SetCooldownFillAmount(1);
        }

        /// <summary>
        ///     生成済みStepViewを設定する。
        /// </summary>
        /// <param name="stepViews"> 設定するStepViewのリスト。 </param>
        public void SetSteps(SkillListStepView[] stepViews)
        {
            _stepViews = stepViews ?? throw new ArgumentNullException(nameof(stepViews));
        }

        /// <inheritdoc />
        public void UpdateSteps(SkillInputProgressUpdateDTO dto)
        {
            if (_stepViews == null)
            {
                return;
            }

            if (dto.PatternMatchCount < 0 || dto.PatternMatchCount > _stepViews.Length)
            {
                Debug.LogError(
                    $"[{nameof(SkillListRowView)}] 入力進捗とスキルの拍子パターン定義が整合していません。入力進捗：{dto.PatternMatchCount}, 拍子パターン長：{_stepViews.Length}",
                    this);
                return;
            }

            bool isProgressReset = !dto.SkillTriggeredFlg && dto.PatternMatchCount < _patternMatchCount;

            for (int i = 0; i < dto.PatternMatchCount; i++)
            {
                _stepViews[i].SetStepOn();
            }

            for (int i = dto.PatternMatchCount; i < _stepViews.Length; i++)
            {
                _stepViews[i].SetStepOff();
            }

            if (dto.SkillTriggeredFlg)
            {
                ProcessSkillTriggered(dto);
                PlayFlareTriggered();
            }
            else if (isProgressReset)
            {
                PlayProgressResetAnimation();
            }

            _patternMatchCount = dto.PatternMatchCount;
        }

        /// <inheritdoc />
        /// <remarks> 一覧UIは常に全スキルを表示するため、この実装では何もしない。 </remarks>
        public void SetVisible(bool visible)
        {
        }

        private void Update()
        {
            if (!_isSkillCoolingDown)
            {
                return;
            }

            float cooldownDuration = _skillReadyTimestamp - _skillTriggeredTimestamp;
            if (cooldownDuration <= 0)
            {
                SetCooldownFillAmount(1);
                ProcessSkillReady();
                return;
            }

            float fillAmount = (Time.unscaledTime - _skillTriggeredTimestamp) / cooldownDuration;
            if (fillAmount >= 1)
            {
                SetCooldownFillAmount(1);
                ProcessSkillReady();
            }
            else
            {
                SetCooldownFillAmount(fillAmount);
            }
        }

        [SerializeField, Tooltip("StepViewを並べる親Transform。")]
        private Transform _stepRoot;
        [SerializeField, Tooltip("クールダウンを表現するための背景。未設定の場合はクールダウン表示なし。")]
        private Image _cooldownBackgroundImage;
        [SerializeField, Tooltip("この行が担当するスキル自体のアイコンを表示するImage。未設定の場合はスキルアイコン表示なし。")]
        private Image _skillIconImage;
        [SerializeField]
        private Material _material;
        [SerializeField]
        private Image _rhythmResetFlareImage;
        [SerializeField]
        private Image _rhythmTriggerdFlareImage;

        private SkillListStepView[] _stepViews;
        private SkillInputProgressAnimationSetting _animationSetting;
        private RectTransform _stepRootRectTransform;
        private MotionHandle _progressResetHandle;
        private MotionHandle _progressFlareHandle;
        private bool _isSkillCoolingDown;
        private int _patternMatchCount;
        private float _skillTriggeredTimestamp;
        private float _skillReadyTimestamp;
        private float _stepRootBaseAnchoredPositionX;

        private void Awake()
        {
            _rhythmResetFlareImage.enabled = false;
            _rhythmTriggerdFlareImage.enabled = false;
        }

        /// <summary>
        ///     破棄時に再生中のアニメーションを停止する。
        /// </summary>
        private void OnDestroy()
        {
            _progressResetHandle.TryCancel();
            _progressFlareHandle.TryCancel();
        }

        /// <summary>
        ///     スキル発動時の処理。
        /// </summary>
        /// <param name="dto"></param>
        private void ProcessSkillTriggered(SkillInputProgressUpdateDTO dto)
        {
            _isSkillCoolingDown = true;
            _skillTriggeredTimestamp = dto.CurrentTimestamp;
            _skillReadyTimestamp = dto.SkillReadyTimestamp;
            SetCooldownFillAmount(0);
        }

        /// <summary>
        ///     スキルクールダウン終了時の処理。
        /// </summary>
        private void ProcessSkillReady()
        {
            _isSkillCoolingDown = false;
        }

        /// <summary>
        ///     クールダウンゲージのfillAmountを設定する。背景未設定の場合は何もしない。
        /// </summary>
        /// <param name="fillAmount"> 設定するfillAmount。 </param>
        private void SetCooldownFillAmount(float fillAmount)
        {
            if (_cooldownBackgroundImage == null)
            {
                return;
            }
            _cooldownBackgroundImage.fillAmount = 1f - fillAmount;
        }

        /// <summary>
        ///     この行が担当するスキルのアイコンを適用する。
        ///     アイコン用Imageを持たないPrefab構成でも動作するよう、未設定時は何もしない。
        /// </summary>
        /// <param name="skillIcon"> 適用するスキルアイコン。未設定の場合はnull。 </param>
        private void ApplySkillIcon(Sprite skillIcon)
        {
            if (_skillIconImage == null)
            {
                return;
            }

            _skillIconImage.sprite = skillIcon;
        }

        /// <summary>
        ///     入力進捗リセット時に行全体の横揺れを再生する。
        /// </summary>
        private void PlayProgressResetAnimation()
        {
            _progressResetHandle.TryComplete();
            Vector2 anchoredPosition = _stepRootRectTransform.anchoredPosition;
            anchoredPosition.x = _stepRootBaseAnchoredPositionX;
            _stepRootRectTransform.anchoredPosition = anchoredPosition;

            _progressResetHandle = LMotion.Punch.Create(
                        _stepRootBaseAnchoredPositionX,
                        _animationSetting.ResetShakeDistance,
                        _animationSetting.ResetShakeDuration)
                    .WithEase(_animationSetting.ResetShakeEase)
                    .WithFrequency(_animationSetting.ResetShakeFrequency)
                    .WithDampingRatio(_animationSetting.ResetShakeDampingRatio)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .BindToAnchoredPositionX(_stepRootRectTransform);
            PlayFlareReset();
        }
        private void PlayFlareTriggered()
        {
            _progressFlareHandle.TryComplete();
            _progressFlareHandle = LMotion.Create(1f, 0f, 0.05f)
                    .WithLoops(8)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .WithOnComplete(() => _rhythmTriggerdFlareImage.enabled = false)
                    .BindToColorA(_rhythmTriggerdFlareImage);
            _rhythmTriggerdFlareImage.enabled = true;
        }
        private void PlayFlareReset()
        {
            _progressFlareHandle.TryComplete();
            _progressFlareHandle = LMotion.Create(1f, 0f, 0.05f)
                    .WithLoops(4)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .WithOnComplete(() => _rhythmResetFlareImage.enabled = false)
                    .BindToColorA(_rhythmResetFlareImage);
            _rhythmResetFlareImage.enabled = true;
        }
    }
}
