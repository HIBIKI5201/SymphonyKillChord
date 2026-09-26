using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using System;
using Unity.Behavior;
using UnityEngine;
namespace KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Condition
{
    /// <summary>
    ///     敵の戦闘 AI が有効かを判定する Behavior Graph の条件ノード。
    /// </summary>
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "IsBattleAiActivated", story: "敵の戦闘AIが有効 [Bool] [State]", category: "Conditions", id: "e481ef1704416ac2b10bcdd0903055fe")]
    public partial class IsBattleAiActivatedCondition : Unity.Behavior.Condition
    {
        [SerializeReference] public BlackboardVariable<EnemyStateFacade> State;
        [SerializeReference] public BlackboardVariable<bool> Bool;


        /// <summary>
        ///     戦闘 AI が有効かが Bool の値と一致するかを返す。
        /// </summary>
        public override bool IsTrue()
        {
            return State.Value.IsBattleAiActivated == Bool.Value;
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