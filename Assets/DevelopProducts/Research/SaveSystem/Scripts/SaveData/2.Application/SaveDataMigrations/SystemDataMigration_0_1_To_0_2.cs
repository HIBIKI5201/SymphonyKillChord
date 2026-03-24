using UnityEngine;

namespace Research.SaveSystem
{
    /// <summary>
    ///     システム情報セーブデータの移行：0.1 ⇒ 0.2
    /// </summary>
    public class SystemDataMigration_0_1_To_0_2 : SaveDataMigrationBase<SystemData>
    {
        public SystemDataMigration_0_1_To_0_2()
        {
            FromVersion = Constants.VERSION_0_1;
            ToVersion = Constants.VERSION_0_2;
        }
        public override async Awaitable Migrate(SystemData saveData)
        {
            // データ移行の処理
            saveData.Version = ToVersion;
            await Awaitable.WaitForSecondsAsync(0.01f);
            Debug.Log($"データを新バージョンへ移行しました：{FromVersion} ⇒ {ToVersion}");
        }
    }
}