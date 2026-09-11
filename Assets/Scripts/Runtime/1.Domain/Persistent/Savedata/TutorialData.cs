using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.Persistent.Savedata
{
    /// <summary>
    ///    プレイヤーのチュートリアル進行状況のセーブデータを表すクラス。
    ///    チュートリアルを完了している場合、プレイヤーはチュートリアルをスキップできるようになる。
    /// </summary>
    [Serializable]
    public sealed class TutorialData
    {
        /// <summary>
        ///     プレイヤーがチュートリアルを完了したかどうかを示すプロパティ。
        /// </summary>
        public bool IsTutorialCompleted => Phase == TutorialPhase.Completed;

        /// <summary> 現在のチュートリアル進行段階です。 </summary>
        public TutorialPhase Phase => _isTutorialCompleted ? TutorialPhase.Completed : _phase;

        /// <summary>
        ///     オープニングシナリオを完了済みにします。
        /// </summary>
        /// <returns> 状態が変化した場合はtrueです。 </returns>
        public bool CompleteOpeningScenario()
        {
            return AdvanceTo(TutorialPhase.OpeningScenarioCompleted);
        }

        /// <summary>
        ///     チュートリアル戦闘を完了済みにします。
        /// </summary>
        /// <returns> 状態が変化した場合はtrueです。 </returns>
        public bool CompleteBattle()
        {
            return AdvanceTo(TutorialPhase.BattleCompleted);
        }

        /// <summary>
        ///     ホームチュートリアルを開始済みにします。
        /// </summary>
        /// <returns> 状態が変化した場合はtrueです。 </returns>
        public bool StartHome()
        {
            return AdvanceTo(TutorialPhase.HomeStarted);
        }

        /// <summary>
        ///     チュートリアルを完了済みにします。
        /// </summary>
        /// <returns> 状態が変化した場合はtrueです。 </returns>
        public bool Complete()
        {
            return AdvanceTo(TutorialPhase.Completed);
        }

        [SerializeField, Tooltip("チュートリアルの進行段階")]
        private TutorialPhase _phase = TutorialPhase.NotStarted;

        [SerializeField, Tooltip("プレイヤーがチュートリアルを完了したかどうか")]
        private bool _isTutorialCompleted = false;

        /// <summary>
        ///     チュートリアル進行段階を前方へだけ進めます。
        /// </summary>
        /// <param name="nextPhase"> 遷移先の進行段階です。 </param>
        /// <returns> 状態が変化した場合はtrueです。 </returns>
        private bool AdvanceTo(TutorialPhase nextPhase)
        {
            TutorialPhase currentPhase = Phase;
            if (nextPhase <= currentPhase)
            {
                return false;
            }

            _phase = nextPhase;
            _isTutorialCompleted = nextPhase == TutorialPhase.Completed;
            return true;
        }
    }
}
