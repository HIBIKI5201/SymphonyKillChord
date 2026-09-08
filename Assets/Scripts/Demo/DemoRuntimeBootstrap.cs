using KillChord.Runtime.Adaptor.InGame.Result;
using KillChord.Runtime.Application.Persistent.Load;
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
using UnityEngine.SceneManagement;

namespace KillChord.Demo
{
    /// <summary>
    ///     体験版セッションを常駐させ、二つのタイマーと専用終了遷移を制御します。
    /// </summary>
    public sealed class DemoRuntimeBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateInstance()
        {
            if (FindAnyObjectByType<DemoRuntimeBootstrap>() != null)
            {
                return;
            }

            GameObject gameObject = new(nameof(DemoRuntimeBootstrap));
            DontDestroyOnLoad(gameObject);
            gameObject.AddComponent<DemoRuntimeBootstrap>();
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
            TryStartSession(isOutGameActive);
            _sessionState.Tick(Time.unscaledDeltaTime, isOutGameActive, _config);

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

            if (_isExitPolicyOwner
                && ServiceLocator.TryGetInstance(out IStageResultExitPolicy registeredPolicy)
                && ReferenceEquals(registeredPolicy, _exitPolicy))
            {
                ServiceLocator.UnregisterInstance<IStageResultExitPolicy>();
            }

            CONFIG_ADDRESS.ReleaseLoadedAsset(this);
        }

        /// <summary>
        ///     チュートリアル戦闘後、初めてOutGameが有効になった時点で両タイマーを開始します。
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
                    || saveData.Tutorial.Phase < TutorialPhase.BattleCompleted)
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

            StageId forcedStageId = new(_config.ForcedStageId);
            if (!stageSelectContainer.StageTree.TryGetDefinition(
                    forcedStageId,
                    out StageDefinition stageDefinition)
                || stageDefinition is not BattleStageDefinition battleStageDefinition
                || !stageSelectContainer.SelectionService.TryPrepareBattleSortie(
                    battleStageDefinition,
                    stageSelectContainer.ReturnSceneName))
            {
                Debug.LogError(
                    $"[{nameof(DemoRuntimeBootstrap)}] 強制出撃ステージを準備できませんでした。"
                    + $" StageId: {_config.ForcedStageId}",
                    this);
                _isForcedSortiePrepared = true;
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
                && string.Equals(scene.name, _config.EndSceneName, StringComparison.Ordinal)
                && ServiceLocator.TryGetInstance<ISceneInitializationReadiness>(out var readiness))
            {
                readiness.Complete(scene.name, true);
            }

            TryResetSaveDataOnEndScene(scene);
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
                await SaveStore.DeleteAsync<SaveData>();
            }
            catch (Exception exception)
            {
                _isSaveDataReset = false;
                Debug.LogException(exception, this);
            }
        }

        private const string CONFIG_ADDRESS = nameof(DemoExperienceConfig);

        private readonly DemoSessionState _sessionState = new();
        private DemoExperienceConfig _config;
        private DemoStageResultExitPolicy _exitPolicy;
        private bool _isExitPolicyOwner;
        private bool _isSceneLoadedSubscribed;
        private bool _isStartingSession;
        private bool _isForcedSortiePrepared;
        private bool _isSaveDataReset;
    }
}
