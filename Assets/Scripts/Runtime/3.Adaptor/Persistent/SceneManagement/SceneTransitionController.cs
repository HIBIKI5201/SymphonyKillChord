using KillChord.Runtime.Application.Persistent.SceneManagement;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.Persistent.SceneManagement
{
    /// <summary>
    ///     Viewからのシーン遷移要求を受け取り、シーン遷移サービスを呼び出すコントローラー。
    /// </summary>
    public class SceneTransitionController
    {
        /// <summary>
        ///     常駐寿命と初期化待機上限を指定して生成します。
        /// </summary>
        public SceneTransitionController(
            SceneTransitionUseCase usecase,
            CancellationToken persistentLifetimeToken = default,
            int maxWaitFrameCount = DEFAULT_MAX_WAIT_FRAME_COUNT)
        {
            _useCase = usecase
                ?? throw new ArgumentNullException(nameof(usecase));
            _persistentLifetimeToken = persistentLifetimeToken;
            if (maxWaitFrameCount <= 0) { throw new ArgumentOutOfRangeException(nameof(maxWaitFrameCount)); }
            _maxWaitFrameCount = maxWaitFrameCount;
        }

        /// <summary> 専用出撃が受付から失敗終端まで進行中の場合はtrueです。 </summary>
        public bool HasScenarioBattleSortie => _isScenarioBattleSortieActive;

        /// <summary> 常駐シーンの寿命です。実シーン操作は停止要求では中断しません。 </summary>
        public CancellationToken PersistentLifetimeToken => _persistentLifetimeToken;

        /// <summary>
        ///     シーン遷移を実行する。
        /// </summary>
        /// <param name="fromSceneName"> 遷移元シーン名。 </param>
        /// <param name="toSceneName"> 遷移先シーン名。 </param>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 成功したらtrue。 </returns>
        public async Task<bool> ChangeSceneAsync(
            string fromSceneName,
            string toSceneName,
            CancellationToken cancellationToken)
        {
            return await _useCase.ChangeSceneAsync(fromSceneName, toSceneName, cancellationToken);
        }

        /// <summary>
        ///     常駐シーンの寿命に従って、ロード画面を閉じずにシーン遷移を実行する。
        ///     遷移元シーンがアンロードされても、遷移先の初期化完了まで待機するために使用する。
        /// </summary>
        /// <param name="fromSceneName"> 遷移元シーン名。 </param>
        /// <param name="toSceneName"> 遷移先シーン名。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        public Task<bool> ChangeSceneKeepingLoadingWithPersistentLifetimeAsync(
            string fromSceneName,
            string toSceneName)
        {
            _persistentLifetimeToken.ThrowIfCancellationRequested();
            return _useCase.ChangeSceneKeepLoadingAsync(
                fromSceneName,
                toSceneName,
                _persistentLifetimeToken);
        }

        /// <summary>
        ///     シーンをAdditiveロードする。
        /// </summary>
        public Task<bool> LoadAdditiveAsync(
            string sceneName,
            CancellationToken cancellationToken)
        {
            return _useCase.LoadAdditiveAsync(
                sceneName,
                cancellationToken);
        }

        /// <summary>
        ///     シーンをアンロードする。
        /// </summary>
        public async Task<bool> UnloadAsync(
            string sceneName,
            CancellationToken cancellationToken)
        {
            return await _useCase.UnloadAsync(
                sceneName,
                cancellationToken);
        }

        /// <summary>
        ///     常駐シーンの寿命に従ってシーンをアンロードする。
        ///     呼び出し元のシーンがアンロードされても、アンロード完了後の後処理まで実行するために使用する。
        /// </summary>
        /// <param name="sceneName"> アンロードするシーン名。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        public Task<bool> UnloadWithPersistentLifetimeAsync(string sceneName)
        {
            _persistentLifetimeToken.ThrowIfCancellationRequested();
            return _useCase.UnloadAsync(sceneName, _persistentLifetimeToken);
        }

        /// <summary>
        ///     対象シーンをUnloadし、ActiveSceneを指定シーンへ戻す。
        /// </summary>
        public Task<bool> UnloadAndSetActiveAsync(
            string unloadSceneName,
            string activeSceneName,
            CancellationToken cancellationToken)
        {
            return _useCase.UnloadAndSetActiveAsync(
                unloadSceneName,
                activeSceneName,
                cancellationToken);
        }

        /// <summary>
        ///     予約・選択を変更する前に、一つだけ専用出撃を受け付けます。
        /// </summary>
        public bool TryBeginScenarioBattleSortie(string inGameSceneName)
        {
            if (HasScenarioBattleSortie || _persistentLifetimeToken.IsCancellationRequested) { return false; }
            _scenarioBattleInitializationStop = CancellationTokenSource.CreateLinkedTokenSource(_persistentLifetimeToken);
            _scenarioBattleInitializationToken = _scenarioBattleInitializationStop.Token;
            _scenarioBattleSceneName = inGameSceneName;
            _isScenarioBattleSortieActive = true;
            return true;
        }

        /// <summary>
        ///     専用出撃に一致する InGame へ初期化停止トークンを渡します。
        /// </summary>
        public bool TryGetScenarioBattleInitialization(string sceneName, out CancellationToken stopToken)
        {
            bool isDedicated = HasScenarioBattleSortie
                && string.Equals(_scenarioBattleSceneName, sceneName, StringComparison.Ordinal);
            stopToken = isDedicated ? _scenarioBattleInitializationToken : default;
            return isDedicated;
        }

        /// <summary>
        ///     対応する InGame 初期化 Task を保持します。
        /// </summary>
        public void RegisterScenarioBattleInitialization(string sceneName, Task initialization)
        {
            if (TryGetScenarioBattleInitialization(sceneName, out _))
            {
                _scenarioBattleInitialization = initialization;
            }
        }

        /// <summary>
        ///     開始済みロードの終了後に、初期化の後続フェーズを停止させます。
        /// </summary>
        public void StopScenarioBattleInitialization()
        {
            _scenarioBattleInitializationStop?.Cancel();
        }

        /// <summary>
        ///     手動復帰の前に初期化 Task の終了を確認します。
        ///     InGame が存在するのに未登録の場合は、終了済みと見なしません。
        /// </summary>
        public async Task<bool> SettleScenarioBattleInitializationAsync(bool requestStop, bool isInGameLoaded)
        {
            if (!HasScenarioBattleSortie) { return false; }
            if (requestStop) { StopScenarioBattleInitialization(); }
            for (int frame = 0; frame < _maxWaitFrameCount; frame++)
            {
                _persistentLifetimeToken.ThrowIfCancellationRequested();
                Task initialization = _scenarioBattleInitialization;
                if (initialization != null && initialization.IsCompleted)
                {
                    try
                    {
                        await initialization;
                    }
                    catch (Exception exception) when (requestStop && !_persistentLifetimeToken.IsCancellationRequested)
                    {
                        // 失敗済みでも Task の終了は確認できるため、手動復帰で残存シーンを整理できます。
                        Debug.LogException(exception);
                    }
                    _persistentLifetimeToken.ThrowIfCancellationRequested();
                    return true;
                }
                if (initialization == null && !isInGameLoaded) { return true; }
                await Awaitable.NextFrameAsync(_persistentLifetimeToken);
            }
            return false;
        }

        /// <summary>
        ///     成功・手動復帰の終端、または常駐終了で専用状態を片付けます。
        /// </summary>
        public void EndScenarioBattleSortie()
        {
            _scenarioBattleInitializationStop?.Dispose();
            _scenarioBattleInitializationStop = null;
            _scenarioBattleInitialization = null;
            _scenarioBattleSceneName = null;
            _scenarioBattleInitializationToken = default;
            _isScenarioBattleSortieActive = false;
        }

        /// <summary>
        ///     常駐寿命で両旧シーンを終了してから InGame をロードします。
        /// </summary>
        public Task<bool> UnloadSourcesThenLoadSceneKeepingLoadingWithPersistentLifetimeAsync(
            string scenarioSceneName, string outGameSceneName, string persistentSceneName, string inGameSceneName)
        {
            _persistentLifetimeToken.ThrowIfCancellationRequested();
            if (!TryGetScenarioBattleInitialization(inGameSceneName, out _)) { return Task.FromResult(false); }
            return _useCase.UnloadSourcesThenLoadSceneKeepLoadingAsync(
                scenarioSceneName, outGameSceneName, persistentSceneName, inGameSceneName,
                _persistentLifetimeToken);
        }

        /// <summary>
        ///     常駐寿命で回復用シーン変更と初期化待機を実行します。
        /// </summary>
        public Task<bool> ChangeSceneWithPersistentLifetimeAsync(string fromSceneName, string toSceneName)
        {
            _persistentLifetimeToken.ThrowIfCancellationRequested();
            return _useCase.ChangeSceneAsync(fromSceneName, toSceneName, _persistentLifetimeToken);
        }

        /// <summary>
        ///     常駐寿命で初期化失敗シーンを再読み込みします。
        /// </summary>
        public Task<bool> ReloadSceneWithPersistentLifetimeAsync(string sceneName)
        {
            _persistentLifetimeToken.ThrowIfCancellationRequested();
            return _useCase.ReloadSceneAsync(sceneName, _persistentLifetimeToken);
        }

        /// <summary>
        ///     常駐寿命で追加シーン終了とActiveScene復元を実行します。
        /// </summary>
        public Task<bool> UnloadAndSetActiveWithPersistentLifetimeAsync(string unloadSceneName, string activeSceneName)
        {
            _persistentLifetimeToken.ThrowIfCancellationRequested();
            return _useCase.UnloadAndSetActiveAsync(unloadSceneName, activeSceneName, _persistentLifetimeToken);
        }

        private const int DEFAULT_MAX_WAIT_FRAME_COUNT = 3600;

        private readonly SceneTransitionUseCase _useCase;
        private readonly CancellationToken _persistentLifetimeToken;
        private readonly int _maxWaitFrameCount;
        private bool _isScenarioBattleSortieActive;
        private string _scenarioBattleSceneName;
        private Task _scenarioBattleInitialization;
        private CancellationTokenSource _scenarioBattleInitializationStop;
        private CancellationToken _scenarioBattleInitializationToken;
    }
}
