namespace KillChord.Runtime.Domain
{
    /// <summary>
    ///     停止距離を表す値オブジェクト。
    /// </summary>
    public readonly struct AttackRange
    {
        public AttackRange(float value)
        {
            Value = value < 0f ? 0f : value;
        }

        public float Value { get; }

        public bool Equals(AttackRange other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is AttackRange other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
