using System;
using UnityEngine;

namespace Mock.MusicBattle
{
    public interface IInputBuffer
    {
        public Action<Vector2> LookAction { get; }
        public Action<float> LockOnSelectAction { get; }
    }
}
