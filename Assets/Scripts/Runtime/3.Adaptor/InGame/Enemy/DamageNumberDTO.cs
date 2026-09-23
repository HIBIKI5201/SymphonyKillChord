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
        /// <param name="isCritical">クリティカルかどうか。</param>
        public DamageNumberDTO(float damage, DamageNumberType type, bool isCritical)
        {
            Damage = damage;
            Type = type;
            IsCritical = isCritical;
        }

        /// <summary> ダメージ数値。 </summary>
        public readonly float Damage { get; }

        /// <summary> ダメージ数値の種類。 </summary>
        public readonly DamageNumberType Type { get; }

        /// <summary> クリティカルかどうか。 </summary>
        public readonly bool IsCritical { get; }
    }
}
