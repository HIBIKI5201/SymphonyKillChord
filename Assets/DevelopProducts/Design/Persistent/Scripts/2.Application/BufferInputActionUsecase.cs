using DevelopProducts.Persistent.Domain.Input;
using UnityEngine;

namespace DevelopProducts.Persistent.Application
{
    /// <summary>
    ///     UnityのInputActionの入力をバッファに記録するユースケース。
    /// </summary>
    public class BufferInputActionUsecase
    {
        public BufferInputActionUsecase(IInputBufferWriter inputBufferWriter)
        {
            _inputBufferWriter = inputBufferWriter;
        }

        public void Execute(InputActionId actionId,
            InputPheseId pheseId,
            float timestamp)
        {
            BufferedInput input = new BufferedInput(actionId, pheseId, timestamp);
            _inputBufferWriter.Push(input);
        }

        private readonly IInputBufferWriter _inputBufferWriter;
    }
}
