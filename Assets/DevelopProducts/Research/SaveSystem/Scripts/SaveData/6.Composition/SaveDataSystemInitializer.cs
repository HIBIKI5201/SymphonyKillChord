using System.Collections.Generic;
using UnityEngine;
namespace Research.SaveSystem
{
    /// <summary>
    ///     セーブデータ管理システムを初期化するコンポーネント。
    /// </summary>
    public class SaveDataSystemInitializer : MonoBehaviour
    {
        private void Awake()
        {
            _saveDataEntity = new SaveDataEntity();
            _saveDataMigration = new SaveDataMigration(GetSaveDataMigrations(), _saveDataEntity.SaveData);
            _saveGamePipeline = new SaveGamePipeline(_saveDataEntity);
            _saveGame = new SaveGame(_saveGamePipeline);
            _loadGamePipeline = new LoadGamePipeline(_saveDataEntity, _saveDataMigration);
            _loadGame = new LoadGame(_loadGamePipeline);
            _saveViewController.Initialize(_saveGame);
            _loadViewController.Initialize(_loadGame);
        }

        [SerializeField, Tooltip("")]
        private SaveViewController _saveViewController;
        [SerializeField, Tooltip("")]
        private LoadViewController _loadViewController;

        private SaveDataEntity _saveDataEntity;
        private SaveLoadEvents _saveLoadEvents;
        private SaveGame _saveGame;
        private LoadGame _loadGame;
        private SaveGamePipeline _saveGamePipeline;
        private LoadGamePipeline _loadGamePipeline;
        private SaveDataMigration _saveDataMigration;

        /// <summary>
        ///     データ移行処理のリストを取得する。
        ///     バージョン更新する度にここで新しい移行処理objectを追加する想定。
        /// </summary>
        /// <returns></returns>
        private List<ISaveDataMigration> GetSaveDataMigrations()
        {
            List<ISaveDataMigration> rst = new();
            rst.Add(new SaveDataMigration_0_1_To_0_2());
            rst.Add(new SaveDataMigration_0_2_To_0_3());
            return rst;
        }
    }
}