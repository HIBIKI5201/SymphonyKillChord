using KillChord.Runtime.Adaptor.InGame.Result;
using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Adaptor.OutGame.Scenario;
using KillChord.Runtime.Adaptor.Persistent.Load;
using KillChord.Runtime.Adaptor.Persistent.SceneManagement;
using KillChord.Runtime.Application.OutGame.Screen;
using KillChord.Runtime.Application.Persistent.SceneManagement;
using KillChord.Runtime.Composition.InGame.Sequence;
using KillChord.Runtime.Composition.OutGame.StageSelect;
using KillChord.Runtime.Domain.OutGame.Screen;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.InfraStructure.Addressables;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.Persistent.Load;
using SymphonyFrameWork.System.SaveSystem;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Threading;
using System.Threading.Tasks;
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
                HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
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
            // TitleとOutGameが同時にロードされている間も、開始通知の購読だけは維持する。
            if (IsSceneLoaded(_config.TitleSceneName))
            {
                _sessionState.End();
                _timerView?.Refresh(false);
                return;
            }

            TryStartSessionFromOpeningScenario();
            TryStartSessionFromTutorialBattle();
            TryStartHomeTimer(isOutGameActive);
            bool isHomeTimerActive = isOutGameActive && _isHomeTimerStarted;
            _sessionState.Tick(Time.unscaledDeltaTime, isHomeTimerActive);
            UpdateTimerPosition(isOutGameActive);
            _timerView?.Refresh(isHomeTimerActive);

            if (_sessionState.IsStarted && _sessionState.IsOverallTimeExpired)
            {
                TryTransitionToEndScene();
                return;
            }

            if (_sessionState.IsHomeTimeExpired && isOutGameActive)
            {
                TryHandleHomeExpiry(stageSelectContainer);
            }
        }

        /// <summary>
        ///     既存の画面状態を参照し、計時や表示条件を変えずにタイマー位置を更新します。
        /// </summary>
        /// <param name="isOutGameActive"> OutGame内にいる場合はtrueです。 </param>
        private void UpdateTimerPosition(bool isOutGameActive)
        {
            ScreenId? screenId = ServiceLocator.TryGetInstance(
                out IScreenStateRepository screenStateRepository)
                ? screenStateRepository.TransitionState.CurrentScreenId
                : null;
            bool isResultActive = ServiceLocator.TryGetInstance(
                out SequenceModuleContainer sequenceContainer)
                && sequenceContainer.SequenceDirector != null
                && sequenceContainer.SequenceDirector.IsResultActive;
            bool isScenarioActive = ServiceLocator.TryGetInstance(
                out SelectedScenarioState selectedScenarioState)
                && selectedScenarioState.HasSelectedScenario
                && IsSceneLoaded(selectedScenarioState.CurrentStageDefinition.TargetSceneName);
            _timerView?.UpdatePosition(screenId, isOutGameActive, isResultActive, isScenarioActive);
        }

        private void OnDestroy()
        {
            _expiryNotificationCancellation?.Cancel();
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
        ///     シナリオ開始を選択している場合、冒頭シナリオのロード完了後に全体タイマーを開始します。
        /// </summary>
        private void TryStartSessionFromOpeningScenario()
        {
            if (_sessionState.IsStarted
                || _config.OverallTimerStartPoint != DemoTimerStartPoint.OpeningScenario
                || !ServiceLocator.TryGetInstance(out SelectedScenarioState selectedScenarioState)
                || !selectedScenarioState.HasSelectedScenario
                || !selectedScenarioState.IsOpeningTutorialScenario
                || !IsSceneLoaded(selectedScenarioState.CurrentStageDefinition.TargetSceneName)
                || !ServiceLocator.TryGetInstance(out LoadingScreenController loadingScreenController)
                || loadingScreenController.IsLoading)
            {
                return;
            }

            _sessionState.Start();
        }

        /// <summary>
        ///     シナリオまたはチュートリアル開始を選択している場合、戦闘のロード完了後に全体タイマーを開始します。
        /// </summary>
        private void TryStartSessionFromTutorialBattle()
        {
            if (_sessionState.IsStarted
                || (_config.OverallTimerStartPoint != DemoTimerStartPoint.OpeningScenario
                    && _config.OverallTimerStartPoint != DemoTimerStartPoint.TutorialBattle)
                || !ServiceLocator.TryGetInstance(out SelectedBattleStageState selectedBattleStageState)
                || !selectedBattleStageState.HasSelectedBattleStage
                || !selectedBattleStageState.CurrentStageDefinition.IsTutorial
                || !TryGetLoadedBattleScenes(out _, out _)
                || !ServiceLocator.TryGetInstance(out LoadingScreenController loadingScreenController)
                || loadingScreenController.IsLoading)
            {
                return;
            }

            _sessionState.Start();
        }

        /// <summary>
        ///     Homeチュートリアル開始状態の保存後にホームタイマーを有効にし、ホームからの再開時は全体タイマーも開始します。
        /// </summary>
        private async void TryStartHomeTimer(bool isOutGameActive)
        {
            if (_isHomeTimerStarted || _isStartingHomeTimer || !isOutGameActive)
            {
                return;
            }

            _isStartingHomeTimer = true;
            int sessionRevision = _sessionRevision;
            try
            {
                SaveData saveData = SaveStore.IsLoaded<SaveData>()
                    ? SaveStore.Get<SaveData>()
                    : await SaveStore.LoadAsync<SaveData>(destroyCancellationToken);
                // Titleへ戻る前に始まった読み込み結果で、次のセッションを開始しない。
                if (sessionRevision != _sessionRevision
                    || IsSceneLoaded(_config.TitleSceneName)
                    || saveData == null
                    || saveData.Tutorial.Phase < TutorialPhase.HomeStarted
                    || (saveData.Tutorial.Phase == TutorialPhase.HomeStarted
                        && !_isHomeTutorialStartedNotified))
                {
                    return;
                }

                _isHomeTimerStarted = true;
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
                _isStartingHomeTimer = false;
            }
        }

        /// <summary>
        ///     ホームチュートリアルが進行中であれば、完了扱いでの終了を要求します。
        /// </summary>
        private void RequestHomeTutorialForceCompleteIfRunning()
        {
            _outGameUIEvent?.OnHomeTutorialForceCompleteRequested?.Invoke();
        }

        /// <summary>
        ///     作戦画面で強制対象ステージを選択し、出撃以外の操作を制限します。
        /// </summary>
        private void ApplyForcedSortie(StageSelectModuleContainer stageSelectContainer)
        {
            if (_isForcedSortiePrepared)
            {
                return;
            }

            // 満了状態を保留として維持し、ロード後の最新の解放状態から選び直す。
            if (ServiceLocator.TryGetInstance(out LoadingScreenController loadingScreenController)
                && loadingScreenController.IsLoading)
            {
                return;
            }

            if (!DemoStageResultExitPolicy.TryGetLatestAvailableBattleStage(
                    stageSelectContainer.StageTree,
                    out BattleStageDefinition battleStageDefinition))
            {
                if (!_hasLoggedMissingForcedSortieStage)
                {
                    _hasLoggedMissingForcedSortieStage = true;
                    Debug.LogError(
                        $"[{nameof(DemoRuntimeBootstrap)}] " +
                        "強制出撃先となる解放済みのバトルステージがありません。ステージツリーの定義と解放状態を確認してください。",
                        this);
                }
                return;
            }

            // UIの準備中や出撃処理中は、次のフレームで対象の取得から再試行する。
            if (!stageSelectContainer.TryForceBattleSortie(battleStageDefinition.StageId))
            {
                return;
            }

            _isForcedSortiePrepared = true;
        }

        /// <summary>
        ///     シーンロード時に体験版セッションと終了処理を更新します。
        /// </summary>
        /// <param name="scene"> ロードされたシーンです。 </param>
        /// <param name="loadSceneMode"> シーンのロード方式です。 </param>
        private void HandleSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (_config == null)
            {
                return;
            }

            if (string.Equals(scene.name, _config.EndSceneName, StringComparison.Ordinal))
            {
                _expiryNotificationCancellation?.Cancel();
                _sessionState.End();
                _timerView?.Refresh(false);
                TryResetSaveDataOnEndScene(scene);
                return;
            }

            if (string.Equals(scene.name, _config.TitleSceneName, StringComparison.Ordinal))
            {
                ResetSessionOnTitleEntry();
            }
        }

        /// <summary>
        ///     OutGameへ入り直した時点でホームタイマーと強制出撃状態を初期化します。
        /// </summary>
        private void ResetHomeTimerOnEntry(bool isOutGameActive)
        {
            if (isOutGameActive != _wasOutGameActive)
            {
                _homeTimerRevision++;
                _homeExpiryNotificationShown = false;
                if (_isShowingHomeNotification) { _expiryNotificationCancellation?.Cancel(); }
            }
            if (isOutGameActive && !_wasOutGameActive)
            {
                _sessionState.ResetHomeTimer();
                _isForcedSortiePrepared = false;
                _hasLoggedMissingForcedSortieStage = false;
            }

            _wasOutGameActive = isOutGameActive;
        }

        /// <summary>
        ///     タイトルへ戻った時点で、次のプレイに持ち越してはいけない体験版状態を初期化します。
        /// </summary>
        private void ResetSessionOnTitleEntry()
        {
            _sessionRevision++;
            _expiryNotificationCancellation?.Cancel();
            _overallExpiryNotificationShown = false;
            _homeExpiryNotificationShown = false;
            _sessionState.Reset();
            _timerView?.Refresh(false);

            if (_isOutGameUiEventSubscribed && _outGameUIEvent != null)
            {
                _outGameUIEvent.OnHomeTutorialStarted -= HandleHomeTutorialStarted;
            }

            _outGameUIEvent = null;
            _isOutGameUiEventSubscribed = false;
            _isHomeTutorialStartedNotified = false;
            _isHomeTimerStarted = false;
            _isTransitioningToEndScene = false;
            _isFinalStageConfigured = false;
            _isForcedSortiePrepared = false;
            _hasLoggedMissingForcedSortieStage = false;
            _wasOutGameActive = false;
            _isSaveDataReset = false;
        }

        /// <summary>
        ///     全体制限時間切れを検知し、現在のゲームシーンから体験版終了シーンへ遷移します。
        /// </summary>
        private async void TryTransitionToEndScene()
        {
            // 全体期限を優先し、ホーム通知の完了後に強制出撃しないよう取り消します。
            if (_isShowingHomeNotification)
            {
                _expiryNotificationCancellation?.Cancel();
                return;
            }
            if (_isTransitioningToEndScene || _config == null || IsSceneTransitionBusy()
                || !TryGetNotificationView(out EventNotificationView notification)
                || notification.IsVisible)
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
                    $"[{nameof(DemoRuntimeBootstrap)}] {nameof(SceneTransitionUsecase)} が取得できません。",
                    this);
                return;
            }

            _isTransitioningToEndScene = true;
            int revision = _sessionRevision;
            bool isSuccess = false;
            try
            {
                if (!_overallExpiryNotificationShown)
                {
                    await ShowExpiryNotificationAsync(notification, "ui.notification.demo_expired");
                    if (revision != _sessionRevision || !_sessionState.IsStarted) { return; }
                    _overallExpiryNotificationShown = true;
                }

                // 通知中に別の遷移が始まった場合は、その終了を待って最新シーンから再試行します。
                if (revision != _sessionRevision || !_sessionState.IsStarted
                    || IsSceneLoaded(_config.TitleSceneName) || IsSceneTransitionBusy())
                {
                    return;
                }
                currentScene = SceneManager.GetActiveScene();
                if (TryGetLoadedBattleScenes(out string battleSceneName, out string inGameSceneName))
                {
                    isSuccess = await sceneTransitionUsecase.UnloadThenChangeSceneAsync(
                        battleSceneName, inGameSceneName, _config.EndSceneName, destroyCancellationToken);
                }
                else
                {
                    isSuccess = await sceneTransitionUsecase.ChangeSceneAsync(
                        currentScene.name, _config.EndSceneName, destroyCancellationToken);
                }
                if (!isSuccess)
                {
                    Debug.LogError($"[{nameof(DemoRuntimeBootstrap)}] 全体制限時間切れ後の体験版終了シーン遷移に失敗しました。", this);
                }
            }
            catch (OperationCanceledException)
            {
                // タイトル復帰・全体期限の優先・破棄による正常な取消しでは後続を実行しません。
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
            finally
            {
                if (revision == _sessionRevision) { _isTransitioningToEndScene = isSuccess; }
            }
        }

        /// <summary>
        ///     ホーム期限の通知完了後、既存の作戦画面と出撃確認へ進めます。
        /// </summary>
        private async void TryHandleHomeExpiry(StageSelectModuleContainer container)
        {
            if (_isShowingHomeNotification || _isForcedSortiePrepared || _isTransitioningToEndScene
                || IsSceneTransitionBusy()
                || !TryGetNotificationView(out EventNotificationView notification)
                || notification.IsVisible)
            {
                return;
            }

            _isShowingHomeNotification = true;
            int revision = _sessionRevision;
            int homeRevision = _homeTimerRevision;
            try
            {
                if (!_homeExpiryNotificationShown)
                {
                    await ShowExpiryNotificationAsync(notification, "ui.notification.home_expired");
                }

                // 別のホーム滞在、タイトル復帰、全体満了へ古い通知の結果を持ち越しません。
                if (revision != _sessionRevision || homeRevision != _homeTimerRevision
                    || !_sessionState.IsStarted || _sessionState.IsOverallTimeExpired
                    || IsSceneLoaded(_config.TitleSceneName)
                    || !ServiceLocator.TryGetInstance(out StageSelectModuleContainer current)
                    || !ReferenceEquals(current, container))
                {
                    return;
                }
                _homeExpiryNotificationShown = true;
                if (IsSceneTransitionBusy()) { return; }
                RequestHomeTutorialForceCompleteIfRunning();
                ApplyForcedSortie(current);
            }
            catch (OperationCanceledException)
            {
                // タイトル復帰・全体期限の優先・破棄による正常な取消しでは後続を実行しません。
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
            finally
            {
                _isShowingHomeNotification = false;
            }
        }

        /// <summary>
        ///     セッションリセットから取り消せる通知だけを所有し、待機終了で解放します。
        /// </summary>
        private async Task ShowExpiryNotificationAsync(EventNotificationView notification, string entry)
        {
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            _expiryNotificationCancellation = cancellation;
            try
            {
                await notification.ShowAsync(entry, cancellation.Token);
            }
            finally
            {
                if (ReferenceEquals(_expiryNotificationCancellation, cancellation))
                {
                    _expiryNotificationCancellation = null;
                }
            }
        }

        /// <summary>
        ///     計時開始後の必須通知欠落を一度だけ記録し、表示前の遷移を防ぎます。
        /// </summary>
        private bool TryGetNotificationView(out EventNotificationView notification)
        {
            if (ServiceLocator.TryGetInstance(out notification) && notification != null)
            {
                _hasLoggedMissingNotificationView = false;
                return true;
            }
            if (!_hasLoggedMissingNotificationView)
            {
                _hasLoggedMissingNotificationView = true;
                Debug.LogError(
                    $"[{nameof(DemoRuntimeBootstrap)}] 常駐通知Viewがありません。Persistentの初期化設定を確認してください。",
                    this);
            }
            return false;
        }

        /// <summary>
        ///     ロード中とシナリオ専用出撃中は期限の後続処理を保留します。
        /// </summary>
        private static bool IsSceneTransitionBusy()
        {
            return (ServiceLocator.TryGetInstance(out LoadingScreenController loading) && loading.IsLoading)
                || (ServiceLocator.TryGetInstance(out SceneTransitionController transition)
                    && transition.HasScenarioBattleSortie);
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
        ///     言語と音量は次のプレイヤーへ引き継ぐため、削除後のセーブデータへ戻して保存します。
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
                // 削除の前に、引き継ぐ言語と音量を控える。
                bool hasPreservedSettings = SaveStore.IsLoaded<SaveData>();
                AudioSettingsData preservedAudioSettings = null;
                GameLanguage preservedLanguage = EnvironmentSettingsData.DEFAULT_LANGUAGE;
                if (hasPreservedSettings)
                {
                    SaveData currentSaveData = SaveStore.Get<SaveData>();
                    preservedAudioSettings = new AudioSettingsData(
                        currentSaveData.AudioSettings.BgmVolume,
                        currentSaveData.AudioSettings.SoundEffectVolume,
                        currentSaveData.AudioSettings.VoiceVolume);
                    preservedLanguage = currentSaveData.EnvironmentSettings.Language;
                }

                // 終了シーンの初期化完了通知は DemoEndSceneInitializer が行う。
                await SaveStore.DeleteAsync<SaveData>();

                if (hasPreservedSettings)
                {
                    SaveData resetSaveData = await SaveStore.LoadAsync<SaveData>(destroyCancellationToken);
                    resetSaveData.AudioSettings.SetVolumes(
                        preservedAudioSettings.BgmVolume,
                        preservedAudioSettings.SoundEffectVolume,
                        preservedAudioSettings.VoiceVolume);
                    resetSaveData.EnvironmentSettings.SetLanguage(preservedLanguage);
                    await SaveStore.SaveAsync<SaveData>();
                }
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
        private bool _isHomeTimerStarted;
        private bool _isStartingHomeTimer;
        private int _sessionRevision;
        private CancellationTokenSource _expiryNotificationCancellation;
        private int _homeTimerRevision;
        private bool _isShowingHomeNotification;
        private bool _hasLoggedMissingNotificationView;
        private bool _homeExpiryNotificationShown;
        private bool _overallExpiryNotificationShown;
        private bool _isTransitioningToEndScene;
        private bool _isFinalStageConfigured;
        private bool _isForcedSortiePrepared;
        private bool _hasLoggedMissingForcedSortieStage;
        private bool _wasOutGameActive;
        private bool _isSaveDataReset;
        private bool _ownsPrefabAssetHandle;

        private static bool _isCreating;
        private static AsyncOperationHandle<GameObject> _prefabAssetHandle;
    }
}
