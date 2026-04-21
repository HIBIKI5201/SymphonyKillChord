using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Mission.ClearCondition
{
    /// <summary>
    ///     クリア条件を表すインターフェース。
    /// </summary>
    public interface IMissionClearCondition
    {
        bool IsSatisfied(MissionProgress progress);
        string GetDescription();
    }
}
