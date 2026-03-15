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
        /// <param name="callback">ロード後実行する処理</param>
        public void LoadGameAsync(Action<KillChordGameData> callback)
        {
            if(_isLoading) return;

            LoadAsyncTask().Forget();
            KillChordGameData saveData = _saveDataEntity.SaveData;
            callback.Invoke(saveData);
        }

        private bool _isLoading;
        private SaveDataEntity _saveDataEntity;
        private SaveLoadEvents _saveLoadEvents;

        /// <summary>
        ///     ロード開始イベントを発火し、一定時間後にロード終了イベントを発火する。
        /// </summary>
        /// <returns></returns>
        private async Awaitable LoadAsyncTask()
        {
            _isLoading = true;
            _saveLoadEvents.InvokeLoadStart();
            await Awaitable.WaitForSecondsAsync(Constants.SAVE_LOAD_WAIT_DUR);
            _isLoading = false;
            _saveLoadEvents.InvokeLoadEnd();
        }
    }
}