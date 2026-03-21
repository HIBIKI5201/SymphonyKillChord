using UnityEngine;
namespace Research.SaveSystem
{
    /// <summary>
    ///     セーブ処理の実装。
    /// </summary>
    public class SaveGamePipeline
    {
        public SaveGamePipeline(SaveDataEntity saveDataEntity)
        {
            _isSaving = false;
            _saveDataEntity = saveDataEntity;
        }

        #region パブリックメソッド
        /// <summary>
        ///     セーブ処理を行う。
        /// </summary>
        /// <param name="newData">セーブデータ</param>
        public void SaveAsync(KillChordGameData newData)
        {
            if (_isSaving) return;
            if (newData == null) return;

            SaveAsyncTask(newData).Forget();
            
        }
        #endregion

        private SaveDataEntity _saveDataEntity;
        private KillChordGameData _saveData;
        private bool _isSaving;

        #region プライベートメソッド
        /// <summary>
        ///     セーブ開始イベントを発火し、一定時間後にセーブ終了イベントを発火する。
        /// </summary>
        /// <returns></returns>
        private async Awaitable SaveAsyncTask(KillChordGameData newData)
        {
            _isSaving = true;
            try
            {
                EventBus<EOnSaveStart>.Raise(new EOnSaveStart());
                _saveData = _saveDataEntity.SaveData;
                SetSaveData(newData); 
                await _saveDataEntity.Save();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"セーブ失敗しました: {e.Message}");
            }
            finally
            {
                EventBus<EOnSaveEnd>.Raise(new EOnSaveEnd());
                _isSaving = false;
            }
        }
        /// <summary>
        ///     セーブデータの値をSymphonyFrameworkのセーブデータオブジェクトに設定する。
        /// </summary>
        /// <param name="newData"></param>
        private void SetSaveData(KillChordGameData newData)
        {
            _saveData.PlayerData = newData.PlayerData;
            _saveData.OutGameData = newData.OutGameData;
            _saveData.SystemData = newData.SystemData;
        }
        #endregion

    }
}