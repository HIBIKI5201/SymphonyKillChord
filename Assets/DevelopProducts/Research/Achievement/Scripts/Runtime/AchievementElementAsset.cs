using UnityEngine;
using SymphonyFrameWork.Attribute;

namespace DevelopProducts.Achievement
{
    [CreateAssetMenu(fileName = nameof(AchievementElementAsset),
        menuName = Const.CREATE_ASSET_MENU + nameof(AchievementElementAsset))]
    public class AchievementElementAsset : ScriptableObject
    {
        [SerializeReference, SubclassSelector]
        private IAchievementCondition[] _conditions;
    }
}
