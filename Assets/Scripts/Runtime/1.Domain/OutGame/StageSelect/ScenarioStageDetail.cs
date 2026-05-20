namespace KillChord.Runtime.Domain.OutGame.StageSelect
{
    /// <summary>
    ///     シナリオステージの詳細情報を表すクラス。
    /// </summary>
    public class ScenarioStageDetail : StageDetailBase
    {
        /// <summary>
        ///     シナリオステージの詳細情報を初期化する。
        /// </summary>
        /// <param name="stageName"> ステージの名前。 </param>
        /// <param name="flavorText"> ステージのフレーバーテキスト。 </param>
        public ScenarioStageDetail(
            string stageName,
            string flavorText)
            : base(stageName, flavorText)
        {
        }
    }
}
