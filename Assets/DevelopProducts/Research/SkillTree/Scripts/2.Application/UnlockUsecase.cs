namespace DevelopProducts.SkillTree
{
    public class UnlockUsecase
    {
    public UnlockUsecase(SkillCanUnlockService skillUnlockService, IPointRepository pointRepository)
        {
            _skillUnlockService = skillUnlockService;
            _pointRepository = pointRepository;
        }
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
