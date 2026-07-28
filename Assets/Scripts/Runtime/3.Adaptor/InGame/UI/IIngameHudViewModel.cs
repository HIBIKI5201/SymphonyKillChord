using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.UI
{
    public interface IIngameHudViewModel
    {
        public void UpdateHealth(in IngameHudDTO dto);
    }
}