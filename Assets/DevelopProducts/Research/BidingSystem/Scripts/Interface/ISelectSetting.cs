using System;
using UnityEngine;

namespace DevelopProducts.BindingSystem
{
    public interface ISelectSetting : ISettingItem
    {
        Array items { get; }
        void MoveNext();
        void MoveBack();
    }
}
