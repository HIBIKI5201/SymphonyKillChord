using KillChord.DebugWorld.Battle;
using KillChord.DebugWorld.Common;
using KillChord.Runtime.Application.Persistent.SceneManagement;
using KillChord.Runtime.Composition.OutGame.StageSelect;
using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.InfraStructure.Addressables;
using KillChord.Runtime.InfraStructure.InGame.Enemy;
using KillChord.Runtime.InfraStructure.OutGame.StageSelect;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.DebugWorld.Hub
{
    /// <summary>
    ///     裏世界のハブシーンを初期化し、ハブ画面とシーン遷移をつなぐクラスです。
    /// </summary>
    public sealed class DebugHubInitializer : MonoBehaviour
    {
        [SerializeField, Tooltip("ハブ画面を表示するUIDocument。")]
        private UIDocument _uiDocument;

        [SerializeField, Tooltip("ステージツリー定義アセットの Addressables キー。")]
        private string _stageTreeAssetKey = "StageTreeAsset";

        [SerializeField, Tooltip("敵Wave定義リポジトリの Addressables キー。")]
        private string _enemyWaveDefinitionRepositoryKey = "EnemyWaveDefinitionRepository";

        [SerializeField, Tooltip("個別の敵定義リポジトリの Addressables キー。")]
        private string _enemyDefinitionRepositoryKey = "EnemyDefinitionRepository";

        private DebugHubView _view;
        private DebugBattleCatalog _catalog;
        private bool _isTransitioning;

        private async void Start()
        {
            // ハブに戻ってきたら、前の戦闘設定を外して通常のゲームに戻す。
            DebugBattleSession.End();

            bool isSuccess = false;
            try
            {
                _catalog = await TryLoadCatalogAsync(destroyCancellationToken);
                isSuccess = TryCreateView();
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
                CompleteSceneInitialization(isSuccess);
            }
        }

        private void OnDestroy()
        {
            _stageTreeAssetKey.ReleaseLoadedAsset(this);
            _enemyWaveDefinitionRepositoryKey.ReleaseLoadedAsset(this);
            _enemyDefinitionRepositoryKey.ReleaseLoadedAsset(this);

            if (_view == null)
            {
                return;
            }

            _view.OnPlayerControlTestRequested -= HandlePlayerControlTestRequested;
            _view.OnEnemyTestRequested -= HandleEnemyTestRequested;
            _view.OnReturnRequested -= HandleReturnRequested;
            _view.Dispose();
            _view = null;
        }

        /// <summary>
        ///     敵を出さない戦闘へ出撃します。
        /// </summary>
        private void HandlePlayerControlTestRequested()
        {
            _ = SortieAsync(new DebugBattleOverrides());
        }

        /// <summary>
        ///     選んだ敵を出し続ける戦闘へ出撃します。
        /// </summary>
        /// <param name="enemyIndex"> 敵の一覧の位置です。 </param>
        /// <param name="enemyAmount"> 1回に出す数です。 </param>
        private void HandleEnemyTestRequested(int enemyIndex, int enemyAmount)
        {
            if (_catalog == null || !_catalog.TryGetEnemyId(enemyIndex, out EnemyDefinitionId enemyId))
            {
                Debug.LogError($"[{nameof(DebugHubInitializer)}] 選んだ敵が見つかりません。Index: {enemyIndex}", this);
                return;
            }

            _ = SortieAsync(new DebugBattleOverrides(enemyId, enemyAmount));
        }

        /// <summary>
        ///     タイトルへ戻るよう求められたら、ハブシーンを閉じてタイトルシーンを開きます。
        /// </summary>
        private async void HandleReturnRequested()
        {
            if (!TryBeginTransition())
            {
                return;
            }

            try
            {
                bool isSuccess = await DebugWorldSceneChanger.ChangeSceneAsync(
                    gameObject.scene.name,
                    DebugWorldSceneNames.TITLE);
                if (!isSuccess)
                {
                    EndTransition();
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                EndTransition();
            }
        }

        /// <summary>
        ///     戦闘設定を登録し、出撃先のステージを選んでインゲームへ移ります。
        ///     <para> 戦闘が終わるとハブシーンへ戻ります。 </para>
        /// </summary>
        /// <param name="overrides"> 登録する戦闘設定です。 </param>
        private async Task SortieAsync(DebugBattleOverrides overrides)
        {
            if (_catalog == null || !_catalog.HasBaseStage || !TryBeginTransition())
            {
                return;
            }

            try
            {
                BattleStageDefinition stage = _catalog.BaseStage;
                DebugBattleSession.Begin(overrides);
                if (!BattleSortieSelectionStateResolver.CreateSelectionService()
                        .TryPrepareBattleSortie(stage, gameObject.scene.name))
                {
                    Debug.LogError(
                        $"[{nameof(DebugHubInitializer)}] 出撃の準備に失敗しました。StageId: {stage.StageId.Value}",
                        this);
                    DebugBattleSession.End();
                    EndTransition();
                    return;
                }

                bool isSuccess = await DebugWorldSceneChanger.ChangeSceneKeepingLoadingAsync(
                    gameObject.scene.name,
                    stage.TargetSceneName);
                if (!isSuccess)
                {
                    DebugBattleSession.End();
                    EndTransition();
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                DebugBattleSession.End();
                EndTransition();
            }
        }

        /// <summary>
        ///     出撃先と敵の一覧を Addressables から読み込みます。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 読み込んだ一覧です。失敗した場合はnullです。 </returns>
        private async Task<DebugBattleCatalog> TryLoadCatalogAsync(CancellationToken cancellationToken)
        {
            try
            {
                StageTreeAsset stageTreeAsset =
                    await _stageTreeAssetKey.LoadAssetAsync<StageTreeAsset>(this, cancellationToken);
                EnemyWaveDefinitionRepository waveDefinitionRepository =
                    await _enemyWaveDefinitionRepositoryKey.LoadAssetAsync<EnemyWaveDefinitionRepository>(
                        this,
                        cancellationToken);
                EnemyDefinitionRepository enemyDefinitionRepository =
                    await _enemyDefinitionRepositoryKey.LoadAssetAsync<EnemyDefinitionRepository>(
                        this,
                        cancellationToken);
                if (stageTreeAsset == null || waveDefinitionRepository == null || enemyDefinitionRepository == null)
                {
                    Debug.LogError($"[{nameof(DebugHubInitializer)}] 戦闘テスト用のデータを読み込めませんでした。", this);
                    return null;
                }

                DebugBattleCatalog catalog = new(
                    stageTreeAsset.Create(waveDefinitionRepository),
                    enemyDefinitionRepository,
                    DebugWorldSceneNames.BATTLE);
                if (!catalog.HasBaseStage)
                {
                    Debug.LogError($"[{nameof(DebugHubInitializer)}] 出撃先にできるバトルステージがありません。", this);
                }

                return catalog;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                // 戦闘テストを使えなくても、ハブ画面とタイトルへ戻る操作は使えるようにする。
                Debug.LogException(exception, this);
                return null;
            }
        }

        /// <summary>
        ///     ハブ画面を組み立てます。
        /// </summary>
        /// <returns> 組み立てられた場合はtrueです。 </returns>
        private bool TryCreateView()
        {
            if (_uiDocument == null || _uiDocument.rootVisualElement == null)
            {
                Debug.LogError($"[{nameof(DebugHubInitializer)}] UIDocumentが設定されていません。", this);
                return false;
            }

            bool isBattleAvailable = _catalog != null && _catalog.HasBaseStage;
            _view = new DebugHubView(_uiDocument.rootVisualElement, _catalog?.EnemyNames, isBattleAvailable);
            _view.OnPlayerControlTestRequested += HandlePlayerControlTestRequested;
            _view.OnEnemyTestRequested += HandleEnemyTestRequested;
            _view.OnReturnRequested += HandleReturnRequested;
            return true;
        }

        /// <summary>
        ///     シーン遷移を始めて、画面の操作を止めます。
        /// </summary>
        /// <returns> 遷移を始められた場合はtrueです。既に遷移中の場合はfalseです。 </returns>
        private bool TryBeginTransition()
        {
            if (_isTransitioning)
            {
                return false;
            }

            _isTransitioning = true;
            _view?.Lock();
            return true;
        }

        /// <summary>
        ///     遷移に失敗したときに、画面の操作を戻します。
        /// </summary>
        private void EndTransition()
        {
            if (this == null)
            {
                return;
            }

            _isTransitioning = false;
            _view?.Unlock();
        }

        /// <summary>
        ///     ロード画面を閉じられるよう、ハブシーンの初期化結果を通知します。
        /// </summary>
        /// <param name="isSuccess"> 初期化に成功した場合はtrueです。 </param>
        private void CompleteSceneInitialization(bool isSuccess)
        {
            if (!ServiceLocator.TryGetInstance(out ISceneInitializationReadiness readiness))
            {
                Debug.LogWarning(
                    $"[{nameof(DebugHubInitializer)}] {nameof(ISceneInitializationReadiness)}が取得できません。"
                    + "常駐シーンを通さずに開いた場合は、タイトルへ戻れません。",
                    this);
                return;
            }

            readiness.Complete(gameObject.scene.name, isSuccess);
        }
    }
}
