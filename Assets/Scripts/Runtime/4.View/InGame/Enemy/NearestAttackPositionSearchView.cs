using KillChord.Runtime.Adaptor.InGame.Enemy;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     最も近い攻撃可能な場所を検索するViewクラス。
    /// </summary>
    public class NearestAttackPositionSearchView : MonoBehaviour, INearestAttackPositionSearchViewModel
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        public void Initialize()
        {
            _timer = 0;
            Vector3 initPos = transform.position;
            _positionSamples = new Vector3[_samplingCount];
            _pathLengths = new float[_samplingCount];
            _nearOptimalIndices = new List<int>(_samplingCount);
            for (int i = 0; i < _positionSamples.Length; i++)
            {
                _positionSamples[i] = initPos;
            }
            _destinationCache = transform.position;
            _path = new NavMeshPath();
        }
        /// <summary>
        ///     最も近い攻撃可能な位置を探索する。
        /// </summary>
        /// <param name="enemyPosition"></param>
        /// <param name="playerPosition"></param>
        /// <param name="attackRangeMin"></param>
        /// <returns></returns>
        public Vector3 FindNearestAttackPosition(Vector3 enemyPosition, Vector3 playerPosition, float attackRangeMin)
        {
            // 探索間隔最中の場合、キャッシュした位置を返却
            if (_timer < _searchInterval)
            {
                return _destinationCache;
            }

            // プレイヤーを中心に、攻撃範囲の円周上にサンプリングポイントを生成
            // 角度に揺らぎを加え、毎回まったく同じ位置取りにならないようにする
            for (int i = 0; i < _positionSamples.Length; i++)
            {
                float angle = i * Mathf.PI * 2 / _samplingCount
                    + Random.Range(-_angleJitterDegrees, _angleJitterDegrees) * Mathf.Deg2Rad;

                Vector3 dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                Vector3 candidate = playerPosition + dir * attackRangeMin;
                _positionSamples[i] = candidate;
                _pathLengths[i] = float.MaxValue;
            }

            float minDistance = float.MaxValue;

            for (int i = 0; i < _positionSamples.Length; i++)
            {
                // ここの1fは、NavMesh上でのサンプリングの半径。敵身長の2倍が推奨な値らしい
                if (!NavMesh.SamplePosition(_positionSamples[i], out NavMeshHit hit, 1f, NavMesh.AllAreas))
                {
                    continue;
                }
                _positionSamples[i] = hit.position;
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
                        // プレイヤーに直撃できるポジションのみ候補として記録する
                        if (_raycastView.CheckCanRaycastHitTarget(_positionSamples[i]))
                        {
                            _pathLengths[i] = pathLength;
                            if (pathLength < minDistance)
                            {
                                minDistance = pathLength;
                            }
                        }
                    }
                }
            }

            // 最短経路そのものではなく、それに近い候補群からランダムに選ぶことで
            // 常に数学的な最適解へ吸い付くような機械的な位置取りを避ける
            Vector3 nearestDestination = minDistance < float.MaxValue
                ? PickRandomNearOptimalPosition(minDistance)
                : enemyPosition;

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
            if (_positionSamples == null) return;
            for (int i = 0; i < _positionSamples.Length; i++)
            {
                Gizmos.color = new Color(0, 200, 200, 50);
                Gizmos.DrawSphere(_positionSamples[i], 0.1f);
#if UNITY_EDITOR
                _raycastView.DrawGizmoLineToTarget(_positionSamples[i]);
#endif
            }
            Gizmos.color = new Color(200, 0, 0, 50);
            Gizmos.DrawSphere(_destinationCache, 0.2f);
        }

        /// <summary>
        ///     最短経路に近い候補の中からランダムに1つ選ぶ。
        /// </summary>
        /// <param name="minDistance">候補群の中の最短経路長。</param>
        private Vector3 PickRandomNearOptimalPosition(float minDistance)
        {
            float threshold = minDistance * _nearOptimalTolerance;
            _nearOptimalIndices.Clear();
            for (int i = 0; i < _pathLengths.Length; i++)
            {
                if (_pathLengths[i] <= threshold)
                {
                    _nearOptimalIndices.Add(i);
                }
            }
            int pickedIndex = _nearOptimalIndices[Random.Range(0, _nearOptimalIndices.Count)];
            return _positionSamples[pickedIndex];
        }

        [Header("性能調整")]
        [SerializeField, Tooltip("探索の侯選ポジション数")] private int _samplingCount;
        [SerializeField, Tooltip("探索間隔(秒)"), Range(0f, 1f)] private float _searchInterval;
        [Header("人間味調整")]
        [SerializeField, Tooltip("サンプリング角度に加えるランダムな揺らぎ幅(度)"), Range(0f, 45f)]
        private float _angleJitterDegrees = 10f;
        [SerializeField, Tooltip("最短経路の何倍までを「同程度に近い」候補として扱うか"), Range(1f, 1.5f)]
        private float _nearOptimalTolerance = 1.15f;
        [Space]
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private EnemyRaycastDetectView _raycastView;
        private Vector3[] _positionSamples;
        private float[] _pathLengths;
        private List<int> _nearOptimalIndices;
        private NavMeshPath _path;
        private float _timer;
        private Vector3 _destinationCache;
    }
}
