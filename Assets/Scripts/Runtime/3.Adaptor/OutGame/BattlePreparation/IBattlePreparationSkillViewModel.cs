using R3;
using System.Collections.Generic;

namespace KillChord.Runtime.Adaptor.OutGame.BattlePreparation
{
    /// <summary>
    ///     戦闘準備画面の装備スキル表示 ViewModel インターフェースです。
    /// </summary>
    public interface IBattlePreparationSkillViewModel
    {
        /// <summary> 現在の装備スキル表示一覧です。 </summary>
        ReadOnlyReactiveProperty<IReadOnlyList<BattlePreparationSkillDTO>> Skills { get; }

        /// <summary>
        ///     DTO から装備スキル表示状態を反映します。
        /// </summary>
        /// <param name="dto"> 装備スキル一覧です。 </param>
        void Apply(in BattlePreparationSkillViewDTO dto);
    }
}
