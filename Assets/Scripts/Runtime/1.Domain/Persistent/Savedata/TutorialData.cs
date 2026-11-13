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
        ///     初めて開始したときは、研究・改造・作戦の各画面の初回の案内を出す状態にします。
        /// </summary>
        /// <returns> 状態が変化した場合はtrueです。 </returns>
        public bool StartHome()
        {
            if (!AdvanceTo(TutorialPhase.HomeStarted))
            {
                return false;
            }

            // この機能より前にチュートリアルを終えたセーブデータには案内を出さないため、ここでだけ立てる。
            _pendingScreenGuides = ScreenGuideKind.All;
            return true;
        }

        /// <summary>
        ///     チュートリアルを完了済みにします。
        /// </summary>
        /// <returns> 状態が変化した場合はtrueです。 </returns>
        public bool Complete()
        {
            return AdvanceTo(TutorialPhase.Completed);
        }

        /// <summary>
        ///     指定した画面の初回の案内がまだ出ていないかを返します。
        /// </summary>
        /// <param name="kind"> 確かめる画面です。 </param>
        /// <returns> 案内を出す必要がある場合はtrueです。 </returns>
        public bool HasPendingScreenGuide(ScreenGuideKind kind)
        {
            return kind != ScreenGuideKind.None && (_pendingScreenGuides & kind) == kind;
        }

        /// <summary>
        ///     指定した画面の初回の案内を出し終えたことにします。
        /// </summary>
        /// <param name="kind"> 案内を出し終えた画面です。 </param>
        /// <returns> 状態が変化した場合はtrueです。 </returns>
        public bool CompleteScreenGuide(ScreenGuideKind kind)
        {
            if (!HasPendingScreenGuide(kind))
            {
                return false;
            }

            _pendingScreenGuides &= ~kind;
            return true;
        }

        [SerializeField, Tooltip("チュートリアルの進行段階")]
        private TutorialPhase _phase = TutorialPhase.NotStarted;

        [SerializeField, Tooltip("プレイヤーがチュートリアルを完了したかどうか")]
        private bool _isTutorialCompleted = false;

        [SerializeField, Tooltip("まだ初回の案内を出していない画面")]
        private ScreenGuideKind _pendingScreenGuides = ScreenGuideKind.None;

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
