namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     ダメージ数値を表示するためのDTO。
    /// </summary>
    public readonly struct DamageNumberDTO
    {
        /// <summary>
        ///     ダメージ数値を表示するためのDTOを作成する。
        /// </summary>
        /// <param name="damage">ダメージ数値。</param>
        /// <param name="type">ダメージ数値の種類。</param>
        public DamageNumberDTO(float damage, DamageNumberType type)
        {
            Damage = damage;
            Type = type;
        }

        /// <summary> ダメージ数値。 </summary>
        public readonly float Damage { get; }

        /// <summary> ダメージ数値の種類。 </summary>
        public readonly DamageNumberType Type { get; }
    }
}
