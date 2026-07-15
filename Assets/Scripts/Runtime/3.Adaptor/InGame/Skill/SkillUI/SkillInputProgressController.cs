using KillChord.Runtime.Domain.InGame.Music;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     スキル入力の進捗を管理するコントローラクラス。
    /// </summary>
    public class SkillInputProgressController
    {
        /// <summary>
        ///     コントローラーを初期化します。
        /// </summary>
        /// <param name="state"> 入力進捗状態です。 </param>
        /// <param name="presenter"> 入力進捗Presenterです。 </param>
        public SkillInputProgressController(
            SkillInputProgressState state,
            SkillInputProgressPresenter presenter)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
        }

        /// <summary>
        ///     入力されたビートに基づいて、スキルの入力進捗を更新する。
        /// </summary>
        /// <param name="now"> 現在時刻です。 </param>
        /// <param name="skillReadyTimestamp"> スキル再使用可能時刻です。 </param>
        /// <param name="inputBeatType"> 入力ビートです。 </param>
        public void UpdateProgress(float now, float skillReadyTimestamp, BeatType inputBeatType)
        {
            _state.CheckInputBeatType(inputBeatType);
            _presenter.UpdateRow(new SkillInputProgressUpdateDTO(_state.CurrentMachedCount, now, skillReadyTimestamp, false));
        }

        /// <summary>
        ///     スキルを発動した場合の更新処理。
        /// </summary>
        /// <param name="now"> 現在時刻です。 </param>
        /// <param name="skillReadyTimestamp"> スキル再使用可能時刻です。 </param>
        public void SkillTriggered(float now, float skillReadyTimestamp)
        {
            _state.ResetProgress();
            _presenter.UpdateRow(new SkillInputProgressUpdateDTO(_state.CurrentMachedCount, now, skillReadyTimestamp, true));
        }

        /// <summary>
        ///     入力進捗をリセットします。
        /// </summary>
        /// <param name="now"> 現在時刻です。 </param>
        /// <param name="skillReadyTimestamp"> スキル再使用可能時刻です。 </param>
        public void ResetProgress(float now, float skillReadyTimestamp)
        {
            _state.ResetProgress();
            _presenter.UpdateRow(new SkillInputProgressUpdateDTO(_state.CurrentMachedCount, now, skillReadyTimestamp, false));
        }

        private readonly SkillInputProgressState _state;
        private readonly SkillInputProgressPresenter _presenter;
    }
}
