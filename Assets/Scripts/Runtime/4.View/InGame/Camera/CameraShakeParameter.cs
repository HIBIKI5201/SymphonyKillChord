using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Camera
{
    /// <summary>
    ///     カメラシェイク1回分の揺れ方を定義するパラメータ。
    ///     LitMotionのPunchモーションの設定値として使用する。
    /// </summary>
    [Serializable]
    public struct CameraShakeParameter
    {
        /// <summary>
        ///     各パラメータを指定してカメラシェイク設定を生成するコンストラクタ。
        /// </summary>
        /// <param name="duration"> 揺れの継続時間(秒)。</param>
        /// <param name="strengthRangeX"> X(左右)方向の振動の強さの抽選範囲。xが最小値、yが最大値。</param>
        /// <param name="strengthRangeY"> Y(上下)方向の振動の強さの抽選範囲。xが最小値、yが最大値。</param>
        /// <param name="strengthRangeZ"> Z(前後)方向の振動の強さの抽選範囲。xが最小値、yが最大値。</param>
        /// <param name="frequency"> 継続時間中の振動回数。</param>
        /// <param name="dampingRatio"> 振動の減衰比。1で完全に減衰し、0で減衰しない。</param>
        public CameraShakeParameter(
            int priority,
            float duration,
            Vector2 strengthRangeX,
            Vector2 strengthRangeY,
            Vector2 strengthRangeZ,
            Vector2 frequency,
            float dampingRatio)
        {
            _priority = priority;
            _duration = duration;
            _strengthRangeX = strengthRangeX;
            _strengthRangeY = strengthRangeY;
            _strengthRangeZ = strengthRangeZ;
            _frequency = frequency;
            _dampingRatio = dampingRatio;
        }
        /// <summary>優先順位（高い数字ほど優先）</summary>
        public readonly int Priority => _priority;

        /// <summary> 揺れの継続時間(秒)。 </summary>
        public readonly float Duration => _duration;

        /// <summary> X(左右)方向の振動の強さの抽選範囲。xが最小値、yが最大値。 </summary>
        public readonly Vector2 StrengthRangeX => _strengthRangeX;

        /// <summary> Y(上下)方向の振動の強さの抽選範囲。xが最小値、yが最大値。 </summary>
        public readonly Vector2 StrengthRangeY => _strengthRangeY;

        /// <summary> Z(前後)方向の振動の強さの抽選範囲。xが最小値、yが最大値。</summary>
        public readonly Vector2 StrengthRangeZ => _strengthRangeZ;

        /// <summary> 継続時間中の振動回数の抽選は範囲。xが最小値、yが最大値。 </summary>
        public readonly Vector2 Frequency => _frequency;

        /// <summary> 振動の減衰比。1で完全に減衰し、0で減衰しない。 </summary>
        public readonly float DampingRatio => _dampingRatio;

        [SerializeField,Tooltip("エフェクト実行の優先順位（高い数字ほど優先）")]
        private int _priority;

        [Min(0f)]
        [SerializeField, Tooltip("揺れの継続時間(秒)。0以下の場合はシェイクしません。")]
        private float _duration;

        [SerializeField, Tooltip("X(左右)方向の振動の強さ(メートル)の抽選範囲。x=最小値, y=最大値。")]
        private Vector2 _strengthRangeX;

        [SerializeField, Tooltip("Y(上下)方向の振動の強さ(メートル)の抽選範囲。x=最小値, y=最大値。")]
        private Vector2 _strengthRangeY;

        [SerializeField, Tooltip("Z(前後)方向の振動の強さ(メートル)の抽選範囲。x=最小値, y=最大値。")]
        private Vector2 _strengthRangeZ;

        [SerializeField, Tooltip("継続時間中の振動回数の抽選範囲（小数は四捨五入）。大きいほど細かく振動します。")]
        private Vector2 _frequency;

        [Range(0f, 1f)]
        [SerializeField, Tooltip("振動の減衰比。1で完全に減衰し、0で減衰せず一定の強さで振動します。")]
        private float _dampingRatio;

        /// <summary>
        ///     抽選範囲の絶対値の最大を返す。
        /// </summary>
        /// <param name="range"> 抽選範囲。xが最小値、yが最大値。</param>
        /// <returns> 範囲内で取り得る強さの絶対値の最大。</returns>
        private static float GetRangeMaxAbsolute(in Vector2 range)
        {
            return Mathf.Max(Mathf.Abs(range.x), Mathf.Abs(range.y));
        }
    }
}
