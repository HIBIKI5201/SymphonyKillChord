using KillChord.Runtime.Domain.Persistent.Savedata;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.Application.Persistent.Savedata
{
    /// <summary>
    ///     環境設定を読み込み、変更要求を直列に保存するサービス。
    /// </summary>
    public sealed class EnvironmentSettingsService : IDisposable
    {
        /// <summary>
        ///     環境設定サービスを初期化する。
        /// </summary>
        public EnvironmentSettingsService(IEnvironmentSettingsRepository environmentSettingsRepository)
        {
            _environmentSettingsRepository = environmentSettingsRepository
                ?? throw new ArgumentNullException(nameof(environmentSettingsRepository));
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary> 保存が上限回数まで失敗したことを通知する。 </summary>
        public event Action<Exception> OnSaveFailed;

        /// <summary>
        ///     保存済みの環境設定を読み込む。
        /// </summary>
        public ValueTask<EnvironmentSettingsData> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            return _environmentSettingsRepository.LoadAsync(cancellationToken);
        }

        /// <summary>
        ///     最新の環境設定を保存キューへ追加する。
        /// </summary>
        public void QueueSave(EnvironmentSettingsData environmentSettings)
        {
            if (environmentSettings == null)
            {
                throw new ArgumentNullException(nameof(environmentSettings));
            }

            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(EnvironmentSettingsService));
            }

            _pendingSettings = environmentSettings.Copy();
            _hasPendingSave = true;

            if (!_isSaving)
            {
                _ = SavePendingSettingsAsync();
            }
        }

        /// <summary>
        ///     保存処理を停止してリソースを解放する。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            OnSaveFailed = null;
        }

        private const int MAX_SAVE_ATTEMPTS = 3;
        private const int INITIAL_RETRY_DELAY_MILLISECONDS = 1000;
        private const int RETRY_DELAY_MULTIPLIER = 2;

        private readonly IEnvironmentSettingsRepository _environmentSettingsRepository;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private EnvironmentSettingsData _pendingSettings;
        private bool _hasPendingSave;
        private bool _isSaving;
        private bool _isDisposed;

        /// <summary>
        ///     保存要求を直列に処理し、保存中に届いた要求は最新値だけを後続保存する。
        /// </summary>
        private async Task SavePendingSettingsAsync()
        {
            _isSaving = true;
            CancellationToken cancellationToken = _cancellationTokenSource.Token;

            try
            {
                // 保存中に新しい変更が来た場合は、最新の設定で保存し直す。
                while (_hasPendingSave && !cancellationToken.IsCancellationRequested)
                {
                    EnvironmentSettingsData settingsToSave = _pendingSettings;
                    _hasPendingSave = false;
                    int retryDelayMilliseconds = INITIAL_RETRY_DELAY_MILLISECONDS;

                    // 失敗した場合は待ち時間を延ばしながら再試行する。
                    for (int saveAttempt = 1; saveAttempt <= MAX_SAVE_ATTEMPTS; saveAttempt++)
                    {
                        try
                        {
                            await _environmentSettingsRepository.SaveAsync(
                                settingsToSave,
                                cancellationToken);
                            break;
                        }
                        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }
                        catch (Exception exception)
                        {
                            Debug.LogError(
                                $"[{nameof(EnvironmentSettingsService)}] 環境設定の保存に失敗しました。{exception}");

                            // 再試行中に新しい変更が来た場合は、その変更の保存へ移る。
                            if (_hasPendingSave)
                            {
                                break;
                            }

                            // 上限まで失敗した場合は、未保存のまま失敗を通知する。
                            if (saveAttempt >= MAX_SAVE_ATTEMPTS)
                            {
                                _hasPendingSave = true;
                                OnSaveFailed?.Invoke(exception);
                                return;
                            }

                            try
                            {
                                await Task.Delay(retryDelayMilliseconds, cancellationToken);
                            }
                            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                            {
                                return;
                            }

                            retryDelayMilliseconds *= RETRY_DELAY_MULTIPLIER;
                        }
                    }
                }
            }
            finally
            {
                _isSaving = false;
            }
        }
    }
}
