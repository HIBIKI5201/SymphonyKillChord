using System;
using System.Collections.Generic;
using UnityEngine;
namespace Research.SaveSystem
{
    /// <summary>
    ///     セーブデータを新しいバージョンに移行するクラス。
    /// </summary>
    /// <typeparam name="TSaveType"></typeparam>
    public class SaveDataMigration<TSaveType>
        where TSaveType : SaveDataBase, new()
    {
        public SaveDataMigration(List<SaveDataMigrationBase<TSaveType>> migrations)
        {
            _migrations = migrations;
        }

        /// <summary>
        ///     セーブデータ移行を実施する。
        /// </summary>
        public async Awaitable DoMigration(TSaveType saveData)
        {
            if (saveData is null || _migrations is null || _migrations.Count == 0) return;

            // 読み取ったセーブデータのバージョンナンバーを取得
            string saveDataVersion = saveData.Version;

            // セーブデータのバージョンナンバーが現在のゲームバージョンより小さい間、
            // 繰り返して現在バージョンと一致するまでデータ移行を行う
            while (CompareVersions(saveDataVersion, Constants.CURRENT_VERSION) < 0)
            {
                SaveDataMigrationBase<TSaveType> mig = _migrations.Find(m => m.FromVersion == saveDataVersion);

                // 移行処理がなかった場合
                if(mig is null)
                {
                    Debug.LogError($"セーブデータ移行失敗：バージョン {saveDataVersion} が見つかりません。");
                    break;
                }

                await mig.Migrate(saveData);
                saveDataVersion = saveData.Version;
            }
        }

        private List<SaveDataMigrationBase<TSaveType>> _migrations;

        private static int CompareVersions(string v1, string v2)
        {
            var parts1 = v1.Split('.');
            var parts2 = v2.Split('.');
            int maxLength = Math.Max(parts1.Length, parts2.Length);

            for (int i = 0; i < maxLength; i++)
            {
                int p1 = i < parts1.Length && int.TryParse(parts1[i], out var n1) ? n1 : 0;
                int p2 = i < parts2.Length && int.TryParse(parts2[i], out var n2) ? n2 : 0;
                if (p1 != p2) return p1.CompareTo(p2);
            }
            return 0;
        }
    }
}