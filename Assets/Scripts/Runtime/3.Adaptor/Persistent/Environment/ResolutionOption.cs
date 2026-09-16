namespace KillChord.Runtime.Adaptor.Persistent.Environment
{
    /// <summary>
    ///     解像度・画面モードの選択肢1件を表す。
    /// </summary>
    public readonly struct ResolutionOption
    {
        /// <summary>
        ///     解像度の選択肢を初期化する。
        /// </summary>
        public ResolutionOption(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <summary> 解像度の幅。 </summary>
        public int Width { get; }

        /// <summary> 解像度の高さ。 </summary>
        public int Height { get; }
    }
}
