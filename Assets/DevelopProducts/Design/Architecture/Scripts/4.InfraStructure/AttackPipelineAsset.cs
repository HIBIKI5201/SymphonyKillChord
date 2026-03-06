using DevelopProducts.Architecture.Application;
using DevelopProducts.Architecture.Utility;
using SymphonyFrameWork.Attribute;
using UnityEngine;

namespace DevelopProducts.Architecture.InfraStructure
{
    /// <summary>
    ///     攻撃パイプラインの設定を保持するアセットクラス。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(AttackPipelineAsset),
        menuName = Const.CREATE_ASSET_PATH + nameof(AttackPipelineAsset))]
    public class AttackPipelineAsset : ScriptableObject
    {
        /// <summary>
        ///     攻撃パイプラインを生成する。
        /// </summary>
        /// <returns> 生成された攻撃パイプライン。 </returns>
        public AttackPipeline Create()
        {
            return new AttackPipeline(_attackModifiers);
        }

        [SerializeReference, SubclassSelector, Tooltip("適用する攻撃修飾子のリスト。")]
        private IAttackModifier[] _attackModifiers;
    }
}
