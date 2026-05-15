using UnityEngine;
using UnityEngine.UI;

namespace DevelopProducts.SkillTree
{
    public class NodeSelectPanelView : MonoBehaviour
    {

        public void SetNode()
        {

        }

        [SerializeField]private Button _unlockButton;
        private void OnUnlockButtonClicked()
        {
            // Unlock button click logic here
        }
        private void Awake()
        {
            _unlockButton.onClick.AddListener(OnUnlockButtonClicked);
        }
        private void OnDestroy()
        {
            _unlockButton.onClick.RemoveListener(OnUnlockButtonClicked);
        }
    }
}
