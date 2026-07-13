using System;

namespace KillChord.Runtime.Adaptor.InGame.Stage
{
    /// <summary>
    ///     ステージ演出要求をViewへ渡すViewModel契約です。
    /// </summary>
    public interface IStageEffectViewModel
    {
        /// <summary> ステージ演出要求を通知します。 </summary>
        event Action<string, StageEffectViewKind> OnEffectRequested;

        /// <summary>
        ///     ステージ演出要求を反映します。
        /// </summary>
        /// <param name="effectId"> 演出IDです。 </param>
        /// <param name="kind"> 演出種類です。 </param>
        void Apply(string effectId, StageEffectViewKind kind);
    }

    /// <summary>
    ///     Viewへ通知するステージ演出種類です。
    /// </summary>
    public enum StageEffectViewKind
    {
        Explosion = 0,
        BuildingCollapse = 1,
        Obstacle = 2,
    }
}
