using KillChord.Runtime.Adaptor.InGame.Battle;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Battle
{
    /// <summary>
    ///     攻撃結果表示用の状態を管理するViewModelクラス。
    /// </summary>
    public class AttackResultViewModel : IAttackResultViewModel
    {
        /// <summary> 攻撃結果が変わったときに発火するイベント。引数はダメージ量と会心かどうか。 </summary>
        public event Action<float, bool> OnChanged;

        /// <summary> 直近の攻撃のダメージ量。 </summary>
        public float Damage { get; private set; }
        /// <summary> 直近の攻撃が会心だったか。 </summary>
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
