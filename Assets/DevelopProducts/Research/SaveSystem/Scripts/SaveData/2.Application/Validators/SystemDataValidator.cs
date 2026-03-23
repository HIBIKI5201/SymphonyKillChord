namespace Research.SaveSystem
{
    /// <summary>
    ///     システム情報をセーブ時の検証。
    /// </summary>
    public class SystemDataValidator : ISaveDataValidator<SystemDataDto>
    {
        public ValidationResult Validate(SystemDataDto dto)
        {
            if (dto.MasterVolume < 0 || dto.MasterVolume > 1)
            {
                return new ValidationResult(false, "マスター音量が不正です。セーブできません。");
            }
            if (dto.BgmVolume < 0 || dto.BgmVolume > 1)
            {
                return new ValidationResult(false, "BGM音量が不正です。セーブできません。");
            }
            if (dto.SeVolume < 0 || dto.SeVolume > 1)
            {
                return new ValidationResult(false, "SE音量が不正です。セーブできません。");
            }
            return new ValidationResult(true, Constants.EMPTY_STRING);
        }
    }
}