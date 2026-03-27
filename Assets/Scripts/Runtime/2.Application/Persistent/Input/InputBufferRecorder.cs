using KillChord.Runtime.Domain;
using UnityEngine;
using UnityEngine.InputSystem;

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
        ///     Vector2値の入力をBufferedInputに変換してバッファに記録する。
        /// </summary>
        /// <param name="actionId"></param>
        /// <param name="actionPhase"></param>
        /// <param name="time"></param>
        /// <param name="value"></param>
        public void Record(InputActionId actionId, InputActionPhase actionPhase, float time, Vector2 value)
        {
            BufferedInput input = new BufferedInput(
                actionId,
                actionPhase,
                time,
                value,
                0.0f);

            _inputBuffer.Push(input);
        }

        /// <summary>
        ///     float値の入力をBufferedInputに変換してバッファに記録する。
        /// </summary>
        /// <param name="actionId"></param>
        /// <param name="actionPhase"></param>
        /// <param name="time"></param>
        /// <param name="value"></param>
        public void Record(InputActionId actionId, InputActionPhase actionPhase, float time, float value)
        {
            BufferedInput input = new BufferedInput(
                actionId,
                actionPhase,
                time,
                Vector2.zero,
                value);

            _inputBuffer.Push(input);
        }

        private readonly BufferedInputBuffer _inputBuffer;
    }
}
