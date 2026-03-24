using System;

namespace Research.SaveSystem
{
    /// <summary>
    ///     アウトゲーム情報をロードするクラス。
    /// </summary>
    public class LoadOutGameDataController
    {
        public LoadOutGameDataController(LoadService<OutGameData, OutGameDataDto> loadService)
        {
            _loadService = loadService;
        }
        /// <summary>
        ///     ロードを行う。
        /// </summary>
        /// <param name="callback">ロード後に実行したい処理</param>
        public void Load(Action<OutGameDataDto> callback)
        {
            _loadService.Load(callback);
        }

        private LoadService<OutGameData, OutGameDataDto> _loadService;
    }
}