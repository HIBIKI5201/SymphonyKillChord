using UnityEngine;

namespace DevelopProducts.Persistent.Domain.Input
{
    /// <summary>
    ///     入力アクションの識別子を定義するクラス。
    ///     ゲーム内で使用される入力アクションを一元管理する。
    /// </summary>
    public static class InputActionIds
    {
        // Common
        public static readonly InputActionId Option = new InputActionId(1);

        // InGame
        public static readonly InputActionId Move = new InputActionId(10);

        // OutGame
        public static readonly InputActionId Submit = new InputActionId(100);
        public static readonly InputActionId Cancel = new InputActionId(101);

        public static string GetActionName(InputActionId actionId)
        {
            if(actionId == Option) return nameof(Option);

            if (actionId == Move) return nameof(Move);

            if (actionId == Submit) return nameof(Submit);
            if (actionId == Cancel) return nameof(Cancel);

            return $"Unknown({actionId.Value})";
        }
    }
}
