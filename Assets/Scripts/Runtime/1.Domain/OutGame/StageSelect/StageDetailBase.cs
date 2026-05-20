namespace KillChord.Runtime.Domain
{
    /// <summary>
    ///  ステージの詳細情報の基底クラス。
    /// </summary>
    public class StageDetailBase
    {
        /// <summary>
        ///     ステージの詳細情報を初期化します。
        /// </summary>
        /// <param name="stageName"> ステージの名前。 </param>
        /// <param name="flavorText"> ステージのフレーバーテキスト。 </param>
        public StageDetailBase(string stageName, string flavorText)
        {
            StageName = stageName;
            FlavorText = flavorText;
        }

        /// <summary> ステージの名前。 </summary>
        public string StageName { get; }
        /// <summary> ステージのフレーバーテキスト。 </summary>
        public string FlavorText { get; }
    }
}
