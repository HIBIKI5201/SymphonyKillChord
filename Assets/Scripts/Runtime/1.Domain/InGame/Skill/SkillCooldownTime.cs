namespace KillChord.Runtime.Domain.InGame.Skill
{
    /// <summary>
    ///     スキルのクールダウン時間を保持するValueObject。
    /// </summary>
    public readonly struct SkillCooldownTime
    {
        /// <summary>
        ///     クールダウン時間を生成する。負の値は 0 に丸める。
        /// </summary>
        public SkillCooldownTime(double value)
        {
            _value = value < 0f ? 0f : value;
        }

        /// <summary> クールダウン時間。 </summary>
        public readonly double Value => _value;
        private readonly double _value;
    }
}
