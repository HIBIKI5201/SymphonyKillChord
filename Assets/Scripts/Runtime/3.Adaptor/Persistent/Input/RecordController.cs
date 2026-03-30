using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    ///     入力を履歴保存用に変換してInputBufferRecorderに渡すクラス。
    /// </summary>
    public class RecordController
    {
        public RecordController(InputBufferRecorder inputBufferRecorder)
        {
            _inputBufferRecorder = inputBufferRecorder;
        }

        /// <summary>
        ///     移動入力を変換してInputBufferRecorderに渡す。
        /// </summary>
        /// <param name="inputContext"></param>
        public void HandleMove(InputContext<Vector2> inputContext)
        {
            InputActionId actionId = InputIdConverter.Convert(inputContext.ActionKind);
            BufferedInput bufferedInput = Convert(actionId, inputContext);
            _inputBufferRecorder.Record(bufferedInput);
        }

        /// <summary>
        ///     float値の入力を変換してInputBufferRecorderに渡す。
        /// </summary>
        /// <param name="inputContext"></param>
        public void HandleButton(InputContext<float> inputContext)
        {
            InputActionId actionId = InputIdConverter.Convert(inputContext.ActionKind);
            BufferedInput bufferedInput = Convert(actionId, inputContext);
            _inputBufferRecorder.Record(bufferedInput);
        }

        private readonly InputBufferRecorder _inputBufferRecorder;

        private BufferedInput Convert(InputActionId id, InputContext<Vector2> context)
        {
            return new(
                id,
                context.Phase,
                context.Timestamp,
                context.Value,
                0f
                );
        }

        private BufferedInput Convert(InputActionId id, InputContext<float> context)
        {
            return new(
                id,
                context.Phase,
                context.Timestamp,
                Vector2.zero,
                context.Value
                );
        }
    }
}
