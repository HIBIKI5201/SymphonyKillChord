using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using System;
using Unity.Behavior;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Action
{
    /// <summary>
    ///     ターゲットへの攻撃を行わせる、敵・ボス共通のアクションノードの基底。
    ///     攻撃を予約し、攻撃が終わるまで実行中を返す。攻撃待機中の動きは派生クラスで足す。
    /// </summary>
    /// <typeparam name="TBattle"> Blackboard に置く戦闘ファサードの型。 </typeparam>
    /// <typeparam name="TState"> Blackboard に置く状態ファサードの型。 </typeparam>
    [Serializable]
    public abstract class AttackTargetActionBase<TBattle, TState> : Unity.Behavior.Action
        where TBattle : BattleAIFacadeBase
        where TState : StateFacadeBase
    {
        [SerializeReference] public BlackboardVariable<TBattle> Battle;
        [SerializeReference] public BlackboardVariable<TState> State;

        /// <summary>
        ///     攻撃を開始する。参照が無い場合は失敗を返す。
        /// </summary>
        protected override Unity.Behavior.Node.Status OnStart()
        {
            if (Battle?.Value == null || State?.Value == null || !HasExtraReferences())
            {
                return Unity.Behavior.Node.Status.Failure;
            }
            BeginWaiting();
            Battle.Value.StartAttack();
            return Unity.Behavior.Node.Status.Running;
        }

        /// <summary>
        ///     攻撃中は実行中を返し、攻撃が終わったら成功を返す。
        /// </summary>
        protected override Unity.Behavior.Node.Status OnUpdate()
        {
            if (!State.Value.IsAttacking)
            {
                return Unity.Behavior.Node.Status.Success;
            }

            UpdateWaiting();
            return Unity.Behavior.Node.Status.Running;
        }

        /// <summary>
        ///     完了または中断時に、攻撃待機中の動きを終了する。
        /// </summary>
        protected override void OnEnd()
        {
            EndWaiting();
        }

        /// <summary>
        ///     派生クラスが追加で使う参照がそろっているかを返す。
        /// </summary>
        /// <returns> そろっていれば true。 </returns>
        protected virtual bool HasExtraReferences()
        {
            return true;
        }

        /// <summary>
        ///     攻撃の予約直前に、待機中の動きを初期化する。
        /// </summary>
        protected virtual void BeginWaiting()
        {
        }

        /// <summary>
        ///     攻撃中の毎フレーム、待機中の動きを更新する。
        /// </summary>
        protected virtual void UpdateWaiting()
        {
        }

        /// <summary>
        ///     ノード終了時に、待機中の動きを終了する。
        /// </summary>
        protected virtual void EndWaiting()
        {
        }
    }
}
