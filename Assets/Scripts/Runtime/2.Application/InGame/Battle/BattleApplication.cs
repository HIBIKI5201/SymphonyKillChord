using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Application
{
    public sealed class BattleApplication
    {
        public BattleApplication(
            CharacterEntity characterEntity,
            AttackExecutor attackExecutor
            )
        {
            _characterEntity = characterEntity;
            _attackExecutor = attackExecutor;
        }

        public IHitTarget hitTarget => _characterEntity;

        public void Attack(IHitTarget toTarget)
        {
            _attackExecutor.Execute(_characterEntity, toTarget, _attackId);
        }
        public void ChangeAttackID(AttackId value)
            => _attackId = value;

        private AttackId _attackId;
        private readonly CharacterEntity _characterEntity;
        private readonly AttackExecutor _attackExecutor;
    }
}
