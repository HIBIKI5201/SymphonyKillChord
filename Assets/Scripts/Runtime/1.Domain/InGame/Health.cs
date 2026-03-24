namespace KillChord.Runtime.Domain
{
    public readonly struct Health
    {
        public Health(float value)
        {
            Value = value;
        }

        public static explicit operator float(Health health) => health.Value;

        public readonly float Value;
    }
}