using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Application
{
    /// <summary>
    ///     入力をBufferedInputに変換してBufferedInputBufferに記録するクラス。
    /// </summary>
    public class InputBufferRecorder
    {
        public InputBufferRecorder(BufferedInputBuffer inputBuffer)
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

        private readonly BufferedInputBuffer _inputBuffer;
    }
}
