using UnityEngine;
using SymphonyFrameWork.Attribute;

namespace DevelopProducts.Achievement
{
    [CreateAssetMenu(fileName = nameof(AchievementElementAsset),
        menuName = Const.CREATE_ASSET_MENU + nameof(AchievementElementAsset))]
    public class AchievementElementAsset : ScriptableObject
    {
        public AchievementElement Asset()
        {
            AchievementElementID[] dependences = new AchievementElementID[_asset.Length];
            for (int i = 0; i < dependences.Length; i++)
            {
                dependences[i] = new(_asset[i].ID);
            }

            return new(new(_id), dependences, _conditions);
        }

        public string ID => _id;

        [SerializeField]
        private string _id;

        [SerializeReference, SubclassSelector]
        private IAchievementCondition[] _conditions;

        [SerializeField]
        private AchievementElementAsset[] _asset;
    }
}
