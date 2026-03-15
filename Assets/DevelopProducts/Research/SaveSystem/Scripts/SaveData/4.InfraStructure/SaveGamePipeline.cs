using UnityEngine;
namespace Research.SaveSystem
{
    /// <summary>
    ///     セーブ処理の実装。
    /// </summary>
    public class SaveGamePipeline
    {
        public SaveGamePipeline(SaveDataEntity saveDataEntity, SaveLoadEvents saveLoadEvents)
        {
            _isSaving = false;
            _saveDataEntity = saveDataEntity;
            _saveLoadEvents = saveLoadEvents;
        }

        #region パブリックメソッド
        /// <summary>
        ///     セーブ処理を行う。
        /// </summary>
        /// <summary>
        /// Initiates saving of the provided game data and persists it to the configured save storage.
        /// </summary>
        /// <param name="data">The game state to save.</param>
        /// <remarks>
        /// If a save is already in progress or <paramref name="data"/> is null, the call has no effect.
        /// Errors that occur during persistence are caught and logged; this method does not throw.
        /// </remarks>
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
        private SaveLoadEvents _saveLoadEvents;
        private bool _isSaving;

        #region プライベートメソッド
        /// <summary>
        ///     セーブ開始イベントを発火し、一定時間後にセーブ終了イベントを発火する。
        /// </summary>
        /// <returns></returns>
        private async Awaitable SaveAsyncTask()
        {
            _isSaving = true;
            _saveLoadEvents.InvokeSaveStart();
            await Awaitable.WaitForSecondsAsync(Constants.SAVE_LOAD_WAIT_DUR);
            _isSaving = false;
            _saveLoadEvents.InvokeSaveEnd();
        }
        /// <summary>
        ///     セーブデータの値をSymphonyFrameworkのセーブデータオブジェクトに設定する。
        /// </summary>
        /// <summary>
        /// Copies relevant game-state fields from the provided KillChordGameData into the pipeline's internal save data object.
        /// </summary>
        /// <param name="newData">Source game data whose values will overwrite the internal save data.</param>
        private void SetSaveData(KillChordGameData newData)
        {
            _saveData.Gold = newData.Gold;
            _saveData.HpMax = newData.HpMax;
            _saveData.Attack = newData.Attack;
            _saveData.CritRate = newData.CritRate;
            _saveData.CritScale = newData.CritScale;
            _saveData.Equipments = newData.Equipments;
            _saveData.Skills = newData.Skills;
            _saveData.MissionProgress = newData.MissionProgress;
            _saveData.MissionUnlock = newData.MissionUnlock;
            _saveData.EquipmentUnlock = newData.EquipmentUnlock;
            _saveData.SkillUnlock = newData.SkillUnlock;
        }
        #endregion

    }
}