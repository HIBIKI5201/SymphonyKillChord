namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     スキル入力進行の1ステップ分の情報を表すDTO。
    /// </summary>
    public readonly struct SkillInputProgressStepDTO
    {
        /// <summary>
        ///     スキル入力進行の1ステップ分の情報を表すDTOを生成する。
        /// </summary>
        /// <param name="beatType"> 表示対象のビートタイプ。 </param>
        /// <param name="isActive"> 入力済みか。 </param>
        public SkillInputProgressStepDTO(int beatType, bool isActive)
        {
            BeatType = beatType;
            IsActive = isActive;
        }

        /// <summary> 表示対象のビートタイプ。 </summary>
        public int BeatType { get; }
        /// <summary> 入力済みとして光らせるか。 </summary>
        public bool IsActive { get; }
    }
}
