namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    ///     コンボを表示するためのDTO。
    /// </summary>
    public readonly ref struct ComboDTO
    {
        /// <summary>
        ///    コンボを表示するためのDTOを作成する。
        /// </summary>
        /// <param name="comboCount"> 表示するコンボ数です。 </param>
        public ComboDTO(int comboCount)
        {
            ComboCount = comboCount;
        }
        /// <summary> コンボ数を取得します。 </summary>
        public int ComboCount { get; }
    }
}
