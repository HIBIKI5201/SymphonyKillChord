using KillChord.Runtime.Adaptor.InGame.Target;
using KillChord.Runtime.Adaptor.InGame.UI;
using KillChord.Runtime.Composition.InGame.Bootstrap;
using KillChord.Runtime.Composition.InGame.Target;
using KillChord.Runtime.View.InGame.UI;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.UI
{
    /// <summary>
    ///     ロックオン中の敵HP HUDに関するクラスの生成と依存関係の解決を行う初期化クラス。
    /// </summary>
    public sealed class HUDEnemyHealthInitializer : InGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(HUDEnemyHealthInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 650;

        /// <summary>
        ///     ターゲットモジュールへ結合して敵HP HUDを構築する。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Ready()
        {
            if (_view == null)
            {
                Debug.LogError($"[{nameof(HUDEnemyHealthInitializer)}] {nameof(_view)} がアサインされていません。", this);
                return false;
            }

            TargetSystemModuleContainer targetSystemModuleContainer = ServiceLocator.GetInstance<TargetSystemModuleContainer>();
            if (targetSystemModuleContainer == null)
            {
                Debug.LogError($"[{nameof(HUDEnemyHealthInitializer)}] {nameof(TargetSystemModuleContainer)} が見つかりません。", this);
                return false;
            }

            Initialize(targetSystemModuleContainer.TargetSystemController);
            return true;
        }

        /// <summary>
        ///     敵HP HUDを構成する各クラスを生成し、依存関係を解決して初期化する。
        /// </summary>
        /// <param name="targetingSystem"> 現在のターゲット情報を解決するシステム。</param>
        public void Initialize(TargetSystemController targetingSystem)
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
