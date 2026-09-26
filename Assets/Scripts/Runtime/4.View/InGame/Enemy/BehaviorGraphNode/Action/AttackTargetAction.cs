using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Action
{
    /// <summary>
    ///     敵にターゲットへの攻撃を行わせる Behavior Graph のアクションノード。
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "AttackTarget", story: "対象を攻撃する [Battle] [State] [Movement]", category: "Action", id: "611c230a6a1f2c1d944d9d2cf1c3a297")]
    public partial class AttackTargetAction : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<EnemyBattleAIFacade> Battle;
        [SerializeReference] public BlackboardVariable<EnemyStateFacade> State;
        [SerializeReference] public BlackboardVariable<EnemyMovementAIFacade> Movement;

        /// <summary> 同じ方向へ横移動する時間の下限（秒）。 </summary>
        private const float MIN_STRAFE_DURATION = 0.6f;
        /// <summary> 同じ方向へ横移動する時間の上限（秒）。 </summary>
        private const float MAX_STRAFE_DURATION = 1.4f;
        private int _strafeDirection;
        private float _remainingStrafeTime;

        /// <summary>
        ///     攻撃を予約し、待機中の横移動方向を初期化する。
        /// </summary>
        protected override Unity.Behavior.Node.Status OnStart()
        {
            if (Battle?.Value == null || State?.Value == null || Movement?.Value == null)
            {
                return Unity.Behavior.Node.Status.Failure;
            }
            SelectStrafeDirection();
            Battle.Value.StartAttack();
            return Unity.Behavior.Node.Status.Running;
        }

        /// <summary>
        ///     予約中の射程と射線を確認し、Graphから横移動を指示する。
        /// </summary>
        protected override Unity.Behavior.Node.Status OnUpdate()
        {
            if (!State.Value.IsAttacking)
            {
                return Unity.Behavior.Node.Status.Success;
            }

            // 障害物で止まっていても、時間満了までは左右を再抽選しない。
            _remainingStrafeTime -= Time.deltaTime;
            if (_remainingStrafeTime <= 0f)
            {
                SelectStrafeDirection();
            }

            if (State.Value.IsTargetInAttackRange && State.Value.IsSightClearToAim)
            {
                Movement.Value.StrafeWhileWaiting(_strafeDirection);
            }
            else
            {
                Movement.Value.StopStrafing();
            }
            return Unity.Behavior.Node.Status.Running;
        }

        /// <summary>
        ///     完了または中断時に、このノードが指示した横移動を終了する。
        /// </summary>
        protected override void OnEnd()
        {
            Movement?.Value?.StopStrafing();
            _remainingStrafeTime = 0f;
        }

        /// <summary>
        ///     次に進む方向と、その方向を維持する時間を抽選する。
        /// </summary>
        private void SelectStrafeDirection()
        {
            _strafeDirection = UnityEngine.Random.value < 0.5f ? -1 : 1;
            _remainingStrafeTime = UnityEngine.Random.Range(MIN_STRAFE_DURATION, MAX_STRAFE_DURATION);
        }
    }
}
