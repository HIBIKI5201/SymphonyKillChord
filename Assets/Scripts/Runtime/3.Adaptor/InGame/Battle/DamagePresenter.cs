using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Adaptor
{
    public sealed class DamagePresenter
    {
        public DamagePresenter(IViewModelDamage viewModel, CharacterEntity characterEntity)
        {
            _viewModel = viewModel;
            _characterEntity = characterEntity;
        }

        public void Push()
        {
            HealthEntity entity = _characterEntity.Health;
            _viewModel.OnDamage(new(entity.MaxHealth, entity.CurrentHealth)); ;
        }

        private readonly CharacterEntity _characterEntity;
        private readonly IViewModelDamage _viewModel;
    }
}
