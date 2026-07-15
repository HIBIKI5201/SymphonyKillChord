namespace KillChord.Runtime.Composition.InGame.Sequence
{
    /// <summary>
    ///     シーケンスモジュールの公開物を保持するContainerです。
    /// </summary>
    public sealed class SequenceModuleContainer
    {
        /// <summary> インゲームシーケンス演出です。 </summary>
        public InGameSequenceDirector SequenceDirector { get; set; }
    }
}
