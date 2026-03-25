using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    public interface IIngameHudViewModel
    {
        public void UpdateHealth(in IngameHudDTO dto);
    }
}