using KillChord.Runtime.View.InGame.Enemy.AIFacade;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     ボスAI用ファサード：戦闘系。
    /// </summary>
    public class BossBattleAIFacade : BattleAIFacadeBase
    {
        /// <summary>
        ///     指示：被弾硬直アニメーションを開始する。
        ///     ボスはスタンしない仕様のため何もしない。
        /// </summary>
        public override void StartStunAnimation()
        {
        }
    }
}
