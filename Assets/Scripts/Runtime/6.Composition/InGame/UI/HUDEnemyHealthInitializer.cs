using KillChord.Runtime.Adaptor.InGame.UI;
using KillChord.Runtime.View.InGame.Target;
using KillChord.Runtime.View.InGame.UI;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.UI
{
    /// <summary>
    ///     ロックオン中の敵HP HUDに関するクラスの生成と依存関係の解決を行う初期化クラス。
    /// </summary>
    public sealed class HUDEnemyHealthInitializer : MonoBehaviour
    {
        /// <summary>
        ///     敵HP HUDを構成する各クラスを生成し、依存関係を解決して初期化する。
        /// </summary>
        /// <param name="targetingSystem"> 現在のターゲット情報を解決するシステム。</param>
        public void Initialize(TargetingSystem targetingSystem)
        {
            if (_view == null)
            {
                Debug.LogError($"{nameof(_view)} がアサインされていません。");
                return;
            }

            _viewModel = new HUDEnemyHealthViewModel(_view);
            _presenter = new HUDEnemyHealthPresenter(targetingSystem, _viewModel);
            _view.OnUpdate += _presenter.Update;
        }

        [SerializeField, Tooltip("ロックオン中の敵HPを表示する View コンポーネント。")]
        private HUDEnemyHealthView _view;

        private HUDEnemyHealthViewModel _viewModel;
        private HUDEnemyHealthPresenter _presenter;

        private void OnDestroy()
        {
            _viewModel?.Dispose();
            _viewModel = null;
            _presenter = null;
        }
    }
}
