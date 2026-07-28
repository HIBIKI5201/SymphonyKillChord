using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     敵生成用のタイマーのインタフェース。
    /// </summary>
    public interface IEnemyWaveTimerView
    {
        /// <summary>
        ///     タイマーを設定する。
        /// </summary>
        /// <param name="time"></param>
        public void SetTimer(float time);
        /// <summary>
        ///     タイマーを停止する。
        /// </summary>
        public void StopTimer();

        /// <summary>
        ///     外部のゲームプレイ開始やタイムアウトによるWave自動生成を抑制します。
        /// </summary>
        /// <param name="suppressed"> 抑制する場合はtrueです。 </param>
        public void SetAutoSpawnSuppressed(bool suppressed);
    }
}
