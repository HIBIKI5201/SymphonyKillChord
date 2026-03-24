using DevelopProducts.Persistent.Application;
using DevelopProducts.Persistent.Domain.Input;
using UnityEngine;

namespace DevelopProducts.Persistent.Adaptor
{
    /// <summary>
    ///     ボタンからの入力をApplicationに橋渡しするクラス。
    /// </summary>
    public class ButtonInputAdaptor
    {
        public ButtonInputAdaptor(
            BufferButtonInputUsecase bufferButtonInputUsecase,
            InputTimestampProvider provider
            )
        {
            _bufferButtonInputUsecase = bufferButtonInputUsecase;
            _timestampProvider = provider;
        }

        public void HandleButton(InputActionId inputActionId, InputPheseId pheseId)
        {
            float timestamp = _timestampProvider.GetTimestamp();
            _bufferButtonInputUsecase.Execute(inputActionId, pheseId, timestamp);
        }

        private readonly BufferButtonInputUsecase _bufferButtonInputUsecase;
        private readonly InputTimestampProvider _timestampProvider; 
    }
}
