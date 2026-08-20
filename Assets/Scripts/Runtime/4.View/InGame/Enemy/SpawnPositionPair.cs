using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     敵生成関連の位置情報を保持するクラス。
    /// </summary>
    public class SpawnPositionPair : MonoBehaviour
    {
        public Transform SpawnPosition => _spawnPosition;
        public Transform EntryPosition => _entryPosition;
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
        private bool _isInUse = false;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Vector3 from = _spawnPosition.position;
            Vector3 to = _entryPosition.position;
            Gizmos.DrawLine(from, to);
        }
    }
}
