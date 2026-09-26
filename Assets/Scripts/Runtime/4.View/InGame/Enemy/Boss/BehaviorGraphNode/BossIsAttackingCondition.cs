using System;
using Unity.Behavior;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     ボスが攻撃中かを判定する Behavior Graph の条件ノード。
    /// </summary>
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "BossIsAttacking", story: "攻撃中である(Boss) [Bool] [State]", category: "Conditions/Boss", id: "07b6c8d9eaeb716345ab78cd9aef1c67")]
    public partial class BossIsAttackingCondition : Unity.Behavior.Condition
    {
        [SerializeReference] public BlackboardVariable<BossStateFacade> State;
        [SerializeReference] public BlackboardVariable<bool> Bool;

        /// <summary>
        ///     攻撃中かが Bool の値と一致するかを返す。
        /// </summary>
        public override bool IsTrue()
        {
            if (State?.Value == null) return false;
            return State.Value.IsAttacking == Bool.Value;
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
