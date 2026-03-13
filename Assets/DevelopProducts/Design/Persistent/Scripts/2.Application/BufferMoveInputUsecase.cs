using DevelopProducts.Persistent.Domain.Input;

namespace DevelopProducts.Persistent.Application
{
    /// <summary>
    ///     UnityのInputActionの入力をバッファに記録するユースケース。
    /// </summary>
    public class BufferMoveInputUsecase
    {
        public BufferMoveInputUsecase(BufferdInputBuffer inputBufferWriter)
        {
            _inputBufferWriter = inputBufferWriter;
        }

        public void Execute(float x,
            float y,
            InputPheseId pheseId,
            float timestamp)
        {
            BufferedInput input = new BufferedInput(InputActionIds.Move, pheseId, timestamp, x, y);
            _inputBufferWriter.Push(input);
        }

        private readonly BufferdInputBuffer _inputBufferWriter;
    }
}
