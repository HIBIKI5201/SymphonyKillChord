using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Enemy
{
    /// <summary>
    ///     ステージに事前配置されている敵を管理するクラス。
    /// </summary>
    public class AssignedEnemyManager : MonoBehaviour
    {
        /// <summary>
        ///     事前配置されている歩兵の位置
        /// </summary>
        public Transform[] Infantries => _infantries;
        /// <summary>
        ///     事前配置されている砲兵の位置
        /// </summary>
        public Transform[] Artillery => _artillery;

        [Header("歩兵")]
        [SerializeField] private Transform[] _infantries;
        [Header("砲兵")]
        [SerializeField] private Transform[] _artillery; // なんとartilleryの複数形はartilleryのまま
    }
}
