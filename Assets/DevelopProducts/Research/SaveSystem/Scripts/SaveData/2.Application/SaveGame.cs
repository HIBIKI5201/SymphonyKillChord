using System;
namespace Research.SaveSystem
{
    /// <summary>
    ///     セーフするユースケース。
    /// </summary>
    public class SaveGame : ISaveService
    {
        public SaveGame(SaveGamePipeline saveGamePipeline)
        {
            _saveGamePipeline = saveGamePipeline;
        }
        #region パブリックメソッド
        /// <summary>
        ///     セーブデータをセーブする。
        /// </summary>
        /// <param name="newData">新データ</param>
        /// <returns></returns>
        public void Save(KillChordGameData newData)
        {
            _saveGamePipeline.SaveAsync(newData);
        }
        #endregion

        private SaveGamePipeline _saveGamePipeline;
    }
}