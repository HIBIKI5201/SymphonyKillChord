using System;
using UnityEngine;

namespace Mock.MusicBattle.Develop
{
    public interface IInputBuffer
    {
        public event Action<Vector2> LookAction;
        public event Action<float> LockOnSelectAction;
    }
}
