using KillChord.Runtime.Adaptor.InGame.Mission;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Mission
{
    /// <summary>
    ///     目標ステップの説明ポップアップを表示するViewクラス。
    ///     ステップごとのポップアップPrefabを生成して表示します。
    ///     入力を受け付けないパッシブな表示のため、スマートフォンのタッチ操作UIを妨げません。
    /// </summary>
    public class MissionStepPopupView : MonoBehaviour, IMissionStepPopupView
    {
        /// <inheritdoc />
        public void Show(GameObject popupPrefab, MissionStepPopupCloseInput closeInput)
        {
            DestroyPopupInstance();
            if (popupPrefab == null || _popupRoot == null)
            {
                SetVisible(false);
                return;
            }

            _popupInstance = Instantiate(popupPrefab, _popupRoot, false);
            if (_popupInstance.TryGetComponent(out TutorialPopupView popupView))
            {
                popupView.SetCloseInput(closeInput);
            }
            else
            {
                Debug.LogWarning(
                    $"[{nameof(MissionStepPopupView)}] {popupPrefab.name} に {nameof(TutorialPopupView)} が無いため、閉じる案内を切り替えられません。",
                    this);
            }

            SetVisible(true);
        }

        /// <inheritdoc />
        public void Hide()
        {
            DestroyPopupInstance();
            SetVisible(false);
        }

        [SerializeField, Tooltip("ポップアップ全体のCanvasGroup。")]
        private CanvasGroup _canvasGroup;

        [SerializeField, Tooltip("ポップアップPrefabを生成する親。")]
        private RectTransform _popupRoot;

        private GameObject _popupInstance;

        /// <summary>
        ///     入力を受け付けない表示専用の設定にする。
        /// </summary>
        private void Awake()
        {
            // 入力を受け付けないパッシブな表示のため、常にfalseに固定する。
            // (表示/非表示はalphaのみで切り替える)
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            Hide();
        }

        /// <summary>
        ///     ポップアップ全体の表示を切り替える。
        /// </summary>
        /// <param name="isVisible"> 表示する場合はtrueです。 </param>
        private void SetVisible(bool isVisible)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = isVisible ? 1f : 0f;
            }
        }

        /// <summary>
        ///     表示中のポップアップを破棄する。
        /// </summary>
        private void DestroyPopupInstance()
        {
            if (_popupInstance == null)
            {
                return;
            }

            Destroy(_popupInstance);
            _popupInstance = null;
        }
    }
}
