using KillChord.Runtime.Application.Persistent.Input;
using KillChord.Runtime.Domain.Persistent.Input;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.Persistent.Input
{
    /// <summary>
    ///     入力を履歴保存用に変換してInputBufferRecorderに渡すクラス。
    /// </summary>
    public class RecordController
    {
        /// <summary>
        ///     入力を記録するレコーダーを指定して生成する。
        /// </summary>
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
        ///     カメラ入力を変換してInputBufferRecorderに渡す。
        /// </summary>
        /// <param name="inputContext"></param>
        public void HandleLook(InputContext<Vector2> inputContext)
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

        /// <summary>
        ///     Vector2 型の入力をバッファ記録用の形式に変換する。
        /// </summary>
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

        /// <summary>
        ///     float 型の入力をバッファ記録用の形式に変換する。
        /// </summary>
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
