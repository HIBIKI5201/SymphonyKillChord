using DevelopProducts.Design.GameMode.Domain;
using SymphonyFrameWork.Attribute;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameModeDefinition", menuName = "Scriptable Objects/GameModeDefinition")]
public class GameModeDefinition : ScriptableObject
{
    public IClearCondition ClearConditions => _clearConditions;
    public IFailCondition FailConditions => _failConditions;
    public List<IEvaluationCondition> EvaluationConditions => _evaluationConditions;

    [Header("クリア条件"),SerializeReference,SubclassSelector]
    private IClearCondition _clearConditions;

    [Header("失敗条件"), SerializeReference, SubclassSelector]
    private IFailCondition _failConditions;

    [Header("評価条件"), SerializeReference, SubclassSelector]
    private List<IEvaluationCondition> _evaluationConditions;
}
