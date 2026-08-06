using System;

namespace KillChord.Runtime.Adaptor.OutGame.BattlePreparation
{
    /// <summary>
    ///     戦闘準備画面に表示する装備スキル一覧です。
    /// </summary>
    public readonly ref struct BattlePreparationSkillViewDTO
    {
        /// <summary>
        ///     表示情報を初期化します。
        /// </summary>
        /// <param name="skills"> スロット順の装備スキル一覧です。 </param>
        public BattlePreparationSkillViewDTO(ReadOnlySpan<BattlePreparationSkillDTO> skills)
        {
            Skills = skills;
        }

        /// <summary> スロット順の装備スキル一覧です。 </summary>
        public ReadOnlySpan<BattlePreparationSkillDTO> Skills { get; }
    }
}
