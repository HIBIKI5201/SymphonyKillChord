using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Battle
{
    /// <summary>
    ///     キャラクターの基本的なデータを保持するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(CharacterData),
    menuName = "KillChord/Character?CharacterData")]
    public class CharacterData : ScriptableObject
    {
        public string CharacterName => _characterName;
        public float MaxHealth => _maxHealth;
        public float MoveSpeed => _moveSpeed;
        public float AttackPower => _attackPower;
        public AttackDefinitionData[] AttackDifinitions =>
            _attackDifinitions == null ? null : (AttackDefinitionData[])_attackDifinitions.Clone();

        [SerializeField] private string _characterName;
        [SerializeField] private float _maxHealth;
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _attackPower;
        [SerializeField] private AttackDefinitionData[] _attackDifinitions;
    }
}
