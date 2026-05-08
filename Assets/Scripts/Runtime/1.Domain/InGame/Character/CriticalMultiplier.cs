using System;

namespace KillChord.Runtime.Domain.InGame.Character
{
    /// <summary>
    ///     クリティカルダメージ倍率を表す構造体。
    /// </summary>
    public readonly struct CriticalMultiplier
    {
        /// <summary>
        ///     クリティカルダメージ倍率を初期化するコンストラクタ。
        /// </summary>
        /// <param name="value"></param>
        public CriticalMultiplier(float value)
        {
            if (value <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "CriticalMultiplierは0以上である必要があります。");
            }


            _value = value;
        }

        /// <summary> クリティカルダメージ倍率を取得する。 </summary>
        public float Value => _value;

        private readonly float _value;
    }
}
