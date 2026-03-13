using DevelopProducts.Persistent.Adaptor;
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
            ButtonInputAdaptor inputAdaptor)
        {
            _buttonInputAdaptor = inputAdaptor;
        }

        /// <summary>
        ///     UI ButtonのOnClickから呼び出す。
        /// </summary>
        public void OnButtonDown()
        {
            InputActionId actionId = new InputActionId(_actionIdValue);

            _buttonInputAdaptor.HandleButton(actionId, InputPheseIds.Performed);
        }

        [SerializeField] private int _actionIdValue;

        private ButtonInputAdaptor _buttonInputAdaptor;
    }
}
