using KillChord.Runtime.Application.InGame.Battle;
using SymphonyFrameWork.Attribute;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Battle
{
    /// <summary>
    ///     攻撃処理のパイプラインを定義するScriptableObject。
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(AttackPipelineAsset),
        menuName = "KillChord/Attack/" + nameof(AttackPipelineAsset))]
    public sealed class AttackPipelineAsset : ScriptableObject
    {
        /// <summary>
        ///     設定済みの攻撃処理のパイプラインを生成する。
        /// </summary>
        /// <returns></returns>
        public AttackPipeline Create()
        {
            return new AttackPipeline(_attackSteps);
        }

        [SerializeReference, SubclassSelector]
        private IAttackStep[] _attackSteps;
    }
}
