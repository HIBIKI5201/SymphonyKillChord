namespace Research.SaveSystem
{
    /// <summary>
    ///     セーブデータ検証のインタフェース。
    /// </summary>
    /// <typeparam name="TDtoType"></typeparam>
    public interface ISaveDataValidator<TDtoType>
    {
        public ValidationResult Validate(TDtoType dto);
    }
}