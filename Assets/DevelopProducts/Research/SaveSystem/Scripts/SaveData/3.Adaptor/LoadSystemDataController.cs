using System;

namespace Research.SaveSystem
{
    /// <summary>
    ///     システム情報をロードするControllerクラス。
    /// </summary>
    public class LoadSystemDataController
    {
        public LoadSystemDataController(LoadService<SystemData, SystemDataDto> loadService)
        {
            _loadService = loadService;
        }
        /// <summary>
        ///     ロードを行う。
        /// </summary>
        /// <param name="callback">ロード後に実行したい処理</param>
        public void Load(Action<SystemDataDto> callback)
        {
            _loadService.Load(callback);
        }

        private LoadService<SystemData, SystemDataDto> _loadService;
    }
}