using System;

namespace KillChord.Runtime.Adaptor.InGame.Battle
{
    /// <summary>
    ///     攻撃成立の表示通知を扱うSignalのインターフェース。
    /// </summary>
    public interface IPlayerAttackSignal
    {
        /// <summary> 攻撃成立時の拍種とジャスト成否を表示側へ通知する。 </summary>
        event Action<int, bool> OnAttackExecuted;

        /// <summary>
        ///     Presenterから受け取った攻撃成立の表示データを通知する。
        /// </summary>
        /// <param name="dto"> 攻撃成立の表示データ。 </param>
        void Push(in PlayerAttackDto dto);
    }
}
