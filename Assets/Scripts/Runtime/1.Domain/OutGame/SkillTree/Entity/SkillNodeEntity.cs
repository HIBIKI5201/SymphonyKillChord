using KillChord.Runtime.Domain.InGame.Skill;
using System;

namespace KillChord.Runtime.Domain.OutGame.SkillTree
{
    /// <summary>
    ///     スキルノードのEntity。
    /// </summary>
    public class SkillNodeEntity
    {
        public SkillNodeEntity(SkillNodeId nodeId,
            int cost, string skillDetail,
            SkillId[] unlockSkillIds,
            IStatusBonusEffect[] statusBonusEffects = null)
        {
            SkillNodeIdVO = nodeId;
            UnlockCost = new UnlockCost(cost);
            _parents = null;
            SkillDetail = skillDetail;
            _unlockSkillIds = unlockSkillIds ?? Array.Empty<SkillId>();
            _statusBonusEffects = statusBonusEffects == null || statusBonusEffects.Length == 0
                ? Array.Empty<IStatusBonusEffect>()
                : (IStatusBonusEffect[])statusBonusEffects.Clone();
        }
        /// <summary> ノードのID。 </summary>
        public SkillNodeId SkillNodeIdVO { get; }
        /// <summary> 解放に必要なコスト。 </summary>
        public UnlockCost UnlockCost { get; }
        /// <summary> スキルの詳細文。 </summary>
        public string SkillDetail { get; }
        /// <summary> 解放されているか。 </summary>
        public bool IsUnlocked => _isUnlocked;

        /// <summary> 親ノード。 </summary>
        public SkillNodeEntity[] Parents => _parents;

        /// <summary> 解放されるスキルの ID。 </summary>
        public SkillId[] UnlockSkillIds => _unlockSkillIds;

        /// <summary>
        ///     親ノードを設定する。
        /// </summary>
        /// <param name="parents"></param>
        public void SetParent(SkillNodeEntity[] parents)
        {
            _parents = parents;
        }

        /// <summary>
        ///     ノードを解放済みにする。
        /// </summary>
        public void Unlock()
        {
            _isUnlocked = true;
        }

        /// <summary>
        ///     ノードを未解放に戻す。
        /// </summary>
        public void Lock()
        {
            _isUnlocked = false;
        }

        /// <summary>
        ///     保持しているステータスボーナス効果を適用する。
        /// </summary>
        /// <param name="builder"> ボーナスの集計先。 </param>
        public void ApplyStatusBonusEffects(PlayerStatusBonusBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            for (int i = 0; i < _statusBonusEffects.Length; i++)
            {
                _statusBonusEffects[i]?.Apply(builder);
            }
        }

        private bool _isUnlocked = false;
        private SkillNodeEntity[] _parents;
        private SkillId[] _unlockSkillIds;
        private IStatusBonusEffect[] _statusBonusEffects;
    }
}
