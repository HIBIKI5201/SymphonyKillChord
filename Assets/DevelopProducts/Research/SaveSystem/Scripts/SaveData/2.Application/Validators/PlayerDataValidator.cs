using System.Linq;

namespace Research.SaveSystem
{
    /// <summary>
    ///     プレイヤー情報をセーブ時の検証。
    /// </summary>
    public class PlayerDataValidator : ISaveDataValidatior<PlayerDataDto>
    {
        public PlayerDataValidator(StageItemDB stageItemDb)
        {
            _stageItemDb = stageItemDb;
        }

        public ValidationResult Validate(PlayerDataDto dto)
        {
            foreach(int skillId in dto.EquippedSkills)
            {
                // 装備中のスキルが存在しないスキルの場合、検証エラーとする
                if(!_stageItemDb.Items.Any(e => e.Id == skillId ))
                {
                    return new ValidationResult(false, "不正なスキルがあります。セーブできません。");
                }
            }
            return new ValidationResult(true, Constants.EMPTY_STRING);
        }

        private StageItemDB _stageItemDb;
    }
}