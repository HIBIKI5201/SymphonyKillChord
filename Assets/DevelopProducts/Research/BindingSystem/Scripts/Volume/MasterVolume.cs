using CriWare;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopProducts.BindingSystem
{
    public class MasterVolume : SliderSettingItemBase
    {
        protected override void ApplyValue(float value)
        {
            //TODO: ここで、実際のマスターボリュームに値を適用する処理を実装する。
        }
        [Header("CRI")]
        [SerializeField] private CriAtomSource _atom;
    }
}
