using UnityEngine;

namespace DevelopProducts.SkillTree
{
    [CreateAssetMenu(fileName = "SkillPointRepository", menuName = "Scriptable Objects/SkillPointRepository")]
    public class SkillPointRepository : ScriptableObject, IPointRepository
    {
        public UnlockPoint GetCurrentPoints()
        {
            return new UnlockPoint(_point);
        }

        public void UsePoints(int points)
        {
            _point = _point - points;
        }

        [SerializeField] private int _point;
    }
}
