namespace KillChord.Runtime.Adaptor.Persistent.Music
{
    /// <summary>
    ///     再生中BGMのセレクターラベル切り替えを行う出力ポート。
    ///     CRIへの依存をView層に閉じ込めるため、内側の層はこのインターフェース越しに操作する。
    /// </summary>
    public interface IBgmSelectorPlayer
    {
        /// <summary>
        ///     再生中BGMのセレクターラベルを設定する。
        /// </summary>
        /// <param name="selectorName"> セレクター名。 </param>
        /// <param name="labelName"> 設定するセレクターラベル名。 </param>
        void SetSelectorLabel(string selectorName, string labelName);
    }
}
