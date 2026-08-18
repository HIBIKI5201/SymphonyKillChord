using LitMotion;
using LitMotion.Extensions;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.UI
{
    public sealed class LetterBoxAnimationGUI : MonoBehaviour
    {
        /// <summary>
        ///     レターボックスを即座に表示状態にする。
        /// </summary>
        public void ActiveAspectImmediate()
        {
            if (!_isValid)
            {
                return;
            }

            Vector2 canvas = _canvas.sizeDelta;
            Vector2 screenSize = AspectToSizeDelta(ASPECT, canvas);
            float letterSizeY = Mathf.Abs(canvas.y - screenSize.y) * HALF;

            _upperLetter.sizeDelta = new Vector2(_canvas.sizeDelta.x, letterSizeY);
            _lowerLetter.sizeDelta = new Vector2(_canvas.sizeDelta.x, letterSizeY);
        }

        /// <summary>
        ///     レターボックスを即座に非表示状態にする。
        /// </summary>
        public void DeactiveAspectImmediate()
        {
            if (!_isValid)
            {
                return;
            }
            _upperLetter.sizeDelta = new Vector2(_canvas.sizeDelta.x, 0f);
            _lowerLetter.sizeDelta = new Vector2(_canvas.sizeDelta.x, 0f);
        }

        /// <summary>
        ///     レターボックスを表示状態へアニメーションさせる。
        /// </summary>
        /// <param name="duration"> アニメーションにかける秒数。</param>
        public void ActiveAspect(float duration)
        {
            if (!_isValid)
            {
                return;
            }
            // 再生中のアニメーションが残っていると値が競合するため、先に破棄する。
            _handle.TryCancel();

            Vector2 canvas = _canvas.sizeDelta;
            Vector2 screenSize = AspectToSizeDelta(ASPECT, canvas);
            float letterSizeY = Mathf.Abs(canvas.y - screenSize.y) * HALF;

            // 上下の帯を同時に動かすため、Joinで並列に繋ぐ。
            _handle = LSequence.Create()
                .Join(LMotion.Create(new Vector2(_canvas.sizeDelta.x, 0f), new Vector2(_canvas.sizeDelta.x, letterSizeY), duration)
                    .BindToSizeDelta(_upperLetter))
                .Join(LMotion.Create(new Vector2(_canvas.sizeDelta.x, 0f), new Vector2(_canvas.sizeDelta.x, letterSizeY), duration)
                    .BindToSizeDelta(_lowerLetter))
                .Run();
        }

        /// <summary>
        ///     レターボックスを非表示状態へアニメーションさせる。
        /// </summary>
        /// <param name="duration"> アニメーションにかける秒数。</param>
        public void DeactiveAspect(float duration)
        {
            if (!_isValid)
            {
                return;
            }
            // 再生中のアニメーションが残っていると値が競合するため、先に破棄する。
            _handle.TryCancel();

            Vector2 canvas = _canvas.sizeDelta;
            Vector2 screenSize = AspectToSizeDelta(ASPECT, canvas);
            float letterSizeY = Mathf.Abs(canvas.y - screenSize.y) * HALF;

            // 上下の帯を同時に動かすため、Joinで並列に繋ぐ。
            _handle = LSequence.Create()
                .Join(LMotion.Create(new Vector2(_canvas.sizeDelta.x, letterSizeY), new Vector2(_canvas.sizeDelta.x, 0f), duration)
                    .BindToSizeDelta(_upperLetter))
                .Join(LMotion.Create(new Vector2(_canvas.sizeDelta.x, letterSizeY), new Vector2(_canvas.sizeDelta.x, 0f), duration)
                    .BindToSizeDelta(_lowerLetter))
                .Run();
        }

        /// <summary>
        ///     アスペクト比を維持したまま、指定サイズ内に収まる最大のサイズを算出する。
        /// </summary>
        /// <param name="aspect"> 目標とする縦横比。</param>
        /// <param name="sizeDelta"> 収める領域のサイズ。</param>
        /// <returns> aspectと同じ比率を持ち、sizeDelta内に収まるサイズ。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2 AspectToSizeDelta(in Vector2 aspect, in Vector2 sizeDelta)
        {
            // 比率が0以下だと除算できないため、そのまま領域サイズを返す。
            if (aspect.x <= 0f || aspect.y <= 0f)
            {
                return sizeDelta;
            }

            // 縦横それぞれの拡大率のうち小さい方を採用すれば、必ず領域内に収まる。
            float scale = Mathf.Min(sizeDelta.x / aspect.x, sizeDelta.y / aspect.y);

            return aspect * scale;
        }

        /// <summary> 余白を上下の帯へ均等に分配するための係数。 </summary>
        private const float HALF = 0.5f;

        /// <summary> 目標とするレターボックスの縦横比。 </summary>
        private static readonly Vector2 ASPECT = new Vector2(4.7f, 1f);

        [Tooltip("レターボックスの基準となるCanvasのRectTransform")]
        [SerializeField] private RectTransform _canvas;

        [Tooltip("画面上側に表示する帯のRectTransform")]
        [SerializeField] private RectTransform _upperLetter;

        [Tooltip("画面下側に表示する帯のRectTransform")]
        [SerializeField] private RectTransform _lowerLetter;

        private MotionHandle _handle;
        private bool _isValid = false;

        /// <summary>
        ///     インスペクターの設定漏れを検出する。
        /// </summary>
        private void Awake()
        {
            if (_canvas == null)
            {
                Debug.LogError($"[{nameof(LetterBoxAnimationGUI)}] Canvasが設定されていません", this);
                return;
            }
            if (_upperLetter == null)
            {
                Debug.LogError($"[{nameof(LetterBoxAnimationGUI)}] Upper Letterが設定されていません", this);
                return;
            }
            if (_lowerLetter == null)
            {
                Debug.LogError($"[{nameof(LetterBoxAnimationGUI)}] Lower Letterが設定されていません", this);
                return;
            }
            _isValid = true;
        }

        /// <summary>
        ///     破棄時に再生中のアニメーションを停止する。
        /// </summary>
        private void OnDestroy()
        {
            _handle.TryCancel();
        }
    }
}
