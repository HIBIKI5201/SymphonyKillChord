namespace Research.SaveSystem
{
    /// <summary>
    ///     システム情報をセーブするContrllerクラス。
    /// </summary>
    public class SaveSystemDataController
    {
        public SaveSystemDataController(SaveService<SystemData, SystemDataDto> service)
        {
            _saveService = service;
        }
        /// <summary>
        ///     セーブを行う。
        /// </summary>
        /// <param name="dto"></param>
        public void Save(SystemDataDto dto)
        {
            _saveService.Save(dto);
        }
        private SaveService<SystemData, SystemDataDto> _saveService;
    }
}