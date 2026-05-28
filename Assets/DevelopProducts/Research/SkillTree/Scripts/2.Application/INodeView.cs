using UnityEngine;

namespace DevelopProducts.SkillTree
{
    public interface INodeView
    {
        void Canlock(bool canLock);
        void Unlock();
    }
}
