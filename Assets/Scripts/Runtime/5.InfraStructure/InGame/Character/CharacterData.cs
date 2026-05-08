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
        /// <summary> キャラクター名を取得する。 </summary>
        public string CharacterName => _characterName;

        /// <summary> 最大HPを取得する。 </summary>
        public float MaxHealth => _maxHealth;

        /// <summary> 攻撃定義の配列を取得する。 </summary>
        public AttackDefinitionData[] AttackDifinitions =>
            _attackDifinitions == null ? null : (AttackDefinitionData[])_attackDifinitions.Clone();

        [SerializeField, Tooltip("キャラクターの名前。")]
        private string _characterName;

        [SerializeField, Tooltip("キャラクターの最大体力。")]
        private float _maxHealth;

        [SerializeField, Tooltip("キャラクターが使用する攻撃の定義リスト。")]
        private AttackDefinitionData[] _attackDifinitions;
    }
}
