using SymphonyFrameWork.System.SaveSystem;
using System;
using UnityEngine;
namespace Research.SaveSystem
{
    /// <summary>
    ///     SymphonyFrameworkを用いたロード機能。
    /// </summary>
    /// <typeparam name="TSaveType"></typeparam>
    /// <typeparam name="TDtoType"></typeparam>
    public class LoadGameSymphonyRepository<TSaveType> : ILoadRepository<TSaveType>
        where TSaveType : SaveDataBase, new()
    {
        public LoadGameSymphonyRepository(SaveDataEntity saveDataEntity, SaveDataMigration<TSaveType> saveDataMigration)
        {
            _saveDataEntity = saveDataEntity;
            _saveDataMigration = saveDataMigration;
        }
        /// <summary>
        ///     ロードを行う。
        /// </summary>
        /// <returns></returns>
        public async Awaitable Load()
        {
            TSaveType saveData = await SaveSystem<TSaveType, NugetDataLoader<TSaveType>>.Get();
            await _saveDataMigration.DoMigration(saveData);
            _saveDataEntity.AssignData(saveData);
        }

        private SaveDataEntity _saveDataEntity;
        private SaveDataMigration<TSaveType> _saveDataMigration;
    }
}