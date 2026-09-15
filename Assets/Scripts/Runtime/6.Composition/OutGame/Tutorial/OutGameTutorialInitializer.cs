using KillChord.Runtime.Adaptor.Persistent.Load;
using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.View.OutGame.Screen;
using SymphonyFrameWork.System.SaveSystem;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.Composition.OutGame.Tutorial
{
    /// <summary>
    ///     アウトゲームチュートリアルの開始条件を判定し、実行ライフサイクルを管理します。
    /// </summary>
    public sealed class OutGameTutorialInitializer : OutGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(OutGameTutorialInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 150;

        private OutGameUIEvent _outGameUIEvent;
        private LoadingScreenController _loadingScreenController;
        private SaveData _loadedSaveData;
        private bool _isWaitingForLoadingCompleted;
        private bool _isTutorialRunning;
        private bool _isInitialized;

        /// <summary>
        ///     チュートリアル進行状況をロードします。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> ロードに成功した場合はtrueです。 </returns>
        public override async Awaitable<bool> ResourceLoadAsync(
            CancellationToken cancellationToken)
        {
            _loadedSaveData = SaveStore.IsLoaded<SaveData>()
                ? SaveStore.Get<SaveData>()
                : await SaveStore.LoadAsync<SaveData>(cancellationToken);
            return _loadedSaveData != null;
        }

        /// <summary>
        ///     アウトゲーム共通サービスとの依存を解決します。
        /// </summary>
        /// <returns> 依存解決に成功した場合はtrueです。 </returns>
        public override bool Build()
        {
            if (!ServiceLocator.TryGetInstance(out _outGameUIEvent))
            {
                Debug.LogError(
                    $"[{nameof(OutGameTutorialInitializer)}] "
                    + $"{nameof(OutGameUIEvent)}を取得できませんでした。",
                    this);
                return false;
            }

            ServiceLocator.TryGetInstance(out _loadingScreenController);
            _isInitialized = true;
            return true;
        }

        /// <summary>
        ///     OutGameのロード完了後にチュートリアル開始処理を予約します。
        /// </summary>
        /// <returns> 開始処理を予約できた場合はtrueです。 </returns>
        public override bool Ready()
        {
            if (!_isInitialized)
            {
                return false;
            }

            StartTutorialAfterLoading();
            return true;
        }

        /// <summary>
        ///     イベント購読と保持した参照を解放します。
        /// </summary>
        public override void Shutdown()
        {
            UnsubscribeLoadingCompleted();
            _outGameUIEvent = null;
            _loadingScreenController = null;
            _loadedSaveData = null;
            _isTutorialRunning = false;
            _isInitialized = false;
        }

        /// <summary>
        ///     OutGameのロード完了通知を受けてチュートリアルを開始します。
        /// </summary>
        /// <param name="isSuccess"> ロードに成功した場合はtrueです。 </param>
        private void HandleLoadingCompleted(bool isSuccess)
        {
            UnsubscribeLoadingCompleted();
            if (isSuccess)
            {
                StartHomeTutorial();
            }
        }

        /// <summary>
        ///     ロード状態に応じて開始するか、完了通知を待ちます。
        /// </summary>
        private void StartTutorialAfterLoading()
        {
            if (_loadingScreenController == null || !_loadingScreenController.IsLoading)
            {
                StartHomeTutorial();
                return;
            }

            _loadingScreenController.LoadingCompleted += HandleLoadingCompleted;
            _isWaitingForLoadingCompleted = true;

            // 状態確認とイベント購読の間にロードが完了した場合にも開始を取りこぼさない。
            if (!_loadingScreenController.IsLoading)
            {
                UnsubscribeLoadingCompleted();
                StartHomeTutorial();
            }
        }

        /// <summary>
        ///     ホームチュートリアルを開始し、実体の完了後に進行状態を保存します。
        /// </summary>
        private async void StartHomeTutorial()
        {
            if (!_isInitialized
                || _isTutorialRunning
                || _loadedSaveData == null
                || _loadedSaveData.Tutorial.Phase < TutorialPhase.BattleCompleted
                || _loadedSaveData.Tutorial.IsTutorialCompleted)
            {
                return;
            }

            _isTutorialRunning = true;
            try
            {
                if (_loadedSaveData.Tutorial.StartHome())
                {
                    await SaveStore.SaveAsync<SaveData>(destroyCancellationToken);
                }

                _outGameUIEvent.OnHomeTutorialStarted?.Invoke();
                await RunHomeTutorialAsync();

                if (_loadedSaveData.Tutorial.Complete())
                {
                    await SaveStore.SaveAsync<SaveData>(destroyCancellationToken);
                }

                _outGameUIEvent.OnHomeTutorialCompleted?.Invoke();
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
                _isTutorialRunning = false;
            }
        }

        /// <summary>
        ///     ホームチュートリアル本体を実行します。
        ///     現在は未実装のため即座に完了します。
        /// </summary>
        private static Task RunHomeTutorialAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        ///     ロード完了イベントの購読を解除します。
        /// </summary>
        private void UnsubscribeLoadingCompleted()
        {
            if (!_isWaitingForLoadingCompleted || _loadingScreenController == null)
            {
                return;
            }

            _loadingScreenController.LoadingCompleted -= HandleLoadingCompleted;
            _isWaitingForLoadingCompleted = false;
        }
    }
}
