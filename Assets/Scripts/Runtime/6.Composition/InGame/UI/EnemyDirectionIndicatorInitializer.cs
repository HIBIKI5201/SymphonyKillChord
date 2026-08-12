using KillChord.Runtime.Adaptor.InGame.UI;
using KillChord.Runtime.Composition.InGame.Bootstrap;
using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.Composition.InGame.Target;
using KillChord.Runtime.InfraStructure.Addressables;
using KillChord.Runtime.Utility.Identity;
using KillChord.Runtime.View.InGame.UI;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.UI
{
    using UnityCamera = UnityEngine.Camera;

    /// <summary>
    ///     敵方向表示の生成とTarget、Player、Cameraへの依存解決を行う。
    /// </summary>
    public sealed class EnemyDirectionIndicatorInitializer : InGameInitializationModuleBase
    {
        /// <summary> モジュール名。 </summary>
        public override string ModuleName => nameof(EnemyDirectionIndicatorInitializer);

        /// <summary> 実行順。 </summary>
        public override int Order => 660;

        /// <summary>
        ///     敵方向表示の設定をAddressablesから読み込む。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 読み込みに成功した場合はtrue。 </returns>
        public override async Awaitable<bool> ResourceLoadAsync(CancellationToken cancellationToken)
        {
            ReleaseLoadedConfig();

            try
            {
                _loadedConfig = await _configKey.LoadAssetAsync<EnemyDirectionIndicatorConfig>(
                    this,
                    cancellationToken);
                if (_loadedConfig != null)
                {
                    return true;
                }

                Debug.LogError(
                    $"[{nameof(EnemyDirectionIndicatorInitializer)}] 敵方向表示のConfigを読み込めませんでした。",
                    this);
                ReleaseLoadedConfig();
                return false;
            }
            catch (OperationCanceledException)
            {
                ReleaseLoadedConfig();
                throw;
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"[{nameof(EnemyDirectionIndicatorInitializer)}] 敵方向表示のConfig読み込みに失敗しました: {exception}",
                    this);
                ReleaseLoadedConfig();
                return false;
            }
        }

        /// <summary>
        ///     Target、Player、Cameraと結合して敵方向表示を構築する。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Ready()
        {
            if (_isInitialized)
            {
                return true;
            }

            Cleanup();

            if (!ValidateConfig())
            {
                return false;
            }

            try
            {
                TargetSystemModuleContainer targetContainer =
                    ServiceLocator.GetInstance<TargetSystemModuleContainer>();
                PlayerModuleContainer playerContainer =
                    ServiceLocator.GetInstance<PlayerModuleContainer>();
                UnityCamera mainCamera = UnityCamera.main;

                if (targetContainer?.TargetSystemViewModel == null
                    || playerContainer?.PlayerView == null
                    || mainCamera == null)
                {
                    Debug.LogError(
                        $"[{nameof(EnemyDirectionIndicatorInitializer)}] Target、Player、Cameraの依存解決に失敗しました。",
                        this);
                    Cleanup();
                    return false;
                }

                Transform playerTransform = playerContainer.PlayerView.transform;
                _view = Instantiate(_viewPrefab, playerTransform, false);
                if (!_view.Initialize(
                        mainCamera,
                        _loadedConfig.MaximumDisplayCount,
                        _loadedConfig.PositionOffset,
                        _loadedConfig.FadeEase,
                        _loadedConfig.FadeDuration))
                {
                    Cleanup();
                    return false;
                }

                _viewModel = new EnemyDirectionIndicatorViewModel(_view);
                _getPlayerPosition = () => playerTransform != null
                    ? playerTransform.position
                    : Vector3.zero;
                _isOutsideViewport = _view.IsOutsideViewport;
                _presenter = new EnemyDirectionIndicatorPresenter(
                    targetContainer.TargetSystemViewModel,
                    _viewModel,
                    _getPlayerPosition,
                    _isOutsideViewport,
                    _loadedConfig.MaximumDistance);
                _view.OnUpdate += _presenter.Update;
                _isInitialized = true;
                return true;
            }
            catch (Exception exception)
            {
                Cleanup();
                Debug.LogException(exception, this);
                return false;
            }
        }

        /// <summary>
        ///     イベント購読と生成した敵方向表示を解放する。
        /// </summary>
        public override void Shutdown()
        {
            Cleanup();
            ReleaseLoadedConfig();
        }

        [SerializeField, SourceDataAddress, Tooltip("敵方向表示ConfigのAddressablesキー。")]
        private string _configKey;

        [SerializeField, Tooltip("プレイヤー配下へ生成する敵方向表示View Prefab。")]
        private EnemyDirectionIndicatorView _viewPrefab;

        private EnemyDirectionIndicatorView _view;
        private EnemyDirectionIndicatorViewModel _viewModel;
        private EnemyDirectionIndicatorPresenter _presenter;
        private Func<Vector3> _getPlayerPosition;
        private Func<Bounds, bool> _isOutsideViewport;
        private EnemyDirectionIndicatorConfig _loadedConfig;
        private bool _isInitialized;

        /// <summary>
        ///     Coordinatorを経由しない破棄でも生成物を解放する。
        /// </summary>
        private void OnDestroy()
        {
            Cleanup();
            ReleaseLoadedConfig();
        }

        /// <summary>
        ///     ConfigとView Prefabの参照および設定値を検証する。
        /// </summary>
        /// <returns> 初期化可能な場合はtrue。 </returns>
        private bool ValidateConfig()
        {
            if (_loadedConfig == null || _viewPrefab == null)
            {
                Debug.LogError(
                    $"[{nameof(EnemyDirectionIndicatorInitializer)}] Configが未ロード、またはView Prefabがアサインされていません。",
                    this);
                return false;
            }

            if (_loadedConfig.MaximumDisplayCount <= 0
                || _loadedConfig.MaximumDisplayCount > EnemyDirectionIndicatorConfig.MAXIMUM_DISPLAY_COUNT
                || _loadedConfig.MaximumDistance < 0f
                || float.IsNaN(_loadedConfig.MaximumDistance)
                || float.IsInfinity(_loadedConfig.MaximumDistance)
                || _loadedConfig.MaximumDistance > Mathf.Sqrt(float.MaxValue)
                || !IsFinite(_loadedConfig.PositionOffset)
                || _loadedConfig.FadeDuration < 0f
                || float.IsNaN(_loadedConfig.FadeDuration)
                || float.IsInfinity(_loadedConfig.FadeDuration))
            {
                Debug.LogError(
                    $"[{nameof(EnemyDirectionIndicatorInitializer)}] Configの設定値が不正です。",
                    this);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Vector3の全成分が有限値か判定する。
        /// </summary>
        /// <param name="value"> 判定する値。 </param>
        /// <returns> 全成分が有限値の場合はtrue。 </returns>
        private static bool IsFinite(in Vector3 value)
        {
            return float.IsFinite(value.x)
                && float.IsFinite(value.y)
                && float.IsFinite(value.z);
        }

        /// <summary>
        ///     購読、ViewModel、Presenter、生成Viewを冪等に解放する。
        /// </summary>
        private void Cleanup()
        {
            if (_view != null && _presenter != null)
            {
                _view.OnUpdate -= _presenter.Update;
            }

            _viewModel?.Dispose();
            _viewModel = null;
            _presenter = null;
            _getPlayerPosition = null;
            _isOutsideViewport = null;

            if (_view != null)
            {
                Destroy(_view.gameObject);
                _view = null;
            }

            _isInitialized = false;
        }

        /// <summary>
        ///     ロード済みConfigとAddressablesハンドルを解放する。
        /// </summary>
        private void ReleaseLoadedConfig()
        {
            _configKey.ReleaseLoadedAsset(this);
            _loadedConfig = null;
        }
    }
}
