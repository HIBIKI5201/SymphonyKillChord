using KillChord.Runtime.Adaptor;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     最も近い攻撃可能な場所を検索するViewクラス。
    /// </summary>
    public class NearestAttackPositionSearchView : MonoBehaviour, INearestAttackPositionSearchViewModel
    {
        public void Initialize()
        {
            _timer = 0;
            Vector3 initPos = transform.position;
            _positionSamples = new Vector3[_samplingCount];
            for(int i = 0; i < _positionSamples.Length; i++)
            {
                _positionSamples[i] = initPos;
            }
            _destinationCache = transform.position;
            _path = new NavMeshPath();
        }
        public Vector3 FindNearestAttackPosition(Vector3 enemyPosition, Vector3 playerPosition, float attackRangeMin)
        {
            if (_timer < _searchInterval)
            {
                return _destinationCache;
            }

            // プレイヤーを中心に、攻撃範囲の円周上にサンプリングポイントを生成
            for (int i = 0; i < _positionSamples.Length; i++)
            {
                float angle = i * Mathf.PI * 2 / _samplingCount;

                Vector3 dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                Vector3 candidate = playerPosition + dir * attackRangeMin;
                _positionSamples[i] = candidate;
            }

            float minDistance = float.MaxValue;
            Vector3 nearestDestination = enemyPosition;

            for (int i = 0; i < _positionSamples.Length; i++)
            {
                // ここの1fは、NavMesh上でのサンプリングの半径。敵身長の2倍が推奨な値らしい 
                NavMesh.SamplePosition(_positionSamples[i], out NavMeshHit hit, 1f, NavMesh.AllAreas);

                // サンプリングポイントからプレイヤーへのパスを計算
                if (_agent.CalculatePath(_positionSamples[i], _path))
                {
                    if (_path.status == NavMeshPathStatus.PathComplete)
                    {
                        float pathLength = 0;
                        // パスの長さを計算
                        for (int j = 1; j < _path.corners.Length; j++)
                        {
                            pathLength += Vector3.Distance(_path.corners[j - 1], _path.corners[j]);
                        }
                        // 最短、かつプレイヤーに直撃できるポジションを目的地とする
                        if (pathLength < minDistance)
                        {
                            if (_raycastView.CheckCanRaycastHitTarget(_positionSamples[i]))
                            {
                                minDistance = pathLength;
                                nearestDestination = _positionSamples[i];
                            }
                        }
                    }
                }
            }
            _destinationCache = nearestDestination;
            _timer = 0;
            return nearestDestination;
        }

        private void Update()
        {
            _timer += Time.deltaTime;
        }
        
        private void OnDrawGizmos()
        {
            if(_positionSamples == null) return;
            for (int i = 0; i < _positionSamples.Length; i++)
            {
                Gizmos.color = new Color(0, 200, 200, 50);
                Gizmos.DrawSphere(_positionSamples[i], 0.1f);
                _raycastView.DrawGizmoLineToTarget(_positionSamples[i]);
            }
            Gizmos.color = new Color(200, 0, 0, 50);
            Gizmos.DrawSphere(_destinationCache, 0.2f);
        }

        [Header("性能調整")]
        [SerializeField, Tooltip("探索の侯選ポジション数")] private int _samplingCount;
        [SerializeField, Tooltip("探索間隔(秒)"), Range(0f,1f)] private float _searchInterval;
        [Space]
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private EnemyRaycastDetectView _raycastView;
        private Vector3[] _positionSamples;
        private NavMeshPath _path;
        private float _timer;
        private Vector3 _destinationCache;
    }
}
