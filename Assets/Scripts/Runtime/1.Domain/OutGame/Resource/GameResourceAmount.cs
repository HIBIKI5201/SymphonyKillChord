using System;

namespace KillChord.Runtime.Domain.OutGame.Resource
{
    /// <summary>
    ///     ゲーム内リソースの種類と数量の組を表す値オブジェクトです。
    /// </summary>
    public readonly struct GameResourceAmount
    {
        /// <summary>
        ///     GameResourceAmount の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="resourceId"> リソースのIDです。 </param>
        /// <param name="amount"> 数量です。0以上である必要があります。 </param>
        public GameResourceAmount(GameResourceId resourceId, int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "リソースの数量は 0 以上である必要があります。");
            }

            _resourceId = resourceId;
            _amount = amount;
        }

        /// <summary> リソースのIDです。 </summary>
        public GameResourceId ResourceId => _resourceId;

        /// <summary> 数量です。 </summary>
        public int Amount => _amount;

        private readonly GameResourceId _resourceId;
        private readonly int _amount;
    }
}
