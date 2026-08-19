namespace KillChord.Runtime.Adaptor.InGame.Skill.Effect
{
    /// <summary>
    ///     再生中スキルエフェクトの操作契約。
    /// </summary>
    public interface ISkillEffectHandle
    {
        /// <summary> 再生中かどうかです。 </summary>
        bool IsPlaying { get; }

        /// <summary>
        ///     エフェクトを停止してプールへ返却する。
        /// </summary>
        void Stop();
    }
}
