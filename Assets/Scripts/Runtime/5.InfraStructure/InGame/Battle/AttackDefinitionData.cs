using KillChord.Runtime.InfraStructure.InGame.Battle;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    [CreateAssetMenu(fileName = "AttackDefinitionData", menuName = "KillChord/AttackDefinitionData")]
    public class AttackDefinitionData : ScriptableObject
    {
        public string AttackName => _attackName;
        public float BaseDamage => _baseDamage;
        public AttackParameterSetData AttackParameterSetData => _attackParameterSetData;
        public AttackPipelineAsset AttackPipelineAsset => _attackPipelineAsset;

        [SerializeField] private string _attackName;
        [SerializeField] private float _baseDamage;
        [SerializeField] private AttackParameterSetData _attackParameterSetData;
        [SerializeField] private AttackPipelineAsset _attackPipelineAsset;
    }
}
