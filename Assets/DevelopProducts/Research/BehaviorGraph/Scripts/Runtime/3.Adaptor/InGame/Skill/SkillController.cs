using DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Music;
using DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Skill;
using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Battle;
using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Skill;

namespace DevelopProducts.BehaviorGraph.Runtime.Adaptor.InGame.Skill
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
        public SkillController(
            IMusicSyncService musicSyncService,
            int[] skillId = null)
        {
            _musicSyncService = musicSyncService;
            skillId ??= new[] { 0 };
            _skillCache = new SkillDefinition[skillId.Length];
        }


        public bool CheckSkill(BattleActionType actionType, int beatType, float unscaledTime)
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