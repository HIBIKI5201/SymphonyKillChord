using UnityEngine;
using KillChord.Runtime.Domain.InGame.Skill;

namespace KillChord.Runtime.Adaptor.OutGame.SkillBuild
{
    /// <summary>
    ///     スキルの情報を View 層へ届けるための DTO。
    /// </summary>
    public readonly struct SkillViewDataDTO
    {
        /// <summary>
        ///     SkillViewDataDTO のコンストラクタ。
        /// </summary>
        /// <param name="skillId"> スキルID。 </param>
        /// <param name="description"> スキルの説明文。 </param>
        /// <param name="type"> スキルのタイプ。 </param>
        /// <param name="icon"> スキルのアイコン画像。 </param>
        /// <param name="level"> スキルのレベル。 </param>
        public SkillViewDataDTO(string skillId, string description, SkillType type, Sprite icon, int level)
        {
            SkillId = skillId;
            Description = description;
            Type = type;
            Icon = icon;
            Level = level;
        }

        /// <summary> スキルID。 </summary>
        public string SkillId { get; }

        /// <summary> スキルの説明文。 </summary>
        public string Description { get; }

        /// <summary> スキルのタイプ。 </summary>
        public SkillType Type { get; }

        /// <summary> スキルのアイコン画像。 </summary>
        public Sprite Icon { get; }

        /// <summary> スキルのレベル。 </summary>
        public int Level { get; }
    }
}
