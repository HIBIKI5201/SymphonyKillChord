using KillChord.Runtime.Domain.InGame.Battle;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Sequence
{
    /// <summary>
    ///     ポーズ中とポーズ解除のカウントダウン中に止まる、戦闘の現在時刻の取得元を作る。
    ///     スキルのクールダウンと、その表示で同じ時刻を使うために共有する。
    /// </summary>
    public static class BattlePlayTimeProvider
    {
        /// <summary>
        ///     戦闘の時計から現在時刻を返す関数を作る。時計が無い場合は実時間を返す。
        /// </summary>
        /// <returns> 現在時刻（秒）を返す関数。 </returns>
        public static Func<float> Create()
        {
            PausableClock playClock =
                ServiceLocator.TryGetInstance(out SequenceModuleContainer sequenceModuleContainer)
                    ? sequenceModuleContainer.PlayClock
                    : null;
            if (playClock == null)
            {
                Debug.LogWarning($"[{nameof(BattlePlayTimeProvider)}] 戦闘の時計が見つからないため、ポーズ中もクールダウンが進みます。");
                return () => Time.unscaledTime;
            }

            return () => playClock.GetTime(Time.unscaledTime);
        }
    }
}
