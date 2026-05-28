using DevelopProducts.BehaviorGraph.Runtime.Domain.Persistent.Input;

namespace DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Battle
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