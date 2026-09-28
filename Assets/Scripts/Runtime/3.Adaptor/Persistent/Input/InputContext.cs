using UnityEngine.InputSystem;

namespace KillChord.Runtime.Adaptor.Persistent.Input
{
    /// <summary>
    ///     入力1件分の共通データ。
    ///     PCやスマホなど、入力デバイスに依存しない形で、入力の内容を表現するための構造体。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct InputContext<T> where T : unmanaged
    {
        /// <summary>
        ///     入力の種類・値・フェーズ・時刻を指定して生成する。
        /// </summary>
        public InputContext(InputActionKind actionId, T value, InputActionPhase phase, float timestamp)
        {
            ActionKind = actionId;
            Value = value;
            Timestamp = timestamp;
            Phase = phase;
        }

        /// <summary>
        ///     Input System のコールバックから値とフェーズを読み取って生成する。
        /// </summary>
        public InputContext(InputActionKind actionId, InputAction.CallbackContext context, float timestamp)
        {
            ActionKind = actionId;
            Value = context.ReadValue<T>();
            Timestamp = timestamp;
            Phase = context.phase;
        }

        /// <summary> 入力アクションの種類。 </summary>
        public InputActionKind ActionKind { get; }
        /// <summary> 入力値。 </summary>
        public T Value { get; }
        /// <summary> 入力された時刻。 </summary>
        public float Timestamp { get; }
        /// <summary> 入力のフェーズ。 </summary>
        public InputActionPhase Phase { get; }
    }
}
