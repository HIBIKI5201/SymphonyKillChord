using UnityEngine;
using UnityEngine.UI;

namespace DevelopProducts.SkillTree
{
    public class NodeView : MonoBehaviour
    {
        [SerializeField] private SkillNodeAsset _skillNodeAsset;
        private Image _icon;
        public void Canlock(bool canlock)
        {
            if (canlock)
            _icon.color = Color.yellow;
        }
        public void Unlock()
        {
            _icon.color = Color.green;
        }
        private void Awake()
        {
            _icon = GetComponent<Image>();
        }
    }
}
