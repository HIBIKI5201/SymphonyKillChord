using SymphonyFrameWork.System.SaveSystem;
using UnityEngine;
namespace Research.SaveSystem
{
    /// <summary>
    ///     SymphonyFrameworkを用いたセーブ機能。
    /// </summary>
    /// <typeparam name="TSaveType"></typeparam>
    /// <typeparam name="TDtoType"></typeparam>
    public class SaveGameSymphonyRepository<TSaveType, TDtoType> : ISaveRepository<TSaveType, TDtoType>
        where TSaveType : SaveDataContent, new()
        where TDtoType : class, new()
    {
        /// <summary>
        ///     セーブを行う。
        /// </summary>
        public async Awaitable Save(TDtoType dto)
        {
            // SaveStore.Getは暗黙ロードを行わないため、未読み込みのときだけLoadAsyncを呼ぶ。
            // LoadAsyncは常に保存先を読み直すため、無条件に呼ぶと未保存のキャッシュを捨ててしまう。
            TSaveType saveData = SaveStore.IsLoaded<TSaveType>()
                ? SaveStore.Get<TSaveType>()
                : await SaveStore.LoadAsync<TSaveType>();
            PropertyCopyUtil.CopyFields(saveData, dto);
            await SaveStore.SaveAsync<TSaveType>();
        }
    }
}