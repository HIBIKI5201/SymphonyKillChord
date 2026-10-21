using KillChord.Runtime.Domain.InGame.Skill;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Domain.OutGame.SkillTree
{
    /// <summary>
    ///     スキルノードのEntity。
    /// </summary>
    public class SkillNodeEntity
    {
        /// <summary>
        ///     スキルツリーのノードを生成する。
        /// </summary>
        public SkillNodeEntity(SkillNodeId nodeId,
            int cost, string skillDetail,
            SkillId[] unlockSkillIds,
            IStatusBonusEffect[] statusBonusEffects = null,
            bool hasSkillSlotBonus = false,
            int maxLevel = DEFAULT_MAX_LEVEL,
            bool isUnlimitedLevel = false,
            int unlockCostIncreasePerLevel = 0)
        {
            if (!isUnlimitedLevel && maxLevel < DEFAULT_MAX_LEVEL)
            {
                throw new ArgumentOutOfRangeException(nameof(maxLevel), "強化上限は1以上を指定する。");
            }

            if (unlockCostIncreasePerLevel < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(unlockCostIncreasePerLevel), "強化ごとのコスト増加量は0以上を指定する。");
            }

            SkillNodeIdVO = nodeId;
            UnlockCost = new UnlockCost(cost);
            _parents = null;
            SkillDetail = skillDetail;
            _unlockSkillIds = unlockSkillIds ?? Array.Empty<SkillId>();
            _statusBonusEffects = statusBonusEffects == null || statusBonusEffects.Length == 0
                ? Array.Empty<IStatusBonusEffect>()
                : (IStatusBonusEffect[])statusBonusEffects.Clone();
            HasSkillSlotBonus = hasSkillSlotBonus;
            MaxLevel = maxLevel;
            IsUnlimitedLevel = isUnlimitedLevel;
            UnlockCostIncreasePerLevel = unlockCostIncreasePerLevel;
        }

        /// <summary> ノードのID。 </summary>
        public SkillNodeId SkillNodeIdVO { get; }
        /// <summary> 解放に必要なコスト。 </summary>
        public UnlockCost UnlockCost { get; }
        /// <summary> スキルの詳細文。 </summary>
        public string SkillDetail { get; }
        /// <summary> 1回以上解放されているか。 </summary>
        public bool IsUnlocked => _level > 0;

        /// <summary> 現在の強化回数。未解放は0。 </summary>
        public int Level => _level;

        /// <summary> 強化上限。<see cref="IsUnlimitedLevel"/>がtrueの場合は使わない。 </summary>
        public int MaxLevel { get; }

        /// <summary> 強化上限が無い(無限強化ノード)か。 </summary>
        public bool IsUnlimitedLevel { get; }

        /// <summary> さらに強化(解放)できるか。 </summary>
        public bool IsLevelUpAvailable => IsUnlimitedLevel || _level < MaxLevel;

        /// <summary> 強化1回ごとに増える解放コスト。 </summary>
        public int UnlockCostIncreasePerLevel { get; }

        /// <summary> 次の1回の強化(解放)に必要なコスト。 </summary>
        public int NextUnlockCost => GetUnlockCost(_level);

        /// <summary> 親ノード。 </summary>
        public SkillNodeEntity[] Parents => _parents;

        /// <summary> 解放されるスキルの ID。 </summary>
        public SkillId[] UnlockSkillIds => _unlockSkillIds;

        /// <summary> 保持しているステータスボーナス効果。 </summary>
        public IReadOnlyList<IStatusBonusEffect> StatusBonusEffects => _statusBonusEffects;

        /// <summary> 解放時にスキル編成枠を1つ増やすか。 </summary>
        public bool HasSkillSlotBonus { get; }

        /// <summary> 強化上限の既定値。通常のノードは1回だけ解放できる。 </summary>
        public const int DEFAULT_MAX_LEVEL = 1;

        /// <summary>
        ///     親ノードを設定する。
        /// </summary>
        /// <param name="parents"></param>
        public void SetParent(SkillNodeEntity[] parents)
        {
            _parents = parents;
        }

        /// <summary>
        ///     ノードを1段階強化する。未解放の場合は解放済みになる。
        /// </summary>
        /// <returns> 強化できた場合はtrue。上限に達している場合はfalse。 </returns>
        public bool Unlock()
        {
            if (!IsLevelUpAvailable)
            {
                return false;
            }

            _level++;
            return true;
        }

        /// <summary>
        ///     ノードを未解放に戻す。
        /// </summary>
        public void Lock()
        {
            _level = 0;
        }

        /// <summary>
        ///     指定した強化回数の状態から、次の1回の強化に必要なコストを取得する。
        /// </summary>
        /// <param name="currentLevel"> 強化前の強化回数。 </param>
        /// <returns> 次の1回の強化に必要なコスト。 </returns>
        public int GetUnlockCost(int currentLevel)
        {
            return checked(UnlockCost.Cost + UnlockCostIncreasePerLevel * Math.Max(0, currentLevel));
        }

        /// <summary>
        ///     未解放から指定した強化回数まで強化するのに必要なコストの合計を取得する。
        /// </summary>
        /// <param name="level"> 強化回数。強化上限の範囲に収めて計算する。 </param>
        /// <returns> コストの合計。 </returns>
        public int GetTotalUnlockCost(int level)
        {
            int clampedLevel = ClampLevel(level);
            int totalCost = 0;
            for (int i = 0; i < clampedLevel; i++)
            {
                totalCost = checked(totalCost + GetUnlockCost(i));
            }

            return totalCost;
        }

        /// <summary>
        ///     指定した強化回数を、強化上限の範囲に収める。
        /// </summary>
        /// <param name="level"> 強化回数。 </param>
        /// <returns> 0以上かつ強化上限以下の強化回数。 </returns>
        public int ClampLevel(int level)
        {
            if (level <= 0)
            {
                return 0;
            }

            return IsUnlimitedLevel ? level : Math.Min(level, MaxLevel);
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

        private int _level = 0;
        private SkillNodeEntity[] _parents;
        private SkillId[] _unlockSkillIds;
        private IStatusBonusEffect[] _statusBonusEffects;
    }
}
