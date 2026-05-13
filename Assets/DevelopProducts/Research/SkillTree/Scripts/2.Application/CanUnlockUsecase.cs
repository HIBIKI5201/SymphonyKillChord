using UnityEngine;

namespace DevelopProducts.SkillTree
{
    public class CanUnlockUsecase
    {
        public CanUnlockUsecase(SkillCanUnlockService skillCanUnlockService)
        {
            _skillCanUnlockService = skillCanUnlockService;
        }
        public bool CheckUnlock(SkillNodeEntity node, int currentPoints)
        {
            return _skillCanUnlockService.CanUnlock(node, currentPoints);
        }
        private readonly SkillCanUnlockService _skillCanUnlockService;
    }
}
