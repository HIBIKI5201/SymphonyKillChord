using UnityEngine;

namespace KillChord.Runtime.Domain.OutGame.SkillTree
{
    /// <summary>
    ///     【一時】プレイヤーステータスを保持するEntity。
    ///     今後はこれを捨て、アウトゲームのプレイヤーデータと連携する。
    /// </summary>
    public class PlayerStatusEntity
    {
        public PlayerStatusEntity(float playerHealth, float playerAttack, float cricitalChance)
        {
            _playerHealth = playerHealth;
            _playerAttack = playerAttack;
            _criticalChance = cricitalChance;
        }
        public float PlayerHealth => _playerHealth;
        public float PlayerAttack => _playerAttack;
        public float CriticalChance => _criticalChance;

        private float _playerHealth;
        private float _playerAttack;
        private float _criticalChance;
    }
}
