using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Battle
{
    /// <summary>
    ///     攻撃のパラメーターセットを保持するScriptableObjectクラス。
    /// </summary>
    [CreateAssetMenu(fileName = "AttackSpecAsset", menuName = "KillChord/Attack/" + nameof(AttackSpecAsset))]
    public class AttackSpecAsset : ScriptableObject
    {
        /// <summary> 確定ダメージを取得する。 </summary>
        public float ConfirmedDamage => _confirmedDamage;

        [SerializeField, Tooltip("確定ダメージ")] private float _confirmedDamage;
    }
}

