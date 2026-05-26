using UnityEngine;

namespace DevelopProducts.SkillTree
{
    public class CanUnlockUsecase
    {
        public CanUnlockUsecase(SkillCanUnlockService skillCanUnlockService, IPointRepository   pointRepository)
        {
            _skillCanUnlockService = skillCanUnlockService;
            _pointRepository = pointRepository;
        }
        public bool CheckUnlock(int cost)
        {
            return _skillCanUnlockService.CanUnlock(cost, _pointRepository.GetCurrentPoints().Point);
        }
        private readonly SkillCanUnlockService _skillCanUnlockService;
        private readonly IPointRepository _pointRepository;
    }
}
