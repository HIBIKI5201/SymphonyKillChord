using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    ///     入力を履歴保存用に変換してInputBufferRecorderに渡すクラス。
    /// </summary>
    public class InputAdaptor
    {
        public InputAdaptor(InputBufferRecorder inputBufferRecorder)
        {
            _inputBufferRecorder = inputBufferRecorder;
        }

        /// <summary>
        ///     移動入力を変換してInputBufferRecorderに渡す。
        /// </summary>
        /// <param name="inputContext"></param>
        public void HandleMove(InputContext<Vector2> inputContext)
        {
            InputActionId actionId = InputIdCoverter.Convert(inputContext.ActionKind);
            _inputBufferRecorder.Record(actionId, inputContext.Phase, inputContext.Timestamp, inputContext.Value);
        }

        /// <summary>
        ///     float値の入力を変換してInputBufferRecorderに渡す。
        /// </summary>
        /// <param name="inputContext"></param>
        public void HandleButton(InputContext<float> inputContext)
        {
            InputActionId actionId = InputIdCoverter.Convert(inputContext.ActionKind);
            _inputBufferRecorder.Record(actionId, inputContext.Phase, inputContext.Timestamp, inputContext.Value);
        }

        private readonly InputBufferRecorder _inputBufferRecorder;
    }
}
