using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Application.InGame.Skill;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Skill;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     スキルの発動条件の判定や実行を制御するコントローラークラス。
    /// </summary>
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

        /// <summary>
        ///     アクションを履歴に登録し、スキルが成立しているかチェックする。
        /// </summary>
        /// <param name="actionType">登録するアクションの種類</param>
        /// <param name="beatType">計算済みの拍子</param>
        /// <returns>スキルが発動したか</returns>
        public bool TryExecuteSkill(BattleActionType actionType, int beatType)
        {
            _musicSyncService.RegisterBattleActionHistory(actionType, beatType);
            
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