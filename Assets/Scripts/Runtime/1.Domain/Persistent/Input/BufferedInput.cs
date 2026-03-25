using UnityEngine;
using UnityEngine.InputSystem;

namespace KillChord.Runtime.Domain
{
    /// <summary>
    ///     履歴に保存される入力データを表す構造体。
    /// </summary>
    public readonly struct BufferedInput
    {
        public BufferedInput(InputActionId actionId,
            InputActionPhase phase,
            float timestamp, 
            Vector2 vectorValue,
            float floatValue
            )
        {
            ActionId = actionId;
            Phase = phase;
            Timestamp = timestamp;
            VectorValue = vectorValue;
            FloatValue = floatValue;
        }

        public InputActionId ActionId { get; }
        public InputActionPhase Phase { get; }
        public float Timestamp { get; }
        public Vector2 VectorValue { get; }
        public float FloatValue { get; }

        public override string ToString()
        {
            return $"ActionId: {ActionId}, Phase: {Phase}, Timestamp: {Timestamp}, VectorValue: {VectorValue}, FloatValue: {FloatValue}";
        }
    }
}
