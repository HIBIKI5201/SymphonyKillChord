using KillChord.Runtime.Adaptor;
using UnityEngine;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     PlayableAnimationControllerを所有してライフサイクルを管理するViewクラス。
    ///     AdaptorからDTOを受け取りViewModelを介してPlayableに反映する。
    ///     ICharacterAnimationControllerを介するため、プレイヤー・敵どちらにも適用可能。
    /// </summary>
    [RequireComponent(typeof(Animator))]
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

            Animator animator = GetComponent<Animator>();
            _playableController = new PlayableAnimationController(animator, clips);
            _isInitialized = true;
        }

        /// <summary> 毎フレームDTOを取得してViewModelを更新しPlayableに反映する。 </summary>
        private void Update()
        {
            if (!_isInitialized)
            {
                return;
            }

            // AdaptorからDTOを取得してViewModelへ反映する
            CharacterAnimationDTO dto = _controller.GetDTO();
            _viewModel.Apply(in dto);

            // ViewModelの値をPlayableに反映する
            _playableController.SetAnimationSpeed(_viewModel.AnimationSpeed);

            // ウェイトをインデックス順に反映する
            for (int i = 0; i < _viewModel.Weights.Length; i++)
            {
                _playableController.SetWeight(i, _viewModel.Weights[i]);
            }
        }

        /// <summary> PlayableGraphを破棄する。 </summary>
        private void OnDestroy()
        {
            _playableController?.Dispose();
        }

        private PlayableAnimationController _playableController;
        private CharacterAnimationViewModel _viewModel;
        private ICharacterAnimationController _controller;
        private bool _isInitialized;
    }
}
