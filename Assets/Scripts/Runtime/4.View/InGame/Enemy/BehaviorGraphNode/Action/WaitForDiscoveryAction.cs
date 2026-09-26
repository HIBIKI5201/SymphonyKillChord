using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Action
{
    /// <summary>
    ///     敵がプレイヤーを発見するまで待機させる Behavior Graph のアクションノード。
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "WaitForDiscovery", story: "プレイヤーを発見するまで待機する [State]", category: "Action", id: "7d4c2f91a8b6403e9f1a2b3c4d5e6f70")]
    public partial class WaitForDiscoveryAction : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<EnemyStateFacade> State;

        /// <summary>
        ///     参照を確認して待機を開始する。参照が無い場合は失敗を返す。
        /// </summary>
        protected override Unity.Behavior.Node.Status OnStart()
        {
            if (State?.Value?.gameObject == null)
            {
                return Unity.Behavior.Node.Status.Failure;
            }
            return Unity.Behavior.Node.Status.Running;
        }

        /// <summary>
        ///     発見するまで実行中を返す。
        ///     硬直が発生した場合は失敗を返し、Selector に硬直分岐を選び直させる。
        /// </summary>
        protected override Unity.Behavior.Node.Status OnUpdate()
        {
            // 硬直が発生した場合、この待機を中断してSelectorに評価をやり直させ、硬直分岐へ切り替える
            if (State.Value.IsStunned)
            {
                return Unity.Behavior.Node.Status.Failure;
            }

            if (State.Value.IsDiscovered)
            {
                return Unity.Behavior.Node.Status.Success;
            }

            if (State.Value.IsPlayerDiscoverable)
            {
                State.Value.Discover();
                return Unity.Behavior.Node.Status.Success;
            }

            // 発見できていない間は、その場で周囲を見回して索敵を続ける
            State.Value.LookAround();
            return Unity.Behavior.Node.Status.Running;
        }

        /// <summary>
        ///     ノード終了時の処理。現在は何もしない。
        /// </summary>
        protected override void OnEnd()
        {
        }
    }
}
