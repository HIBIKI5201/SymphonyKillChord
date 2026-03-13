using UnityEngine;
using DevelopProducts.Persistent.Application;
using DevelopProducts.Persistent.Domain.Input;

namespace DevelopProducts.Persistent.Adaptor
{
    /// <summary>
    ///     移動入力をApplication層に橋渡しするクラス。
    /// </summary>
    public class MoveInputAdaptor
    {
        public MoveInputAdaptor(
            BufferMoveInputUsecase bufferMoveInputUsecase,
            InputTimestampProvider provider
        )
        {
            _bufferMoveInputUsecase = bufferMoveInputUsecase;
            _timestampProvider = provider;
        }

        public void HandleMove(Vector2 direction, InputPheseId pheseId)
        {
            float timestamp = _timestampProvider.GetTimestamp();
            _bufferMoveInputUsecase.Execute(direction.x, direction.y, pheseId, timestamp);
        }

        public void Release()
        {
            float timestamp = _timestampProvider.GetTimestamp();
            _bufferMoveInputUsecase.Execute(0f, 0f, InputPheseIds.Canceled, timestamp);

        }

        private readonly BufferMoveInputUsecase _bufferMoveInputUsecase;
        private readonly InputTimestampProvider _timestampProvider;
    }
}
