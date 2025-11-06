using UnityEngine;
using UnityEngine.UI;

namespace Mock.TPS
{
    public class HealthbarManager : MonoBehaviour
    {
        [SerializeField]
        private Image _healthBar;

        public void SetHealthBar(float currentHealth, float maxHealth)
        {
            _healthBar.fillAmount = currentHealth / maxHealth;
        }
    }
}
