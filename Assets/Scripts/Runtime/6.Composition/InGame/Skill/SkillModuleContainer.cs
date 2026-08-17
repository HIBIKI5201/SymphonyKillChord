using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Application.InGame.Battle;

namespace KillChord.Runtime.Composition.InGame.Skill
{
    /// <summary>
    ///     スキルモジュールの公開物を保持するContainerです。
    /// </summary>
    public sealed class SkillModuleContainer
    {
        /// <summary>
        ///     Containerを生成します。
        /// </summary>
        /// <param name="skillResultViewModel"> スキル結果ViewModelです。 </param>
        public SkillModuleContainer(ISkillResultViewModel skillResultViewModel)
        {
            SkillResultViewModel = skillResultViewModel;
        }

        /// <summary> スキル制御Controllerです。 </summary>
        public SkillController SkillController { get; private set; }

        /// <summary> スキル結果ViewModelです。 </summary>
        public ISkillResultViewModel SkillResultViewModel { get; }

        /// <summary> 攻撃後効果Serviceです。 </summary>
        public PendingAttackEffectService PendingAttackEffectService { get; private set; }

        /// <summary>
        ///     公開するControllerを設定します。
        /// </summary>
        /// <param name="skillController"> スキル制御Controllerです。 </param>
        public void SetSkillController(SkillController skillController)
        {
            SkillController = skillController;
        }

        /// <summary>
        ///     攻撃後効果Serviceを設定します。
        /// </summary>
        /// <param name="pendingAttackEffectService"> 攻撃後効果Serviceです。 </param>
        public void SetPendingAttackEffectService(PendingAttackEffectService pendingAttackEffectService)
        {
            PendingAttackEffectService = pendingAttackEffectService;
        }
    }
}
