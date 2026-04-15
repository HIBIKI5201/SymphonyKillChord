using System;
using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Character
{
    /// <summary>
    ///     キャラクターの名前を表す値オブジェクト。
    /// </summary>
    public readonly struct CharacterName
    {
        public CharacterName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                {
                throw new ArgumentException("name must not be null or empty.", nameof(value));
            }

            _value = value;
        }

        public string Value => _value;

        private readonly string _value;
    }
}
