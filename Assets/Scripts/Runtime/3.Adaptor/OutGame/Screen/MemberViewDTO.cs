namespace KillChord.Runtime.Adaptor.OutGame.Screen
{
    /// <summary>
    ///     View へ渡す制作メンバー1人分の DTO。
    /// </summary>
    /// <remarks>
    ///     一覧としてコレクションに格納するため、<c>ref struct</c> ではなく <c>readonly struct</c> で定義します。
    /// </remarks>
    public readonly struct MemberViewDTO
    {
        /// <summary>
        ///     DTO を初期化します。
        /// </summary>
        /// <param name="name"> メンバー名です。 </param>
        /// <param name="className"> 役職名です。 </param>
        /// <param name="affiliationName"> 所属組織名です。 </param>
        public MemberViewDTO(string name, string className, string affiliationName)
        {
            Name = name;
            ClassName = className;
            AffiliationName = affiliationName;
        }

        /// <summary> メンバー名を取得します。 </summary>
        public string Name { get; }

        /// <summary> 役職名を取得します。 </summary>
        public string ClassName { get; }

        /// <summary> 所属組織名を取得します。 </summary>
        public string AffiliationName { get; }
    }
}
