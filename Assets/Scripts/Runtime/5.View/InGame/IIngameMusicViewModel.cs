using KillChord.Runtime.Adaptor;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public interface IIngameMusicViewModel
    {
        public void UpdateBgm(in IngameMusicDTO dto);
    }
}