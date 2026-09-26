using KillChord.Runtime.Utility.Identity;

namespace KillChord.Runtime.Domain.OutGame.Resource
{
    /// <summary>
    ///     ゲーム内リソースのコレクションキーと、コードから参照する既知のリソースIDを定義するクラスです。
    ///     <para>
    ///         既知のIDは DataID と同じ規則（コレクションキーと文字列IDのハッシュ）で算出します。
    ///         プランナーがリソース定義アセットの DataID に同じ文字列を入力すれば、このIDと一致します。
    ///     </para>
    /// </summary>
    public static class GameResourceIds
    {
        /// <summary> リソース定義の DataID に使用するコレクションキーです。 </summary>
        public const string COLLECTION_KEY = "GameResource";

        /// <summary> スキル解放・パラメーター強化に使用する研究ポイントです。 </summary>
        public static readonly GameResourceId ResearchPoint =
            new GameResourceId(DataIDHasher.Compute(COLLECTION_KEY, "ResearchPoint"));

        /// <summary> スキルのレベルアップに使用する改造ポイントです。 </summary>
        public static readonly GameResourceId SkillLevelupPoint =
            new GameResourceId(DataIDHasher.Compute(COLLECTION_KEY, "SkillLevelupPoint"));
    }
}
