using Codice.Client.Common;
using UnityEngine;

namespace Mock.MusicBattle.Enemy
{
    [CreateAssetMenu(fileName = "EnemySpawnSO", menuName = "Mock/MusicBattle/Enemy/EnemySpawnSO")]
    public class EnemySpawnSO : ScriptableObject
    {
        public float XRange;
        public float YRange;
        public float ZRange;
        public int MaxEnemyCount;
    }
}
