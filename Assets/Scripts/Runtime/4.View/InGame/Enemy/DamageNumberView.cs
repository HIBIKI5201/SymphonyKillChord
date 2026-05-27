using KillChord.Runtime.Adaptor.InGame.Enemy;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public class DamageNumberView : MonoBehaviour
    {
        public void Play(float damage)
        {
            _damageText.text = Mathf.CeilToInt(damage).ToString();

            LMotion.Create(1f, 0f, _duration)
                .WithEase(Ease.OutQuad)
                .WithOnComplete(() =>
                {
                    Destroy(gameObject);
                })
                .BindToColorA(_damageText);
        }


        [SerializeField]
        private TMP_Text _damageText;

        [SerializeField] private float _duration;
    }
}
