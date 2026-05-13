using UnityEngine;

namespace DevelopProducts.SkillTree
{
    public class CanUnlockUsecase
    {
        public CanUnlockUsecase(SkillCanUnlockService skillCanUnlockService)
        {
            _skillCanUnlockService = skillCanUnlockService;
        }
        public bool CheckUnlock(SkillNodeEntity node, SkillTreeEntity tree, int currentPoints)
        {
            return _skillCanUnlockService.CanUnlock(node, tree, currentPoints);
        }
        private readonly SkillCanUnlockService _skillCanUnlockService;
    }
}
