using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopProducts.BindingSystem
{
    /// <summary>
    ///     「←  項目  →」のように選択肢を切り替える設定項目の共通基底クラス。
    /// </summary>
    [Serializable]
    public abstract class SelectSettingItemBase : SettingItemBase, ISelectSetting
    {
        [SerializeField] private TextMeshProUGUI _valueText;
        [SerializeField] protected int _defaultIndex;

        [Header("切り替えボタン")]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _nextButton;

        private int _currentIndex;

        public Array Items => GetItems();
        public int CurrentIndex => _currentIndex;

        /// <summary>選択肢の配列。</summary>
        protected abstract Array GetItems();

        /// <summary>表示用ラベルを取得する。</summary>
        protected abstract string GetLabel(int index);

        /// <summary>値を実システムに反映する処理。</summary>
        protected abstract void ApplyValue(int index);

        /// <summary>
        ///     初期表示時のインデックスを取得する。
        ///     現在のシステム状態に対応するインデックスが存在する場合はそれを返すようにオーバーライドする。
        ///     既定では_defaultIndexを返す。
        /// </summary>
        protected virtual int GetCurrentIndex() => _defaultIndex;

        public override void Initialize(PauseMenuPanelView pauseMenu)
        {
            base.Initialize(pauseMenu);
            _currentIndex = Mathf.Clamp(GetCurrentIndex(), 0, Items.Length - 1);

            if (_backButton != null) _backButton.onClick.AddListener(MoveBack);
            if (_nextButton != null) _nextButton.onClick.AddListener(MoveNext);
        }

        public override void Load() => Apply();

        public override void Apply()
        {
            ApplyValue(_currentIndex);
            Refresh();
        }

        public override void ResetToDefault()
        {
            _currentIndex = Mathf.Clamp(_defaultIndex, 0, Items.Length - 1);
            Apply();
        }

        public void MoveNext()
        {
            _currentIndex = (_currentIndex + 1) % Items.Length;
            Apply();
        }

        public void MoveBack()
        {
            _currentIndex = (_currentIndex - 1 + Items.Length) % Items.Length;
            Apply();
        }

        private void Refresh()
        {
            if (_valueText != null) _valueText.text = GetLabel(_currentIndex);
        }
    }
}
