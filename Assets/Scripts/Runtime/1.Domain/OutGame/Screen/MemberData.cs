namespace KillChord.Runtime.Domain.OutGame.Screen
{
    /// <summary>
    ///     制作メンバー1人分の情報を保持する値オブジェクト。
    /// </summary>
    public readonly struct MemberData
    {
        /// <summary>
        ///     制作メンバー情報を初期化します。
        /// </summary>
        /// <param name="name"> メンバー名です。 </param>
        /// <param name="className"> 役職名です。 </param>
        /// <param name="affiliationName"> 所属組織名です。 </param>
        public MemberData(MemberName name, MemberClassName className, MemberAffiliationName affiliationName)
        {
            Name = name;
            Class = className;
            Affiliation = affiliationName;
        }

        /// <summary> メンバー名を取得します。 </summary>
        public MemberName Name { get; }

        /// <summary> 役職名を取得します。 </summary>
        public MemberClassName Class { get; }

        /// <summary> 所属組織名を取得します。 </summary>
        public MemberAffiliationName Affiliation { get; }
    }
}
