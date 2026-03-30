using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    /// <summary>
    ///     攻撃定義の初期設定値を保持するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(AttackDefinitionData), 
        menuName = "KillChord/Attack")]
    public class AttackDefinitionData : ScriptableObject
    {
        public AttackId AttackId => _attackId;
        public float BaseDamage => _baseDamage;

        [SerializeField] private AttackId _attackId;
        [SerializeField] private float _baseDamage;
    }
}
