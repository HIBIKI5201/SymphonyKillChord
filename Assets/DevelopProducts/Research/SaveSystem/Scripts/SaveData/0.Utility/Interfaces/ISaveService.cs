namespace Research.SaveSystem
{
    /// <summary>
    ///     セーブ機能Serviceのインタフェース。
    /// </summary>
    /// <typeparam name="TSaveType"></typeparam>
    /// <typeparam name="TDtoType"></typeparam>
    public interface ISaveService<TSaveType, TDtoType>
    {
        /// <summary>
        ///     セーブを行う。
        /// </summary>
        /// <param name="dto"></param>
        public void Save(TDtoType dto);
    }
}