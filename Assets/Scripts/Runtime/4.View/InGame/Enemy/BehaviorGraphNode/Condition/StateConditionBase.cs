using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using System;
using Unity.Behavior;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Condition
{
    /// <summary>
    ///     状態ファサードの値が Bool と一致するかを判定する、敵・ボス共通の条件ノードの基底。
    ///     派生クラスは判定に使う値の読み出しだけを実装する。
    /// </summary>
    /// <typeparam name="TState"> Blackboard に置く状態ファサードの型。 </typeparam>
    [Serializable]
    public abstract class StateConditionBase<TState> : Unity.Behavior.Condition
        where TState : StateFacadeBase
    {
        [SerializeReference] public BlackboardVariable<TState> State;
        [SerializeReference] public BlackboardVariable<bool> Bool;

        /// <summary>
        ///     状態の値が Bool の値と一致するかを返す。参照が無い場合は false を返す。
        /// </summary>
        public override bool IsTrue()
        {
            if (State?.Value == null || Bool == null) return false;
            return ReadState(State.Value) == Bool.Value;
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

        /// <summary>
        ///     判定に使う状態の値を読み出す。
        /// </summary>
        /// <param name="state"> 状態ファサード。 </param>
        /// <returns> 判定に使う値。 </returns>
        protected abstract bool ReadState(TState state);
    }
}
