using System.Collections.Generic;

namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     深さ優先探索を使用して、選択されたノードから解放されているノードまでの最適な経路を見つけるアルゴリズムサービス。
    ///     アプリケーション層
    /// </summary>
    [System.Serializable]
    public class AnyNodeSearchService : IAlgorithmService
    {
        /// <summary>
        ///     選択されたノード(未解放)から解放されているノードまで辿り一番コストが掛からない経路を探す
        /// </summary>
        /// <param name="target">選択したノード</param>
        /// <param name="tree">スキルツリー</param>
        /// <returns></returns>
        public PathResult FindPath(SkillNodeEntity target, ISkillTreeRepository skillTreeRepository)
        {
            //  既に開放済みなら経路不要
            if (target.IsUnlocked)
                return new PathResult(new List<SkillNodeEntity>(), new UnlockCost(0));

            var bestPath = new List<SkillNodeEntity>();
            var totalCosts = int.MaxValue;
            var currentPath = new List<SkillNodeEntity>();
            var visited = new HashSet<SkillNodeEntity>();

            //  選択ノードを基点に探索する
            Search(target, skillTreeRepository, currentPath, visited, 0, ref bestPath, ref totalCosts);

            //  パスの数が0だったら何も無し
            if (bestPath.Count == 0)
                return new PathResult(new List<SkillNodeEntity>(), new UnlockCost(0));

            //　最適通路が見つかったらパスと総コストを生成して返す
            return new PathResult(bestPath, new UnlockCost(totalCosts));
        }
        /// <summary>
        ///     探索アルゴリズムを使用して、指定されたスキルノードへの最適な経路とそのコストを計算します。
        ///     開放済みノードに到達したとき、現在の経路コストとベストを比較する
        ///     
        ///     コストが同じルートのタイブレーク
        ///     ポップ数(経路上のノード数)最小
        /// </summary>
        /// <param name="target">探索の目標となるスキルノード。</param>
        /// <param name="skillTreeEntity">探索対象となるスキルツリー全体を表すエンティティ。</param>
        /// <param name="currentPath">現在の探索経路を表すスキルノードのリスト。探索の進行に応じて更新されます。</param>
        /// <param name="visited">現在のい経路探索上で探索済みのノードセット。</param>
        /// <param name="currentCost">現在の経路における累積コスト。</param>
        /// <param name="bestPath">最適経路が見つかった場合に、その経路のスキルノードリストが格納される参照。</param>
        /// <param name="bestCost">最適経路のコストが見つかった場合に、そのコストが格納される参照。</param>
        private void Search(SkillNodeEntity target,
                            ISkillTreeRepository skillTreeRepository,
                            List<SkillNodeEntity> currentPath,
                            HashSet<SkillNodeEntity> visited,
                            int currentCost,
                            ref List<SkillNodeEntity> bestPath,
                            ref int bestCost)
        {
            //  解放済みノードに到達 ➝　ルートのコストを評価
            if (target.IsUnlocked)
            {
                // コストを評価して同じコストだったら次にポップ数を見る
                bool isBetter =
                    currentCost < bestCost ||
                    (currentCost == bestCost && currentPath.Count < bestPath.Count);

                // 現在の最適パスより優れていたら変更する
                if (isBetter)
                {
                    bestCost = currentCost;
                    bestPath = new List<SkillNodeEntity>(currentPath);
                }

                return;
            }
            //  自己ループ・サイクル防止（Insert前にチェック）
            if (visited.Contains(target))
                return;

            //  現在のノードを経路と訪問済み集合に追加してから親へ進む
            currentPath.Insert(0, target);
            visited.Add(target);
            int totalCost = currentCost + target.UnlockCost.Cost;

            //  もしコストが最小コストを上回ったら枝を切る
            if (totalCost > bestCost)
            {
                currentPath.RemoveAt(0);
                visited.Remove(target);
                return;
            }
            // 現在のノードから親のノードを取得する
            var parents = skillTreeRepository.GetParentNodes(target.SkillNodeIdVO.Id);

            //　親なし(ルートに到達したが未解放)　➝　経路無し
            if (parents.Count == 0)
            {
                currentPath.RemoveAt(0);
                visited.Remove(target);
                return;
            }

            foreach (var parent in parents)
            {
                Search(parent, skillTreeRepository, currentPath, visited, totalCost, ref bestPath, ref bestCost);
            }

            currentPath.RemoveAt(0);
            visited.Remove(target);
        }
    }
}
