using KillChord.Runtime.Application;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Application.InGame.Skill;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using System.Collections.Generic;
using System.Diagnostics;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    public class SkillController : IViewAction
    {
        public SkillController(
            ISkillRepository skillRepository,
            ISkillVisual[] skillVisuals,
            int[] skillId = null,
            SkillResultPresenter presenter = null)
        {
            skillId ??= new[] { 0 };
            _skillCache = new SkillDefinition[skillId.Length];

            for (int i = 0; i < skillId.Length; i++)
            {
                _skillCache[i] = skillRepository.GetSkill(skillId[i]);
            }

            _skillVisuals = new Dictionary<int, ISkillVisual>();
            if (skillVisuals != null)
            {
                foreach (var visual in skillVisuals)
                {
                    _skillVisuals[visual.Id] = visual;
                }
            }

            _presenter = presenter;
        }

        public void SetUsecase(SkillUsecase usecase)
        {
            _skillUseCase = usecase;
        }

        /// <summary>
        ///     スキル発動をチェックし、実行する。
        /// </summary>
        public bool CheckSkill(BattleActionType actionType, BeatType beatType, float unscaledTime)
        {
            if (_skillUseCase.TryExecuteSkill(
                    _skillCache,
                    actionType,
                    beatType,
                    unscaledTime,
                    out var executedSkill))
            {
                _presenter?.Push(executedSkill);
                return true;
            }

            return false;
        }

        public void Execute(int skillId)
        {
            if (_skillVisuals.TryGetValue(skillId, out var visual))
            {
                visual.Execute();
            }
        }

        private readonly SkillDefinition[] _skillCache;
        private readonly Dictionary<int, ISkillVisual> _skillVisuals;
        private readonly SkillResultPresenter _presenter;
        private SkillUsecase _skillUseCase;
    }
}