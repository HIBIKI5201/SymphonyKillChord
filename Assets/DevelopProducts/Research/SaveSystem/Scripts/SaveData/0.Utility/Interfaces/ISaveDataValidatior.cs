namespace Research.SaveSystem
{
    /// <summary>
    ///     セーブデータ検証のインタフェース。
    /// </summary>
    /// <typeparam name="TDtoType"></typeparam>
    public interface ISaveDataValidatior<TDtoType>
    {
        public ValidationResult Validate(TDtoType dto);
    }
}