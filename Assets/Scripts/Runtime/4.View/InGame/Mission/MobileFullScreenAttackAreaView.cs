using System;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace KillChord.Runtime.View.InGame.Mission
{
    /// <summary>
    ///     スマートフォンで説明ポップアップ表示中に、画面全体を攻撃ボタンの判定領域にするViewクラス。
    ///     ポップアップ表示中は他のUIにタップ判定を吸われて攻撃ボタンを押せなくなるため、
    ///     最前面に透明な攻撃判定を重ねて進行不能を防ぎます。
    /// </summary>
    public sealed class MobileFullScreenAttackAreaView : IDisposable
    {
        /// <summary>
        ///     全画面の攻撃判定領域を生成します。生成直後は非表示です。
        /// </summary>
        /// <param name="parent"> 判定領域を配置する親Transform。スマートフォン用Canvasのルートを想定します。 </param>
        public MobileFullScreenAttackAreaView(Transform parent)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            _areaObject = new GameObject(AREA_OBJECT_NAME, typeof(RectTransform));

            // OnScreenButtonはOnEnable時に入力デバイスへ登録されるため、controlPathの設定前に有効化しない。
            _areaObject.SetActive(false);
            _areaObject.layer = parent.gameObject.layer;

            RectTransform rectTransform = (RectTransform)_areaObject.transform;
            rectTransform.SetParent(parent, false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.SetAsLastSibling();

            // 親Canvas内の他UIより手前でレイキャストを受けるため、ネストしたCanvasで描画順を上書きする。
            // 描画順の上書き自体は、ネストしたCanvasとして有効化される Show で適用する。
            _canvas = _areaObject.AddComponent<Canvas>();
            _areaObject.AddComponent<GraphicRaycaster>();

            Image image = _areaObject.AddComponent<Image>();
            image.color = Color.clear;
            image.raycastTarget = true;

            OnScreenButton onScreenButton = _areaObject.AddComponent<OnScreenButton>();
            onScreenButton.controlPath = ATTACK_CONTROL_PATH;
        }

        /// <summary>
        ///     攻撃判定領域を表示します。
        /// </summary>
        public void Show()
        {
            if (_areaObject == null)
            {
                return;
            }

            _areaObject.SetActive(true);

            // 非アクティブ時はルートCanvas扱いとなりoverrideSortingの設定が保持されないため、有効化後に設定する。
            _canvas.overrideSorting = true;
            _canvas.sortingOrder = SORTING_ORDER;
        }

        /// <summary>
        ///     攻撃判定領域を非表示にします。
        /// </summary>
        public void Hide()
        {
            if (_areaObject == null)
            {
                return;
            }

            _areaObject.SetActive(false);
        }

        /// <summary>
        ///     生成した判定領域を破棄します。
        /// </summary>
        public void Dispose()
        {
            if (_areaObject == null)
            {
                return;
            }

            Object.Destroy(_areaObject);
            _areaObject = null;
        }

        private const string AREA_OBJECT_NAME = "MobileFullScreenAttackArea";
        private const string ATTACK_CONTROL_PATH = "<Gamepad>/buttonSouth";
        private const int SORTING_ORDER = 32000;

        private GameObject _areaObject;
        private Canvas _canvas;
    }
}
