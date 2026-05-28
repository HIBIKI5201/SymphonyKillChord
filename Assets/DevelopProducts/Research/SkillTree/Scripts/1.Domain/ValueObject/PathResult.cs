using System.Collections.Generic;
using UnityEngine;

namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     経路探索の結果。
    /// </summary>
    public readonly struct PathResult
    {
        public PathResult(
            List<SkillNodeEntity> path,
            UnlockCost totalCost)
        {
            Path = path;
            TotalCost = totalCost;
        }

        /// <summary>解放が必要なノードを根元から順に並べたリスト。</summary>
        public List<SkillNodeEntity> Path { get; }

        /// <summary>経路上の全ノードのコスト合計。</summary>
        public UnlockCost TotalCost { get; }

        /// <summary>経路が見つからなかったか判定する。</summary>
        public bool IsEmpty => Path == null || Path.Count == 0;
    }
}
