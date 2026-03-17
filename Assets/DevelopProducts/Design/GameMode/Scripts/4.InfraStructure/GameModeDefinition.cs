using DevelopProducts.Design.GameMode.Domain;
using DevelopProducts.Design.GameMode.Utility;
using SymphonyFrameWork.Attribute;
using System.Collections.Generic;
using UnityEngine;

namespace DevelopProducts.Design.GameMode.InfraStructure
{
    /// <summary>
    ///     ステージのクリア条件、失敗条件、評価条件を定義するクラス。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(GameModeDefinition), menuName = Const.CREATE_ASSET_PATH + nameof(GameModeDefinition))]
    public class GameModeDefinition : ScriptableObject
    {
        public IClearCondition ClearConditions => _clearConditions;
        public IFailCondition FailConditions => _failConditions;
        public List<IEvaluationCondition> EvaluationConditions => _evaluationConditions;

        [Header("クリア条件"), SerializeReference, SubclassSelector]
        private IClearCondition _clearConditions;

        [Header("失敗条件"), SerializeReference, SubclassSelector]
        private IFailCondition _failConditions;

        [Header("評価条件"), SerializeReference, SubclassSelector]
        private List<IEvaluationCondition> _evaluationConditions;
    }
}
