using DevelopProducts.BehaviorGraph.Runtime.Domain.Persistent.Input;

namespace DevelopProducts.BehaviorGraph.Runtime.Application.Persistent.Input
{
    /// <summary>
    ///     入力をBufferedInputに変換してBufferedInputBufferに記録するクラス。
    /// </summary>
    public class InputBufferRecorder
    {
        public InputBufferRecorder(InputBufferingQueue inputBuffer)
        {
            _inputBuffer = inputBuffer;
        }

        /// <summary>
        ///     入力をBufferedInputに変換してバッファに記録する。
        /// </summary>
        /// <param name="actionId"></param>
        /// <param name="actionPhase"></param>
        /// <param name="time"></param>
        /// <param name="value"></param>
        public void Record(BufferedInput input) => _inputBuffer.Push(input);

        private readonly InputBufferingQueue _inputBuffer;
    }
}
