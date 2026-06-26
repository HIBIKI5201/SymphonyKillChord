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
    }
}
