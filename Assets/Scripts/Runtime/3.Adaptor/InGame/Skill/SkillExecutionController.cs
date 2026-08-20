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
        /// <summary> 対象不成立時のポリシーです。 </summary>
        private const SkillExecutionFailurePolicy TARGET_REJECT_POLICY = SkillExecutionFailurePolicy.ResetProgressOnly;

        /// <summary>
        ///     コントローラーを初期化します。
        /// </summary>
        public SkillExecutionController(
            SkillResultPresenter presenter,
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
        /// </summary>
        /// <param name="beatType"> 現在ビートです。 </param>
        /// <param name="now"> 現在時刻です。 </param>
        /// <param name="battleActionType"> 行動種別です。 </param>
        /// <param name="isJustHit"> ジャスト入力によるスキル発動かどうか。 </param>
        /// <returns> 実行結果です。 </returns>
        public SkillExecutionResult TryExecuteSkill(BeatType beatType, float now, BattleActionType battleActionType, bool isJustHit)
        {
            if (!_skillCooldownState.IsSkillReady(now))
            {
                Debug.Log($"[SkillExecutionController] クールダウン中。ID：{_skillDefinition.Id.Value}");
                return new SkillExecutionResult(SkillExecutionResultType.CooldownBlocked);
            }

            _skillRhythmState.Enqueue(beatType, now, battleActionType);
            ReadOnlySpan<BeatType> inputHistory = _skillRhythmState.GetHistoryBeatType();
            bool isInputMatch = _skillCheckService.CheckInput(_skillDefinition, inputHistory);

            if (!isInputMatch)
            {
                _progressController.UpdateProgress(now, _skillCooldownState.SkillReadyTimestamp, beatType);
                return new SkillExecutionResult(SkillExecutionResultType.InputProgressed);
            }

            bool canExecute = _skillUseCase.TryExecuteSkill(_skillDefinition, beatType, isJustHit);
            if (canExecute)
            {
                ExecuteVisual();
                _presenter.Push(_skillDefinition);
                _skillRhythmState.Clear();
                _skillCooldownState.SetSkillCooldown(now);
                _progressController.SkillTriggered(now, _skillCooldownState.SkillReadyTimestamp);
                return new SkillExecutionResult(
                    SkillExecutionResultType.Executed,
                    _skillDefinition.AnimationKey,
                    _skillDefinition.EffectSpec.SkillNormalAttackDamagePolicy);
            }

            ApplyFailurePolicy(now);
            Debug.LogWarning(
                $"[{nameof(SkillExecutionController)}] 入力パターンは一致しましたが、" +
                $"対象を解決できないためスキルを実行しませんでした。ID: {_skillDefinition.Id.Value}, " +
                $"TargetingType: {_skillDefinition.EffectSpec.TargetingType}");
            return new SkillExecutionResult(SkillExecutionResultType.RejectedByTargetPolicy);
        }

        /// <summary>
        ///     視覚演出を実行します。
        /// </summary>
        private void ExecuteVisual()
        {
            _skillVisual?.Execute();
        }

        /// <summary>
        ///     失敗時ポリシーに応じて内部状態を更新します。
        /// </summary>
        /// <param name="now"> 現在時刻です。 </param>
        private void ApplyFailurePolicy(float now)
        {
            switch (TARGET_REJECT_POLICY)
            {
                case SkillExecutionFailurePolicy.KeepProgress:
                    return;
                case SkillExecutionFailurePolicy.ResetProgressOnly:
                    _skillRhythmState.Clear();
                    _progressController.ResetProgress(now, _skillCooldownState.SkillReadyTimestamp);
                    return;
                case SkillExecutionFailurePolicy.ResetProgressAndConsumeCooldown:
                    _skillRhythmState.Clear();
                    _skillCooldownState.SetSkillCooldown(now);
                    _progressController.SkillTriggered(now, _skillCooldownState.SkillReadyTimestamp);
                    return;
            }
        }

        private readonly SkillResultPresenter _presenter;
        private readonly SkillInputProgressController _progressController;
        private readonly SkillCooldownState _skillCooldownState;
        private readonly SkillUsecase _skillUseCase;
        private readonly SkillCheckService _skillCheckService;
        private readonly ISkillVisual _skillVisual;
        private readonly SkillDefinition _skillDefinition;
        private readonly SkillRhythmState _skillRhythmState;
    }
}
