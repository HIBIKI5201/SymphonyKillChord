using KillChord.Runtime.Adaptor.InGame.Sequence;
using KillChord.Runtime.Domain.InGame.Battle;

namespace KillChord.Runtime.Composition.InGame.Sequence
{
    /// <summary>
    ///     シーケンスモジュールの公開物を保持するContainerです。
    /// </summary>
    public sealed class SequenceModuleContainer
    {
        /// <summary> インゲームシーケンス演出です。 </summary>
        public InGameSequenceDirector SequenceDirector { get; set; }
        /// <summary> ポーズ機能のコントローラーです。</summary>
        public BattlePauseController BattlePauseController { get; set; }
        /// <summary> ポーズ中とポーズ解除のカウントダウン中に止まる戦闘の時計です。 </summary>
        public PausableClock PlayClock { get; set; }
    }
}
