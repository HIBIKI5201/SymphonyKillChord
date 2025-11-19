using System;
using UnityEngine;

namespace Mock.MusicBattle
{
    public interface IInputBuffer
    {
        public event Action<Vector2> LookAction;
        public event Action<float> LockOnSelectAction;
    }
}
