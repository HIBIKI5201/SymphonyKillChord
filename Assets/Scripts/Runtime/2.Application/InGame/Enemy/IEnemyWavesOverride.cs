using KillChord.Runtime.Domain.InGame.Enemy;

namespace KillChord.Runtime.Application.InGame.Enemy
{
    /// <summary>
    ///     ステージの敵Wave定義の代わりに使うWaveを渡す契約です。
    ///     <para> ServiceLocatorに登録されている間だけ、インゲームの敵Waveを差し替えます。 </para>
    /// </summary>
    public interface IEnemyWavesOverride
    {
        /// <summary>
        ///     差し替えに使う敵Waveを生成します。
        ///     <para> Waveは進行状態を持つため、呼ばれるたびに新しいインスタンスを返します。 </para>
        /// </summary>
        /// <returns> 差し替える敵Waveです。差し替えない場合はnullです。 </returns>
        EnemyWaves CreateEnemyWaves();
    }
}
