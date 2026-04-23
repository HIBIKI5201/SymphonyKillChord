using System;

namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     ミッションの経過時間を管理するクラス。
    ///     ミッション開始からの経過時間を追跡し、必要に応じて更新することができます。
    /// </summary>
    public readonly struct MissionElapsedTime : IEquatable<MissionElapsedTime>
    {
        public MissionElapsedTime(float second)
        {
            _value = second < 0f ? 0f : second;
        }

        public float Value => _value;

        public bool IsOver(float targetTime)
        {
            return _value >= targetTime;
        }

        public MissionElapsedTime AdvanceTime(float deltaTime)
        {
            if (deltaTime < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            return new MissionElapsedTime(_value + deltaTime);
        }

        public bool Equals(MissionElapsedTime other)
        {
            return _value.Equals(other._value);
        }

        private readonly float _value;
    }
}
