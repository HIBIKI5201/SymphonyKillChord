using System.Collections.Generic;

namespace DevelopProducts.Design.GameMode.Application
{
    /// <summary>
    ///     評価条件の達成状況を表すクラス。
    /// </summary>
    public class EvaluationResult
    {
        public EvaluationResult(IReadOnlyList<string> achivedDescriptions)
        {
            AchivedDescriptions = achivedDescriptions;
        }

        public IReadOnlyList<string> AchivedDescriptions { get; }
        public int AchivedCount => AchivedDescriptions.Count;
    }
}
