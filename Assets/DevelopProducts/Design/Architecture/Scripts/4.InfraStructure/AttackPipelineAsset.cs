using DevelopProducts.Architecture.Application;
using DevelopProducts.Architecture.Utility;
using SymphonyFrameWork.Attribute;
using UnityEngine;

namespace DevelopProducts.Architecture.InfraStructure
{
    [CreateAssetMenu(fileName = nameof(AttackPipelineAsset),
        menuName = Const.CREATE_ASSET_PATH + nameof(AttackPipelineAsset))]
    public class AttackPipelineAsset : ScriptableObject
    {
        public AttackPipeline Create()
        {
            return new AttackPipeline(_attackModifiers);
        }

        [SerializeReference, SubclassSelector]
        private IAttackModifier[] _attackModifiers;
    }
}
