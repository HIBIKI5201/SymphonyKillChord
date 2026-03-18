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
        /// <param name="data">セーブデータ</param>
        public void  SaveAsync(KillChordGameData data)
        {
            if (_isSaving) return;
            if (data == null) return;

            SaveAsyncTask().Forget();
            _saveData = _saveDataEntity.SaveData;
            SetSaveData(data);
            try
            {
                _saveDataEntity.Save();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"セーブ失敗しました: {e.Message}");
            }
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
        private async Awaitable SaveAsyncTask()
        {
            _isSaving = true;
            EventBus<EOnSaveStart>.Raise(new EOnSaveStart());
            await Awaitable.WaitForSecondsAsync(Constants.SAVE_LOAD_WAIT_DUR);
            _isSaving = false;
            EventBus<EOnSaveEnd>.Raise(new EOnSaveEnd());
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