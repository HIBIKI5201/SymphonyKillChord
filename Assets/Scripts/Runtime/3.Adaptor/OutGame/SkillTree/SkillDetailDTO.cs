using UnityEngine;

namespace KillChord.Runtime.Adaptor.OutGame.SkillTree
{
    /// <summary>
    ///     スキル詳細にデータを渡すためのDTO。
    /// </summary>
    public readonly ref struct SkillDetailDTO
    {
        public SkillDetailDTO(
            int skillnodeId,
            bool hasSkill,
            string skillName,
            string skillCommand,
            string skillGenre,
            Sprite skillGenreIcon,
            string skillDetail,
            int unlockCost,
            bool canUnlock,
            bool unlocked,
            bool hasPreviewVideo)
        {
            SkillNodeId = skillnodeId;
            HasSkill = hasSkill;
            SkillName = skillName == null ? "" : skillName;
            SkillCommand = skillCommand == null ? "" : skillCommand;
            SkillGenre = skillGenre == null ? "" : skillGenre;
            SkillGenreIcon = skillGenreIcon;
            SkillDetail = skillDetail == null ? "" : skillDetail;
            UnlockCost = unlockCost;
            CanUnlock = canUnlock;
            Unlocked = unlocked;
            HasPreviewVideo = hasPreviewVideo;
        }
        /// <summary> スキルノードのID </summary>
        public readonly int SkillNodeId;
        /// <summary> ノードがスキルを解放するか(falseの場合はステータス強化のみのノード) </summary>
        public readonly bool HasSkill;
        /// <summary> ノードが解放するスキルの名前 </summary>
        public readonly string SkillName;
        /// <summary> ノードが解放するスキルの発動コマンド </summary>
        public readonly string SkillCommand;
        /// <summary> ノードが解放するスキルのジャンル表示文 </summary>
        public readonly string SkillGenre;
        /// <summary> ノードが解放するスキルのジャンルアイコン </summary>
        public readonly Sprite SkillGenreIcon;
        /// <summary> スキルの詳細文 </summary>
        public readonly string SkillDetail;
        /// <summary> 解放するための必要ポイント </summary>
        public readonly int UnlockCost;
        /// <summary> 解放可否 </summary>
        public readonly bool CanUnlock;
        /// <summary> 解放済みか </summary>
        public readonly bool Unlocked;
        /// <summary> プレビュー動画があるか </summary>
        public readonly bool HasPreviewVideo;
    }
}
