using System;
using Unity.Behavior;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     ボスの照準の射線が通っているかを判定する Behavior Graph の条件ノード。
    /// </summary>
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "BossIsAimSightClear", story: "敵との間に遮蔽物がない(Boss) [Bool] [State]", category: "Conditions/Boss", id: "18c7d9eafbfc827456bc89dea0fb2d78")]
    public partial class BossIsAimSightClearCondition : Unity.Behavior.Condition
    {
        [SerializeReference] public BlackboardVariable<BossStateFacade> State;
        [SerializeReference] public BlackboardVariable<bool> Bool;

        /// <summary>
        ///     射線が通っているかが Bool の値と一致するかを返す。
        /// </summary>
        public override bool IsTrue()
        {
            if (State?.Value == null) return false;
            return State.Value.IsSightClearToAim == Bool.Value;
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
