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
            if(_mobileInputButtonViews != null)
            {
                for (int i = 0; i < _mobileInputButtonViews.Length; i++)
                {
                    if (_mobileInputButtonViews[i] == null)
                    {
                        continue;
                    }

                    _mobileInputButtonViews[i].Initialize(
                        persistentInstaller.BufferButtonInputUsecase,
                        persistentInstaller.TimestampProvider);
                }
            }

            if(_mobileMoveButtonViews != null)
            {
                for (int i = 0; i < _mobileMoveButtonViews.Length; i++)
                {
                    if (_mobileMoveButtonViews[i] == null)
                    {
                        continue;
                    }

                    _mobileMoveButtonViews[i].Initialize(
                        persistentInstaller.BufferMoveInputUsecase,
                        persistentInstaller.TimestampProvider);
                }
            }
        }
    }
}