using KillChord.Runtime.Domain.InGame.Enemy;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     砲弾ViewとDomainの仲介。
    /// </summary>
    public class ShellSpecPresenter
    {
        public ShellSpecPresenter(ShellEntity entity)
        {
            _entity = entity;
        }

        /// <summary> 爆発半径 </summary>
        public float ExplosionRadius => _entity.AttackSpec.ExplosionRadius;

        private ShellEntity _entity;
    }
}
