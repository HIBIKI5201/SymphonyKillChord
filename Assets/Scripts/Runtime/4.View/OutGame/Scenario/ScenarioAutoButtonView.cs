using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View.OutGame.Scenario
{
    /// <summary>
    ///     シナリオの自動送り状態をAutoボタンの配色へ反映する。
    /// </summary>
    [DisallowMultipleComponent, RequireComponent(typeof(Button))]
    public sealed class ScenarioAutoButtonView : MonoBehaviour
    {
        /// <summary>
        ///     表示モデルを差し替え、現在の自動送り状態へ同期する。
        /// </summary>
        public void Initialize(ScenarioViewModel viewModel)
        {
            Unsubscribe();
            CacheButtonColors();
            _viewModel = viewModel;
            if (isActiveAndEnabled) { Subscribe(); }
        }

        [SerializeField, Tooltip("自動送りが有効な時のAutoボタン色。")]
        private Color _enabledColor = Color.cyan;

        private Button _button;
        private ColorBlock _defaultColors;
        private ColorBlock _enabledColors;
        private ScenarioViewModel _viewModel;
        private bool _isSubscribed;

        /// <summary>
        ///     有効化時に購読し直し、非表示中の状態変更を反映する。
        /// </summary>
        private void OnEnable()
        {
            Subscribe();
        }

        /// <summary>
        ///     無効化時は購読を解除し、元の配色へ戻す。
        /// </summary>
        private void OnDisable()
        {
            Unsubscribe();
            if (_button != null) { _button.colors = _defaultColors; }
        }

        /// <summary>
        ///     破棄時に表示モデルへの購読を解除する。
        /// </summary>
        private void OnDestroy()
        {
            Unsubscribe();
        }

        /// <summary>
        ///     自動送り状態の変更をボタンへ反映する。
        /// </summary>
        private void AutoAdvanceChangedHandler()
        {
            ApplyAutoAdvanceState();
        }

        /// <summary>
        ///     初期配色と有効時の配色を一度だけ保存する。
        /// </summary>
        private void CacheButtonColors()
        {
            if (_button != null) { return; }
            _button = GetComponent<Button>();
            _defaultColors = _button.colors;
            _enabledColors = CreateEnabledColors(_defaultColors, _enabledColor);
        }

        /// <summary>
        ///     状態変更を購読し、現在値も直ちに描画する。
        /// </summary>
        private void Subscribe()
        {
            if (_viewModel == null || _isSubscribed) { return; }
            _viewModel.OnAutoAdvanceChanged += AutoAdvanceChangedHandler;
            _isSubscribed = true;
            ApplyAutoAdvanceState();
        }

        /// <summary>
        ///     古い表示モデルからの通知を解除する。
        /// </summary>
        private void Unsubscribe()
        {
            if (!_isSubscribed) { return; }
            _viewModel.OnAutoAdvanceChanged -= AutoAdvanceChangedHandler;
            _isSubscribed = false;
        }

        /// <summary>
        ///     生成済みの配色を選び、Button自身の遷移処理へ渡す。
        /// </summary>
        private void ApplyAutoAdvanceState()
        {
            _button.colors = _viewModel.IsAutoAdvance ? _enabledColors : _defaultColors;
        }

        /// <summary>
        ///     押下・無効状態などの設定を維持し、有効時の配色を生成する。
        /// </summary>
        private static ColorBlock CreateEnabledColors(ColorBlock colors, Color enabledColor)
        {
            colors.normalColor = enabledColor;
            colors.highlightedColor = enabledColor;
            colors.selectedColor = enabledColor;
            return colors;
        }
    }
}
