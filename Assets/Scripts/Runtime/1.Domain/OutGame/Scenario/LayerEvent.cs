using System;

namespace KillChord.Runtime.Domain.OutGame.Scenario
{
    /// <summary>
    /// シナリオ中で表示レイヤー順を変更するイベント。
    /// </summary>
    public class LayerEvent : IScenarioEvent
    {
        /// <summary>
        /// レイヤー変更イベントを初期化する。
        /// </summary>
        public LayerEvent(LayerTarget target, int order)
        {
            Target = target;
            Order = order;
        }

        /// <summary> Target を取得する。 </summary>
        public LayerTarget Target { get; }
        /// <summary> Order を取得する。 </summary>
        public int Order { get; }

        /// <summary> RequirePlayerAdvance を取得する。 </summary>
        public bool RequirePlayerAdvance => false;
    }
}