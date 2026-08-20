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
        where TSaveType : class, new()
        where TDtoType : class, new()
    {
        /// <summary>
        ///     セーブを行う。
        /// </summary>
        public async Awaitable Save(TDtoType dto)
        {
            TSaveType saveData = await SaveSystem<TSaveType, NugetDataLoader<TSaveType>>.Get();
            PropertyCopyUtil.CopyFields(saveData, dto);
            await SaveSystem<TSaveType, NugetDataLoader<TSaveType>>.Save();
        }
    }
}