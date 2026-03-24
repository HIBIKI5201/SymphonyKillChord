namespace Research.SaveSystem
{
    /// <summary>
    ///     アウトゲーム情報をセーブするControllerクラス。
    /// </summary>
    public class SaveOutGameDataController
    {
        public SaveOutGameDataController(SaveService<OutGameData, OutGameDataDto> service)
        {
            _saveService = service;
        }
        /// <summary>
        ///     セーブを行う。
        /// </summary>
        /// <param name="dto"></param>
        public void Save(OutGameDataDto dto)
        {
            _saveService.Save(dto);
        }
        private SaveService<OutGameData, OutGameDataDto> _saveService;
    }
}
