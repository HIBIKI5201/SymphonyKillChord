using KillChord.Runtime.Domain.InGame.Enemy;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     砲弾ViewとDomainの仲介。
    /// </summary>
    public class ShellSpecPresenter
    {
        /// <summary>
        ///     砲弾のエンティティを指定して生成する。
        ///     null の場合は例外を投げる。
        /// </summary>
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
