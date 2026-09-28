using UnityEngine;

namespace KillChord.Runtime.Domain.OutGame.SkillTree
{
    /// <summary>
    ///     【一時】プレイヤーステータスを保持するEntity。
    ///     今後はこれを捨て、アウトゲームのプレイヤーデータと連携する。
    /// </summary>
    public class PlayerStatusEntity
    {
        /// <summary>
        ///     体力・攻撃力・会心率を指定して生成する。
        /// </summary>
        public PlayerStatusEntity(float playerHealth, float playerAttack, float cricitalChance)
        {
            _playerHealth = playerHealth;
            _playerAttack = playerAttack;
            _criticalChance = cricitalChance;
        }

        /// <summary> プレイヤーの体力。 </summary>
        public float PlayerHealth => _playerHealth;
        /// <summary> プレイヤーの攻撃力。 </summary>
        public float PlayerAttack => _playerAttack;
        /// <summary> プレイヤーの会心率。 </summary>
        public float CriticalChance => _criticalChance;

        private float _playerHealth;
        private float _playerAttack;
        private float _criticalChance;
    }
}
