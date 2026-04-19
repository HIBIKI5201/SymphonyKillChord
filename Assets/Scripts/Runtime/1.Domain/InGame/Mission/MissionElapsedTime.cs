using System;

namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     ミッションの経過時間を管理するクラス。
    ///     ミッション開始からの経過時間を追跡し、必要に応じて更新することができます。
    /// </summary>
    public class MissionElapsedTime
    {
        public float Value => _value;

        public void AdvanceTime(float deltaTime)
        {
            if (deltaTime < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime), "deltaTime must be non-negative.");
            }

            _value += deltaTime;
        }

        private float _value;
    }
}
