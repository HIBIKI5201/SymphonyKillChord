namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    ///     Viewから送られる攻撃コマンドの種類を表す列挙型。
    /// </summary>
    public enum AttackCommandType
    {
        Normal = 0,
        SkillA = 1,
        SkillB = 2,
        Ultimate = 3,
    }
}