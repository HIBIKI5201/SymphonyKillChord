using KillChord.Runtime.Domain.Persistent.Savedata;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.Application.Persistent.Savedata
{
    /// <summary>
    ///     音量設定を読み込み、変更要求を直列に保存するサービス。
    /// </summary>
    public sealed class AudioSettingsService : IDisposable
    {
        /// <summary>
        ///     音量設定サービスを初期化する。
        /// </summary>
        public AudioSettingsService(IAudioSettingsRepository audioSettingsRepository)
        {
            _audioSettingsRepository = audioSettingsRepository
                ?? throw new ArgumentNullException(nameof(audioSettingsRepository));
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        ///     保存済みの音量設定を読み込む。
        /// </summary>
        public ValueTask<AudioSettingsData> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            return _audioSettingsRepository.LoadAsync(cancellationToken);
        }

        /// <summary>
        ///     最新の音量設定を保存キューへ追加する。
        /// </summary>
        public void QueueSave(AudioSettingsData audioSettings)
        {
            if (audioSettings == null)
            {
                throw new ArgumentNullException(nameof(audioSettings));
            }

            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AudioSettingsService));
            }

            _pendingSettings = audioSettings.Copy();
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
        }

        private readonly IAudioSettingsRepository _audioSettingsRepository;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private AudioSettingsData _pendingSettings;
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
                while (_hasPendingSave && !cancellationToken.IsCancellationRequested)
                {
                    AudioSettingsData settingsToSave = _pendingSettings;
                    _hasPendingSave = false;

                    try
                    {
                        await _audioSettingsRepository.SaveAsync(
                            settingsToSave,
                            cancellationToken);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                    catch (Exception exception)
                    {
                        Debug.LogError(
                            $"[{nameof(AudioSettingsService)}] 音量設定の保存に失敗しました。{exception}");
                        _hasPendingSave = true;
                        return;
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
