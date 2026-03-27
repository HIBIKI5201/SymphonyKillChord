using KillChord.Runtime.Application;
using UnityEngine;
using UnityEngine.AI;

namespace KillChord.Runtime.InfraStructure
{
    /// <summary>
    ///     敵のナビメッシュエージェント。NavMeshAgentを使用して敵の移動を制御する。
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavMeshEnemyAgent : MonoBehaviour, IEnemyNavigationAgent
    {
        public bool IsReady => _agent != null && _agent.isOnNavMesh;

        public void SetMoveSpeed(float speed)
        {
            _agent.speed = speed;
        }

        public void MoveTo(Vector3 destination)
        {
            _agent.isStopped = false;
            _agent.SetDestination(destination);
        }

        public void Stop()
        {
            _agent.isStopped = true;
        }

        private NavMeshAgent _agent;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        private void Reset()
        {
            _agent = GetComponent<NavMeshAgent>();
        }
    }
}
