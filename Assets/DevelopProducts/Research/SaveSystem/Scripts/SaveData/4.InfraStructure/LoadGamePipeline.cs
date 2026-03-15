using System;
using UnityEngine;
namespace Research.SaveSystem
{
    /// <summary>
    ///     ロード処理の実装。
    /// </summary>
    public class LoadGamePipeline
    {
        public LoadGamePipeline(SaveDataEntity saveDataEntity, SaveLoadEvents saveLoadEvents)
        {
            _saveDataEntity = saveDataEntity;
            _saveLoadEvents = saveLoadEvents;
        }
        /// <summary>
        ///     ロード処理を行う。
        /// </summary>
        /// <summary>
        /// Begins the game load workflow if no load is already running.
        /// </summary>
        /// <param name="callback">Action invoked with the loaded <see cref="KillChordGameData"/> when loading completes.</param>
        public void LoadGameAsync(Action<KillChordGameData> callback)
        {
            if(_isLoading) return;

            LoadAsyncTask(callback).Forget();
        }

        private bool _isLoading;
        private SaveDataEntity _saveDataEntity;
        private SaveLoadEvents _saveLoadEvents;

        /// <summary>
        ///     ロード開始イベントを発火し、一定時間後にロード終了イベントを発火する。
        /// </summary>
        /// <summary>
        /// Executes the load workflow: signals load start, waits for the configured delay, invokes the provided callback with the saved game data, and signals load end while ensuring the loading flag is cleared.
        /// </summary>
        /// <param name="callback">A delegate invoked with the loaded <see cref="KillChordGameData"/> after the wait period.</param>
        /// <returns>An awaitable that completes when the load workflow (events, delay, and callback invocation) has finished.</returns>
        private async Awaitable LoadAsyncTask(Action<KillChordGameData> callback)
        {
            _isLoading = true;
            try
            {
                _saveLoadEvents.InvokeLoadStart();
                await Awaitable.WaitForSecondsAsync(Constants.SAVE_LOAD_WAIT_DUR);
                callback.Invoke(_saveDataEntity.SaveData);
            }
            finally
            {
                _isLoading = false;
                _saveLoadEvents.InvokeLoadEnd();
            }
        }
    }
}