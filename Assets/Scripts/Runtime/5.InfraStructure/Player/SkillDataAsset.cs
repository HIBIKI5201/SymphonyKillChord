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
    [CreateAssetMenu(fileName = "SkillData", menuName = "Game/SkillData")]
    public class SkillDataAsset : ScriptableObject
    {
        public int Id => _id;
        public BeatType[] Pattern => _pattern;
        public ISkillEffect SkillEffect => _skillEffect;
        public string AnimationKey => _animationKey;

        /// <summary>
        ///     Domain層のSkillDataに変換する。
        /// </summary>
        public SkillData ToDomain()
        {
            return new SkillData(_id, _pattern, _skillEffect, _animationKey);
        }

        [SerializeField] private int _id;
        [SerializeField] private BeatType[] _pattern;
        [SerializeReference, SubclassSelector] private ISkillEffect _skillEffect;
        [SerializeField, Tooltip("スキル発動時に再生するアニメーションキー。空なら通常攻撃アニメーションを使う。")]
        private string _animationKey;

    }
}