namespace Research.SaveSystem
{
    /// <summary>
    ///     プレイヤー情報をセーブするController。
    /// </summary>
    public class SavePlayerDataController
    {
        public SavePlayerDataController(SaveService<PlayerData, PlayerDataDto> service)
        {
            _saveService = service;
        }
        /// <summary>
        ///     セーブを行う。
        /// </summary>
        /// <param name="dto"></param>
        public void Save(PlayerDataDto dto)
        {
            _saveService.Save(dto);
        }
        private SaveService<PlayerData, PlayerDataDto> _saveService;
    }
}