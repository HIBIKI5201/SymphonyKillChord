using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Enemy
{
    /// <summary>
    ///     ボスの攻撃パターン1種ぶんのインスペクタ設定。
    /// </summary>
    [Serializable]
    public sealed class BossAttackEntry
    {
        public BossAttackEntry(BossAttackKind kind, int attackIndex, EnemyMusicSpec musicSpec)
        {
            _kind = kind;
            _attackIndex = attackIndex;
            _musicSpec = musicSpec;
        }
        /// <summary> 実行に使うControllerの種別。 </summary>
        public BossAttackKind Kind => _kind;
        /// <summary> CombatSpec上の攻撃定義インデックス。 </summary>
        public int AttackIndex => _attackIndex;
        /// <summary> この攻撃の発動タイミングデータ。 </summary>
        public EnemyMusicSpec MusicSpec => _musicSpec;
        
        private BossAttackKind _kind;
        private int _attackIndex;
        private EnemyMusicSpec _musicSpec;

    }
}
