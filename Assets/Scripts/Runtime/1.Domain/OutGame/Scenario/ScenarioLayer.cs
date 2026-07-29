namespace KillChord.Runtime.Domain.OutGame.Scenario
{
    /// <summary>
    /// シナリオ表示の重ね順を決める UI 要素の種別。
    /// 並び順は設定アセット側で背面→前面のリストとして定義する。
    /// </summary>
    public enum ScenarioLayer
    {
        /// <summary> 背景。 </summary>
        Background,
        /// <summary> 立ち絵（左右中央をまとめた1バンド）。 </summary>
        Portrait,
        /// <summary> テキストボックス。 </summary>
        Text,
        /// <summary> 演出（黒フェード等）。 </summary>
        Effect,
    }
}
