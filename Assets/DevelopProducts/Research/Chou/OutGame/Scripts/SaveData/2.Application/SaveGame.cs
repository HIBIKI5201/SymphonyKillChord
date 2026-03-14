using System;
namespace Research.Chou.OutGame
{
    /// <summary>
    ///     セーフするユースケース。
    /// </summary>
    public class SaveGame
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
        public void SaveGameAsync(KillChordGameData newData)
        {
            _saveGamePipeline.SaveAsync(newData);
        }
        #endregion

        private SaveGamePipeline _saveGamePipeline;
    }
}