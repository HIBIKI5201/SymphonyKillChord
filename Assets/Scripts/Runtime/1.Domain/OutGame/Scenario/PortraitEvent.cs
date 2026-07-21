using System;

namespace KillChord.Runtime.Domain.OutGame.Scenario
{
    /// <summary>
    /// シナリオ中で立ち絵表示を指示するイベント。
    /// </summary>
    public class PortraitEvent : IScenarioEvent
    {
        /// <summary>
        /// 立ち絵イベントを初期化する。
        /// </summary>
        public PortraitEvent(
            PortraitSlot slot,
            PortraitId portraitId,
            float positionX,
            float positionY,
            float scale,
            bool visible)
        {
            Slot = slot;
            PortraitId = portraitId;
            PositionX = positionX;
            PositionY = positionY;
            Scale = scale <= 0f ? 1f : scale;
            Visible = visible;
        }

        /// <summary> Slot を取得する。 </summary>
        public PortraitSlot Slot { get; }
        /// <summary> PortraitId を取得する。 </summary>
        public PortraitId PortraitId { get; }
        /// <summary> PositionX を取得する。 </summary>
        public float PositionX { get; }
        /// <summary> PositionY を取得する。 </summary>
        public float PositionY { get; }
        /// <summary> Scale を取得する。 </summary>
        public float Scale { get; }
        /// <summary> Visible を取得する。 </summary>
        public bool Visible { get; }

        /// <summary> RequirePlayerAdvance を取得する。 </summary>
        public bool RequirePlayerAdvance => false;
    }
}
