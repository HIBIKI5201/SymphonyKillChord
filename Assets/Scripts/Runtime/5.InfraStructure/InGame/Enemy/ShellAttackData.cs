using KillChord.Runtime.Utility;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Enemy
{
    /// <summary>
    ///     砲弾固有の攻撃関連情報。
    /// </summary>
    [CreateAssetMenu(fileName = "ShellAttackData", menuName = PathConst.CREATE_ASSET_MENU_PATH + "Enemy/" + nameof(ShellAttackData))]
    public class ShellAttackData : ScriptableObject
    {
        /// <summary>
        ///     爆発半径。
        /// </summary>
        public float ExplosionRadius => _explosionRadius;

        [SerializeField, Range(0f, 32f), Tooltip("爆発半径")]
        private float _explosionRadius;
    }
}
