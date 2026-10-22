using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     ボスAI用ファサード：状態判定系。
    ///     ボスはスタンしない仕様のため、硬直系のメンバーは何もしない。
    /// </summary>
    public class BossStateFacade : StateFacadeBase
    {
        /// <summary> 硬直中か。ボスはスタンしないため常に false。 </summary>
        public override bool IsStunned => false;

        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="aiController"> ボスのAIコントローラー。 </param>
        /// <param name="target"> 攻撃対象。 </param>
        /// <param name="raycastDetectView"> 射線判定に使う View。 </param>
        public void Initialize(BossAIController aiController, Transform target, EnemyRaycastDetectView raycastDetectView)
        {
            InitializeCommon(aiController, target, raycastDetectView);
        }

        /// <summary>
        ///     硬直発生。ボスはスタンしないため何もしない。
        /// </summary>
        public override void Stunned()
        {
        }

        /// <summary>
        ///     硬直回復。ボスはスタンしないため何もしない。
        /// </summary>
        public override void StunRecover()
        {
        }
    }
}
