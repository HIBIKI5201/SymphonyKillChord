using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     ダメージ数値を表示するクラス。
    /// </summary>
    public class DamageNumberView : MonoBehaviour
    {
        /// <summary>
        ///     ダメージ演出を再生する。
        /// </summary>
        /// <param name="damage"></param>
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


        [SerializeField, Tooltip("ダメージ数値のテキスト")]
        private TMP_Text _damageText;

        [SerializeField, Tooltip("ダメージ演出の継続時間")]
        private float _duration;
    }
}
