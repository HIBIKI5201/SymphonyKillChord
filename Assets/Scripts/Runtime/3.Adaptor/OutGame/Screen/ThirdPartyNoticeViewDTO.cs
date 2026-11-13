namespace KillChord.Runtime.Adaptor.OutGame.Screen
{
    /// <summary>
    ///     View へ渡すサードパーティ素材1件分の表記の DTO。
    /// </summary>
    /// <remarks>
    ///     一覧としてコレクションに格納するため、<c>ref struct</c> ではなく <c>readonly struct</c> で定義します。
    /// </remarks>
    public readonly struct ThirdPartyNoticeViewDTO
    {
        /// <summary>
        ///     DTO を初期化します。
        /// </summary>
        /// <param name="name"> 素材名です。 </param>
        /// <param name="holder"> 権利者・著作表記です。 </param>
        /// <param name="license"> ライセンス名です。 </param>
        /// <param name="text"> 著作表記や許諾表示の本文です。 </param>
        public ThirdPartyNoticeViewDTO(string name, string holder, string license, string text)
        {
            Name = name;
            Holder = holder;
            License = license;
            Text = text;
        }

        /// <summary> 素材名を取得します。 </summary>
        public string Name { get; }

        /// <summary> 権利者・著作表記を取得します。 </summary>
        public string Holder { get; }

        /// <summary> ライセンス名を取得します。 </summary>
        public string License { get; }

        /// <summary> 著作表記や許諾表示の本文を取得します。 </summary>
        public string Text { get; }
    }
}
