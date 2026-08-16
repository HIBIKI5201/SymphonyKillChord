namespace KillChord.Runtime.Utility.Persistent
{
    /// <summary>
    ///     イベント定義：プレイヤーがダメージを受けた時
    /// </summary>
    public readonly struct EOnPlayerTakeDamage : IEvent
    {
        /// <summary> 受けたダメージ量(正の値)。 </summary>
        public readonly float Damage;

        /// <summary>
        ///     受けたダメージ量を指定してイベントを生成するコンストラクタ。
        /// </summary>
        /// <param name="damage"> 受けたダメージ量(正の値)。</param>
        public EOnPlayerTakeDamage(float damage)
        {
            Damage = damage;
        }
    }
}
