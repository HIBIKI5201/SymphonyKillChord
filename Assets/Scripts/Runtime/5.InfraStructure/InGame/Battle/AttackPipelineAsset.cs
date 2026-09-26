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
    /// <summary>
    ///     攻撃処理のステップ構成を設定するデータ。
    /// </summary>
    public sealed class AttackPipelineAsset : ScriptableObject
    {
        /// <summary>
        ///     設定済みの攻撃処理のパイプラインを生成する。
        /// </summary>
        /// <returns> 生成された攻撃処理のパイプライン。 </returns>
        public AttackPipeline Create()
        {
            return new AttackPipeline(_attackSteps);
        }

        [SerializeReference, SubclassSelector, Tooltip("攻撃ステップの配列")] 
        private IAttackStep[] _attackSteps;
    }
}
