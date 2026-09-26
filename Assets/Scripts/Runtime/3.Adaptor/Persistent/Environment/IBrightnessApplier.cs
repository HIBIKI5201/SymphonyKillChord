namespace KillChord.Runtime.Adaptor.Persistent.Environment
{
    /// <summary>
    ///     画面の明るさをデバイスへ適用する共通インターフェース。
    /// </summary>
    public interface IBrightnessApplier
    {
        /// <summary>
        ///     明るさを設定する。
        /// </summary>
        /// <param name="brightness"> 0～1に正規化した明るさ。 </param>
        void SetBrightness(float brightness);

        /// <summary>
        ///     明るさを取得する。
        /// </summary>
        float GetBrightness();
    }
}
