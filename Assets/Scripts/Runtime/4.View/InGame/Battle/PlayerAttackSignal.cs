using KillChord.Runtime.Adaptor.InGame.Battle;
using System;

namespace KillChord.Runtime.View.InGame.Battle
{
    /// <summary>
    ///     Presenterから受け取った攻撃成立を表示側へ通知するイベントバス。
    /// </summary>
    public sealed class PlayerAttackSignal : IPlayerAttackSignal, IDisposable
    {
        /// <summary> 攻撃成立時の拍種とジャスト成否を通知する。 </summary>
        public event Action<int, bool> OnAttackExecuted;

        /// <summary>
        ///     攻撃成立の表示データを購読側へ通知する。
        /// </summary>
        /// <param name="dto"> Presenterが生成した表示データ。 </param>
        public void Push(in PlayerAttackDto dto)
        {
            OnAttackExecuted?.Invoke(dto.BeatCount, dto.IsJustHit);
        }

        /// <summary>
        ///     所有元の終了時に購読を解放する。
        /// </summary>
        public void Dispose()
        {
            OnAttackExecuted = null;
        }
    }
}
