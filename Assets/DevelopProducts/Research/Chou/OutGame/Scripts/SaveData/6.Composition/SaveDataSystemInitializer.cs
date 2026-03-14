using UnityEngine;
namespace Research.Chou.OutGame
{
    /// <summary>
    ///     セーブデータ管理システムを初期化するコンポーネント。
    /// </summary>
    public class SaveDataSystemInitializer : MonoBehaviour
    {
        private void Awake()
        {
            _saveDataEntity = new SaveDataEntity();
            _saveLoadEvents = new SaveLoadEvents();
            _saveGamePipeline = new SaveGamePipeline(_saveDataEntity, _saveLoadEvents);
            _saveGame = new SaveGame(_saveGamePipeline);
            _loadGamePipeline = new LoadGamePipeline(_saveDataEntity, _saveLoadEvents);
            _loadGame = new LoadGame(_loadGamePipeline);
            _saveViewController.Initialize(_saveGame, _saveLoadEvents);
            _loadViewController.Initialize(_loadGame, _saveLoadEvents);
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
    }
}