using DevelopProducts.BehaviorGraph.Runtime.Adaptor;
using System;
using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.View
{
    /// <summary>
    ///     攻撃結果表示用の状態を管理するViewModelクラス。
    /// </summary>
    public class AttackResultViewModel : IAttackResultViewModel
    {
        public event Action<float, bool> OnChanged;

        public float Damage { get; private set; }
        public bool IsCritical { get; private set; }

        /// <summary>
        ///     DTOを受け取って状態を更新し、OnChangedイベントを発火させるメソッド。
        /// </summary>
        /// <param name="dto"></param>
        public void Push(in AttackResultDTO dto)
        {
            Damage = dto.Damage;
            IsCritical = dto.IsCritical;
            OnChanged?.Invoke(Damage, IsCritical);
        }
    }
}
