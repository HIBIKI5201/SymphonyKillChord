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
            _isLoading = false;
            _saveDataEntity = saveDataEntity;
            _saveDataMigration = saveDataMigration;
        }
        /// <summary>
        ///     ロード処理を行う。
        /// </summary>
        /// <param name="callback">ロード後実行する処理</param>
        public void LoadGameAsync(Action<KillChordGameData> callback)
        {
            if (_isLoading) return;

            LoadAsyncTask(callback).Forget();
        }

        private bool _isLoading;
        private SaveDataEntity _saveDataEntity;
        private SaveDataMigration _saveDataMigration;

        /// <summary>
        ///     ロード開始イベントを発火してロードする。<br/>
        ///     ロード完了後、ロード完了イベントを発火する。
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        private async Awaitable LoadAsyncTask(Action<KillChordGameData> callback)
        {
            _isLoading = true;
            try
            {
                EventBus<EOnLoadStart>.Raise(new EOnLoadStart());
                await _saveDataEntity.Load();
                _saveDataMigration.DoMigration(_saveDataEntity.SaveData);
                callback?.Invoke(_saveDataEntity.SaveData);
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                EventBus<EOnLoadEnd>.Raise(new EOnLoadEnd());
                _isLoading = false;
            }
        }
    }
}