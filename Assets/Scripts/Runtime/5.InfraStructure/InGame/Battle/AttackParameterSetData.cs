using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    /// <summary>
    ///     攻撃のパラメーターセットを保持するScriptableObjectクラス。
    /// </summary>
    [CreateAssetMenu(fileName = "AttackParameterSetData", menuName = "KillChord/Attack/" + nameof(AttackParameterSetData))]
    public class AttackParameterSetData : ScriptableObject
    {
        /// <summary> クリティカルヒットの確率を取得する。 </summary>
        public float CriticalChance => _criticalChance;
        /// <summary> クリティカルダメージの倍率を取得する。 </summary>
        public float CriticalDamageMultiplier => _criticalDamageMultiplier;
        /// <summary> 確定ダメージを取得する。 </summary>
        public float ConfirmedDamage => _confirmedDamage;

        [SerializeField, Tooltip("クリティカルヒットの確率")] private float _criticalChance;
        [SerializeField, Tooltip("クリティカルダメージの倍率")] private float _criticalDamageMultiplier;
        [SerializeField, Tooltip("確定ダメージ")] private float _confirmedDamage;
    }
}
