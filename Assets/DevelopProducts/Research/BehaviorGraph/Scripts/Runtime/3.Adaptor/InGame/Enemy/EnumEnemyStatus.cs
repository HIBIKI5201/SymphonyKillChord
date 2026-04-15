using System;
namespace DevelopProducts.BehaviorGraph.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     敵状態を示すEnum。
    /// </summary>
    [Flags]
    public enum EnumEnemyStatus
    {
        /// <summary> 待機 </summary>
        Idle = 0,
        /// <summary> 射程入り </summary>
        TargetInSight = 1 << 0,
        /// <summary> 攻撃可能 </summary>
        TargetAimable = 1 << 1,
        /// <summary> 移動中 </summary>
        Moving = 1 << 2,
        /// <summary> 照準中 </summary>
        Aiming = 1 << 3,
        /// <summary> 攻撃実行中 </summary>
        Attacking = 1 << 4,
        /// <summary> 被弾硬直中 </summary>
        HitStun = 1 << 5,
    }
}