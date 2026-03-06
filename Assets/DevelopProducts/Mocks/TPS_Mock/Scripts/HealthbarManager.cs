using UnityEngine;
using UnityEngine.UI;

namespace Mock.TPS
{
    public class HealthbarManager : MonoBehaviour
    {
        [SerializeField]
        private Image _bar;
        [SerializeField]
        private Image _guage;

        public void SetHealthBar(float currentHealth, float maxHealth)
        {
            _guage.fillAmount = currentHealth / maxHealth;
        }

        public void MovePosition(Vector3 worldPosition)
        {
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(worldPosition);
            _bar.rectTransform.position = screenPoint;
        }
    }
}
