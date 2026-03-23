using System.Linq;

namespace Research.SaveSystem
{
    /// <summary>
    ///     アウトゲーム情報をセーブ時の検証。
    /// </summary>
    public class OutGameDataValidator : ISaveDataValidatior<OutGameDataDto>
    {
        public OutGameDataValidator(StageItemDB stageItemDb, SkillItemDB skillItemDb)
        {
            _stageItemDb = stageItemDb;
            _skillItemDb = skillItemDb;
        }
        public ValidationResult Validate(OutGameDataDto dto)
        {
            foreach (int stageId in dto.StageUnlock)
            {
                // 解放済みステージのIDが存在しない場合、検証エラーとする
                if (!_stageItemDb.Items.Any(e => e.Id == stageId))
                {
                    return new ValidationResult(false, "不正なステージ情報があります。セーブできません。");
                }
            }

            foreach (int skillId in dto.SkillUnlock)
            {
                // 解放済みスキルのIDが存在しない場合、検証エラーとする
                if (!_skillItemDb.Items.Any(e => e.Id == skillId))
                {
                    return new ValidationResult(false, "不正なスキル情報があります。セーブできません。");
                }
            }

            return new ValidationResult(true, Constants.EMPTY_STRING);
        }

        private StageItemDB _stageItemDb;
        private SkillItemDB _skillItemDb;
    }
}