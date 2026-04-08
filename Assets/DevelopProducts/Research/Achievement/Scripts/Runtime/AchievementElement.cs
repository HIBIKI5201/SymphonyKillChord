using UnityEngine;
using SymphonyFrameWork.Attribute;

namespace DevelopProducts.Achievement
{
    [CreateAssetMenu(fileName = nameof(AchievementElement),
        menuName = Const.CREATE_ASSET_MENU + nameof(AchievementElement))]
    public class AchievementElement : ScriptableObject
    {
        [SerializeReference, SubclassSelector]
        private IAchievementCondition[] _conditions;
    }
}
