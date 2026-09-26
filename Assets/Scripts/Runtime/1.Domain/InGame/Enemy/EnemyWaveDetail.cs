namespace KillChord.Runtime.Domain.InGame.Enemy
{
    /// <summary>
    ///     敵の1Waveの定義詳細。
    /// </summary>
    public readonly struct EnemyWaveDetail
    {
        /// <summary>
        ///     個別の敵定義IDと生成数で初期化します。
        /// </summary>
        /// <param name="enemyDefinitionId"> 生成する個別の敵定義IDです。 </param>
        /// <param name="amount"> 生成数です。 </param>
        public EnemyWaveDetail(EnemyDefinitionId enemyDefinitionId, int amount)
        {
            EnemyDefinitionId = enemyDefinitionId;
            EnemyAmount = amount;
        }

        /// <summary> 生成する個別の敵定義IDです。 </summary>
        public readonly EnemyDefinitionId EnemyDefinitionId;
        /// <summary> 敵の数 </summary>
        public readonly int EnemyAmount;
    }
}
