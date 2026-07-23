using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Player;
using KillChord.Runtime.Utility.Identity;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.Player
{
    /// <summary>
    ///     スキルデータの設定を保持するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = "SkillTemplate", menuName = "Game/SkillTemplate")]
    public class SkillTemplateAsset : ScriptableObject
    {
        /// <summary> スキルIDです。 </summary>
        public SkillId Id => new SkillId(_id.Id);

        /// <summary> 表示名です。 </summary>
        public string DisplayName => _displayName;

        /// <summary> スキル詳細です。 </summary>
        public string SkillDetail => _skillDetail;

        /// <summary> 入力パターンです。 </summary>
        public BeatType[] Pattern => _pattern;

        /// <summary> クールダウンの分子です。 </summary>
        public int CooldownNumerator => _cooldownNumerator;

        /// <summary> クールダウンの分母です。 </summary>
        public int CooldownDenomimator => _cooldownDenomimator;

        /// <summary> スキルの種類です。 </summary>
        public SkillType[] SkillType => _skillType;

        /// <summary> スキルのレベルです。 </summary>
        public SkillLevel Level => new SkillLevel(_level);

        /// <summary> 効果定義です。 </summary>
        public SkillEffectSpec EffectSpec => new SkillEffectSpec(_skillEffectType, _skillTargetingType);

        /// <summary> アニメーションキーです。 </summary>
        public string AnimationKey => _animationKey;

        /// <summary>
        ///     Domain層のSkillDataに変換する。
        /// </summary>
        /// <returns> Domain層のテンプレートです。 </returns>
        public SkillTemplate ToDomain()
        {
            return new SkillTemplate(
                Id, _pattern, _skillType, Level, _cooldownNumerator, _cooldownDenomimator,
                EffectSpec, _animationKey, _displayName, _skillDetail);
        }

        [SerializeField, Tooltip("スキルIDです。")]
        [SourceDataCollection("Skill")]
        private DataID _id;

        [SerializeField, Tooltip("スキル表示名です。")] private string _displayName;

        [SerializeField, Tooltip("入力パターンです。")] private BeatType[] _pattern;

        [SerializeField, TextArea, Tooltip("スキル詳細です。")] private string _skillDetail;

        [SerializeField, Tooltip("スキルの種類です。")]
        private SkillType[] _skillType;

        [SerializeField, Min(0), Tooltip("スキルのレベルです。")]
        private int _level;

        [SerializeField, Min(0), Tooltip("小節単位で表すクールダウン時間の分子です。")]
        private int _cooldownNumerator = 1;

        [SerializeField, Min(1), Tooltip("小節単位で表すクールダウン時間の分母です。")]
        private int _cooldownDenomimator = 1;

        [SerializeField, Tooltip("スキル効果の識別子です。")]
        private SkillEffectType _skillEffectType;

        [SerializeField, Tooltip("スキル対象の解決ルールです。")]
        private SkillTargetingType _skillTargetingType;

        [SerializeField, Tooltip("スキル発動時に再生するアニメーションキー。空なら通常攻撃アニメーションを使う。")]
        private string _animationKey;
    }
}
