using KillChord.Runtime.View.InGame.Enemy;
using SymphonyFrameWork.Attribute;
using UnityEngine;

namespace KillChord.Develop
{
    public class SpawnPointsPairDrawer : MonoBehaviour
    {
        [SerializeField]
        private Transform _spawnPointsParent;

        [SerializeField, ReadOnly]
        private SpawnPositionPair[] _spawnPositionPairs;

        private void OnValidate()
        {
            if (_spawnPointsParent == null) { return; }

            _spawnPositionPairs = _spawnPointsParent.GetComponentsInChildren<SpawnPositionPair>();
        }

        private void OnDrawGizmosSelected()
        {
            foreach (SpawnPositionPair pair in _spawnPositionPairs)
            {

                Gizmos.DrawLine(pair.SpawnPosition.position, pair.EntryPosition.position);
            }
        }
    }
}
