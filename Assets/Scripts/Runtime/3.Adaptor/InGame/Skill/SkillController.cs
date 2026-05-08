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
    /// <summary>
    /// スキルの表示・入力チェックを仲介するコントローラクラス。
    /// </summary>
    public class SkillController : IViewAction
    {
        /// <summary>
        /// コンストラクタ。リポジトリから指定されたスキルを読み込み、視覚演出を登録する。
        /// </summary>
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

        /// <summary>
        /// ユースケースを設定する。
        /// </summary>
        /// <param name="usecase">スキル処理のユースケース</param>
        public void SetUsecase(SkillUsecase usecase)
        {
            _skillUseCase = usecase;
        }

        /// <summary>
        /// 指定された行動と入力でスキルの発動判定を行い、発動した場合は実行する。
        /// </summary>
        /// <returns>スキルが発動した場合はtrue、それ以外はfalse</returns>
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

        /// <summary>
        /// 指定したスキルIDに対応する視覚演出を実行する。
        /// </summary>
        /// <param name="skillId">実行するスキルのID</param>
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