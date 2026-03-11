using DevelopProducts.Persistent.Domain.Input;
using UnityEngine;

namespace DevelopProducts.Persistent.Application
{
    /// <summary>
    ///     ボタン入力をバッファに記録するユースケース。
    /// </summary>
    public class BufferButtonInputUsecase
    {
        public BufferButtonInputUsecase(IInputBufferWriter inputBufferWriter)
        {
            _inputBufferWriter = inputBufferWriter;
        }

        public void Execute(InputActionId actionId,
            InputPheseId pheseId,
            float timestamp)
        {
            BufferedInput input = new BufferedInput(actionId, pheseId, timestamp, 0f, 0f);
            _inputBufferWriter.Push(input);
        }

        private readonly IInputBufferWriter _inputBufferWriter;
    }
}