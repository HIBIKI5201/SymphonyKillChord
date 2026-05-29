using TMPro;
using UnityEngine;

namespace DevelopProducts.SkillTree
{
    public class PointChecker : MonoBehaviour
    {
        [SerializeField] private SkillPointRepository _skillPointReposiroty;
        [SerializeField] private TMP_Text _pointText;
        private void LateUpdate()
        {
            _pointText.text = _skillPointReposiroty.GetCurrentPoints().Point.ToString();
        }
    }
}
