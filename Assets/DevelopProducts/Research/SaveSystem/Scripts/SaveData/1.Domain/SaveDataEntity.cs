using SymphonyFrameWork.System;
namespace Research.SaveSystem
{
    /// <summary>
    ///     セーブデータのエンティティ定義。
    /// </summary>
    public class SaveDataEntity
    {
        public KillChordGameData SaveData
        {
            get
            {
                if (_saveData is null)
                    Load();
                return _saveData;
            }
        }
        public SaveDataEntity()
        {
            Load();
        }
        /// <summary>
        ///     セーブする。
        /// </summary>
        public void Save()
        {
            SaveDataSystem<KillChordGameData>.Save();
        }

        /// <summary>
        ///     ロードする
        /// </summary>
        public void Load()
        {
            _saveData = SaveDataSystem<KillChordGameData>.Data;
        }

        private KillChordGameData _saveData;
    }
}