using KillChord.Runtime.Utility.Identity;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     敵生成関連の位置情報を保持するクラス。
    /// </summary>
    public class SpawnPositionPair : MonoBehaviour
    {
        /// <summary> 敵を生成する位置。 </summary>
        public Transform SpawnPosition => _spawnPosition;
        /// <summary> 生成した敵が最初に向かう位置。 </summary>
        public Transform EntryPosition => _entryPosition;
        /// <summary> シーン内でこの生成位置を一意に識別するIDです。 </summary>
        public DataID SpawnPointId => _spawnPointId;
        /// <summary> 使用中であるか </summary>
        public bool IsInUse => _isInUse;

        /// <summary>
        ///     使用中フラグを設定する。
        /// </summary>
        /// <param name="value"></param>
        public void SetInUse(bool value)
        {
            _isInUse = value;
        }

        [SerializeField, Tooltip("場外の出現位置")] private Transform _spawnPosition;
        [SerializeField, Tooltip("入場演出の移動目的地")] private Transform _entryPosition;
        [SerializeField, SourceDataCollection(SPAWN_POINT_COLLECTION_KEY, isSceneScoped: true)]
        [Tooltip("シーン内でこの生成位置を一意に識別するID。Wave設定から候補地として参照されます。")]
        private DataID _spawnPointId;

        /// <summary>
        ///     スポーンポイントIDのCollectionKeyです。SourceDataProviderへは登録せず、
        ///     DataIDRebuildMenuでのハッシュ再計算とWave側の参照でのみ使用します。
        /// </summary>
        public const string SPAWN_POINT_COLLECTION_KEY = "SpawnPoint";
        private bool _isInUse = false;

        /// <summary>
        ///     生成位置から進入位置までの線をシーンビューに表示する。
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Vector3 from = _spawnPosition.position;
            Vector3 to = _entryPosition.position;
            Gizmos.DrawLine(from, to);
        }
    }
}
