using LitMotion;
using LitMotion.Extensions;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View.InGame.Skill
{
    /// <summary>
    ///     スキル一覧UIの、コマンド1拍分の表示を管理するクラス。
    /// </summary>
    public sealed class SkillListStepView : MonoBehaviour
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="data"> 拍子ごとの表示設定。 </param>
        /// <param name="animationSetting"> 入力成功時のアニメーション設定。 </param>
        public void Initialize(
            in SkillBeatVisualSetting data,
            SkillInputProgressAnimationSetting animationSetting)
        {
            _animationSetting = animationSetting ?? throw new ArgumentNullException(nameof(animationSetting));
            _baseLocalScale = transform.localScale;
            _baseLocalEulerAngleZ = transform.localEulerAngles.z;

            if (_iconImage != null)
            {
                //_iconImage.sprite = data.Icon;
            }

            _onColor = data.ActiveColor;
            _offColor = data.NormalColor;
            SetStepOff();
        }

        /// <summary>
        ///     入力済みにする。
        /// </summary>
        public void SetStepOn()
        {
            ApplyColor(_onColor);

            _iconImage.color = Color.clear;

            if (_isActive)
            {
                return;
            }

            _isActive = true;
            PlayInputSuccessAnimation();
        }

        /// <summary>
        ///     未入力にする。
        /// </summary>
        public void SetStepOff()
        {
            ApplyColor(_offColor);

            _iconImage.color = Color.black;

            if (_isActive)
            {
                _inputSuccessMotion.TryCancel();
                ResetAnimationTransform();
            }

            _isActive = false;
        }

        [SerializeField, Tooltip("背景色を反映するImage。")]
        private Image _backgroundImage;

        [SerializeField, Tooltip("アイコンを表示するImage。")]
        private Image _iconImage;

        private Color _onColor;
        private Color _offColor;
        private SkillInputProgressAnimationSetting _animationSetting;
        private MotionHandle _inputSuccessMotion;
        private Vector3 _baseLocalScale;
        private float _baseLocalEulerAngleZ;
        private bool _isActive;

        /// <summary>
        ///     破棄時に再生中のアニメーションを停止する。
        /// </summary>
        private void OnDestroy()
        {
            _inputSuccessMotion.TryCancel();
        }

        /// <summary>
        ///     入力済み・未入力に応じた色を反映する。
        /// </summary>
        /// <param name="color"> 反映する色。 </param>
        private void ApplyColor(Color color)
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.color = color;
            }
        }

        /// <summary>
        ///     入力成功時の拡大と左右回転アニメーションを再生する。
        /// </summary>
        private void PlayInputSuccessAnimation()
        {
            _inputSuccessMotion.TryCancel();
            ResetAnimationTransform();

            Vector3 scaleStrength = _baseLocalScale * (_animationSetting.InputSuccessScaleMultiplier - 1f);
            _inputSuccessMotion = LSequence.Create()
                .Join(LMotion.Punch.Create(
                        _baseLocalScale,
                        scaleStrength,
                        _animationSetting.InputSuccessDuration)
                    .WithEase(_animationSetting.InputSuccessEase)
                    .WithFrequency(1)
                    .BindToLocalScale(transform))
                .Join(LMotion.Punch.Create(
                        _baseLocalEulerAngleZ,
                        _animationSetting.InputSuccessRotationAngle,
                        _animationSetting.InputSuccessDuration)
                    .WithEase(_animationSetting.InputSuccessEase)
                    .WithFrequency(_animationSetting.InputSuccessRotationFrequency)
                    .WithDampingRatio(_animationSetting.InputSuccessRotationDampingRatio)
                    .BindToLocalEulerAnglesZ(transform))
                .Run(sequence => sequence.WithScheduler(MotionScheduler.UpdateIgnoreTimeScale));
        }

        /// <summary>
        ///     アニメーション対象のTransformを初期状態へ戻す。
        /// </summary>
        private void ResetAnimationTransform()
        {
            transform.localScale = _baseLocalScale;
            Vector3 localEulerAngles = transform.localEulerAngles;
            localEulerAngles.z = _baseLocalEulerAngleZ;
            transform.localEulerAngles = localEulerAngles;
        }
    }
}
