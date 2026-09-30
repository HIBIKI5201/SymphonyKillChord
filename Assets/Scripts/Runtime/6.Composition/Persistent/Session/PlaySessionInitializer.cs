using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Adaptor.OutGame.Scenario;
using KillChord.Runtime.Adaptor.OutGame.StageSelect;
using KillChord.Runtime.Application.Persistent.SceneManagement;
using KillChord.Runtime.Composition.OutGame.StageSelect;
using KillChord.Runtime.Composition.Persistent.Bootstrap;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.InfraStructure.Addressables;
using KillChord.Runtime.InfraStructure.InGame.Enemy;
using KillChord.Runtime.InfraStructure.OutGame.StageSelect;
using KillChord.Runtime.Utility.Identity;
using SymphonyFrameWork.Attribute;
using SymphonyFrameWork.System.SaveSystem;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KillChord.Runtime.Composition.Persistent.Session
{
    /// <summary>
    ///     プレイセッションのセーブデータから起動時に開くシーンを決め、シーン間で持ち込む選択状態を用意する。
    ///     <para>
    ///         シーン間の選択状態（バトル・ミッション・シナリオ・予約済み遷移）は常駐シーンが登録するため、
    ///         どのシーンから開始しても各シーンの初期化に必要な状態が揃う。
    ///         起動後は画面の切り替わりを監視し、最後にいた画面と選択中のステージをセーブデータへ記録する。
    ///     </para>
    /// </summary>
    public sealed class PlaySessionInitializer : PersistentInitializationModuleBase, IPersistentFirstSceneProvider
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(PlaySessionInitializer);

        /// <summary> 実行順です。セーブデータとシーン遷移の初期化より後に実行します。 </summary>
        public override int Order => 15;

        /// <summary>
        ///     プレイセッションを読み込み、再開するステージの定義を解決します。
        ///     <para> 再開できない内容でも起動は止めず、タイトルから始めます。 </para>
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 常にtrue。 </returns>
        public override async Awaitable<bool> ResourceLoadAsync(CancellationToken cancellationToken)
        {
            _sessionData = SaveStore.IsLoaded<PlaySessionData>()
                ? SaveStore.Get<PlaySessionData>()
                : await SaveStore.LoadAsync<PlaySessionData>(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            // ステージを伴う再開地点だけ、ステージツリーから定義を引く。
            if (_sessionData.Scene == PlaySessionScene.Scenario || _sessionData.Scene == PlaySessionScene.InGame)
            {
                _resumeStageDefinition = await LoadStageDefinitionAsync(
                    new StageId(_sessionData.StageId),
                    cancellationToken);
            }

            return true;
        }

        /// <summary>
        ///     シーン間の選択状態を登録し、再開地点に合わせて選択状態と最初に開くシーンを決めます。
        /// </summary>
        /// <returns> 選択状態を登録できた場合はtrue。 </returns>
        public override bool Build()
        {
            _selectedBattleStageState = GetOrRegister<SelectedBattleStageState>(ref _ownsBattleStageState);
            _selectedMissionState = GetOrRegister<SelectedMissionState>(ref _ownsMissionState);
            _selectedScenarioState = GetOrRegister<SelectedScenarioState>(ref _ownsScenarioState);
            _pendingNodeTransitionState = GetOrRegister<PendingNodeTransitionState>(ref _ownsPendingNodeTransitionState);
            if (_selectedBattleStageState == null || _selectedMissionState == null || _selectedScenarioState == null
                || _pendingNodeTransitionState == null)
            {
                Debug.LogError($"[{nameof(PlaySessionInitializer)}] シーン間の選択状態を登録できませんでした。", this);
                return false;
            }

            ResolveFirstScene();
            return true;
        }

        /// <summary>
        ///     画面の切り替わりの監視を始めます。
        /// </summary>
        /// <returns> 常にtrue。 </returns>
        public override bool Ready()
        {
            if (ServiceLocator.TryGetInstance(out ISceneInitializationReadiness readiness))
            {
                _sceneInitializationNotifier = readiness as ISceneInitializationNotifier;
            }

            if (_sceneInitializationNotifier == null)
            {
                Debug.LogWarning(
                    $"[{nameof(PlaySessionInitializer)}] シーンの初期化完了を購読できないため、プレイセッションを記録しません。",
                    this);
                return true;
            }

            _sceneInitializationNotifier.OnSceneInitialized += SceneInitializedHandler;
            _selectedBattleStageState.OnSelectionChanged += SelectionChangedHandler;
            _selectedScenarioState.OnSelectionChanged += SelectionChangedHandler;
            return true;
        }

        /// <summary>
        ///     監視を止め、自身が登録した選択状態を解除します。
        /// </summary>
        public override void Shutdown()
        {
            if (_sceneInitializationNotifier != null)
            {
                _sceneInitializationNotifier.OnSceneInitialized -= SceneInitializedHandler;
                _sceneInitializationNotifier = null;
            }

            if (_selectedBattleStageState != null)
            {
                _selectedBattleStageState.OnSelectionChanged -= SelectionChangedHandler;
            }

            if (_selectedScenarioState != null)
            {
                _selectedScenarioState.OnSelectionChanged -= SelectionChangedHandler;
            }

            UnregisterIfOwned(_selectedBattleStageState, ref _ownsBattleStageState);
            UnregisterIfOwned(_selectedMissionState, ref _ownsMissionState);
            UnregisterIfOwned(_selectedScenarioState, ref _ownsScenarioState);
            UnregisterIfOwned(_pendingNodeTransitionState, ref _ownsPendingNodeTransitionState);
            _selectedBattleStageState = null;
            _pendingNodeTransitionState = null;
            _resumeStageTree = null;
            _selectedMissionState = null;
            _selectedScenarioState = null;
            _sessionData = null;
            _resumeStageDefinition = null;
        }

        /// <summary>
        ///     最初に開くシーンを取得します。
        /// </summary>
        /// <param name="sceneName"> 最初に開くシーン名です。 </param>
        /// <param name="isKeepLoading"> 読み込んだシーンがロード画面を引き継いで閉じる場合はtrueです。 </param>
        /// <returns> 再開するシーンがある場合はtrueです。 </returns>
        public bool TryGetFirstScene(out string sceneName, out bool isKeepLoading)
        {
            sceneName = _firstSceneName;
            isKeepLoading = _isFirstSceneKeepLoading;
            return !string.IsNullOrWhiteSpace(sceneName);
        }

        [SerializeField, SceneNameSelector, Tooltip("タイトルのシーン名。このシーンに戻るとプレイセッションを消去する。")]
        private string _titleSceneName = "Title";

        [SerializeField, SceneNameSelector, Tooltip("アウトゲームのシーン名。バトル終了後の帰還先にも使う。")]
        private string _outGameSceneName = "OutGame";

        [SerializeField, SourceDataAddress, Tooltip("ステージツリー定義アセットの Addressables キーです。")]
        private string _stageTreeAssetKey = "StageTreeAsset";

        [SerializeField, SourceDataAddress, Tooltip("敵Wave定義リポジトリの Addressables キーです。")]
        private string _enemyWaveDefinitionRepositoryKey = "EnemyWaveDefinitionRepository";

        private PlaySessionData _sessionData;
        private StageDefinition _resumeStageDefinition;
        private StageTree _resumeStageTree;
        private SelectedBattleStageState _selectedBattleStageState;
        private SelectedMissionState _selectedMissionState;
        private SelectedScenarioState _selectedScenarioState;
        private PendingNodeTransitionState _pendingNodeTransitionState;
        private ISceneInitializationNotifier _sceneInitializationNotifier;
        private string _firstSceneName;
        private bool _isFirstSceneKeepLoading;
        private bool _ownsBattleStageState;
        private bool _ownsMissionState;
        private bool _ownsScenarioState;
        private bool _ownsPendingNodeTransitionState;
        private bool _isSaving;
        private bool _hasPendingSave;

        /// <summary>
        ///     シーンの初期化完了に合わせて再開地点を記録します。
        /// </summary>
        /// <param name="sceneName"> 初期化が完了したシーン名。 </param>
        private void SceneInitializedHandler(string sceneName)
        {
            // タイトルへ戻ったら、次回もタイトルから始める。
            if (string.Equals(sceneName, _titleSceneName, StringComparison.Ordinal))
            {
                if (_sessionData != null && _sessionData.Clear())
                {
                    _ = SaveSessionAsync();
                }

                return;
            }

            RecordCurrentScreen();
        }

        /// <summary>
        ///     選択中のステージが変わったときに再開地点を記録し直します。
        /// </summary>
        private void SelectionChangedHandler()
        {
            RecordCurrentScreen();
        }

        /// <summary>
        ///     ステージツリーを読み込み、指定したステージの定義を取得します。
        /// </summary>
        /// <param name="stageId"> 取得するステージのID。 </param>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> ステージの定義。取得できない場合はnull。 </returns>
        private async Task<StageDefinition> LoadStageDefinitionAsync(StageId stageId, CancellationToken cancellationToken)
        {
            try
            {
                StageTreeAsset stageTreeAsset =
                    await _stageTreeAssetKey.LoadAssetAsync<StageTreeAsset>(this, cancellationToken);
                EnemyWaveDefinitionRepository waveDefinitionRepository =
                    await _enemyWaveDefinitionRepositoryKey.LoadAssetAsync<EnemyWaveDefinitionRepository>(
                        this,
                        cancellationToken);
                if (stageTreeAsset == null || waveDefinitionRepository == null)
                {
                    Debug.LogWarning(
                        $"[{nameof(PlaySessionInitializer)}] ステージツリーを読み込めないため、タイトルから始めます。",
                        this);
                    return null;
                }

                StageTree stageTree = stageTreeAsset.Create(waveDefinitionRepository);
                _resumeStageTree = stageTree;
                if (!stageTree.TryGetDefinition(stageId, out StageDefinition stageDefinition))
                {
                    Debug.LogWarning(
                        $"[{nameof(PlaySessionInitializer)}] 再開するステージが見つからないため、タイトルから始めます。StageId: {stageId}",
                        this);
                    return null;
                }

                return stageDefinition;
            }
            finally
            {
                // 定義を取り出した後はアセットを参照しないため、すぐに解放する。
                _stageTreeAssetKey.ReleaseLoadedAsset(this);
                _enemyWaveDefinitionRepositoryKey.ReleaseLoadedAsset(this);
            }
        }

        /// <summary>
        ///     再開地点に合わせて選択状態を用意し、最初に開くシーンを決めます。
        ///     <para> 再開できない場合は最初に開くシーンを決めず、常駐シーンの既定（タイトル）に任せます。 </para>
        /// </summary>
        private void ResolveFirstScene()
        {
            _firstSceneName = null;
            _isFirstSceneKeepLoading = false;
            if (_sessionData == null)
            {
                return;
            }

            switch (_sessionData.Scene)
            {
                case PlaySessionScene.OutGame:
                    _firstSceneName = _outGameSceneName;
                    return;

                case PlaySessionScene.Scenario when _resumeStageDefinition is ScenarioStageDefinition scenarioStage:
                    if (_sessionData.IsOpeningTutorialScenario)
                    {
                        // タイトルから始めた場合と同じく、完了後はチュートリアル戦闘へ直接進む。
                        _selectedScenarioState.SelectOpeningTutorialScenario(scenarioStage);
                        if (!OpeningTutorialRoute.TryReserveTutorialBattle(
                                _resumeStageTree, scenarioStage, _pendingNodeTransitionState, _outGameSceneName))
                        {
                            Debug.LogWarning(
                                $"[{nameof(PlaySessionInitializer)}] チュートリアル戦闘を予約できないため、OutGame から開始します。",
                                this);
                        }
                    }
                    else
                    {
                        _selectedScenarioState.SelectScenario(scenarioStage);
                    }

                    _firstSceneName = scenarioStage.TargetSceneName;
                    return;

                case PlaySessionScene.InGame when _resumeStageDefinition is BattleStageDefinition battleStage:
                    if (!BattleSortieSelectionStateResolver.CreateSelectionService()
                            .TryPrepareBattleSortie(battleStage, _outGameSceneName))
                    {
                        Debug.LogWarning(
                            $"[{nameof(PlaySessionInitializer)}] 再開するバトルステージを選択できないため、タイトルから始めます。",
                            this);
                        return;
                    }

                    // インゲームは初期化の完了時にロード画面を閉じるため、開いたまま引き渡す。
                    _firstSceneName = battleStage.TargetSceneName;
                    _isFirstSceneKeepLoading = true;
                    return;

                case PlaySessionScene.None:
                    return;

                default:
                    Debug.LogWarning(
                        $"[{nameof(PlaySessionInitializer)}] 再開地点とステージの種類が一致しないため、タイトルから始めます。" +
                        $" Scene: {_sessionData.Scene}, StageId: {_sessionData.StageId}",
                        this);
                    return;
            }
        }

        /// <summary>
        ///     読み込まれているシーンと選択状態から今いる画面を判定し、再開地点として記録します。
        /// </summary>
        private void RecordCurrentScreen()
        {
            if (_sessionData == null)
            {
                return;
            }

            bool isChanged;
            if (_selectedBattleStageState.HasSelectedBattleStage
                && IsSceneLoaded(_selectedBattleStageState.InGameSceneName))
            {
                isChanged = _sessionData.RecordInGame(
                    _selectedBattleStageState.CurrentStageDefinition.StageId.Value);
            }
            else if (_selectedScenarioState.HasSelectedScenario
                && IsSceneLoaded(_selectedScenarioState.CurrentStageDefinition.TargetSceneName))
            {
                isChanged = _sessionData.RecordScenario(
                    _selectedScenarioState.CurrentStageDefinition.StageId.Value,
                    _selectedScenarioState.IsOpeningTutorialScenario);
            }
            else if (IsSceneLoaded(_outGameSceneName))
            {
                isChanged = _sessionData.RecordOutGame();
            }
            else
            {
                // 遷移の途中で判定できない場合は、次の切り替わりで記録する。
                return;
            }

            if (isChanged)
            {
                _ = SaveSessionAsync();
            }
        }

        /// <summary>
        ///     プレイセッションを保存します。
        ///     <para> 保存に失敗してもゲームの進行は止めず、次の記録で保存し直します。 </para>
        ///     <para>
        ///         保存先は同じ一時ファイルを使うため、保存中に来た要求は重ねず、保存の完了後にまとめて保存し直します。
        ///     </para>
        /// </summary>
        private async Task SaveSessionAsync()
        {
            if (_isSaving)
            {
                _hasPendingSave = true;
                return;
            }

            _isSaving = true;
            try
            {
                do
                {
                    _hasPendingSave = false;
                    await SaveStore.SaveAsync<PlaySessionData>(destroyCancellationToken);
                }
                while (_hasPendingSave);
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
                _isSaving = false;
            }
        }

        /// <summary>
        ///     指定した型の状態がServiceLocatorにあれば取得し、無ければ生成して登録します。
        /// </summary>
        /// <typeparam name="T"> 状態の型。 </typeparam>
        /// <param name="isOwned"> このモジュールが登録した場合にtrueを設定します。 </param>
        /// <returns> 登録済みの状態。登録できない場合はnull。 </returns>
        private static T GetOrRegister<T>(ref bool isOwned) where T : class, new()
        {
            if (ServiceLocator.TryGetInstance(out T instance))
            {
                return instance;
            }

            instance = new T();
            isOwned = ServiceLocator.RegisterInstance(instance);
            return isOwned ? instance : null;
        }

        /// <summary>
        ///     このモジュールが登録した状態だけを解除します。
        /// </summary>
        /// <typeparam name="T"> 状態の型。 </typeparam>
        /// <param name="instance"> 登録した状態。 </param>
        /// <param name="isOwned"> このモジュールが登録した場合はtrue。解除後にfalseへ戻します。 </param>
        private static void UnregisterIfOwned<T>(T instance, ref bool isOwned) where T : class
        {
            if (!isOwned)
            {
                return;
            }

            if (ServiceLocator.TryGetInstance(out T registered) && ReferenceEquals(registered, instance))
            {
                ServiceLocator.UnregisterInstance<T>();
            }

            isOwned = false;
        }

        /// <summary>
        ///     指定したシーンが読み込まれているか判定します。
        /// </summary>
        /// <param name="sceneName"> 判定するシーン名。 </param>
        /// <returns> 読み込まれている場合はtrue。 </returns>
        private static bool IsSceneLoaded(string sceneName)
        {
            return !string.IsNullOrWhiteSpace(sceneName) && SceneManager.GetSceneByName(sceneName).isLoaded;
        }
    }
}
