namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///    解放可能かを調べるUsecase 
    /// </summary>
    public class CanUnlockUsecase
    {
        /// <summary>
        ///     コンストラクタ
        /// </summary>
        /// <param name="skillCanUnlockService"></param>
        /// <param name="pointRepository"></param>
        public CanUnlockUsecase(SkillCanUnlockService skillCanUnlockService, IPointRepository   pointRepository)
        {
            _skillCanUnlockService = skillCanUnlockService;
            _pointRepository = pointRepository;
        }
        /// <summary>
        ///     解放できるかチェックする
        /// </summary>
        /// <param name="cost"></param>
        /// <returns></returns>
        public bool CheckUnlock(int cost)
        {
            return _skillCanUnlockService.CanUnlock(cost, _pointRepository.GetCurrentPoints().Point);
        }
        private readonly SkillCanUnlockService _skillCanUnlockService;
        private readonly IPointRepository _pointRepository;
    }
}
