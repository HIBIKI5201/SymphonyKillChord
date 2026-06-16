using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Application.InGame.Skill;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    public class SkillExecutionController
    {
        public SkillExecutionController(SkillResultPresenter presenter, SkillInputProgressController progressController,
            SkillCooldownState skillCooldownState, SkillUsecase skillUseCase, SkillCheckService skillCheckService,
            ISkillVisual skillVisual, SkillDefinition skillDefinition,
            SkillRhythmState skillRhythmState, IMusicSyncService musicSyncService)
        {
            _presenter = presenter;
            _progressController = progressController;
            _skillCooldownState = skillCooldownState;
            _skillUseCase = skillUseCase;
            _skillCheckService = skillCheckService;
            _skillVisual = skillVisual;
            _skillDefinition = skillDefinition;
            _skillRhythmState = skillRhythmState;
            _musicSyncService = musicSyncService;
        }

        public SkillDefinition SkillDefinition => _skillDefinition;

        public bool TryExecuteSkill(BeatType beatType, float now, BattleActionType battleActionType, out string animationKey)
        {
            _musicSyncService.RegisterBattleActionHistory(battleActionType, beatType, now);

            bool inputMaches = false;
            animationKey = null;
            if (_skillCooldownState.IsSkillReady(now))
            {
                _skillRhythmState.Enqueue(beatType, now, battleActionType);
                ReadOnlySpan<BeatType> inputHistory = _skillRhythmState.GetHistoryBeatType();
                inputMaches = _skillCheckService.CheckInput(_skillDefinition, inputHistory);
                // TODO スキルUI更新
                if (inputMaches)
                {
                    bool canExecute = _skillUseCase.TryExecuteSkill(_skillDefinition, battleActionType, beatType, now);
                    if (canExecute)
                    {
                        ExecuteVisual(_skillDefinition.Id.Value, _skillDefinition.AnimationKey);
                        _presenter.Push(_skillDefinition);
                        // TODO スキルUI発動関連
                        _skillRhythmState.Clear();
                        _skillCooldownState.SetSkillCooldown(now); 
                        return true;
                    }
                }
            }
            Debug.Log($"[SkillExecutionController] クールダウン中。ID：{_skillDefinition.Id.Value}");
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
        private readonly IMusicSyncService _musicSyncService;
        private ISkillVisual _skillVisual;
        private SkillDefinition _skillDefinition;
        private SkillRhythmState _skillRhythmState;
    }
}
