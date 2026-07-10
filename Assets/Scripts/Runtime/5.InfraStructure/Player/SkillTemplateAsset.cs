using KillChord.Runtime.Domain;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.Player;
using SymphonyFrameWork.Attribute;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.Player
{
    /// <summary>
    ///     スキルデータの設定を保持するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = "SkillTemplate", menuName = "Game/SkillTemplate")]
    public class SkillTemplateAsset : ScriptableObject
    {
        public int Id => _id;
        public BeatType[] Pattern => _pattern;
        public int CooldownNumerator => _cooldownNumerator;
        public int CooldownDenomimator => _cooldownDenomimator;
        public ISkillEffect SkillEffect => _skillEffect;
        public string AnimationKey => _animationKey;

        /// <summary>
        ///     Domain層のSkillDataに変換する。
        /// </summary>
        public SkillTemplate ToDomain()
        {
            return new SkillTemplate(_id, _pattern, _cooldownNumerator, _cooldownDenomimator, _skillEffect, _animationKey);
        }

        [SerializeField] private int _id;
        [SerializeField] private BeatType[] _pattern;
        [SerializeField, Min(0), Tooltip("小節単位で表すクールダウン時間の分子")]
        private int _cooldownNumerator = 1;
        [SerializeField, Min(1), Tooltip("小節単位で表すクールダウン時間の分母")]
        private int _cooldownDenomimator = 1;
        [SerializeReference, SubclassSelector] private ISkillEffect _skillEffect;
        [SerializeField, Tooltip("スキル発動時に再生するアニメーションキー。空なら通常攻撃アニメーションを使う。")]
        private string _animationKey;

    }
}
