using System;
using UnityEngine;
namespace Research.SaveSystem
{
    /// <summary>
    ///     ロード処理の実装。
    /// </summary>
    public class LoadGamePipeline
    {
        public LoadGamePipeline(SaveDataEntity saveDataEntity, SaveDataMigration saveDataMigration)
        {
            _saveDataEntity = saveDataEntity;
            _saveDataMigration = saveDataMigration;
        }
        /// <summary>
        ///     ロード処理を行う。
        /// </summary>
        /// <param name="callback">ロード後実行する処理</param>
        public void LoadGameAsync(Action<KillChordGameData> callback)
        {
            if(_isLoading) return;

            LoadAsyncTask(callback).Forget();
            _saveDataMigration.DoMigration();
            callback.Invoke(_saveDataEntity.SaveData);
        }

        private bool _isLoading;
        private SaveDataEntity _saveDataEntity;
        private SaveDataMigration _saveDataMigration;

        /// <summary>
        ///     ロード開始イベントを発火し、一定時間後にロード終了イベントを発火する。
        /// </summary>
        /// <returns></returns>
        private async Awaitable LoadAsyncTask(Action<KillChordGameData> callback)
        {
            _isLoading = true;
            EventBus<EOnLoadStart>.Raise(new EOnLoadStart());
            await Awaitable.WaitForSecondsAsync(Constants.SAVE_LOAD_WAIT_DUR);
            _isLoading = false;
            EventBus<EOnLoadEnd>.Raise(new EOnLoadEnd());
        }
    }
}