using Mock.MusicBattle.Battle;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // OrderByを使用するため追加

namespace Mock.MusicBattle.Develop
{
    /// <summary>
    ///     カメラのデバッグ用のロックオンターゲットのコンテナ。
    /// </summary>
    public class LockOnTargetContainerForCamera : MonoBehaviour, ILockOnTargetContainer
    {
        // CONSTRUCTOR
        // PUBLIC_EVENTS
        #region パブリックプロパティ
        /// <summary>
        ///     すべてのロックオン可能なターゲットの読み取り専用リストを取得します。
        /// </summary>
        public IReadOnlyList<Transform> Targets => _targets;

        /// <summary>
        ///     近い順にソートされたロックオン可能なターゲットの読み取り専用リストを取得します。
        ///     今回はデバッグ用のため、Targetsプロパティをそのまま返します。
        /// </summary>
        public IReadOnlyList<Transform> NearerTargets => Targets.OrderBy(t => Vector3.Distance(transform.position, t.position)).ToList();
        #endregion

        // INTERFACE_PROPERTIES
        // PUBLIC_CONSTANTS
        // PUBLIC_METHODS
        // PUBLIC_INTERFACE_METHODS
        // PUBLIC_ENUM_DEFINITIONS
        // PUBLIC_CLASS_DEFINITIONS
        // PUBLIC_STRUCT_DEFINITIONS
        // CONSTANTS
        #region インスペクター表示フィールド
        /// <summary> ロックオン対象となるTransformのリスト。 </summary>
        [SerializeField, Tooltip("ロックオン対象となるTransformのリスト。")]
        private List<Transform> _targets = new();
        #endregion

        // PRIVATE_FIELDS
        // UNITY_LIFECYCLE_METHODS
        // EVENT_HANDLER_METHODS
        // PROTECTED_INTERFACE_VIRTUAL_METHODS
        // PRIVATE_METHODS
        // PRIVATE_ENUM_DEFINITIONS
        // PRIVATE_CLASS_DEFINITIONS
        // PRIVATE_STRUCT_DEFINITIONS
    }
}
