using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Character
{
    public readonly struct CharacterName
    {
        public CharacterName(string value)
        {
            Value = value;
        }

        public readonly string Value;
    }
}
