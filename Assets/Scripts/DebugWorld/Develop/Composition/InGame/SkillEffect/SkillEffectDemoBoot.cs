using KillChord.Runtime.Composition.Bootstrap;
using KillChord.Runtime.Composition.InGame.Bootstrap;
using KillChord.Runtime.View.Persistent.Load;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KillChord.Develop.Composition.InGame.SkillEffect
{
    /// <summary>
    ///     デモシーン向けに、常駐シーンのフローを介さずインゲーム初期化モジュールを実行するブートクラス。
    ///     ロード画面やステージシーンの読み込みを伴わないため、単体シーンで動作確認ができる。
    /// </summary>
    public sealed class SkillEffectDemoBoot : MonoBehaviour
    {
        /// <summary> 初期化が完了しているかどうかです。 </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        ///     初期化完了を待機します。
        /// </summary>
        /// <returns> 初期化完了を待機するAwaitableです。 </returns>
        public Awaitable WaitForInitializationAsync()
        {
            return _completionSource.Awaitable;
        }

        [SerializeField, Tooltip("シーン内の初期化モジュールを自動収集するかです。falseの場合は手動指定分のみ実行します。")]
        private bool _collectsModulesInScene = true;

        [SerializeField, Tooltip("手動で実行する初期化モジュールです。自動収集を使わない場合に指定します。")]
        private InGameInitializationModuleBase[] _modules;

        [SerializeField, Tooltip("常駐シーンのロード画面を非表示にするかです。デモシーンでは暗転が残るためtrueにします。")]
        private bool _hidesLoadingScreen = true;

        /// <summary>
        ///     初期化ライフサイクルを開始します。
        /// </summary>
        private async void Start()
        {
            try
            {
                if (_hidesLoadingScreen)
                {
                    // ロード画面の出現を待つと初期化が遅れるため、別処理として並行させる。
                    _ = StartHidingLoadingScreenAsync();
                }

                List<IInGameInitializationModule> modules = ResolveModules();
                if (modules.Count == 0)
                {
                    Debug.LogWarning($"[{nameof(SkillEffectDemoBoot)}] 初期化モジュールが見つかりません。", this);
                    CompleteInitialization(true);
                    return;
                }

                bool isSuccess = await _initializationCoordinator.InitializeAsync(
                    modules,
                    null,
                    destroyCancellationToken);
                if (!isSuccess)
                {
                    Debug.LogError($"[{nameof(SkillEffectDemoBoot)}] 初期化に失敗しました。", this);
                }

                _executedModules = modules;
                CompleteInitialization(isSuccess);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                CompleteInitialization(false);
            }
        }

        /// <summary>
        ///     登録順の逆順でモジュールを終了します。
        /// </summary>
        private void OnDestroy()
        {
            if (_executedModules == null)
            {
                return;
            }

            for (int i = _executedModules.Count - 1; i >= 0; i--)
            {
                _executedModules[i]?.Shutdown();
            }

            _executedModules = null;
        }

        /// <summary>
        ///     常駐シーンのロード画面を探して非表示にし、再表示されないか一定時間監視します。
        /// </summary>
        /// <returns> 監視処理を待機するAwaitableです。 </returns>
        private async Awaitable StartHidingLoadingScreenAsync()
        {
            try
            {
                // デモシーンには遷移先が無く、ロードセッションが完了しないため暗転が残り続ける。
                // セッション制御は常駐シーン側の都合で差し替わるため、表示そのものを落とす。
                // 全シーン検索は高コストのため、毎フレームではなく間隔を空けて実行する。
                for (int i = 0; i < MAX_LOADING_SCREEN_WATCH_COUNT; i++)
                {
                    if (HideLoadingScreen())
                    {
                        return;
                    }

                    for (int waitFrame = 0; waitFrame < LOADING_SCREEN_WATCH_INTERVAL_FRAME; waitFrame++)
                    {
                        await Awaitable.NextFrameAsync(destroyCancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        /// <summary>
        ///     ロード画面のViewが存在すれば非表示にします。
        /// </summary>
        /// <returns> 非表示にできた場合はtrueです。 </returns>
        private bool HideLoadingScreen()
        {
            LoadingScreenView loadingScreenView = FindAnyObjectByType<LoadingScreenView>(FindObjectsInactive.Exclude);
            if (loadingScreenView == null)
            {
                return false;
            }

            loadingScreenView.gameObject.SetActive(false);
            return true;
        }

        /// <summary>
        ///     実行対象の初期化モジュールを実行順に並べて取得します。
        /// </summary>
        /// <returns> 実行対象モジュール一覧です。 </returns>
        private List<IInGameInitializationModule> ResolveModules()
        {
            IEnumerable<InGameInitializationModuleBase> sourceModules = _collectsModulesInScene
                ? FindObjectsByType<InGameInitializationModuleBase>(FindObjectsSortMode.None)
                : _modules ?? Array.Empty<InGameInitializationModuleBase>();

            return sourceModules
                .Where(module => module != null && module.isActiveAndEnabled)
                .Cast<IInGameInitializationModule>()
                .OrderBy(module => module.Order)
                .ToList();
        }

        /// <summary>
        ///     初期化完了を確定し、待機者へ通知します。
        /// </summary>
        /// <param name="isSuccess"> 初期化に成功した場合はtrueです。 </param>
        private void CompleteInitialization(bool isSuccess)
        {
            _isInitialized = isSuccess;
            _completionSource.TrySetResult();
        }

        private const int MAX_LOADING_SCREEN_WATCH_COUNT = 40;
        private const int LOADING_SCREEN_WATCH_INTERVAL_FRAME = 15;

        private readonly AwaitableCompletionSource _completionSource = new();
        private readonly InGameInitializationCoordinator _initializationCoordinator = new();
        private List<IInGameInitializationModule> _executedModules;
        private bool _isInitialized;
    }
}
