using KillChord.Runtime.InfraStructure.InGame.Enemy;
using System;
using UnityEngine;

namespace DevelopProducts.Boss
{
    /// <summary>
    ///     ボスの攻撃パターン1種ぶんのインスペクタ設定。
    /// </summary>
    [Serializable]
    public sealed class BossAttackEntry
    {
        [SerializeField, Tooltip("CombatSpec上の攻撃定義インデックス")] private int _attackIndex;
        [SerializeField, Tooltip("この攻撃の発動タイミング（拍子・拍目）")] private EnemyMusicSpecAsset _timingData;
        [SerializeField, Tooltip("実行に使うControllerの種別")] private BossAttackKind _kind;

        /// <summary> CombatSpec上の攻撃定義インデックス。 </summary>
        public int AttackIndex => _attackIndex;
        /// <summary> この攻撃の発動タイミングデータ。 </summary>
        public EnemyMusicSpecAsset TimingData => _timingData;
        /// <summary> 実行に使うControllerの種別。 </summary>
        public BossAttackKind Kind => _kind;
    }
}

