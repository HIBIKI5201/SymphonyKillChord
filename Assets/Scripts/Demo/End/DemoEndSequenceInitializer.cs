using KillChord.Runtime.Adaptor.Persistent.Input;
using KillChord.Runtime.Adaptor.Persistent.Load;
using KillChord.Runtime.Application.Persistent.SceneManagement;
using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.Composition.Persistent.Input;
using KillChord.Runtime.View.Persistent.Input;
using KillChord.Runtime.View.Persistent.Music;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KillChord.Demo.End
{
    /// <summary>
    ///     体験版終了演出の再生順序と、タイトルへの復帰入力を制御するクラスです。
    /// </summary>
    public sealed class DemoEndSequenceInitializer : OutGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(DemoEndSequenceInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 30;

        /// <summary>
        ///     演出に必要なPersistentサービスを解決します。
        /// </summary>
        /// <returns> 成功した場合はtrueです。 </returns>
        public override bool Build()
        {
            if (!ValidateReferences())
            {
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out MusicPlayer musicPlayer))
            {
                Debug.LogError(
                    $"[{nameof(DemoEndSequenceInitializer)}] {nameof(MusicPlayer)} が取得できません。",
                    this);
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out _sceneTransitionUsecase))
            {
                Debug.LogError(
                    $"[{nameof(DemoEndSequenceInitializer)}] " +
                    $"{nameof(SceneTransitionUseCase)} が取得できません。",
                    this);
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out _playerInputView))
            {
                Debug.LogError(
                    $"[{nameof(DemoEndSequenceInitializer)}] {nameof(PlayerInputView)} が取得できません。",
                    this);
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out _inputComposition)
                || _inputComposition.GetInputMapController == null)
            {
                Debug.LogError(
                    $"[{nameof(DemoEndSequenceInitializer)}] {nameof(InputComposition)} が取得できません。",
                    this);
                return false;
            }

            // ロード画面が閉じるまで演出開始を待つために参照する。取得できなくても再生は続行する。
            ServiceLocator.TryGetInstance(out _loadingScreenController);

            // MusicPlayerはCue切り替えと音量制御の両方の実装を兼ねる。
            _bgmView.Initialize(musicPlayer, musicPlayer);
            return true;
        }

        /// <summary>
        ///     演出の再生を開始します。
        /// </summary>
        /// <returns> 常にtrueです。 </returns>
        public override bool Ready()
        {
            // 案内が出るまでは操作を受け付けない。
            _inputComposition.GetInputMapController.EnableOnly(InputMapNames.Common);
            _sequenceCancellationTokenSource?.Cancel();
            _sequenceCancellationTokenSource?.Dispose();
            _sequenceCancellationTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            _ = RunSequenceAsync(_sequenceCancellationTokenSource.Token);
            return true;
        }

        /// <summary>
        ///     演出を中断して購読を解除します。
        /// </summary>
        public override void Shutdown()
        {
            _sequenceCancellationTokenSource?.Cancel();
            _sequenceCancellationTokenSource?.Dispose();
            _sequenceCancellationTokenSource = null;
            UnsubscribeAttackInput();
            _movieView?.Stop();
            _bgmView?.StopAndRestoreVolume();
            _isTransitioning = false;
        }

        private const int MAX_LOADING_WAIT_FRAME_COUNT = 1800;

        [SerializeField, Tooltip("体験版終了演出の再生設定です。")]
        private DemoEndSequenceConfig _config;

        [SerializeField, Tooltip("デモムービーTimelineを再生するViewです。")]
        private DemoEndMovieView _movieView;

        [SerializeField, Tooltip("暗転と案内UIを表示するViewです。")]
        private DemoEndView _endView;

        [SerializeField, Tooltip("BGMの再生とフェードアウトを行うViewです。")]
        private DemoEndBgmView _bgmView;

        private SceneTransitionUseCase _sceneTransitionUsecase;
        private PlayerInputView _playerInputView;
        private InputComposition _inputComposition;
        private LoadingScreenController _loadingScreenController;
        private CancellationTokenSource _sequenceCancellationTokenSource;
        private bool _isAttackSubscribed;
        private bool _isAttackRequested;
        private bool _isTransitioning;

        /// <summary>
        ///     破棄時に購読を解除します。
        /// </summary>
        private void OnDestroy()
        {
            Shutdown();
        }

        /// <summary>
        ///     攻撃入力を受け取り、タイトルへの復帰を要求します。
        /// </summary>
        /// <param name="inputContext"> 受け取った攻撃入力です。 </param>
        private void HandleAttackInput(InputContext<float> inputContext)
        {
            if (inputContext.Phase != InputActionPhase.Performed)
            {
                return;
            }

            _isAttackRequested = true;
        }

        /// <summary>
        ///     ムービー再生から案内表示、タイトル復帰までを順に実行します。
        /// </summary>
        /// <param name="cancellationToken"> 演出を中断するためのトークンです。 </param>
        private async Awaitable RunSequenceAsync(CancellationToken cancellationToken)
        {
            try
            {
                // ロード画面の裏でムービーが進行しないよう、閉じるまで待つ。
                await WaitForLoadingScreenClosedAsync(cancellationToken);

                _bgmView.Play(_config.BattleBgmCueName);

                // カメラワークと暗転はTimelineが持つ。停止まで待機する。
                await _movieView.PlayAsync(cancellationToken);

                // Timelineの復元でアニメート値が戻っても黒画面を維持する。
                _endView.ShowBlackoutImmediate();

                await _endView.ShowEndUiAsync(
                    _config.EndUiFadeInDuration,
                    cancellationToken);

                // BGMのフェードアウトと入力受付は並行して進める。
                _ = FadeOutBgmAsync(cancellationToken);

                while (!cancellationToken.IsCancellationRequested)
                {
                    await WaitForAttackInputAsync(cancellationToken);
                    if (await ReturnToTitleAsync(cancellationToken))
                    {
                        return;
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
        ///     ロード画面が閉じるまで待機します。
        /// </summary>
        /// <param name="cancellationToken"> キャンセル用のトークンです。 </param>
        private async Awaitable WaitForLoadingScreenClosedAsync(CancellationToken cancellationToken)
        {
            if (_loadingScreenController == null)
            {
                return;
            }

            for (int waitFrameCount = 0;
                waitFrameCount < MAX_LOADING_WAIT_FRAME_COUNT;
                waitFrameCount++)
            {
                if (!_loadingScreenController.IsLoading)
                {
                    return;
                }

                await Awaitable.NextFrameAsync(cancellationToken);
            }

            Debug.LogWarning(
                $"[{nameof(DemoEndSequenceInitializer)}] " +
                "ロード画面の終了待機がタイムアウトしたため、演出を開始します。",
                this);
        }

        /// <summary>
        ///     案内表示から一定時間後にBGMをフェードアウトさせます。
        /// </summary>
        /// <param name="cancellationToken"> キャンセル用のトークンです。 </param>
        private async Awaitable FadeOutBgmAsync(CancellationToken cancellationToken)
        {
            try
            {
                await DelayUnscaledAsync(
                    _config.BgmFadeOutDelaySeconds,
                    cancellationToken);

                await _bgmView.FadeOutAsync(
                    _config.BgmFadeOutDuration,
                    cancellationToken);
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
        ///     入力受付までの待ち時間を置いてから、攻撃入力が行われるまで待機します。
        /// </summary>
        /// <param name="cancellationToken"> キャンセル用のトークンです。 </param>
        private async Awaitable WaitForAttackInputAsync(CancellationToken cancellationToken)
        {
            await DelayUnscaledAsync(
                _config.InputAcceptDelaySeconds,
                cancellationToken);

            _isAttackRequested = false;
            SubscribeAttackInput();
            _endView.SetPromptVisible(true);

            while (!_isAttackRequested)
            {
                await Awaitable.NextFrameAsync(cancellationToken);
            }

            UnsubscribeAttackInput();
        }

        /// <summary>
        ///     BGMを停止して背景シーンを外し、タイトルシーンへ遷移します。
        /// </summary>
        /// <param name="cancellationToken"> 遷移を中断するためのトークンです。 </param>
        /// <returns> タイトルシーンへ遷移できた場合はtrueです。 </returns>
        private async Awaitable<bool> ReturnToTitleAsync(CancellationToken cancellationToken)
        {
            if (_isTransitioning)
            {
                return false;
            }

            _isTransitioning = true;

            // 次シーンへ音量比率0を持ち越さないよう、遷移前に元の比率へ戻す。
            _bgmView.StopAndRestoreVolume();

            bool isSuccess = await _sceneTransitionUsecase.UnloadThenChangeSceneAsync(
                _config.BackgroundSceneName,
                gameObject.scene.name,
                _config.TitleSceneName,
                cancellationToken);

            if (isSuccess)
            {
                return true;
            }

            Debug.LogError(
                $"[{nameof(DemoEndSequenceInitializer)}] タイトルシーンへの遷移に失敗しました。",
                this);
            _isTransitioning = false;
            return false;
        }

        /// <summary>
        ///     攻撃入力の購読を開始し、入力マップを有効化します。
        /// </summary>
        private void SubscribeAttackInput()
        {
            if (_isAttackSubscribed)
            {
                return;
            }

            _inputComposition.GetInputMapController.EnableCommonWith(InputMapNames.InGame);
            _playerInputView.OnAttackInput += HandleAttackInput;
            _isAttackSubscribed = true;
        }

        /// <summary>
        ///     攻撃入力の購読を解除します。
        /// </summary>
        private void UnsubscribeAttackInput()
        {
            if (!_isAttackSubscribed || _playerInputView == null)
            {
                _isAttackSubscribed = false;
                return;
            }

            _playerInputView.OnAttackInput -= HandleAttackInput;
            _isAttackSubscribed = false;
        }

        /// <summary>
        ///     必須の参照が設定されているか検証します。
        /// </summary>
        /// <returns> すべて設定されている場合はtrueです。 </returns>
        private bool ValidateReferences()
        {
            if (_config == null
                || _movieView == null
                || _endView == null
                || _bgmView == null)
            {
                Debug.LogError(
                    $"[{nameof(DemoEndSequenceInitializer)}] 必須の参照が設定されていません。",
                    this);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     TimeScaleに影響されない待機を行います。
        /// </summary>
        /// <param name="seconds"> 待機する秒数です。 </param>
        /// <param name="cancellationToken"> キャンセル用のトークンです。 </param>
        private static async Awaitable DelayUnscaledAsync(
            float seconds,
            CancellationToken cancellationToken)
        {
            float elapsedSeconds = 0.0f;
            while (elapsedSeconds < seconds)
            {
                await Awaitable.NextFrameAsync(cancellationToken);
                elapsedSeconds += Time.unscaledDeltaTime;
            }
        }
    }
}
