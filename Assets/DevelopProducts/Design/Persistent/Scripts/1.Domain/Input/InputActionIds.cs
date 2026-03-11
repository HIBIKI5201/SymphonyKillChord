using UnityEngine;

namespace DevelopProducts.Persistent.Domain.Input
{
    /// <summary>
    ///     入力アクションの識別子を定義するクラス。
    ///     ゲーム内で使用される入力アクションを一元管理する。
    /// </summary>
    public static class InputActionIds
    {
        public static readonly InputActionId Move = new InputActionId(1);
        public static readonly InputActionId Attack = new InputActionId(2);
        public static readonly InputActionId Skill = new InputActionId(3);

        public static readonly InputActionId Submit = new InputActionId(100);
        public static readonly InputActionId Cancel = new InputActionId(101);

        public static string GetActionName(InputActionId actionId)
        {
            if (actionId == Move) return nameof(Move);
            if (actionId == Attack) return nameof(Attack);
            if (actionId == Skill) return nameof(Skill);

            if (actionId == Submit) return nameof(Submit);
            if (actionId == Cancel) return nameof(Cancel);

            return $"Unknown({actionId.Value})";
        }
    }
}
