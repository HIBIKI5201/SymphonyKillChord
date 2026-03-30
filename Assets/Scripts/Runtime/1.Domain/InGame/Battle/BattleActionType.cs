using KillChord.Runtime.Domain.Persistent.Input;

namespace KillChord.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     バトルアクションの定義
    /// </summary>
    public enum BattleActionType
    {
        Attack = InputActionId.Attack,
        Dodge = InputActionId.Dodge,
    }
}