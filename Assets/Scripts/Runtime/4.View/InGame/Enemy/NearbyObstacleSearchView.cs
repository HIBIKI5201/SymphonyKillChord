using KillChord.Runtime.Adaptor.InGame.Enemy;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     周囲の障害物を検索するViewクラス。
    /// </summary>
    public class NearbyObstacleSearchView : MonoBehaviour, IObstacleSearchViewModel
    {
        /// <summary>
        ///     指定位置周辺で最も近い障害物の位置を検索する。
        /// </summary>
        /// <param name="sourcePosition"></param>
        /// <param name="obstaclePosition"></param>
        /// <returns> 障害物が見つかった場合はtrue。 </returns>
        public bool TryFindNearestObstaclePosition(Vector3 sourcePosition, out Vector3 obstaclePosition)
        {
            int hitCount = Physics.OverlapSphereNonAlloc(sourcePosition, _searchRadius, _hitColliders, _obstacleLayers);
            if (hitCount <= 0)
            {
                obstaclePosition = sourcePosition;
                return false;
            }

            float minDistanceSqr = float.MaxValue;
            Vector3 nearestPoint = sourcePosition;
            for (int i = 0; i < hitCount; i++)
            {
                Vector3 closestPoint = _hitColliders[i].ClosestPoint(sourcePosition);
                float distanceSqr = (closestPoint - sourcePosition).sqrMagnitude;
                if (distanceSqr < minDistanceSqr)
                {
                    minDistanceSqr = distanceSqr;
                    nearestPoint = closestPoint;
                }
            }

            obstaclePosition = nearestPoint;
            return true;
        }

        [SerializeField, Tooltip("障害物検索半径(m)。"), Range(0f, 30f)]
        private float _searchRadius = 10f;
        [SerializeField, Tooltip("障害物として扱うレイヤー。視線遮蔽(EnemyRaycastDetectViewのHitLayers)と同じレイヤーを設定してください。")]
        private LayerMask _obstacleLayers;
        [SerializeField, Tooltip("OverlapSphereで一度に取得する最大コライダー数。"), Min(1)]
        private int _resultArraySize = 8;
        private Collider[] _hitColliders;

        /// <summary>
        ///     検索結果バッファの初期化処理。
        /// </summary>
        private void Awake()
        {
            _hitColliders = new Collider[_resultArraySize];
        }
    }
}
