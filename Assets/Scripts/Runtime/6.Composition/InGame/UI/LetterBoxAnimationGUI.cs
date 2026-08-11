using LitMotion;
using LitMotion.Extensions;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    public sealed class LetterBoxAnimationGUI : MonoBehaviour
    {
        public void ActiveAspectImmediate()
        {
            Vector2 canvas = _canvas.sizeDelta;
            Vector2 screenSize = AspectToSizeDelta(ASPECT, canvas);
            float letterSizeY = Mathf.Abs(canvas.y - screenSize.y) * 0.5f;

            _upperLetter.sizeDelta = new Vector2(_upperLetter.sizeDelta.x, letterSizeY);
            _lowerLetter.sizeDelta = new Vector2(_lowerLetter.sizeDelta.x, letterSizeY);
        }
        public void DeactiveAspectImmediate()
        {
            _upperLetter.sizeDelta = new Vector2(_upperLetter.sizeDelta.x, 0f);
            _lowerLetter.sizeDelta = new Vector2(_lowerLetter.sizeDelta.x, 0f);
        }
        public void ActiveAspect(float duration)
        {
            _handle.TryCancel();
            Vector2 canvas = _canvas.sizeDelta;
            Vector2 screenSize = AspectToSizeDelta(ASPECT, canvas);
            float letterSizeY = Mathf.Abs(canvas.y - screenSize.y) * 0.5f;
            _handle = LSequence.Create()
                .Join(LMotion.Create(0f, letterSizeY, duration)
                    .BindToSizeDeltaY(_upperLetter))
                .Join(LMotion.Create(0f, letterSizeY, duration)
                    .BindToSizeDeltaY(_lowerLetter))
                .Run();
        }
        public void DeactiveAspect(float duration)
        {
            _handle.TryCancel();
            Vector2 canvas = _canvas.sizeDelta;
            Vector2 screenSize = AspectToSizeDelta(ASPECT, canvas);
            float letterSizeY = Mathf.Abs(canvas.y - screenSize.y) * 0.5f;

            _handle = LSequence.Create()
                .Join(LMotion.Create(letterSizeY, 0f, duration)
                    .BindToSizeDeltaY(_upperLetter))
                .Join(LMotion.Create(letterSizeY, 0f, duration)
                    .BindToSizeDeltaY(_lowerLetter))
                .Run();
        }


        [SerializeField] private RectTransform _canvas;
        [SerializeField] private RectTransform _upperLetter;
        [SerializeField] private RectTransform _lowerLetter;
        private void Awake()
        {
            if (_canvas != null)
            {
                Debug.LogWarning($"[{nameof(LetterBoxAnimationGUI)}] Canvasが設定されていません", this);
            }
            if (_upperLetter != null)
            {
                Debug.LogWarning($"[{nameof(LetterBoxAnimationGUI)}] Upper Letterが設定されていません", this);
            }
            if (_lowerLetter != null)
            {
                Debug.LogWarning($"[{nameof(LetterBoxAnimationGUI)}] Lower Letterが設定されていません", this);
            }
        }
        private void OnDestroy()
        {
            _handle.TryCancel();
        }

        /// <summary>
        ///     アスペクト比を維持したまま、指定サイズ内に収まる最大のサイズを算出する。
        /// </summary>
        /// <param name="aspect"> 目標とする縦横比。</param>
        /// <param name="sizeDelta"> 収める領域のサイズ。</param>
        /// <returns> aspectと同じ比率を持ち、sizeDelta内に収まるサイズ。</returns> 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 AspectToSizeDelta(in Vector2 aspect, in Vector2 sizeDelta)
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

        private MotionHandle _handle;
        private static readonly Vector2 ASPECT = new Vector2(2.35f, 1f);
    }
}
