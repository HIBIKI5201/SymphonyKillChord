using KillChord.Runtime.Utility;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Enemy
{
    /// <summary>
    ///     敵移動のデータを保持するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(EnemyMoveData), menuName = PathConst.CREATE_ASSET_MENU_PATH + "Enemy/" + nameof(EnemyMoveData))]
    public class EnemyMoveData : ScriptableObject
    {
        /// <summary> 移動速度 </summary>
        public float MoveSpeed => _moveSpeed;
        /// <summary> 最小攻撃距離 </summary>
        public float AttackRangeMin => _attackRangeMin;
        /// <summary> 最大攻撃距離 </summary>
        public float AttackRangeMax => _attackRangeMax;

        [SerializeField, Tooltip("移動速度")] private float _moveSpeed;
        [SerializeField, Tooltip("最小攻撃距離")] private float _attackRangeMin;
        [SerializeField, Tooltip("最大攻撃距離")] private float _attackRangeMax;
    }
}
