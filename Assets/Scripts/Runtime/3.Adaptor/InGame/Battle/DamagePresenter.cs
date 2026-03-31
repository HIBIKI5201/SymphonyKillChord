using KillChord.Runtime.Domain.InGame.Character;

namespace KillChord.Runtime.Adaptor.InGame.Battle
{
    /// <summary>
    ///     ダメージ情報をViewModelへ伝えるためのプレゼンタークラス。
    /// </summary>
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
