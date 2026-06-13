using CriWare;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopProducts.BindingSystem
{
    public class MasterVolume : ISliderSetting
    {
        public Slider Slider => _slider;

        public void Apply()
        {
            _atom.volume = Slider.value;
        }

        public void Load()
        {
            throw new System.NotImplementedException();
        }

        public void ResetToDefault()
        {
            throw new System.NotImplementedException();
        }
        [Header("スライダーの設定")]
        [SerializeField] private Slider _slider;
        [Header("CRI")]
        [SerializeField] private CriAtomSource _atom;
        [Header("テキストの設定")]
        [SerializeField] private string _displayName;
        [SerializeField] private string _description;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
    }
}
