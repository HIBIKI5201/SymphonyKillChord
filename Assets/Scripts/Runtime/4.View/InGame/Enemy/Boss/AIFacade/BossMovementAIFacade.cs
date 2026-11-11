using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     ボスAI用ファサード：移動系。
    /// </summary>
    [AddComponentMenu("KillChord/View/Enemy/BossMovementAIFacade")]
    public class BossMovementAIFacade : MovementAIFacadeBase<BossMoveView>
    {
    }
}
