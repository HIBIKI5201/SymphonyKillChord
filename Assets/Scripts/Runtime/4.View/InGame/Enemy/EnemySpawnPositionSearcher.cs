using System;
using UnityEngine;
using UnityEngine.AI;

namespace KillChord.Runtime.View.InGame.Enemy
{
    public class EnemySpawnPositionSearcher : MonoBehaviour
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="playerTransform"></param>
        public void Initialize(UnityEngine.Camera camera, Transform playerTransform)
        {
            _camera = camera;
            _playerTransform = playerTransform;
            cameraPlanes = new Plane[CAMERA_PLANES_COUNT];
            _path = new NavMeshPath();
            _candidatePositions = new Vector3[_samplingCount];
        }

        /// <summary>
        ///     敵を生成できる位置を探索する。
        /// </summary>
        /// <param name="distance">プレイヤーからの距離</param>
        /// <param name="positions">生成位置を格納する配列</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void FindSpawnPositions(float distance, Vector3[] positions)
        {
            if(positions == null)
            {
                throw new ArgumentNullException("positions", "生成位置配列がNULL。");
            }
            int posIndex = 0;

            // プレイヤーを中心に、一定距離の円周上に候補位置を取得
            for (int i = 0; i < _candidatePositions.Length; i++)
            {
                float angle = i * Mathf.PI * 2 / _samplingCount;

                Vector3 dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                Vector3 candidate = _playerTransform.position + dir * distance;
                _candidatePositions[i] = candidate;
            }

            for (int i = 0; i < _candidatePositions.Length; i++)
            {
                // 候補位置がCameraに映っている場合、スキップする
                GeometryUtility.CalculateFrustumPlanes(_camera, cameraPlanes);
                Bounds bounds = new Bounds(_candidatePositions[i], Vector3.one * 0.5f);
                if (GeometryUtility.TestPlanesAABB(cameraPlanes, bounds))
                {
                    continue;
                }

                // 候補位置がプレイヤーに到達できない場合、スキップする
                if (NavMesh.SamplePosition(_candidatePositions[i], out NavMeshHit hit, 1f, NavMesh.AllAreas))
                {
                    NavMesh.CalculatePath(_candidatePositions[i], _playerTransform.position, NavMesh.AllAreas, _path);
                    if (_path.status != NavMeshPathStatus.PathComplete)
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }
                // 見つかった位置を引数に設定する
                positions[posIndex] = _candidatePositions[i];
                posIndex++;
                if(posIndex >= positions.Length)
                {
                    return;
                }
            }
            // 見つかった位置の数が足りない場合、最後に見つかった位置を不足分に設定する
            if(posIndex < positions.Length)
            {
                // 一つも見つからなかった場合、既定位置を設定する
                if (posIndex == 0)
                {
                    positions[0] = _defaultPosition.position;
                }
                for(int i = posIndex + 1; i < positions.Length; i++)
                {
                    positions[i] = positions[posIndex];
                }
            }
        }

        /// <summary> カメラ視野を表す平面の数。探索用 </summary>
        private const int CAMERA_PLANES_COUNT = 6;
        
        [Header("性能調整")]
        [SerializeField, Tooltip("探索の侯選ポジション数")]
        private int _samplingCount = 36;
        [Space]
        [SerializeField, Tooltip("生成位置が見つからない場合の位置")]
        private Transform _defaultPosition;

        private Vector3[] _candidatePositions;
        private NavMeshPath _path;
        private Plane[] cameraPlanes;
        private Transform _playerTransform;
        private UnityEngine.Camera _camera;

#if UNITY_EDITOR
        [Header("デバッグ用")]
        [SerializeField] private bool _drawGizmoFlg = false;
        private void OnDrawGizmos()
        {
            if (!_drawGizmoFlg) return;
            Gizmos.color = Color.yellow;
            for(int i = 0; i < _candidatePositions?.Length; i++)
            {
                Gizmos.DrawCube(_candidatePositions[i], Vector3.one / 2);
            }
        }
    }
#endif
}
