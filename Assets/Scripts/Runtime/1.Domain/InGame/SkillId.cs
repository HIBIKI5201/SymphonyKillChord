using System;
using UnityEngine;

namespace KillChord.Runtime.Domain
{
    public readonly struct SkillId : IEquatable<SkillId>
    {
        public int Value => _value;
        private readonly int _value;

        public SkillId(int value)
        {
            _value = value;
        }

        public static bool operator ==(SkillId left, SkillId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SkillId left, SkillId right)
        {
            return !(left == right);
        }

        public bool Equals(SkillId other)
        {
            return _value == other._value;
        }

        public override bool Equals(object obj)
        {
            return obj is SkillId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _value;
        }
    }
}