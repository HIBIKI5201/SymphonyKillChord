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
            _skillCash = new SkillDefinition[skillId.Length];

            for (int i = 0; i < skillId.Length; i++)
            {
                _skillCash[i] = skillRepository.GetSkill(skillId[i]);
            }
        }

        /// <summary>
        ///　スキルが成立しているかどうかを調べる
        /// </summary>
        /// <param name="actionType"></param>
        /// <returns>登録した</returns>
        public int CheckSkill(BattleActionType actionType)
        {
            _musicSyncService.RegisterBattleActionHistory(actionType);

            if (SkillCheckService.TryCheckSkills(
                    _skillCash,
                    _musicSyncService.GetBeatTypeHistory(),
                    out var index, out var type))
            {
                _skillCash[index].SkillExecute();
            }

            return type;
        }

        private readonly IMusicSyncService _musicSyncService;
        private readonly SkillDefinition[] _skillCash;
    }
}