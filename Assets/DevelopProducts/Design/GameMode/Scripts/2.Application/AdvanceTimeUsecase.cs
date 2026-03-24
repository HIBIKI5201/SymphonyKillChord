using DevelopProducts.Design.GameMode.Domain;
using UnityEngine;

namespace DevelopProducts.Design.GameMode.Application
{
    /// <summary>
    ///     時間を進めるユースケース。ゲームのメインループで呼び出され、ステージの経過時間を更新する役割を持つ。
    /// </summary>
    public class AdvanceTimeUsecase
    {
        public AdvanceTimeUsecase(StageRuntimeContext runtimeContext)
        {
            _runtimeContext = runtimeContext;
        }

        public void Execute(float deltaTime)
        {
            _runtimeContext.StageTimeState.UpdateTimer(deltaTime);
        }

        private readonly StageRuntimeContext _runtimeContext;
    }
}
