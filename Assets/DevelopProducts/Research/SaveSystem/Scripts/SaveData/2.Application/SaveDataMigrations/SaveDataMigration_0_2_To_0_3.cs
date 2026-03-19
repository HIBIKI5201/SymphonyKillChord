using UnityEngine;

namespace Research.SaveSystem {
    public class SaveDataMigration_0_2_To_0_3 : ISaveDataMigration
    {
        public string FromVersion => Constants.VERSION_0_2;

        public string ToVersion => Constants.VERSION_0_3;

        public void Migrate(KillChordGameData saveData)
        {
            // データ移行の処理
            Debug.Log($"データを新バージョンへ移行しました：{FromVersion} ⇒ {ToVersion}");
            saveData.VersionNo = ToVersion;
        }
    }
}