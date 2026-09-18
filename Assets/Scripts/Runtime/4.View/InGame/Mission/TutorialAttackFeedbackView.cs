using KillChord.Runtime.Adaptor.InGame.Mission;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View.InGame.Mission
{
    /// <summary>
    ///     色指定攻撃の結果を、事前生成した表示枠でゲージ上へ表示します。
    /// </summary>
    public sealed class TutorialAttackFeedbackView : MonoBehaviour, ITutorialAttackFeedbackView
    {
        /// <inheritdoc />
        public void ShowFeedback(bool isSuccess)
        {
            if (this == null || !isActiveAndEnabled || _items == null)
            {
                return;
            }

            // 全枠が使用中でも新しい攻撃を捨てず、最も古い枠から再利用する。
            _items[_nextItemIndex].Show(isSuccess);
            _nextItemIndex = (_nextItemIndex + 1) % _items.Length;
        }

        /// <summary>
        ///     表示中の結果をすべて消し、次の表示を初期状態から開始します。
        /// </summary>
        public void ClearFeedback()
        {
            if (_items == null)
            {
                return;
            }

            foreach (PopupItem item in _items)
            {
                item?.Hide();
            }
            _nextItemIndex = 0;
        }

        private const int DEFAULT_POOL_SIZE = 16;
        private const int MAX_POOL_SIZE = 64;
        private const float FADE_IN_SECONDS = 0.3f;
        private const float FADE_OUT_SECONDS = 0.5f;
        private static readonly Color SUCCESS_GLOW_COLOR = new Color(0.2f, 1f, 0.3f, 1f);
        private static readonly Color MISS_GLOW_COLOR = new Color(1f, 0.2f, 0.2f, 1f);

        [SerializeField, Tooltip("CanvasGroup、結果テキスト、背面の光Imageを持つ表示テンプレートです。")]
        private RectTransform _popupTemplate;

        [SerializeField, Range(1, MAX_POOL_SIZE), Tooltip("攻撃前に生成して再利用する表示枠数です。")]
        private int _poolSize = DEFAULT_POOL_SIZE;

        private PopupItem[] _items;
        private int _nextItemIndex;

        /// <summary>
        ///     連続攻撃で生成負荷が発生しないよう、表示枠を最初にまとめて生成します。
        /// </summary>
        private void Awake()
        {
            if (_popupTemplate == null
                || _popupTemplate.GetComponent<CanvasGroup>() == null
                || _popupTemplate.GetComponentInChildren<TMP_Text>(true) == null
                || _popupTemplate.GetComponentInChildren<Image>(true) == null)
            {
                Debug.LogError($"[{nameof(TutorialAttackFeedbackView)}] ポップアップのテンプレート参照が不足しています。", this);
                return;
            }

            _popupTemplate.gameObject.SetActive(false);
            _items = new PopupItem[Mathf.Clamp(_poolSize, 1, MAX_POOL_SIZE)];
            for (int i = 0; i < _items.Length; i++)
            {
                RectTransform instance = Instantiate(_popupTemplate, _popupTemplate.parent, false);
                _items[i] = new PopupItem(instance);
            }
        }

        /// <summary>
        ///     ゲーム時間で演出を進め、ポーズ中は表示位置と透明度を保持します。
        /// </summary>
        private void Update()
        {
            if (_items == null || Time.deltaTime <= 0f)
            {
                return;
            }

            foreach (PopupItem item in _items)
            {
                item.Advance(Time.deltaTime);
            }
        }

        /// <summary>
        ///     非表示やシーン切り替えで演出を残しません。
        /// </summary>
        private void OnDisable()
        {
            ClearFeedback();
        }

        /// <summary>
        ///     このViewが生成した表示枠を解放します。
        /// </summary>
        private void OnDestroy()
        {
            if (_items == null)
            {
                return;
            }

            foreach (PopupItem item in _items)
            {
                item?.Destroy();
            }
            _items = null;
        }

        /// <summary>
        ///     一回の攻撃結果の表示要素と進行時間を保持します。
        /// </summary>
        private sealed class PopupItem
        {
            /// <summary>
            ///     生成済みの表示枠から必要な要素を取得します。
            /// </summary>
            /// <param name="root"> この表示枠のルートです。 </param>
            public PopupItem(RectTransform root)
            {
                _root = root;
                _origin = root.anchoredPosition;
                _group = root.GetComponent<CanvasGroup>();
                _text = root.GetComponentInChildren<TMP_Text>(true);
                _glow = root.GetComponentInChildren<Image>(true);
                _group.blocksRaycasts = false;
                _group.interactable = false;
                _text.raycastTarget = false;
                _glow.raycastTarget = false;
                Hide();
            }

            /// <summary>
            ///     成功・失敗に応じて表示を初期化します。
            /// </summary>
            /// <param name="isSuccess"> 指定された色の攻撃ならtrueです。 </param>
            public void Show(bool isSuccess)
            {
                _elapsed = 0f;
                _riseHeight = Mathf.Max(0f, _root.rect.height);
                _root.anchoredPosition = _origin;
                _group.alpha = 0f;
                _text.text = isSuccess ? "Success" : "Miss";
                _text.color = Color.white;
                _glow.color = isSuccess ? SUCCESS_GLOW_COLOR : MISS_GLOW_COLOR;
                _root.gameObject.SetActive(true);
                _isPlaying = true;
            }

            /// <summary>
            ///     フェードイン後、表示枠一個分の高さへ上昇しながら消えます。
            /// </summary>
            /// <param name="deltaTime"> このフレームのゲーム時間です。 </param>
            public void Advance(float deltaTime)
            {
                if (!_isPlaying)
                {
                    return;
                }

                _elapsed += deltaTime;
                if (_elapsed < FADE_IN_SECONDS)
                {
                    _group.alpha = Mathf.Clamp01(_elapsed / FADE_IN_SECONDS);
                    return;
                }

                float fadeOutProgress = (_elapsed - FADE_IN_SECONDS) / FADE_OUT_SECONDS;
                if (fadeOutProgress >= 1f)
                {
                    Hide();
                    return;
                }

                _group.alpha = 1f - fadeOutProgress;
                _root.anchoredPosition = _origin + Vector2.up * (_riseHeight * fadeOutProgress);
            }

            /// <summary>
            ///     表示枠を非表示にし、再利用できる状態へ戻します。
            /// </summary>
            public void Hide()
            {
                _isPlaying = false;
                _elapsed = 0f;
                if (_root == null)
                {
                    return;
                }
                _group.alpha = 0f;
                _root.anchoredPosition = _origin;
                _root.gameObject.SetActive(false);
            }

            /// <summary>
            ///     所有している表示オブジェクトを破棄します。
            /// </summary>
            public void Destroy()
            {
                if (_root != null)
                {
                    Object.Destroy(_root.gameObject);
                }
            }

            private readonly RectTransform _root;
            private readonly CanvasGroup _group;
            private readonly TMP_Text _text;
            private readonly Image _glow;
            private readonly Vector2 _origin;
            private float _elapsed;
            private float _riseHeight;
            private bool _isPlaying;
        }
    }
}
