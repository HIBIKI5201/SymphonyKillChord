using UnityEngine;

namespace Research.SaveSystem
{
    /// <summary>
    ///     アウトゲームセーブデータの移行：0.1 ⇒ 0.2
    /// </summary>
    public class OutGameDataMigration_0_1_To_0_2 : SaveDataMigrationBase<OutGameData>
    {
        public OutGameDataMigration_0_1_To_0_2()
        {
            FromVersion = Constants.VERSION_0_1;
            ToVersion = Constants.VERSION_0_2;
        }
        public override async Awaitable Migrate(OutGameData saveData)
        {
            // データ移行の処理
            saveData.Version = ToVersion;
            await Awaitable.WaitForSecondsAsync(0.01f);
            Debug.Log($"データを新バージョンへ移行しました：{FromVersion} ⇒ {ToVersion}");
        }
    }
}