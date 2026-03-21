using SymphonyFrameWork.System.SaveSystem;
using UnityEngine;
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
        public async Awaitable Save()
        {
            await SaveSystem<KillChordGameData, NugetDataLoader<KillChordGameData>>.Save();
        }

        /// <summary>
        ///     ロードする
        /// </summary>
        public async Awaitable Load()
        {
            _saveData = await SaveSystem<KillChordGameData, NugetDataLoader<KillChordGameData>>.Get();
        }

        private KillChordGameData _saveData;
    }
}