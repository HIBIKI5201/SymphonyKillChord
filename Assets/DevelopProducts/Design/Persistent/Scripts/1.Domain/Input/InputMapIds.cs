using UnityEngine;

namespace DevelopProducts.Persistent.Domain.Input
{
    /// <summary>
    ///     入力マップのIDを定義するクラス。
    ///     入力マップは、ゲーム内のアクションと入力を紐づけるためのもの。
    /// </summary>
    public readonly struct InputMapIds
    {
        public static readonly InputMapId Common = new(1);
        public static readonly InputMapId InGame = new(2);
        public static readonly InputMapId OutGame = new(3);

        public static string GetMapName(InputMapId mapId)
        {
            if (mapId == Common) return nameof(Common);
            if (mapId == InGame) return nameof(InGame);
            if (mapId == OutGame) return nameof(OutGame);

            return $"Unknown({mapId.Value})";
        }
    }
}
