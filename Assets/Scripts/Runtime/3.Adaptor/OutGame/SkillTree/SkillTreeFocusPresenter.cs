using KillChord.Runtime.Application.OutGame.SkillTree;
using KillChord.Runtime.Domain.OutGame.SkillTree;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Adaptor.OutGame.SkillTree
{
    /// <summary>
    ///     スキルツリーの初期フォーカス対象をViewModelへ反映するPresenter。
    /// </summary>
    public sealed class SkillTreeFocusPresenter
    {
        /// <summary>
        ///     初期フォーカス対象の取得元と反映先を設定する。
        /// </summary>
        /// <param name="skillTreeService"> スキルツリーの探索サービス。 </param>
        /// <param name="viewModel"> 初期フォーカス対象の反映先。 </param>
        public SkillTreeFocusPresenter(
            SkillTreeService skillTreeService,
            ISkillTreeFocusViewModel viewModel)
        {
            _skillTreeService = skillTreeService
                ?? throw new ArgumentNullException(nameof(skillTreeService));
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }

        /// <summary>
        ///     現在の解放状態から初期フォーカス対象を取得してViewModelへ反映する。
        /// </summary>
        /// <param name="visibleCandidateNodeIds"> 表示中の初期フォーカス候補ノードID。 </param>
        /// <returns> 初期フォーカス対象を取得できた場合はtrue。 </returns>
        public bool Push(IReadOnlyList<int> visibleCandidateNodeIds)
        {
            if (visibleCandidateNodeIds == null)
            {
                throw new ArgumentNullException(nameof(visibleCandidateNodeIds));
            }

            SkillNodeId[] candidateNodeIds = new SkillNodeId[visibleCandidateNodeIds.Count];
            for (int i = 0; i < visibleCandidateNodeIds.Count; i++)
            {
                candidateNodeIds[i] = new SkillNodeId(visibleCandidateNodeIds[i]);
            }

            if (!_skillTreeService.TryGetNearestLockedNodeIdsFromStart(
                    candidateNodeIds,
                    out IReadOnlyList<SkillNodeId> nodeIds))
            {
                _viewModel.SetFocusTargets(Array.Empty<int>());
                return false;
            }

            int[] nodeIdValues = new int[nodeIds.Count];
            for (int i = 0; i < nodeIds.Count; i++)
            {
                nodeIdValues[i] = nodeIds[i].Id;
            }

            _viewModel.SetFocusTargets(nodeIdValues);
            return true;
        }

        private readonly SkillTreeService _skillTreeService;
        private readonly ISkillTreeFocusViewModel _viewModel;
    }
}
