using SymphonyFrameWork.Attribute;
using UnityEngine;

namespace DevelopProducts.BindingSystem
{
    /// <summary>
    ///     パネル全体の管理を行うクラス
    /// </summary>
    public class SettingPanelView : MonoBehaviour
    {
        /// <summary>
        ///   すべての設定項目に対してApplyを呼び出す
        /// </summary>
        public void ApplyAll()
        {
            foreach (var item in _settings)
            {
                item.Apply();
            }
        }
        /// <summary>
        ///     すべての設定項目に対してLoadを呼び出す
        /// </summary>
        public void LoadAll()
        {
            foreach (var item in _settings)
            {
                item.Load();
            }
        }
        /// <summary>
        ///     すべての設定項目に対してResetToDefaultを呼び出す
        /// </summary>
        public void ResetAllToDefault()
        {
            foreach (var item in _settings)
            {
                item.ResetToDefault();
            }
        }
        /// <summary>
        ///    パネルの表示・非表示を切り替える
        /// </summary>
        /// <param name="active"></param>
        public void SetPanelActive(bool active)
        {
            this.gameObject.SetActive(active);
        }
        /// <summary>
        ///   パネル内の各設定項目を初期化する。必要に応じてPauseMenuPanelViewを渡して、
        ///   項目間の連携を行うことも可能。
        /// </summary>
        /// <param name="pauseMenu"></param>
        public void Initialize(PauseMenuPanelView pauseMenu)
        {
            foreach (var item in _settings)
            {
                item.Initialize(pauseMenu);
            }
            this.gameObject.SetActive(false);
            ApplyAll();
        }
        [SubclassSelector, SerializeReference] private ISettingItem[] _settings;
    }
}
