using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Application.InGame.Skill;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Persistent.Music;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    public class SkillController
    {
        public SkillController(
            ISkillRepository skillRepository,
            IMusicSyncService musicSyncService,
            int[] skillId = null)
        {
            _musicSyncService = musicSyncService;
            skillId ??= new[] { 0 };
            _skillCache = new SkillDefinition[skillId.Length];

            for (int i = 0; i < skillId.Length; i++)
            {
                _skillCache[i] = skillRepository.GetSkill(skillId[i]);
            }
        }

        public bool CheckSkill(BattleActionType actionType, BeatType beatType, float unscaledTime)
        {
            _musicSyncService.RegisterBattleActionHistory(actionType, beatType, unscaledTime);

            if (SkillCheckService.TryCheckSkills(
                    _skillCache,
                    _musicSyncService.GetBeatTypeHistory(),
                    out var index, out _))
            {
                _skillCache[index].SkillExecute();
                return true;
            }

            return false;
        }

        private readonly IMusicSyncService _musicSyncService;
        private readonly SkillDefinition[] _skillCache;
    }
}