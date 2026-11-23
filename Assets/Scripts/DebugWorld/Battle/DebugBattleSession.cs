using KillChord.Runtime.Application.InGame.Enemy;
using KillChord.Runtime.Application.Persistent.Savedata;
using SymphonyFramework.Services;
using System;

namespace KillChord.DebugWorld.Battle
{
    /// <summary>
    ///     裏世界の戦闘設定をServiceLocatorへ登録・解除するクラスです。
    /// </summary>
    public static class DebugBattleSession
    {
        /// <summary>
        ///     戦闘設定を登録し、次のインゲームで敵Waveの差し替えとセーブの停止を有効にします。
        /// </summary>
        /// <param name="overrides"> 登録する戦闘設定です。 </param>
        public static void Begin(DebugBattleOverrides overrides)
        {
            if (overrides == null) { throw new ArgumentNullException(nameof(overrides)); }

            End();
            Partitura.Register<IEnemyWavesOverride>(overrides, ServiceLifetimeEnum.Persistent);
            Partitura.Register<ISaveSuppression>(overrides, ServiceLifetimeEnum.Persistent);
        }

        /// <summary>
        ///     登録した戦闘設定を解除し、通常のゲームに戻します。
        /// </summary>
        public static void End()
        {
            if (Partitura.TryGet(out IEnemyWavesOverride wavesOverride)
                && wavesOverride is DebugBattleOverrides)
            {
                Partitura.Unregister<IEnemyWavesOverride>();
            }

            if (Partitura.TryGet(out ISaveSuppression saveSuppression)
                && saveSuppression is DebugBattleOverrides)
            {
                Partitura.Unregister<ISaveSuppression>();
            }
        }
    }
}
