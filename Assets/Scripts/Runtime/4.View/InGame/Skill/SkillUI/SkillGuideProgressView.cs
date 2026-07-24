using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.View.InGame.Music;
using LitMotion;
using LitMotion.Extensions;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View.InGame.Skill
{
    /// <summary>
    ///     リズムGUI下部に表示するスキル入力進行UIのView。
    ///     常に「未入力時は走り出し、入力中は次の1拍」のアイコンだけを、
    ///     ACLikeRhythmGuideViewの同じ拍のジャストタイミング位置に左右対称に表示する。
    ///     クールダウン中はゲージで進捗を示す。
    /// </summary>
    public sealed class SkillGuideProgressView : MonoBehaviour, ISkillInputProgressRowView
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="stepSettings"> スキルパターンの各拍子に対応する表示設定（Signatures順）。 </param>
        /// <param name="animationSetting"> アニメーション設定。 </param>
        /// <param name="rhythmGuideView"> ジャストタイミング位置の参照元となるリズムガイドView。未設定の場合はX座標を更新しない。 </param>
        public void Initialize(
            SkillBeatVisualSetting[] stepSettings,
            SkillInputProgressAnimationSetting animationSetting,
            ACLikeRhythmGuideView rhythmGuideView)
        {
            _stepSettings = stepSettings ?? throw new ArgumentNullException(nameof(stepSettings));
            _animationSetting = animationSetting ?? throw new ArgumentNullException(nameof(animationSetting));
            _rhythmGuideView = rhythmGuideView;
            _baseLocalScale = _leftIconImage.rectTransform.localScale;
            _baseLocalEulerAngleZ = _leftIconImage.rectTransform.localEulerAngles.z;
            ApplyStep(0);
        }

        /// <inheritdoc />
        public void UpdateSteps(SkillInputProgressUpdateDTO dto)
        {
            if (dto.PatternMatchCount < 0 || dto.PatternMatchCount > _stepSettings.Length)
            {
                Debug.LogError(
                    $"[{nameof(SkillGuideProgressView)}] 入力進捗とスキルの拍子パターン定義が整合していません。入力進捗：{dto.PatternMatchCount}, 拍子パターン長：{_stepSettings.Length}",
                    this);
                return;
            }

            bool isProgressReset = !dto.SkillTriggeredFlg && dto.PatternMatchCount < _patternMatchCount;
            bool isProgressAdvanced = !dto.SkillTriggeredFlg && dto.PatternMatchCount > _patternMatchCount;

            if (dto.SkillTriggeredFlg)
            {
                ApplyStep(0);
                ProcessSkillTriggered(dto);
            }
            else if (isProgressReset)
            {
                ApplyStep(0);
                PlayResetShakeAnimation();
            }
            else if (isProgressAdvanced)
            {
                ApplyStep(dto.PatternMatchCount);
                PlayAppearAnimation();
            }

            _patternMatchCount = dto.PatternMatchCount;
        }

        private void FixedUpdate()
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

        private void OnDestroy()
        {
            _appearMotion.TryCancel();
            _resetShakeMotion.TryCancel();
        }

        /// <summary>
        ///     指定インデックスの拍子アイコンを、左右対称のジャストタイミング位置に表示する。
        /// </summary>
        /// <param name="index"> 表示するSignaturesのインデックス。 </param>
        private void ApplyStep(int index)
        {
            SkillBeatVisualSetting setting = _stepSettings[index];
            _leftIconImage.sprite = setting.Icon;
            _leftIconImage.color = setting.ActiveColor;
            _rightIconImage.sprite = setting.Icon;
            _rightIconImage.color = setting.ActiveColor;

            if (_rhythmGuideView != null
                && _rhythmGuideView.TryGetJustTimingXPosition(setting.BeatType, out float xPosition))
            {
                SetAnchoredX(_leftIconImage.rectTransform, -xPosition);
                SetAnchoredX(_rightIconImage.rectTransform, xPosition);
            }
        }

        /// <summary>
        ///     RectTransformのX座標のみを設定する。
        /// </summary>
        private static void SetAnchoredX(RectTransform rectTransform, float x)
        {
            Vector2 anchoredPosition = rectTransform.anchoredPosition;
            anchoredPosition.x = x;
            rectTransform.anchoredPosition = anchoredPosition;
        }

        /// <summary>
        ///     アイコン出現時の拡大と左右回転アニメーションを再生する。
        /// </summary>
        private void PlayAppearAnimation()
        {
            _appearMotion.TryCancel();
            ResetIconTransforms();

            Vector3 scaleStrength = _baseLocalScale * (_animationSetting.InputSuccessScaleMultiplier - 1f);
            _appearMotion = LSequence.Create()
                .Join(LMotion.Punch.Create(_baseLocalScale, scaleStrength, _animationSetting.InputSuccessDuration)
                    .WithEase(_animationSetting.InputSuccessEase)
                    .WithFrequency(1)
                    .BindToLocalScale(_leftIconImage.rectTransform))
                .Join(LMotion.Punch.Create(_baseLocalScale, scaleStrength, _animationSetting.InputSuccessDuration)
                    .WithEase(_animationSetting.InputSuccessEase)
                    .WithFrequency(1)
                    .BindToLocalScale(_rightIconImage.rectTransform))
                .Join(LMotion.Punch.Create(_baseLocalEulerAngleZ, _animationSetting.InputSuccessRotationAngle, _animationSetting.InputSuccessDuration)
                    .WithEase(_animationSetting.InputSuccessEase)
                    .WithFrequency(_animationSetting.InputSuccessRotationFrequency)
                    .WithDampingRatio(_animationSetting.InputSuccessRotationDampingRatio)
                    .BindToLocalEulerAnglesZ(_leftIconImage.rectTransform))
                .Join(LMotion.Punch.Create(_baseLocalEulerAngleZ, _animationSetting.InputSuccessRotationAngle, _animationSetting.InputSuccessDuration)
                    .WithEase(_animationSetting.InputSuccessEase)
                    .WithFrequency(_animationSetting.InputSuccessRotationFrequency)
                    .WithDampingRatio(_animationSetting.InputSuccessRotationDampingRatio)
                    .BindToLocalEulerAnglesZ(_rightIconImage.rectTransform))
                .Run(sequence => sequence.WithScheduler(MotionScheduler.UpdateIgnoreTimeScale));
        }

        /// <summary>
        ///     入力リセット時の横揺れアニメーションを再生する。
        ///     X座標はジャストタイミング位置を保持する必要があるため、位置ではなく回転の揺れで表現する。
        /// </summary>
        private void PlayResetShakeAnimation()
        {
            _resetShakeMotion.TryCancel();
            ResetIconTransforms();

            _resetShakeMotion = LSequence.Create()
                .Join(LMotion.Punch.Create(_baseLocalEulerAngleZ, _animationSetting.ResetShakeDistance, _animationSetting.ResetShakeDuration)
                    .WithEase(_animationSetting.ResetShakeEase)
                    .WithFrequency(_animationSetting.ResetShakeFrequency)
                    .WithDampingRatio(_animationSetting.ResetShakeDampingRatio)
                    .BindToLocalEulerAnglesZ(_leftIconImage.rectTransform))
                .Join(LMotion.Punch.Create(_baseLocalEulerAngleZ, _animationSetting.ResetShakeDistance, _animationSetting.ResetShakeDuration)
                    .WithEase(_animationSetting.ResetShakeEase)
                    .WithFrequency(_animationSetting.ResetShakeFrequency)
                    .WithDampingRatio(_animationSetting.ResetShakeDampingRatio)
                    .BindToLocalEulerAnglesZ(_rightIconImage.rectTransform))
                .Run(sequence => sequence.WithScheduler(MotionScheduler.UpdateIgnoreTimeScale));
        }

        /// <summary>
        ///     アイコンのスケール・回転を初期状態へ戻す（X座標はジャストタイミング位置を保持するため触れない）。
        /// </summary>
        private void ResetIconTransforms()
        {
            _leftIconImage.rectTransform.localScale = _baseLocalScale;
            _rightIconImage.rectTransform.localScale = _baseLocalScale;

            Vector3 leftEuler = _leftIconImage.rectTransform.localEulerAngles;
            leftEuler.z = _baseLocalEulerAngleZ;
            _leftIconImage.rectTransform.localEulerAngles = leftEuler;

            Vector3 rightEuler = _rightIconImage.rectTransform.localEulerAngles;
            rightEuler.z = _baseLocalEulerAngleZ;
            _rightIconImage.rectTransform.localEulerAngles = rightEuler;
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
            _cooldownBackgroundImage.fillAmount = fillAmount;
        }

        [SerializeField, Tooltip("左側に表示する拍子アイコンのImage。")]
        private Image _leftIconImage;
        [SerializeField, Tooltip("右側に表示する拍子アイコンのImage。")]
        private Image _rightIconImage;
        [SerializeField, Tooltip("クールダウンを表現するための背景。未設定の場合はクールダウン表示なし。")]
        private Image _cooldownBackgroundImage;

        private SkillBeatVisualSetting[] _stepSettings;
        private SkillInputProgressAnimationSetting _animationSetting;
        private ACLikeRhythmGuideView _rhythmGuideView;
        private MotionHandle _appearMotion;
        private MotionHandle _resetShakeMotion;
        private Vector3 _baseLocalScale;
        private float _baseLocalEulerAngleZ;
        private bool _isSkillCoolingDown;
        private int _patternMatchCount;
        private float _skillTriggeredTimestamp;
        private float _skillReadyTimestamp;
    }
}
