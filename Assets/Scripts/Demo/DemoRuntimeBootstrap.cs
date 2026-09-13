using KillChord.Runtime.Adaptor.InGame.Result;
using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Application.Persistent.SceneManagement;
using KillChord.Runtime.Composition.OutGame.StageSelect;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.InfraStructure.Addressables;
using KillChord.Runtime.View.OutGame.Screen;
using SymphonyFrameWork.System.SaveSystem;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace KillChord.Demo
{
    /// <summary>
    ///     体験版セッションを常駐させ、二つのタイマーと専用終了遷移を制御します。
    /// </summary>
    public sealed class DemoRuntimeBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _isCreating = false;
            _prefabAssetHandle = default;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static async void CreateInstance()
        {
            if (_isCreating
                || ServiceLocator.TryGetInstance<IDemoSession>(out _)
                || FindAnyObjectByType<DemoRuntimeBootstrap>() != null)
            {
                return;
            }

            _isCreating = true;
            AsyncOperationHandle<GameObject> handle = default;
            try
            {
                handle = Addressables.LoadAssetAsync<GameObject>(PREFAB_ADDRESS);
                GameObject prefab = await handle.Task;
                if (!Application.isPlaying)
                {
                    return;
                }

                if (prefab == null)
                {
                    Debug.LogError(
                        $"[{nameof(DemoRuntimeBootstrap)}] {PREFAB_ADDRESS} をロードできませんでした。");
                    return;
                }

                if (ServiceLocator.TryGetInstance<IDemoSession>(out _)
                    || FindAnyObjectByType<DemoRuntimeBootstrap>() != null)
                {
                    return;
                }

                GameObject instance = Instantiate(prefab);
                DemoRuntimeBootstrap bootstrap =
                    instance.GetComponent<DemoRuntimeBootstrap>();
                if (bootstrap == null)
                {
                    Debug.LogError(
                        $"[{nameof(DemoRuntimeBootstrap)}] PrefabにBootstrapがありません。",
                        instance);
                    Destroy(instance);
                    return;
                }

                _prefabAssetHandle = handle;
                bootstrap._ownsPrefabAssetHandle = true;
                DontDestroyOnLoad(instance);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                if (!_prefabAssetHandle.IsValid() && handle.IsValid())
                {
                    Addressables.Release(handle);
                }

                _isCreating = false;
            }
        }

        private async void Start()
        {
            try
            {
                _config = await CONFIG_ADDRESS.LoadAssetAsync<DemoExperienceConfig>(
                    this,
                    destroyCancellationToken);
                if (_config == null)
                {
                    Debug.LogError($"[{nameof(DemoRuntimeBootstrap)}] {CONFIG_ADDRESS} をロードできませんでした。", this);
                    return;
                }

                _sessionState.Configure(_config);
                _isSessionOwner =
                    ServiceLocator.RegisterInstance<IDemoSession>(_sessionState);
                if (!_isSessionOwner)
                {
                    Debug.LogError(
                        $"[{nameof(DemoRuntimeBootstrap)}] {nameof(IDemoSession)} を登録できませんでした。",
                        this);
                    return;
                }

                _timerView ??= GetComponentInChildren<DemoTimerView>(true);
                if (_timerView == null)
                {
                    Debug.LogError(
                        $"[{nameof(DemoRuntimeBootstrap)}] {nameof(DemoTimerView)} がありません。",
                        this);
                }
                else
                {
                    _timerView.Initialize(_sessionState);
                }
                _exitPolicy = new DemoStageResultExitPolicy(_sessionState, _config);
                _isExitPolicyOwner = ServiceLocator.RegisterInstance<IStageResultExitPolicy>(_exitPolicy);
                SceneManager.sceneLoaded += HandleSceneLoaded;
                _isSceneLoadedSubscribed = true;
                TryResetSaveDataOnEndScene(SceneManager.GetActiveScene());
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        private void Update()
        {
            if (_config == null)
            {
                return;
            }

            bool isOutGameActive = ServiceLocator.TryGetInstance(
                out StageSelectModuleContainer stageSelectContainer);
            ResetHomeTimerOnEntry(isOutGameActive);

            if (isOutGameActive && !_isFinalStageConfigured)
            {
                _isFinalStageConfigured =
                    _exitPolicy.TryConfigureFinalStage(stageSelectContainer.StageTree);
            }

            TrySubscribeHomeTutorialStarted(isOutGameActive);
            TryStartSession(isOutGameActive);
            _sessionState.Tick(Time.unscaledDeltaTime, isOutGameActive);
            _timerView?.Refresh(isOutGameActive);

            if (_sessionState.IsStarted && _sessionState.IsOverallTimeExpired)
            {
                TryTransitionToEndScene();
                return;
            }

            if (_sessionState.IsHomeTimeExpired && isOutGameActive)
            {
                ApplyForcedSortie(stageSelectContainer);
            }
        }

        private void OnDestroy()
        {
            if (_isSceneLoadedSubscribed)
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
            }

            if (_isOutGameUiEventSubscribed && _outGameUIEvent != null)
            {
                _outGameUIEvent.OnHomeTutorialStarted -= HandleHomeTutorialStarted;
            }

            if (_isExitPolicyOwner
                && ServiceLocator.TryGetInstance(out IStageResultExitPolicy registeredPolicy)
                && ReferenceEquals(registeredPolicy, _exitPolicy))
            {
                ServiceLocator.UnregisterInstance<IStageResultExitPolicy>();
            }

            if (_isSessionOwner
                && ServiceLocator.TryGetInstance<IDemoSession>(out var registeredSession)
                && ReferenceEquals(registeredSession, _sessionState))
            {
                ServiceLocator.UnregisterInstance<IDemoSession>();
            }

            CONFIG_ADDRESS.ReleaseLoadedAsset(this);
            if (_ownsPrefabAssetHandle && _prefabAssetHandle.IsValid())
            {
                Addressables.Release(_prefabAssetHandle);
                _prefabAssetHandle = default;
            }
        }

        /// <summary>
        ///     Homeチュートリアル開始状態が保存された後、初めてOutGameが有効になった時点でタイマーを開始します。
        /// </summary>
        private async void TryStartSession(bool isOutGameActive)
        {
            if (_sessionState.IsStarted || _isStartingSession || !isOutGameActive)
            {
                return;
            }

            _isStartingSession = true;
            try
            {
                SaveData saveData = SaveStore.IsLoaded<SaveData>()
                    ? SaveStore.Get<SaveData>()
                    : await SaveStore.LoadAsync<SaveData>(destroyCancellationToken);
                if (saveData == null
                    || saveData.Tutorial.Phase < TutorialPhase.HomeStarted
                    || (saveData.Tutorial.Phase == TutorialPhase.HomeStarted
                        && !_isHomeTutorialStartedNotified))
                {
                    return;
                }

                _sessionState.Start();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
            finally
            {
                _isStartingSession = false;
            }
        }

        /// <summary>
        ///     強制対象ステージを準備し、すでに表示中の場合も含めて操作制限を反映します。
        /// </summary>
        private void ApplyForcedSortie(StageSelectModuleContainer stageSelectContainer)
        {
            if (!ServiceLocator.TryGetInstance(out BattlePreparationScreen preparationScreen))
            {
                return;
            }

            preparationScreen.SetForcedSortieMode(true);
            if (_isForcedSortiePrepared)
            {
                return;
            }

            if (!DemoStageResultExitPolicy.TryGetLatestAvailableBattleStage(
                    stageSelectContainer.StageTree,
                    out BattleStageDefinition battleStageDefinition)
                || !stageSelectContainer.SelectionService.TryPrepareBattleSortie(
                    battleStageDefinition,
                    stageSelectContainer.ReturnSceneName))
            {
                Debug.LogError(
                    $"[{nameof(DemoRuntimeBootstrap)}] " +
                    "解放済みの最新バトルステージを強制出撃先に設定できませんでした。",
                    this);
                return;
            }

            _isForcedSortiePrepared = true;
            if (ServiceLocator.TryGetInstance(out OutGameUIEvent outGameUIEvent))
            {
                outGameUIEvent.OnShownBattlePreparationScreen?.Invoke();
            }
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (_config != null
                && string.Equals(scene.name, _config.EndSceneName, StringComparison.Ordinal))
            {
                _sessionState.End();
                _timerView?.Refresh(false);
            }

            TryResetSaveDataOnEndScene(scene);
        }

        /// <summary>
        ///     OutGameへ入り直した時点でホームタイマーと強制出撃状態を初期化します。
        /// </summary>
        private void ResetHomeTimerOnEntry(bool isOutGameActive)
        {
            if (isOutGameActive && !_wasOutGameActive)
            {
                _sessionState.ResetHomeTimer();
                _isForcedSortiePrepared = false;
            }

            _wasOutGameActive = isOutGameActive;
        }

        /// <summary>
        ///     全体制限時間切れを検知し、現在のゲームシーンから体験版終了シーンへ遷移します。
        /// </summary>
        private async void TryTransitionToEndScene()
        {
            if (_isTransitioningToEndScene || _config == null)
            {
                return;
            }

            Scene currentScene = SceneManager.GetActiveScene();
            if (string.Equals(currentScene.name, _config.EndSceneName, StringComparison.Ordinal))
            {
                _sessionState.End();
                _timerView?.Refresh(false);
                return;
            }

            if (!ServiceLocator.TryGetInstance(out SceneTransitionUsecase sceneTransitionUsecase))
            {
                Debug.LogError(
                    $"[{nameof(DemoRuntimeBootstrap)}] " +
                    $"{nameof(SceneTransitionUsecase)} が取得できません。",
                    this);
                return;
            }

            _isTransitioningToEndScene = true;
            try
            {
                bool isSuccess;
                if (TryGetLoadedBattleScenes(
                        out string battleSceneName,
                        out string inGameSceneName))
                {
                    isSuccess = await sceneTransitionUsecase.UnloadThenChangeSceneAsync(
                        battleSceneName,
                        inGameSceneName,
                        _config.EndSceneName,
                        destroyCancellationToken);
                }
                else
                {
                    isSuccess = await sceneTransitionUsecase.ChangeSceneAsync(
                        currentScene.name,
                        _config.EndSceneName,
                        destroyCancellationToken);
                }

                if (!isSuccess)
                {
                    Debug.LogError(
                        $"[{nameof(DemoRuntimeBootstrap)}] " +
                        "全体制限時間切れ後の体験版終了シーン遷移に失敗しました。",
                        this);
                    _isTransitioningToEndScene = false;
                }
            }
            catch (OperationCanceledException)
            {
                _isTransitioningToEndScene = false;
            }
            catch (Exception exception)
            {
                _isTransitioningToEndScene = false;
                Debug.LogException(exception, this);
            }
        }

        /// <summary>
        ///     現在ロード中の戦闘ステージとインゲーム基盤シーンを取得します。
        /// </summary>
        private static bool TryGetLoadedBattleScenes(
            out string battleSceneName,
            out string inGameSceneName)
        {
            battleSceneName = string.Empty;
            inGameSceneName = string.Empty;

            if (!ServiceLocator.TryGetInstance(out SelectedBattleStageState selectedBattleStageState)
                || !selectedBattleStageState.HasSelectedBattleStage)
            {
                return false;
            }

            battleSceneName = selectedBattleStageState.BattleSceneName;
            inGameSceneName = selectedBattleStageState.InGameSceneName;
            return IsSceneLoaded(battleSceneName) && IsSceneLoaded(inGameSceneName);
        }

        /// <summary> 指定したシーンがロード済みの場合はtrueを返します。 </summary>
        private static bool IsSceneLoaded(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            return scene.IsValid() && scene.isLoaded;
        }

        /// <summary>
        ///     Homeチュートリアル開始状態の保存完了通知を受け取ります。
        /// </summary>
        private void HandleHomeTutorialStarted()
        {
            _isHomeTutorialStartedNotified = true;
        }

        /// <summary>
        ///     専用終了画面が表示された時点でセーブデータを削除します。
        /// </summary>
        private async void TryResetSaveDataOnEndScene(Scene scene)
        {
            if (_isSaveDataReset
                || _config == null
                || !string.Equals(scene.name, _config.EndSceneName, StringComparison.Ordinal))
            {
                return;
            }

            _isSaveDataReset = true;
            try
            {
                // 終了シーンの初期化完了通知は DemoEndSceneInitializer が行う。
                await SaveStore.DeleteAsync<SaveData>();
            }
            catch (Exception exception)
            {
                _isSaveDataReset = false;
                Debug.LogException(exception, this);
            }
        }

        /// <summary>
        ///     OutGameサービスの構築後にHomeチュートリアル開始通知を購読します。
        /// </summary>
        /// <param name="isOutGameActive"> OutGame内にいる場合はtrueです。 </param>
        private void TrySubscribeHomeTutorialStarted(bool isOutGameActive)
        {
            if (!isOutGameActive
                || !ServiceLocator.TryGetInstance(out OutGameUIEvent currentOutGameUIEvent))
            {
                return;
            }

            if (_isOutGameUiEventSubscribed
                && ReferenceEquals(_outGameUIEvent, currentOutGameUIEvent))
            {
                return;
            }

            if (_isOutGameUiEventSubscribed && _outGameUIEvent != null)
            {
                _outGameUIEvent.OnHomeTutorialStarted -= HandleHomeTutorialStarted;
            }

            _outGameUIEvent = currentOutGameUIEvent;
            _outGameUIEvent.OnHomeTutorialStarted += HandleHomeTutorialStarted;
            _isOutGameUiEventSubscribed = true;
        }

        private const string CONFIG_ADDRESS = nameof(DemoExperienceConfig);
        private const string PREFAB_ADDRESS = "DemoRuntime";

        private readonly DemoSessionState _sessionState = new();
        [SerializeField, Tooltip("常駐表示する体験版タイマーViewです。")]
        private DemoTimerView _timerView;

        private DemoExperienceConfig _config;
        private DemoStageResultExitPolicy _exitPolicy;
        private OutGameUIEvent _outGameUIEvent;
        private bool _isExitPolicyOwner;
        private bool _isSessionOwner;
        private bool _isSceneLoadedSubscribed;
        private bool _isOutGameUiEventSubscribed;
        private bool _isHomeTutorialStartedNotified;
        private bool _isStartingSession;
        private bool _isTransitioningToEndScene;
        private bool _isFinalStageConfigured;
        private bool _isForcedSortiePrepared;
        private bool _wasOutGameActive;
        private bool _isSaveDataReset;
        private bool _ownsPrefabAssetHandle;

        private static bool _isCreating;
        private static AsyncOperationHandle<GameObject> _prefabAssetHandle;
    }
}
