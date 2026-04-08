using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    [CreateAssetMenu(fileName = "AttackParameterSetData", menuName = "KillChord/Attack/" + nameof(AttackParameterSetData))]
    public class AttackParameterSetData : ScriptableObject
    {
        public float CriticalChance => _criticalChance;
        public float CriticalDamageMultiplier => _criticalDamageMultiplier;
        public float ConfirmedDamage => _confirmedDamage;

        [SerializeField] private float _criticalChance;
        [SerializeField] private float _criticalDamageMultiplier;
        [SerializeField] private float _confirmedDamage;
    }
}
