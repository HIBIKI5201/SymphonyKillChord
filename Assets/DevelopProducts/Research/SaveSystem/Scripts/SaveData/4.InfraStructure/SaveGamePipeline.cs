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
        /// <param name="newData"></param>
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