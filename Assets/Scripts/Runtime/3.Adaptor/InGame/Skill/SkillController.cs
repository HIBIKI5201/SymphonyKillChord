using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Adaptor
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
            _skillCash = new SkillDefinition[skillId.Length];

            for (int i = 0; i < skillId.Length; i++)
            {
                _skillCash[i] = skillRepository.GetSkill(skillId[i]);
            }
        }

        public void CheckSkill(ActionType actionType)
        {
            _musicSyncService.RegisterBattleActionHistory(actionType);

            if (SkillCheckService.TryCheckSkills(
                    _skillCash,
                    _musicSyncService.GetBeatTypeHistory(),
                    out var index))
            {
                _skillCash[index].SkillExecute();
            }
        }

        private readonly IMusicSyncService _musicSyncService;
        private readonly SkillDefinition[] _skillCash;
    }
}