using DevelopProducts.Design.GameMode.Domain;
using UnityEngine;

namespace DevelopProducts.Design.GameMode.Application
{
    /// <summary>
    ///     プライヤーにダメージを与えるユースケース。
    /// </summary>
    public class DamagePlayerUsecase
    {
        public DamagePlayerUsecase(StageRuntimeContext runtimeContext)
        {
            _runtimeContext = runtimeContext;
        }

        public void Execute(int damage)
        {
            _runtimeContext.PlayerRuntimeState.TakeDamage(damage);
        }

        private readonly StageRuntimeContext _runtimeContext;
    }
}
