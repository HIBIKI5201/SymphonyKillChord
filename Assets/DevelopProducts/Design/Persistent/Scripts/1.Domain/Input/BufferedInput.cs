using UnityEngine;

namespace DevelopProducts.Persistent.Domain.Input
{
    /// <summary>
    ///     バッファに記録する入力1件分イベントを表す構造体。
    /// </summary>
    public readonly struct BufferedInput
    {
        public BufferedInput(InputActionId actionId, InputPheseId pheseId, float timestamp)
        {
            ActionId = actionId;
            PheseId = pheseId;
            Timestamp = timestamp;
        }

        public InputActionId ActionId { get; }
        public InputPheseId PheseId { get; }
        public float Timestamp { get; }

        public override string ToString()
        {
            return $"BufferedInput(ActionId: {InputActionIds.GetActionName(ActionId)}, PheseId: {InputPheseIds.GetPheseName(PheseId)}, Timestamp: {Timestamp})";
        }
    }
}
