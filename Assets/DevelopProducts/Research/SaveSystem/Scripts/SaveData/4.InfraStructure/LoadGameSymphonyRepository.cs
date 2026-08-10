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
        /// <summary>
        ///     ロード先のEntityとマイグレーション処理を受け取る。
        /// </summary>
        /// <param name="saveDataEntity">ロード結果を格納するEntity。</param>
        /// <param name="saveDataMigration">ロード後に適用するマイグレーション。</param>
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
            // SaveStore.Getは暗黙ロードを行わないため、LoadAsyncで実体を取得する。
            TSaveType saveData = await SaveStore.LoadAsync<TSaveType>();
            await _saveDataMigration.DoMigration(saveData);
            _saveDataEntity.AssignData(saveData);
        }

        private SaveDataEntity _saveDataEntity;
        private SaveDataMigration<TSaveType> _saveDataMigration;
    }
}