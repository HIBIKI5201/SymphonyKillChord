namespace KillChord.Runtime.Adaptor.Persistent.Environment
{
    /// <summary>
    ///     表示言語を実行環境へ適用する契約。
    /// </summary>
    public interface ILanguageApplier
    {
        /// <summary>
        ///     指定した表示言語を適用する。
        /// </summary>
        /// <param name="localeCode"> 適用するLocaleコード。 </param>
        void Apply(string localeCode);
    }
}
