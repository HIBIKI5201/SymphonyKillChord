namespace KillChord.Runtime.Domain.InGame.Enemy
{
    /// <summary>
    ///     敵の1Waveの定義詳細。
    /// </summary>
    public readonly struct EnemyWaveDetail
    {
        public EnemyWaveDetail(EnemyType enemyType, int amount)
        {
            EnemyType = enemyType;
            EnemyAmount = amount;
        }

        /// <summary> 敵種類 </summary>
        public readonly EnemyType EnemyType;
        /// <summary> 敵の数 </summary>
        public readonly int EnemyAmount;
    }
}
