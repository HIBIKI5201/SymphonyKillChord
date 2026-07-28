using System;

namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     ミッションの経過時間を管理するクラス。
    ///     ミッション開始からの経過時間を追跡し、必要に応じて更新することができます。
    /// </summary>
    public readonly struct MissionElapsedTime : IEquatable<MissionElapsedTime>
    {
        /// <summary>
        ///     MissionElapsedTime 構造体の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="second">秒数。</param>
        public MissionElapsedTime(float second)
        {
            if (float.IsNaN(second) || float.IsInfinity(second))
            {
                throw new ArgumentOutOfRangeException(
                nameof(second),
                "second must be finite.");
            }

            _value = second < 0f ? 0f : second;
        }

        /// <summary> 経過時間を取得します。 </summary>
        public float Value => _value;

        /// <summary>
        ///     指定した時間を経過しているかどうかを判定します。
        /// </summary>
        /// <param name="targetTime">対象の時間。</param>
        /// <returns>経過している場合は true、そうでない場合は false。</returns>
        public bool IsOver(float targetTime)
        {
            return _value >= targetTime;
        }

        /// <summary>
        ///     時間を進めます。
        /// </summary>
        /// <param name="deltaTime">進める時間。</param>
        /// <returns>新しい経過時間。</returns>
        public MissionElapsedTime AdvanceTime(float deltaTime)
        {
            if (deltaTime < 0f || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime))
            {
                throw new ArgumentOutOfRangeException(
                nameof(deltaTime),
                "deltaTime must be non-negative and finite.");
            }

            return new MissionElapsedTime(_value + deltaTime);
        }

        /// <summary>
        ///     他のオブジェクトと等しいかどうかを判定します。
        /// </summary>
        /// <param name="other">比較対象のオブジェクト。</param>
        /// <returns>等しい場合は true、そうでない場合は false。</returns>
        public bool Equals(MissionElapsedTime other)
        {
            return _value.Equals(other._value);
        }

        /// <summary> 経過時間。 </summary>
        private readonly float _value;
    }
}
