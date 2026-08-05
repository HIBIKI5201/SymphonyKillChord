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

        private const float DEFAULT_HORIZONTAL_OFFSET_RANGE = 0.3f;
        private const float DEFAULT_UPWARD_OFFSET_RANGE = 0.5f;

        [SerializeField, Tooltip("ダメージ数値のテキスト")]
        private TMP_Text _damageText;

        [SerializeField, Tooltip("ダメージ演出の継続時間")]
        private float _duration;

        [SerializeField, Tooltip("生成位置の横方向のランダム幅（メートル）。±この値の範囲でずらす。0でずらさない")]
        private float _randomHorizontalOffsetRange = DEFAULT_HORIZONTAL_OFFSET_RANGE;

        [SerializeField, Tooltip("生成位置の上方向のランダム幅（メートル）。0からこの値の範囲で上にだけずらす。下へは動かさない")]
        private float _randomUpwardOffsetRange = DEFAULT_UPWARD_OFFSET_RANGE;

        /// <summary>
        ///     生成位置をランダムにずらす。
        ///     多段ヒットや複数対象で同時に表示された数値が重なって読めなくなるのを防ぐ。
        ///     生成位置が敵の足元にあるため、縦方向は上にのみずらして地面へ潜らせない。
        /// </summary>
        private void ApplyRandomOffset()
        {
            float horizontal = _randomHorizontalOffsetRange <= 0f
                ? 0f
                : Random.Range(-_randomHorizontalOffsetRange, _randomHorizontalOffsetRange);

            float upward = _randomUpwardOffsetRange <= 0f
                ? 0f
                : Random.Range(0f, _randomUpwardOffsetRange);

            transform.localPosition += new Vector3(horizontal, upward, 0f);
        }
    }
}
