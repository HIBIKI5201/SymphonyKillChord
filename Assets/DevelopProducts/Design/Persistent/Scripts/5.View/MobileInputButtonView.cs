using DevelopProducts.Persistent.Application;
using DevelopProducts.Persistent.Domain.Input;
using UnityEngine;

namespace DevelopProducts.Persistent.View
{
    /// <summary>
    ///     UI Buttonにアタッチして使用する、モバイル向けの入力ボタンのView。
    ///     ボタンが押されたときに、BufferButtonInputUsecaseを呼び出して入力をバッファに記録する。
    /// </summary>
    public class MobileInputButtonView : MonoBehaviour
    {
        public void Initialize(
            BufferButtonInputUsecase bufferButtonInputUsecase,
            InputTimestampProvider inputTimestampProvider)
        {
            _bufferButtonInputUsecase = bufferButtonInputUsecase;
            _inputTimestampProvider = inputTimestampProvider;
        }

        /// <summary>
        ///     UI ButtonのOnClickから呼び出す。
        /// </summary>
        public void OnButtonDown()
        {
            InputActionId actionId = new InputActionId(_actionIdValue);

            _bufferButtonInputUsecase.Execute(
                actionId,
                InputPheseIds.Performed,
                _inputTimestampProvider.GetTimestamp()
                );
        }

        [SerializeField] private int _actionIdValue;

        private BufferButtonInputUsecase _bufferButtonInputUsecase;
        private InputTimestampProvider _inputTimestampProvider;
    }
}
