using Mock.MusicBattle.MusicSync;

namespace Mock.MusicBattle.Player
{
    public readonly struct PlayerStatusTransfer
    {
        public PlayerStatusTransfer(PlayerStatus status, IMusicBuffer buffer)
        {
            Value = status;
            _buffer = buffer;
        }

        public readonly PlayerStatus Value;
        public double DodgeDuration => Value.DodgeDuration.GetLength(_buffer);
        public double PostAttackMoveLockDuration => Value.DodgeDuration.GetLength(_buffer);

        private readonly IMusicBuffer _buffer;
    }
}
