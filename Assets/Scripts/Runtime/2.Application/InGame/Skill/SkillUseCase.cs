using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Application.InGame.Skill;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using System.Collections.Generic;

namespace KillChord.Runtime.Application
{
    /// <summary>
    ///     スキル発動の判定と実行を行うユースケース。
    /// </summary>
    public class SkillUsecase
    {
        public SkillUsecase(
            IMusicSyncService musicSyncService,
            SkillCheckService skillCheckService,
            IViewAction viewAction)
        {
            _musicSyncService = musicSyncService;
            _skillCheckService = skillCheckService;
            _viewAction = viewAction;
        }

        public bool TryExecuteSkill(
            IReadOnlyList<SkillDefinition> equipmentSkills,
            BattleActionType actionType,
            BeatType beatType,
            float unscaledTime,
            out SkillDefinition executedSkill)
        {
            _musicSyncService.RegisterBattleActionHistory(actionType, beatType, unscaledTime);

            if (_skillCheckService.TryCheckSkills(
                    equipmentSkills,
                    _musicSyncService.GetBeatTypeHistory(),
                    out var index, out _))
            {
                executedSkill = equipmentSkills[index];
                executedSkill.Effect.Execute();
                _viewAction.Execute(executedSkill.Id.Value);

                return true;
            }

            executedSkill = null;
            return false;
        }

        private readonly IMusicSyncService _musicSyncService;
        private readonly SkillCheckService _skillCheckService;
        private readonly IViewAction _viewAction;
    }
}
