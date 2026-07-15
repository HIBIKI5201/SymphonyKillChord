using KillChord.Runtime.Adaptor.InGame.Stage;
using System;

namespace KillChord.Runtime.View.InGame.Stage
{
    /// <summary>
    ///     ステージ演出要求をViewへ通知するViewModelです。
    /// </summary>
    public sealed class StageEffectViewModel : IStageEffectViewModel
    {
        /// <summary> ステージ演出要求を通知します。 </summary>
        public event Action<int, StageEffectViewKind> OnEffectRequested;

        /// <summary>
        ///     ステージ演出要求を反映します。
        /// </summary>
        /// <param name="effectId"> 演出IDです。 </param>
        /// <param name="kind"> 演出種類です。 </param>
        public void Apply(int effectId, StageEffectViewKind kind)
        {
            OnEffectRequested?.Invoke(effectId, kind);
        }
    }
}
