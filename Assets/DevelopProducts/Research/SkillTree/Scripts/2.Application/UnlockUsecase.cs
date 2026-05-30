namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     ノードを解放するUsecase
    /// </summary>
    public class UnlockUsecase
    {
        /// <summary>
        ///     コンストラクタ
        /// </summary>
        /// <param name="skillUnlockService"></param>
        /// <param name="pointRepository"></param>
        public UnlockUsecase(SkillCanUnlockService skillUnlockService, IPointRepository pointRepository)
        {
            _skillUnlockService = skillUnlockService;
            _pointRepository = pointRepository;
        }
        /// <summary>
        ///     ノードを解放する
        /// </summary>
        /// <param name="totalCost"></param>
        /// <returns></returns>
        public bool Unlock(int totalCost)
        {
            if (!_skillUnlockService.CanUnlock(totalCost, _pointRepository.GetCurrentPoints().Point))
            {
                return false;
            }
            _pointRepository.UsePoints(totalCost);
            return true;
        }
        private readonly SkillCanUnlockService _skillUnlockService;
        private readonly IPointRepository _pointRepository;
    }
}
