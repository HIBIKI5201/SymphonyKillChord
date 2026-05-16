using UnityEngine;

namespace DevelopProducts.SkillTree
{
    [CreateAssetMenu(fileName = "SkillPointReposiroty", menuName = "Scriptable Objects/SkillPointReposiroty")]
    public class SkillPointReposiroty : ScriptableObject, IPointRepository
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
