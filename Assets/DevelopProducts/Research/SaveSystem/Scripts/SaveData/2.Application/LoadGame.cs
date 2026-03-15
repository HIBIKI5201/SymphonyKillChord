using System;
namespace Research.SaveSystem
{
    /// <summary>
    ///     ロードするユースケース
    /// </summary>
    public class LoadGame
    {
        public LoadGame(LoadGamePipeline loadGamePipeline)
        {
            _loadGamePipeline = loadGamePipeline;
        }

        #region パブリックメソッド
        /// <summary>
        ///     セーブデータをロードする。
        /// </summary>
        /// <param name="callback">ロード後行う処理</param>
        public void LoadGameAsync(Action<KillChordGameData> callback)
        {
            _loadGamePipeline.LoadGameAsync(callback);
        }
        #endregion

        private LoadGamePipeline _loadGamePipeline;
    }
}