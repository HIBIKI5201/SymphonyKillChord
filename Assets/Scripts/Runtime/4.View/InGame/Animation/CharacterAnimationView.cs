using KillChord.Runtime.Adaptor;
using UnityEngine;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     PlayableAnimationControllerを所有してライフサイクルを管理するViewクラス。
    ///     AdaptorからDTOを受け取りViewModelを介してPlayableに反映する。
    ///     ICharacterAnimationControllerを介するため、プレイヤー・敵どちらにも適用可能。
    /// </summary>
    public sealed class CharacterAnimationView : MonoBehaviour
    {
        /// <summary>
        ///     依存コンポーネントを注入してPlayableAnimationControllerを構築する。
        /// </summary>
        /// <param name="controller"> アニメーション操作を委譲するAdaptor。 </param>
        /// <param name="repository"> クリップをStateで取得するリポジトリ。 </param>
        public void Initialize(ICharacterAnimationController controller, AnimationClip[] clips)
        {
            _controller = controller;
            _viewModel = new CharacterAnimationViewModel();

            if (_animator == null)
                _animator = GetComponent<Animator>();

            _playableController = new PlayableAnimationController(_animator, clips);
            _isInitialized = true;
            _controller.OnOneShotRequested += HandleOneShotRequested;
        }

        /// <summary> 毎フレームDTOを取得してViewModelを更新しPlayableに反映する。 </summary>
        private void Update()
        {
            if (!_isInitialized)
            {
                return;
            }

            // AdaptorからDTOを取得してViewModelへ反映する。
            CharacterAnimationDTO dto = _controller.GetDTO();
            _viewModel.Apply(in dto);

            // 攻撃オーバーレイ処理（ランプアップ → フェードアウト）。
            if (_attackOverlayRemaining > 0f && _viewModel.Weights != null)
            {
                float elapsed = _attackOverlayDuration - _attackOverlayRemaining;
                float rampUp = Mathf.Max(0.001f, _attackOverlayDuration * _attackRampUpRatio);
                float weight;
                if (elapsed < rampUp)
                {
                    // ランプアップ（0 -> 1）。
                    weight = Mathf.InverseLerp(0f, rampUp, elapsed);
                }
                else
                {
                    // ランプダウン（1 -> 0）。
                    float downElapsed = elapsed - rampUp;
                    float downDuration = Mathf.Max(0.0001f, _attackOverlayDuration - rampUp);
                    weight = Mathf.Lerp(1f, 0f, downElapsed / downDuration);
                }

                // 現在のウェイトと今回計算したウェイトを比較して高い方を採用する（重ね掛け防止）。
                _viewModel.Weights[_overlayIndex] = Mathf.Max(_viewModel.Weights[_overlayIndex], weight);

                float otherScale = 1f - weight;
                for (int i = 0; i < _viewModel.Weights.Length; i++)
                {
                    if (i == _overlayIndex) continue;
                    _viewModel.Weights[i] *= otherScale;
                }

                _attackOverlayRemaining -= Time.deltaTime;
            }

            // ViewModelの値をPlayableに反映する
            _playableController.SetAnimationSpeed(_viewModel.AnimationSpeed);

            // ウェイトをインデックス順に反映する
            for (int i = 0; i < _viewModel.Weights.Length; i++)
            {
                _playableController.SetWeight(i, _viewModel.Weights[i]);
            }
        }

        /// <summary> ワンショット再生要求イベントを処理する。 </summary>
        private void HandleOneShotRequested(int index)
        {
            //再生。
            _playableController.PlayOneShot(index);

            // クリップ長を取り、現在のアニメ速度で割る → 実再生時間にする。
            float clipLen = _playableController.GetClipLength(index);
            float speed = Mathf.Max(0.0001f, _viewModel.AnimationSpeed);
            _attackOverlayDuration = clipLen / speed;
            _attackOverlayRemaining = _attackOverlayDuration;
            _overlayIndex = index;
        }

        /// <summary> PlayableGraphを破棄する。 </summary>
        private void OnDestroy()
        {
            _playableController?.Dispose();
            if (_controller != null)
            {
                _controller.OnOneShotRequested -= HandleOneShotRequested;
            }
        }

        // 立ち上げ比率（総時間に対する）。
        [SerializeField] private float _attackRampUpRatio = 0.25f;
        [SerializeField] private Animator _animator;

        private PlayableAnimationController _playableController;
        private CharacterAnimationViewModel _viewModel;
        private ICharacterAnimationController _controller;
        private bool _isInitialized;

        // 攻撃オーバーレイ時間（実行時に clip.length / animationSpeed で設定される）
        private float _attackOverlayDuration;
        private float _attackOverlayRemaining;

        private int _overlayIndex;
    }
}
