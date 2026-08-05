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
            if (_damageText == null)
            {
                Debug.LogError($"[{nameof(DamageNumberView)}] TMP_Text が未設定です。", this);
                return;
            }

            _damageText.text = Mathf.RoundToInt(damage).ToString();

            ApplyRandomOffset();

            LMotion.Create(1f, 0f, _duration)
                .WithEase(Ease.OutQuad)
                .WithOnComplete(() =>
                {
                    Destroy(gameObject);
                })
                .BindToColorA(_damageText);
        }

        private const float DEFAULT_OFFSET_RANGE_X = 8f;
        private const float DEFAULT_OFFSET_RANGE_Y = 5f;

        [SerializeField, Tooltip("ダメージ数値のテキスト")]
        private TMP_Text _damageText;

        [SerializeField, Tooltip("ダメージ演出の継続時間")]
        private float _duration;

        [SerializeField, Tooltip("生成位置のランダムオフセットの範囲。自身のローカル座標での±値。0にするとずらさない")]
        private Vector2 _randomOffsetRange = new Vector2(DEFAULT_OFFSET_RANGE_X, DEFAULT_OFFSET_RANGE_Y);

        /// <summary>
        ///     生成位置をランダムにずらす。
        ///     多段ヒットや複数対象で同時に表示された数値が重なって読めなくなるのを防ぐ。
        /// </summary>
        private void ApplyRandomOffset()
        {
            if (_randomOffsetRange == Vector2.zero)
            {
                return;
            }

            transform.localPosition += new Vector3(
                Random.Range(-_randomOffsetRange.x, _randomOffsetRange.x),
                Random.Range(-_randomOffsetRange.y, _randomOffsetRange.y),
                0f);
        }
    }
}
