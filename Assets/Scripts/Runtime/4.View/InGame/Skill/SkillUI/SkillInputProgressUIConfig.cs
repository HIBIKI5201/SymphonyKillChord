using LitMotion;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill
{
    /// <summary>
    ///     スキル入力進行UIの表示設定。
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(SkillInputProgressUIConfig),
        menuName = "KillChord/InGame/Skill/SkillInputProgressUIConfig")]
    public class SkillInputProgressUIConfig : ScriptableObject
    {
        /// <summary>
        ///     View層用の表示設定を生成する。
        /// </summary>
        public SkillInputProgressViewSetting Create()
        {
            if (_settings == null || _settings.Count == 0)
            {
                throw new System.InvalidOperationException("スキル入力進行UIの表示設定が存在しません。");
            }

            List<SkillBeatVisualSetting> settings = new();
            HashSet<int> seenBeatTypes = new();

            for (int i = 0; i < _settings.Count; i++)
            {
                SkillBeatVisualSettingConfig config = _settings[i]
                    ?? throw new System.InvalidOperationException($"スキル入力進行UIの表示設定がnullです。インデックス: {i}");

                SkillBeatVisualSetting setting = config.Create();
                if (!seenBeatTypes.Add(setting.BeatType))
                {
                    throw new System.InvalidOperationException(
                    $"{name}: BeatType {setting.BeatType} が重複しています。");
                }

                settings.Add(setting);
            }

            SkillInputProgressAnimationSetting animationSetting = new(
                _inputSuccessScaleMultiplier,
                _inputSuccessRotationAngle,
                _inputSuccessDuration,
                _inputSuccessEase,
                _inputSuccessRotationFrequency,
                _inputSuccessRotationDampingRatio,
                _resetShakeDistance,
                _resetShakeDuration,
                _resetShakeEase,
                _resetShakeFrequency,
                _resetShakeDampingRatio);

            return new SkillInputProgressViewSetting(settings, animationSetting);
        }

        [SerializeField, Tooltip("拍子ごとの表示設定。")]
        private List<SkillBeatVisualSettingConfig> _settings = new();

        [Header("入力成功アニメーション")]
        [SerializeField, Min(1f), Tooltip("入力成功時に点が一瞬大きくなる倍率。")]
        private float _inputSuccessScaleMultiplier = 1.25f;

        [SerializeField, Min(0f), Tooltip("入力成功時に点が左右へ揺れる最大回転角度。")]
        private float _inputSuccessRotationAngle = 12f;

        [SerializeField, Min(0.01f), Tooltip("入力成功アニメーションの長さ（秒）。")]
        private float _inputSuccessDuration = 0.3f;

        [SerializeField, Tooltip("入力成功アニメーションのイージング。")]
        private Ease _inputSuccessEase = Ease.OutQuad;

        [SerializeField, Min(1), Tooltip("入力成功時の左右回転の振動数。")]
        private int _inputSuccessRotationFrequency = 5;

        [SerializeField, Min(0f), Tooltip("入力成功時の左右回転の減衰率。大きいほど早く揺れが収まる。")]
        private float _inputSuccessRotationDampingRatio = 1f;

        [Header("進捗リセットアニメーション")]
        [SerializeField, Min(0f), Tooltip("進捗リセット時にゲージ全体が横へ揺れる最大距離。")]
        private float _resetShakeDistance = 20f;

        [SerializeField, Min(0.01f), Tooltip("進捗リセット時の横揺れ時間（秒）。")]
        private float _resetShakeDuration = 0.35f;

        [SerializeField, Tooltip("進捗リセット時の横揺れイージング。")]
        private Ease _resetShakeEase = Ease.OutQuad;

        [SerializeField, Min(1), Tooltip("進捗リセット時の横揺れ振動数。")]
        private int _resetShakeFrequency = 6;

        [SerializeField, Min(0f), Tooltip("進捗リセット時の横揺れ減衰率。大きいほど早く揺れが収まる。")]
        private float _resetShakeDampingRatio = 1f;
    }
}
