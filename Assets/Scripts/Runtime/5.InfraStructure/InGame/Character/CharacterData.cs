using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Character
{
    /// <summary>
    ///     キャラクターの基本的なデータを保持するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(CharacterData),
    menuName = "KillChord/Character/CharacterData")]
    public class CharacterData : ScriptableObject
    {
        public string CharacterName => _characterName;
        public float MaxHealth => _maxHealth;
        public AttackDefinitionData[] AttackDifinitions =>
            _attackDifinitions == null ? null : (AttackDefinitionData[])_attackDifinitions.Clone();

        [SerializeField] private string _characterName;
        [SerializeField] private float _maxHealth;
        [SerializeField] private AttackDefinitionData[] _attackDifinitions;
    }
}
