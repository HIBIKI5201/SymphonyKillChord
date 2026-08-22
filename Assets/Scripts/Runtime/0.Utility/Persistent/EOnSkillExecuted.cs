namespace KillChord.Runtime.Utility.Persistent
{
    /// <summary>
    ///     イベント定義：スキルが発動した時。
    ///     入力パターン一致後に実行が成立した時点で発火する。対象の有無は問わない。
    /// </summary>
    public readonly struct EOnSkillExecuted : IEvent
    {
        /// <summary> 発動したスキルのID。 </summary>
        public readonly int SkillId;

        /// <summary>
        ///     発動したスキルのIDを指定してイベントを生成するコンストラクタ。
        /// </summary>
        /// <param name="skillId"> 発動したスキルのID。</param>
        public EOnSkillExecuted(int skillId)
        {
            SkillId = skillId;
        }
    }
}
