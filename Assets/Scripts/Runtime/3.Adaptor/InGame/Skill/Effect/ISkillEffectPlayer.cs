namespace KillChord.Runtime.Adaptor.InGame.Skill.Effect
{
    /// <summary>
    ///     スキルエフェクトの再生をViewへ依頼する契約。
    /// </summary>
    public interface ISkillEffectPlayer
    {
        /// <summary>
        ///     指定IDのスキルエフェクトを再生する。
        /// </summary>
        /// <param name="effectId"> 再生するエフェクトのIDです。 </param>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <returns> 再生に成功した場合はハンドル、失敗した場合はnull。 </returns>
        ISkillEffectHandle Play(SkillEffectId effectId, in SkillEffectContext context);

        /// <summary>
        ///     再生中のスキルエフェクトをすべて停止する。
        /// </summary>
        void StopAll();
    }
}
