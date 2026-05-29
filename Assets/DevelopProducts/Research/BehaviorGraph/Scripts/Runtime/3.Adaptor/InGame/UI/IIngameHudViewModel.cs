using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.Adaptor
{
    public interface IIngameHudViewModel
    {
        public void UpdateHealth(in IngameHudDTO dto);
    }
}