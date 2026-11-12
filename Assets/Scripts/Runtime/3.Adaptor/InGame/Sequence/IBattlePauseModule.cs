using System;

namespace KillChord.Runtime.Adaptor.InGame.Sequence
{
    /// <summary>
    ///     戦闘ポーズモジュールのインタフェース。
    /// </summary>
    public interface IBattlePauseModule
    {
        /// <summary> ポーズ解除のカウントダウンが終わり、戦闘を再開した時に発火するイベント。 </summary>
        public event Action OnResumeCountdownCompleted;

        /// <summary>
        ///     戦闘ポーズを実行する。
        /// </summary>
        /// <returns> ポーズできた場合は true </returns>
        public bool TryPause();

        /// <summary>
        ///     戦闘ポーズをすぐに解除する。カウントダウン中なら取りやめてから解除する。
        /// </summary>
        public void Resume();

        /// <summary>
        ///     BGMだけを再生して、拍に合わせたポーズ解除のカウントダウンを始める。
        ///     戦闘はカウントダウンが終わるまで止めたままにする。
        /// </summary>
        /// <returns> カウントダウンを始めた場合は true。始められない場合は false。 </returns>
        public bool TryBeginResumeCountdown();

        /// <summary>
        ///     ポーズ解除のカウントダウンを取りやめ、ポーズ状態へ戻す。
        /// </summary>
        public void CancelResumeCountdown();
    }
}
