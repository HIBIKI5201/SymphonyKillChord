using KillChord.Runtime.Domain.OutGame.StageSelect;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Application.OutGame.StageSelect
{
    /// <summary>
    ///     ノード再生後に適用する後続処理ルールを解決します。
    /// </summary>
    public sealed class NodeTransitionRuleResolver
    {
        /// <summary>
        ///     ルール解決器を初期化します。
        /// </summary>
        /// <param name="stageTree"> 参照するステージツリーです。 </param>
        /// <param name="rules"> 解決候補のルール一覧です。 </param>
        public NodeTransitionRuleResolver(
            StageTree stageTree,
            IReadOnlyList<NodeTransitionRule> rules)
        {
            _stageTree = stageTree ?? throw new ArgumentNullException(nameof(stageTree));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        /// <summary>
        ///     指定ステージに適用できる後続処理を解決します。
        /// </summary>
        /// <param name="currentStageDefinition"> 現在のステージ定義です。 </param>
        /// <param name="isTutorialCompleted"> チュートリアル完了状態です。 </param>
        /// <param name="resolvedRule"> 解決したルールです。 </param>
        /// <param name="targetStageDefinition"> 遷移先ステージ定義です。 </param>
        /// <returns> 解決に成功した場合はtrueです。 </returns>
        public bool TryResolve(
            StageDefinition currentStageDefinition,
            bool isTutorialCompleted,
            out NodeTransitionRule resolvedRule,
            out StageDefinition targetStageDefinition)
        {
            resolvedRule = null;
            targetStageDefinition = null;

            if (currentStageDefinition == null)
            {
                return false;
            }

            for (int i = 0; i < _rules.Count; i++)
            {
                NodeTransitionRule rule = _rules[i];
                if (rule == null || !rule.IsMatch(currentStageDefinition, isTutorialCompleted))
                {
                    continue;
                }

                if (!_stageTree.TryGetNode(rule.TargetStageId, out StageNode targetNode)
                    || targetNode?.Definition == null)
                {
                    continue;
                }

                if (resolvedRule == null || rule.Priority > resolvedRule.Priority)
                {
                    resolvedRule = rule;
                    targetStageDefinition = targetNode.Definition;
                }
            }

            return resolvedRule != null && targetStageDefinition != null;
        }

        private readonly StageTree _stageTree;
        private readonly IReadOnlyList<NodeTransitionRule> _rules;
    }
}
