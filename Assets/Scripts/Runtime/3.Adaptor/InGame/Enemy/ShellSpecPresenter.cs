using KillChord.Runtime.Domain.InGame.Enemy;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     砲弾ViewとDomainの仲介。
    /// </summary>
    public class ShellSpecPresenter
    {
        public ShellSpecPresenter(ShellEntity entity)
        {
            if(entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "ShellEntityがNULLです。");
            }
            _entity = entity;
        }

        /// <summary> 爆発半径 </summary>
        public float ExplosionRadius => _entity.AttackSpec.ExplosionRadius;

        private ShellEntity _entity;
    }
}
