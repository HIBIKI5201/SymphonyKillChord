using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using System;
using Unity.Behavior;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Condition
{
    /// <summary>
    ///     敵がプレイヤーを発見済みかを判定する Behavior Graph の条件ノード。
    /// </summary>
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "IsDiscovered", story: "プレイヤーを発見済みである [Bool] [State]", category: "Conditions", id: "b3f8a1d4e2c9407fa6b1c8d3e5f70a92")]
    public partial class IsDiscoveredCondition : Unity.Behavior.Condition
    {
        [SerializeReference] public BlackboardVariable<EnemyStateFacade> State;
        [SerializeReference] public BlackboardVariable<bool> Bool;

        /// <summary>
        ///     発見済みかが Bool の値と一致するかを返す。
        /// </summary>
        public override bool IsTrue()
        {
            if (State?.Value == null) return false;
            return State.Value.IsDiscovered == Bool.Value;
        }

        /// <summary>
        ///     条件評価の開始時の処理。現在は何もしない。
        /// </summary>
        public override void OnStart()
        {
        }

        /// <summary>
        ///     条件評価の終了時の処理。現在は何もしない。
        /// </summary>
        public override void OnEnd()
        {
        }
    }
}
