using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.View.InGame.Sequence;
using R3;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View.InGame.Mission
{
    /// <summary>
    ///     会話UIのViewクラス。
    /// </summary>
    public sealed class MissionDialogueView : MonoBehaviour, IGameplayControllable
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        public void Initialize(MissionDialogueViewModel viewModel, MissionDialogueController controller)
        {
            _subscription?.Dispose();
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _wasVisible = false;
            _animation.HideImmediate();
            _subscription = _viewModel.Revision.Subscribe(_ => Refresh());
        }

        /// <summary>
        ///     終了時の購読と演出を解除する。
        /// </summary>
        public void Shutdown()
        {
            _subscription?.Dispose();
            _subscription = null;
            _controller = null;
            _viewModel = null;
            _wasVisible = false;
            _animation.HideImmediate();
        }

        /// <inheritdoc />
        public void StartGameplay()
        {
            _controller?.StartGameplay(); 
        }

        /// <inheritdoc />
        public void StopGameplay()
        {
            _controller?.StopGameplay();
        }

        [SerializeField, Tooltip("会話テキスト")]
        private TMP_Text _text;
        [SerializeField, Tooltip("顔画像")]
        private Image _portrait;
        [SerializeField, Tooltip("会話UI演出コンポーネント")]
        private MissionDialogueAnimationBase _animation;
        [SerializeField, Tooltip("会話UIのCanvasGroup")]
        private CanvasGroup _canvasGroup;

        private MissionDialogueViewModel _viewModel;
        private MissionDialogueController _controller;
        private IDisposable _subscription;
        private bool _wasVisible;

        private void Awake()
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _animation.HideImmediate();
        }

        private void Update()
        {
            _controller?.Tick(Time.unscaledDeltaTime);
        }

        private void OnDisable()
        {
            _controller?.StopGameplay();
        }

        private void OnDestroy()
        {
            _subscription?.Dispose();
        }

        /// <summary>
        ///     会話UI内容を更新する。
        /// </summary>
        private void Refresh()
        {
            _text.text = _viewModel.Text ?? string.Empty;
            _portrait.sprite = _viewModel.Portrait;
            _portrait.enabled = _viewModel.Portrait != null;
            _animation.SetPaused(_viewModel.IsPaused);
            bool wasVisible = _wasVisible;
            _wasVisible = _viewModel.IsVisible;

            if (_viewModel.IsImmediate)
            {
                _animation.HideImmediate();
            }
            else if (_wasVisible && !wasVisible)
            {
                _animation.Show();
            }
            else if (!_wasVisible && wasVisible)
            {
                int version = _viewModel.Version;
                _animation.Hide(() => _controller?.NotifyHidden(version));
            }
        }
    }
}
