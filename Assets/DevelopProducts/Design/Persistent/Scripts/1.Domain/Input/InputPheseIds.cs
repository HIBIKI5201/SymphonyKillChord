using UnityEngine;

namespace DevelopProducts.Persistent.Domain.Input
{
    /// <summary>
    ///     入力フェーズの識別子を定義するクラス。
    /// </summary>
    public static class InputPheseIds
    {
        public static readonly InputPheseId Started = new InputPheseId(1);
        public static readonly InputPheseId Performed = new InputPheseId(2);
        public static readonly InputPheseId Canceled = new InputPheseId(3);

        public static string GetPheseName(InputPheseId pheseId)
        {
            if (pheseId == Started) return nameof(Started);
            if (pheseId == Performed) return nameof(Performed);
            if (pheseId == Canceled) return nameof(Canceled);

            return $"Unknown({pheseId.Value})";
        }
    }
}
