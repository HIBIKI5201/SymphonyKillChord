using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KillChord.Runtime.Domain.Persistent.Input
{
    /// <summary>
    ///     履歴に保存される入力データを表す構造体。
    /// </summary>
    public readonly struct BufferedInput : IComparable<BufferedInput>
    {
        /// <summary>
        ///     バッファに記録する入力1件を生成する。
        /// </summary>
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

        /// <summary> 入力アクションの ID。 </summary>
        public InputActionId ActionId { get; }
        /// <summary> 入力のフェーズ。 </summary>
        public InputActionPhase Phase { get; }
        /// <summary> 入力された時刻。 </summary>
        public float Timestamp { get; }
        /// <summary> Vector2 型の入力値。 </summary>
        public Vector2 VectorValue { get; }
        /// <summary> float 型の入力値。 </summary>
        public float FloatValue { get; }

        /// <summary>
        ///     入力時刻で順序を比較する。
        /// </summary>
        public int CompareTo(BufferedInput other)
        {
            return Timestamp.CompareTo(other.Timestamp);
        }

        /// <summary>
        ///     デバッグ用に入力内容を文字列で返す。
        /// </summary>
        public override string ToString()
        {
            return $"ActionId: {ActionId}, Phase: {Phase}, Timestamp: {Timestamp}, VectorValue: {VectorValue}, FloatValue: {FloatValue}";
        }
    }
}
