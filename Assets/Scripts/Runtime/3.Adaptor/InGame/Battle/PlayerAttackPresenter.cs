using KillChord.Runtime.Domain.InGame.Music;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Battle
{
    /// <summary>
    ///     攻撃成立の判定結果をDTOへ変換し、表示用Signalへ渡すPresenter。
    /// </summary>
    public sealed class PlayerAttackPresenter
    {
        /// <summary>
        ///     攻撃成立の表示用Signalを受け取る。
        /// </summary>
        /// <param name="signal"> Compositionから注入される表示用Signal。 </param>
        public PlayerAttackPresenter(IPlayerAttackSignal signal)
        {
            _signal = signal ?? throw new ArgumentNullException(nameof(signal));
        }

        /// <summary>
        ///     入力時に確定した攻撃の判定結果を表示側へ渡す。
        /// </summary>
        /// <param name="beatType"> 入力時に確定した拍種。 </param>
        /// <param name="isJustHit"> 攻撃とスキルに適用したジャスト成否。 </param>
        public void Push(BeatType beatType, bool isJustHit)
        {
            PlayerAttackDto dto = new PlayerAttackDto((int)beatType, isJustHit);
            _signal.Push(in dto);
        }

        private readonly IPlayerAttackSignal _signal;
    }
}
