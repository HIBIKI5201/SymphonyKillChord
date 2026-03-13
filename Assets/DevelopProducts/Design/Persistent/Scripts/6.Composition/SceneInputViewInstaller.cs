using DevelopProducts.Persistent.Adaptor;
using DevelopProducts.Persistent.View;
using UnityEngine;

namespace DevelopProducts.Persistent.Composition
{
    /// <summary>
    ///     シーンに配置して、SceneInputViewをインストールするクラス。
    ///     モバイル向けの入力ボタンのViewを初期化する。
    /// </summary>
    public class SceneInputViewInstaller : MonoBehaviour
    {
        [SerializeField] private MobileInputButtonView[] _mobileInputButtonViews;
        [SerializeField] private MobileMoveButtonView[] _mobileMoveButtonViews;

        private void Start()
        {
            PersistentInputInstaller persistentInstaller = FindFirstObjectByType<PersistentInputInstaller>();

            if (persistentInstaller == null)
            {
                Debug.LogError("PersistentInputInstaller が見つかりません。");
                return;
            }

            ButtonInputAdaptor buttonInputAdaptor = new ButtonInputAdaptor(
                persistentInstaller.BufferButtonInputUsecase,
                persistentInstaller.TimestampProvider);

            MoveInputAdaptor moveInputAdaptor = new MoveInputAdaptor(
                persistentInstaller.BufferMoveInputUsecase,
                persistentInstaller.TimestampProvider);

            if (_mobileInputButtonViews != null)
            {
                foreach (var buttonView in _mobileInputButtonViews)
                {
                    if (buttonView == null) continue;
                    buttonView.Initialize(buttonInputAdaptor);
                }
            }

            if (_mobileMoveButtonViews != null)
            {
                foreach (var moveView in _mobileMoveButtonViews)
                {
                    if (moveView == null) continue;
                    moveView.Initialize(moveInputAdaptor);
                }
            }
        }
    }
}