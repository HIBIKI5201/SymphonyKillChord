namespace KillChord.Runtime.Domain.OutGame.Screen
{
    /// <summary>
    ///     画面遷移の種類を表す列挙型。
    /// </summary>
    public enum ScreenTransitionType
    {
        /// <summary> 前の画面を隠す。 </summary>
        Replace,
        /// <summary> 前の画面に重ねる。 </summary>
        Overlay,
        /// <summary> 画面遷移後履歴を消す。 </summary>
        Reset,
    }
}
