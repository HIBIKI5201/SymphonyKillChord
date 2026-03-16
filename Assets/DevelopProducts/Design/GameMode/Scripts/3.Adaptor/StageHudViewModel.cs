
namespace DevelopProducts.Design.GameMode.Adaptor
{
    /// <summary>
    ///     ステージのHUDに表示する情報をまとめたViewModelクラス。
    ///     プレイヤーのHP、経過時間、ステージの結果などを保持し、HUDの表示に使用される。
    /// </summary>
    public class StageHudViewModel
    {
        public int CurrentPlayerHp { get; set; }
        public int MaxPlayerHp { get; set; }
        public float ElapsedTime { get; set; }
        public string ResultText { get; set; }
        public int EvaluationCount { get; set; }
    }
}
