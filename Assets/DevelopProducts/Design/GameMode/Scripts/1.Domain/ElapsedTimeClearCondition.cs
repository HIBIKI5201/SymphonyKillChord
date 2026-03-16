using UnityEngine;

namespace DevelopProducts.Design.GameMode.Domain
{
    /// <summary>
    ///     ステージの経過時間に関するクリア条件。
    ///     例えば、何秒間生存することなど、ステージの経過時間に関連する条件を管理するためのクラス。
    /// </summary>
    public class ElapsedTimeClearCondition : IClearCondition
    {
        public bool IsSatisfied(StageRuntimeContext context)
        {
            return context.StageTimeState.ElapsedTime >= _requiredTime;
        }

        public string GetDescription()
        {
            return $"{_requiredTime}秒生存する";
        }

        [SerializeField] private float _requiredTime;
    }
}
