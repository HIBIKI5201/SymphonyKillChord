namespace KillChord.Runtime.Domain
{
    public readonly struct CriticalDamage
    {
        /// <summary>
        ///     クリティカルダメージ倍率のインスタンスを初期化する。
        /// </summary>
        public CriticalDamage(float value)
        {
            _value = value;
        }

        /// <summary> クリティカルダメージ倍率。 </summary>
        public float Value => _value;

        private readonly float _value;
    }
}
