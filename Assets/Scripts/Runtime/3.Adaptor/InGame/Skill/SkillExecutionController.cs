using KillChord.Runtime.Application.InGame.Skill;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     スキルの発動を管理するコントローラー。
    /// </summary>
    public class SkillExecutionController
    {
        public SkillExecutionController(SkillResultPresenter presenter,
            SkillInputProgressController progressController,
            SkillCooldownState skillCooldownState,
            SkillUsecase skillUseCase,
            SkillCheckService skillCheckService,
            ISkillVisual skillVisual,
            SkillDefinition skillDefinition,
            SkillRhythmState skillRhythmState)
        {
            _presenter = presenter;
            _progressController = progressController;
            _skillCooldownState = skillCooldownState;
            _skillUseCase = skillUseCase;
            _skillCheckService = skillCheckService;
            _skillVisual = skillVisual;
            _skillDefinition = skillDefinition;
            _skillRhythmState = skillRhythmState;
        }

        /// <summary>
        ///     スキル発動を試す。
        ///     発動したか、及びスキルのアニメーションのキーを返却する。
        /// </summary>
        /// <param name="beatType"></param>
        /// <param name="now"></param>
        /// <param name="battleActionType"></param>
        /// <param name="animationKey"></param>
        /// <returns></returns>
        public bool TryExecuteSkill(BeatType beatType, float now, BattleActionType battleActionType, out string animationKey)
        {
            bool isInputMatch = false;
            animationKey = null;

            // クールダウン中の場合、後続処理を飛ばす
            if (!_skillCooldownState.IsSkillReady(now))
            {
                Debug.Log($"[SkillExecutionController] クールダウン中。ID：{_skillDefinition.Id.Value}");
                return false;
            }

            // スキルごとの入力履歴を登録し、発動判定を行う
            _skillRhythmState.Enqueue(beatType, now, battleActionType);
            ReadOnlySpan<BeatType> inputHistory = _skillRhythmState.GetHistoryBeatType();
            isInputMatch = _skillCheckService.CheckInput(_skillDefinition, inputHistory);

            if (isInputMatch)
            {
                bool canExecute = _skillUseCase.TryExecuteSkill(_skillDefinition, beatType);
                // 目標関係で発動できなかった場合、エフェクトやアニメーション表現をしない
                if (canExecute)
                {
                    ExecuteVisual(_skillDefinition.Id.Value, _skillDefinition.AnimationKey);
                    _presenter.Push(_skillDefinition);
                }
                _skillRhythmState.Clear();
                _skillCooldownState.SetSkillCooldown(now);
                _progressController.SkillTriggered(now, _skillCooldownState.SkillReadyTimestamp);
                animationKey = _skillDefinition.AnimationKey;
                return true;
            }

            _progressController.UpdateProgress(now, _skillCooldownState.SkillReadyTimestamp, beatType);
            return false;
        }

        /// <summary>
        /// 指定したスキルIDに対応する視覚演出を実行する。
        /// </summary>
        /// <param name="skillId">実行するスキルのID</param>
        private void ExecuteVisual(int skillId, string animationKey = null)
        {
            _skillVisual?.Execute();
        }

        private readonly SkillResultPresenter _presenter;
        private readonly SkillInputProgressController _progressController;
        private readonly SkillCooldownState _skillCooldownState;
        private readonly SkillUsecase _skillUseCase;
        private readonly SkillCheckService _skillCheckService;
        private ISkillVisual _skillVisual;
        private SkillDefinition _skillDefinition;
        private SkillRhythmState _skillRhythmState;
    }
}
