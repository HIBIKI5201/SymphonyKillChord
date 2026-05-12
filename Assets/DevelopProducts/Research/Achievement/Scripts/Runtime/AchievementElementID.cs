using System;
using UnityEngine;

namespace DevelopProducts.Achievement
{
    public readonly struct AchievementElementID : IEquatable<AchievementElementID>
    {
        public AchievementElementID(string value)
        {
            _value = value;
        }

        public bool Equals(AchievementElementID other) => _value == other._value;

        private readonly string _value;
    }
}
