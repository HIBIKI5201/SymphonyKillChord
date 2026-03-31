using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Battle;

namespace KillChord.Runtime.Adaptor.InGame.Battle
{
    public sealed class BattleController
    {
        public BattleController(
            BattleApplication damageApplication,
            AttackCommandState attackCommandState,
            DamagePresenter damagePresenter)
        {
            _application = damageApplication;
            _commandState = attackCommandState;
            _damagePresenter = damagePresenter;
        }
        public BattleApplication DamageApplication => _application;

        public void Attack(BattleController toTarget)
        {
            if (toTarget is null)
                throw new System.ArgumentNullException(nameof(toTarget));

            IHitTarget hitTarget = toTarget.DamageApplication.hitTarget;
            _application.Attack(hitTarget);

            toTarget.UpdateView();
        }
        public void UpdateView()
        {
            _damagePresenter?.Push();
        }
        public void ChangeAttackID(AttackCommandType type)
        {
            _commandState.SelectAttack(type);
            _application.ChangeAttackID(_commandState.SelectedAttackId);
        }

        private readonly BattleApplication _application;
        private readonly AttackCommandState _commandState;
        private readonly DamagePresenter _damagePresenter;
    }
}
