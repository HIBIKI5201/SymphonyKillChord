using UnityEngine.InputSystem;

namespace DevelopProducts.BehaviorGraph.Runtime.Adaptor.Persistent.Input
{
    /// <summary>
    ///     入力1件分の共通データ。
    ///     PCやスマホなど、入力デバイスに依存しない形で、入力の内容を表現するための構造体。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct InputContext<T> where T : unmanaged
    {
        public InputContext(InputActionKind actionId, T value, InputActionPhase phase, float timestamp)
        {
            ActionKind = actionId;
            Value = value;
            Timestamp = timestamp;
            Phase = phase;
        }

        public InputContext(InputActionKind actionId, InputAction.CallbackContext context, float timestamp)
        {
            ActionKind = actionId;
            Value = context.ReadValue<T>();
            Timestamp = timestamp;
            Phase = context.phase;
        }

        public InputActionKind ActionKind { get; }
        public T Value { get; }
        public float Timestamp { get; }
        public InputActionPhase Phase { get; }
    }
}
