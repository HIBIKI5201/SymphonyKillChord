using System.Collections.Generic;
using UnityEngine;
namespace Research.SaveSystem
{
    public class SaveDataMigration
    {
        public SaveDataMigration(List<ISaveDataMigration> migrations)
        {
            _migrations = migrations;
        }

        /// <summary>
        ///     セーブデータ移行を実施する。
        /// </summary>
        public void DoMigration(KillChordGameData saveData)
        {
            if (saveData is null || _migrations is null || _migrations.Count == 0) return;

            // 読み取ったセーブデータのバージョンナンバーを取得
            string saveDataVersion = saveData.VersionNo;

            // セーブデータのバージョンナンバーが現在のゲームバージョンより小さい間、
            // 繰り返して現在バージョンと一致するまでデータ移行を行う
            while (saveDataVersion.CompareTo(Constants.CURRENT_VERSION) < 0)
            {
                ISaveDataMigration mig = _migrations.Find(m => m.FromVersion == saveDataVersion);

                // 移行処理がなかった場合
                if(mig is null)
                {
                    Debug.LogError($"セーブデータ移行失敗：バージョン {saveDataVersion} が見つかりません。");
                    break;
                }

                mig.Migrate(saveData);
                saveDataVersion = saveData.VersionNo;
            }
        }

        private List<ISaveDataMigration> _migrations;
    }
}