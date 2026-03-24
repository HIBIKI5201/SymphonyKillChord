using System;

namespace Research.SaveSystem
{
    /// <summary>
    ///     プレイヤー情報をロードするControllerクラス。
    /// </summary>
    public class LoadPlayerDataController
    {
        public LoadPlayerDataController(LoadService<PlayerData, PlayerDataDto> loadService)
        {
            _loadService = loadService;
        }
        /// <summary>
        ///     ロードを行う。
        /// </summary>
        /// <param name="callback">ロード後に実行したい処理</param>
        public void Load(Action<PlayerDataDto> callback)
        {
            _loadService.Load(callback);
        }

        private LoadService<PlayerData, PlayerDataDto> _loadService;
    }
}