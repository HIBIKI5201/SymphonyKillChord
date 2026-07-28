using System;
using TMPro;
using UnityEngine;

namespace DevelopProducts.BindingSystem
{
    /// <summary>
    ///     設定項目の共通基底クラス。
    ///     項目名のラベル表示など、共通する処理をまとめる。
    /// </summary>
    [Serializable]
    public abstract class SettingItemBase : ISettingItem
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private string _displayName;

        public virtual void Initialize(PauseMenuPanelView pauseMenu)
        {
            if (_nameText != null) _nameText.text = _displayName;
        }

        public abstract void Load();
        public abstract void Apply();
        public abstract void ResetToDefault();
    }
}
