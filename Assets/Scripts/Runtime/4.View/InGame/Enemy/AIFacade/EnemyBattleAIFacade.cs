using KillChord.Runtime.Utility.Diagnostics;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy.AIFacade
{
    /// <summary>
    ///     敵AI用ファサード：戦闘系。
    /// </summary>
    [AddComponentMenu("KillChord/View/Enemy/EnemyBattleAIFacade")]
    public class EnemyBattleAIFacade : BattleAIFacadeBase
    {
        /// <summary>
        ///     指示：被弾硬直アニメーションを開始する。
        /// </summary>
        public override void StartStunAnimation()
        {
            DevLog.Log("被弾アニメーション実装待ち");
        }
    }
}
