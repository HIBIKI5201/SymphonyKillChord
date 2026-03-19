using SymphonyFrameWork.System.SaveSystem;
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
            SaveSystem<KillChordGameData, NugetDataLoader<KillChordGameData>>.Save();
        }

        /// <summary>
        ///     ロードする
        /// </summary>
        public void Load()
        {
            _saveData = SaveSystem<KillChordGameData, NugetDataLoader<KillChordGameData>>.Get().Result;
        }

        private KillChordGameData _saveData;
    }
}