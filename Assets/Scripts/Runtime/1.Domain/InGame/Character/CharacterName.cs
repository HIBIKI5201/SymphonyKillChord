using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Character
{
    public readonly struct CharacterName
    {
        public CharacterName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                { throw new ArgumentException("name must not be null or empty.", nameof(value)); }

            Value = value;
        }

        public readonly string Value;
    }
}
