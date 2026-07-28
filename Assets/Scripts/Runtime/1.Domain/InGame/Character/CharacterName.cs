using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Character
{
    /// <summary>
    ///     キャラクターの名前を表す値オブジェクト。
    /// </summary>
    public readonly struct CharacterName
    {
        /// <summary>
        ///     キャラクター名を初期化するコンストラクタ。
        /// </summary>
        /// <param name="value"></param>
        public CharacterName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                {
                throw new ArgumentException("name must not be null or empty.", nameof(value));
            }

            _value = value;
        }

        /// <summary> 名前を取得する。 </summary>
        public string Value => _value;

        private readonly string _value;
    }
}
