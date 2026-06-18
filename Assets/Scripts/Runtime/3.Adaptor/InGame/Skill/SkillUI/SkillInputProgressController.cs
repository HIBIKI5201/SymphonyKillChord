using KillChord.Runtime.Domain.InGame.Music;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     スキル入力の進捗を管理するコントローラクラス。
    /// </summary>
    public class SkillInputProgressController
    {
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
        /// <param name="now"></param>
        /// <param name="skillReadyTimestamp"></param>
        /// <param name="inputBeatType"></param>
        public void UpdateProgress(float now, float skillReadyTimestamp, BeatType inputBeatType)
        {
            _state.CheckInputBeatType(inputBeatType);
            _presenter.UpdateRow(new SkillInputProgressUpdateDTO(_state.CurrentMachedCount, now, skillReadyTimestamp, false));
        }

        /// <summary>
        ///     スキルを発動した場合の更新処理。
        /// </summary>
        /// <param name="now"></param>
        /// <param name="skillReadyTimestamp"></param>
        public void SkillTriggered(float now, float skillReadyTimestamp)
        {
            _state.ResetProgress();
            _presenter.UpdateRow(new SkillInputProgressUpdateDTO(_state.CurrentMachedCount, now, skillReadyTimestamp, true));
        }

        private readonly SkillInputProgressState _state;
        private readonly SkillInputProgressPresenter _presenter;
    }
}
