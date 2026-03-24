using UnityEngine;
namespace Research.SaveSystem
{
    /// <summary>
    ///     セーブ機能のRepositoryのインタフェース。
    /// </summary>
    /// <typeparam name="TSaveType"></typeparam>
    /// <typeparam name="TDtoType"></typeparam>
    public interface ISaveRepository<TSaveType, TDtoType>
    {
        public Awaitable Save(TDtoType dto);
    }
}