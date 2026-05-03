using KillChord.Runtime.Domain;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.Player;
using SymphonyFrameWork.Attribute;
using UnityEngine;

namespace SymphonyKillChord.InfraStructure.Player
{
    /// <summary>
    ///     スキルデータの設定を保持するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = "SkillData", menuName = "Game/SkillData")]
    public class SkillDataAsset : ScriptableObject
    {
        /// <summary>
        ///     Domain層のSkillDataに変換する。
        /// </summary>
        public SkillData ToDomain()
        {
            return new SkillData(_id, _pattern, _skillEffect, _skillVisual);
        }

        public int Id => _id;
        public BeatType[] Pattern => _pattern;
        public ISkillEffect SkillEffect => _skillEffect;
        public ISkillVisual SkillVisual => _skillVisual;

        [SerializeField] private int _id;
        [SerializeField] private BeatType[] _pattern;
        [SerializeReference, SubclassSelector] private ISkillEffect _skillEffect;
        [SerializeReference, SubclassSelector] private ISkillVisual _skillVisual;
    }
}