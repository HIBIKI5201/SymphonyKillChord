using NUnit.Framework.Constraints;
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
            _saveDataEntity = new();

            _systemDataValidator = new SystemDataValidator();
            _playerDataValidator = new PlayerDataValidator(_stageItemDB);
            _outGameDataValidator = new OutGameDataValidator(_stageItemDB, _skillItemDB);

            _saveSystemDataRepo = new SaveGameSymphonyRepository<SystemData, SystemDataDto>();
            _savePlayerDataRepo = new SaveGameSymphonyRepository<PlayerData, PlayerDataDto>();
            _saveOutGameDataRepo = new SaveGameSymphonyRepository<OutGameData, OutGameDataDto>();

            _saveSystemDataService = new SaveService<SystemData, SystemDataDto>(_saveSystemDataRepo, _systemDataValidator);
            _savePlayerDataService = new SaveService<PlayerData, PlayerDataDto>(_savePlayerDataRepo, _playerDataValidator);
            _saveOutGameDataService = new SaveService<OutGameData, OutGameDataDto>(_saveOutGameDataRepo, _outGameDataValidator);

            _saveSystemDataController = new SaveSystemDataController(_saveSystemDataService);
            _savePlayerDataController = new SavePlayerDataController(_savePlayerDataService);
            _saveOutGameDataController = new SaveOutGameDataController(_saveOutGameDataService);

            InitSaveDataMigrasions();

            _loadSystemDataRepo = new LoadGameSymphonyRepository<SystemData>(_saveDataEntity, _systemDataMigration);
            _playerDataRepo = new LoadGameSymphonyRepository<PlayerData>(_saveDataEntity, _playerDataMigration);
            _outGameDataRepo = new LoadGameSymphonyRepository<OutGameData>(_saveDataEntity, _outGameDataMigration);

            _loadSystemDataService = new LoadService<SystemData, SystemDataDto>(_saveDataEntity, _loadSystemDataRepo);
            _loadPlayerDataService = new LoadService<PlayerData, PlayerDataDto>(_saveDataEntity, _playerDataRepo);
            _loadOutGameDataService = new LoadService<OutGameData, OutGameDataDto>(_saveDataEntity, _outGameDataRepo);

            _loadSystemDataController = new LoadSystemDataController(_loadSystemDataService);
            _loadPlayerDataController = new LoadPlayerDataController(_loadPlayerDataService);
            _loadOutGameDataController = new LoadOutGameDataController( _loadOutGameDataService);

            _homeView.Init(_saveSystemDataController, _savePlayerDataController, _saveOutGameDataController,
                _loadSystemDataController, _loadPlayerDataController, _loadOutGameDataController);
        }

        [SerializeField, Tooltip("")]
        private TestHomeView _homeView;
        [SerializeField, Tooltip("")]
        private SkillItemDB _skillItemDB;
        [SerializeField, Tooltip("")]
        private StageItemDB _stageItemDB;

        private SaveDataEntity _saveDataEntity;

        private SaveGameSymphonyRepository<SystemData, SystemDataDto> _saveSystemDataRepo;
        private SaveGameSymphonyRepository<PlayerData, PlayerDataDto> _savePlayerDataRepo;
        private SaveGameSymphonyRepository<OutGameData, OutGameDataDto> _saveOutGameDataRepo;

        private SaveService<SystemData, SystemDataDto> _saveSystemDataService;
        private SaveService<PlayerData, PlayerDataDto> _savePlayerDataService;
        private SaveService<OutGameData, OutGameDataDto> _saveOutGameDataService;

        private SystemDataValidator _systemDataValidator;
        private PlayerDataValidator _playerDataValidator;
        private OutGameDataValidator _outGameDataValidator;

        private SaveSystemDataController _saveSystemDataController;
        private SavePlayerDataController _savePlayerDataController;
        private SaveOutGameDataController _saveOutGameDataController;

        private LoadGameSymphonyRepository<SystemData> _loadSystemDataRepo;
        private LoadGameSymphonyRepository<PlayerData> _playerDataRepo;
        private LoadGameSymphonyRepository<OutGameData> _outGameDataRepo;

        private SaveDataMigration<SystemData> _systemDataMigration;
        private SaveDataMigration<PlayerData> _playerDataMigration;
        private SaveDataMigration<OutGameData> _outGameDataMigration;

        private LoadService<SystemData, SystemDataDto> _loadSystemDataService;
        private LoadService<PlayerData, PlayerDataDto> _loadPlayerDataService;
        private LoadService<OutGameData, OutGameDataDto> _loadOutGameDataService;

        private LoadSystemDataController _loadSystemDataController;
        private LoadPlayerDataController _loadPlayerDataController;
        private LoadOutGameDataController _loadOutGameDataController;


        /// <summary>
        ///     データ移行処理の初期化を行う。
        ///     バージョン更新する度にここで新しい移行処理objectを追加する想定。
        ///     移行処理の1個1個もScriptableObject化したほうが良い…？
        /// </summary>
        /// <returns></returns>
        private void InitSaveDataMigrasions()
        {
            List<SaveDataMigrationBase<SystemData>> systemDataMigrations = new();
            systemDataMigrations.Add(new SystemDataMigration_0_1_To_0_2());
            _systemDataMigration = new(systemDataMigrations);

            List<SaveDataMigrationBase<PlayerData>> playerDataMigrasions = new();
            playerDataMigrasions.Add(new PlayerDataMigration_0_1_To_0_2()); 
            _playerDataMigration = new(playerDataMigrasions);

            List<SaveDataMigrationBase<OutGameData>> outGameDataMigrations = new();
            outGameDataMigrations.Add(new OutGameDataMigration_0_1_To_0_2());
            _outGameDataMigration = new(outGameDataMigrations);
        }
    }
}