using UnityEngine;

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

        /// <summary>
        ///     エフェクトの再生完了を待機する。
        /// </summary>
        /// <returns> 再生完了を待機するAwaitableです。 </returns>
        Awaitable WaitForCompletionAsync();
    }
}
