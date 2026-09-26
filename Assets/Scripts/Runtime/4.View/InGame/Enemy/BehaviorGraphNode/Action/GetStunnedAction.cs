using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Action
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "GetStunned", story: "スタン状態を開始する [State] [Battle]", category: "Action", id: "459e141cce9d40aaaebad1a7c2283299")]
    public partial class GetStunnedAction : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<EnemyStateFacade> State;
        [SerializeReference] public BlackboardVariable<EnemyBattleAIFacade> Battle;

        [SerializeField, Tooltip("硬直解除の直後、体勢を立て直すために動きを止めておく時間(秒)の基準値。")]
        private float _postRecoveryDelaySeconds = 0.3f;
        [SerializeField, Tooltip("上記の基準値に対するランダムな幅の割合。個体ごとに立て直し時間をばらつかせる。")]
        private float _postRecoveryDelayVariance = 0.3f;

        private bool _isRecovering;
        private float _recoveryTimer;
        private float _recoveryDuration;

        protected override Unity.Behavior.Node.Status OnStart()
        {
            if (State?.Value?.gameObject == null || Battle?.Value == null)
            {
                return Unity.Behavior.Node.Status.Failure;
            }

            EnemyStateFacade state = State.Value;
            EnemyBattleAIFacade battle = Battle.Value;
            battle.CancelAttack();
            state.Stunned();
            _isRecovering = false;
            _recoveryTimer = 0f;
            return Unity.Behavior.Node.Status.Running;
        }

        protected override Unity.Behavior.Node.Status OnUpdate()
        {
            if (State.Value.IsStunned) return Unity.Behavior.Node.Status.Running;

            // 硬直解除の瞬間に即座に動き出すと機械的に見えるため、一呼吸だけ立て直す時間を置く
            if (!_isRecovering)
            {
                _isRecovering = true;
                _recoveryTimer = 0f;
                _recoveryDuration = _postRecoveryDelaySeconds
                    * (1f + UnityEngine.Random.Range(-_postRecoveryDelayVariance, _postRecoveryDelayVariance));
            }

            _recoveryTimer += Time.deltaTime;
            if (_recoveryTimer < _recoveryDuration) return Unity.Behavior.Node.Status.Running;

            return Unity.Behavior.Node.Status.Success;
        }

        protected override void OnEnd()
        {
        }
    }
}
