namespace KillChord.Runtime.Domain.Persistent.Savedata
{
    /// <summary>
    ///     シーンを跨ぐチュートリアルの進行段階です。
    /// </summary>
    public enum TutorialPhase
    {
        NotStarted = 0,
        OpeningScenarioCompleted = 1,
        BattleCompleted = 2,
        HomeStarted = 3,
        Completed = 4,
    }
}
