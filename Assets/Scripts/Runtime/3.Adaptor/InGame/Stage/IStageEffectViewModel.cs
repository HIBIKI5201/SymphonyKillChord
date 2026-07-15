using System;

namespace KillChord.Runtime.Adaptor.InGame.Stage
{
    /// <summary>
    ///     ステージ演出要求をViewへ渡すViewModel契約です。
    /// </summary>
    public interface IStageEffectViewModel
    {
        /// <summary> ステージ演出要求を通知します。 </summary>
        event Action<int, StageEffectViewKind> OnEffectRequested;

        /// <summary>
        ///     ステージ演出要求を反映します。
        /// </summary>
        /// <param name="effectId"> 演出IDです。 </param>
        /// <param name="kind"> 演出種類です。 </param>
        void Apply(int effectId, StageEffectViewKind kind);
    }
}
