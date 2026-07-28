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
        public DamageNumberDTO(float damage)
        {
            Damage = damage;
        }

        /// <summary> ダメージ数値。 </summary>
        public readonly float Damage { get; }
    }
}
