using UnityEngine;
namespace Research.SaveSystem
{
    /// <summary>
    ///     ロード機能Repositoryのインタフェース。
    /// </summary>
    /// <typeparam name="TSaveType"></typeparam>
    public interface ILoadRepository<TSaveType>
    {
        public Awaitable Load();
    }
}