using UnityEngine;

namespace DevelopProducts.SkillTree
{
    public readonly struct UnlockPoint
    {
        public UnlockPoint(int point)
        {
            if (point < 0)
            {
                throw new System.ArgumentException($"アンロックポイントは正の数である必要があります");
            }
            Point = point;
        }
        public int Point { get; }
    }
}
