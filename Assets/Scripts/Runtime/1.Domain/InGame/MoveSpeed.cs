namespace KillChord.Runtime.Domain
{
    public readonly struct MoveSpeed
    {
        public MoveSpeed(float value)
        {
            Value = value;
        }

        public readonly float Value;

        public static explicit operator float(MoveSpeed value)
            => value.Value;
    }
}
