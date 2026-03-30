using System;

namespace KillChord.Runtime.Domain.InGame.Character
{
    [Serializable]
    public readonly struct MoveSpeed
    {
        public MoveSpeed(float value)
        {
            Value = value;
        }

        public readonly float Value;

        public static explicit operator float(MoveSpeed value)
            => value.Value;
        public static bool operator ==(MoveSpeed left, MoveSpeed right)
            => left.Value == right.Value;
        public static bool operator !=(MoveSpeed left, MoveSpeed right)
            => left.Value != right.Value;
        public override bool Equals(object obj)
        {
            return obj is MoveSpeed moveSpeed && moveSpeed.Value == Value;
        }
        public bool Equals(MoveSpeed other)
        {
            return other.Value == Value;
        }
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
