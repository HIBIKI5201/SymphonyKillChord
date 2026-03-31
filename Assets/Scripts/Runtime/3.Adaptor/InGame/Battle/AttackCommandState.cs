using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Domain.InGame.Battle;

namespace KillChord.Runtime.Adaptor.InGame.Battle
{
    /// <summary>
    ///     現在選択されている攻撃コマンドの状態を管理するクラス。
    /// </summary>
    public class AttackCommandState
    {
        public AttackId SelectedAttackId { get; private set; } = AttackId.Normal;

        /// <summary>
        ///     攻撃コマンドの種類を受け取り、選択されている攻撃IDを更新するメソッド。
        /// </summary>
        /// <param name="commandType"></param>
        public void SelectAttack(AttackCommandType commandType)
        {
            SelectedAttackId = commandType switch
            {
                AttackCommandType.Normal => AttackId.Normal,
                AttackCommandType.SkillA => AttackId.SkillA,
                AttackCommandType.SkillB => AttackId.SkillB,
                AttackCommandType.Ultimate => AttackId.Ultimate,
                _ => throw new System.ArgumentOutOfRangeException(nameof(commandType), commandType, "Unsupported attack command."),
            };
        }
    }
}
