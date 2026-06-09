using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DevelopProducts.BindingSystem
{
    public class BindingSystem : MonoBehaviour
    {
        private void Awake()
        {

        }

        [SerializeField] private InputActionAsset _inputActionAsset;
        [SerializeField] private Image _settingPanel;
        [SerializeField] private Image _volumePanel;
        [SerializeField] private Image _keyBindingPanel;
    }
}
