using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Application.InGame.Skill;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using System.Diagnostics;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    public class SkillController
    {
        public SkillController(
            ISkillRepository skillRepository,
            IMusicSyncService musicSyncService,
            SkillCheckService skillCheckService,
            int[] skillId = null,
            SkillResultPresenter presenter = null)
        {
            _musicSyncService = musicSyncService;
            _skillCheckService = skillCheckService;
            skillId ??= new[] { 0 };
            _skillCache = new SkillDefinition[skillId.Length];

            for (int i = 0; i < skillId.Length; i++)
            {
                _skillCache[i] = skillRepository.GetSkill(skillId[i]);
            }

            _presenter = presenter;
        }

        public bool CheckSkill(BattleActionType actionType, BeatType beatType, float unscaledTime)
        {
            _musicSyncService.RegisterBattleActionHistory(actionType, beatType, unscaledTime);

            if (_skillCheckService.TryCheckSkills(
                    _skillCache,
                    _musicSyncService.GetBeatTypeHistory(),
                    out var index, out _))
            {
                _skillCache[index].SkillExecute();
                _presenter?.Push(_skillCache[index]);
                return true;
            }

            return false;
        }

        private readonly IMusicSyncService _musicSyncService;
        private readonly SkillDefinition[] _skillCache;
        private readonly SkillResultPresenter _presenter;
        private readonly SkillCheckService _skillCheckService;
    }
}