namespace KillChord.Runtime.Domain.InGame.Skill
{
    /// <summary>
    ///     スキルのクールダウン時間を保持するValueObject。
    /// </summary>
    public readonly struct SkillCooldownTime
    {
        public SkillCooldownTime(double value)
        {
            _value = value < 0f ? 0f : value;
        }
        public readonly double Value => _value;
        private readonly double _value;
    }
}
