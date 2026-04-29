using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Application.InGame.Enemy;
using KillChord.Runtime.Composition.InGame.Music;
using KillChord.Runtime.View.InGame.Enemy;
using KillChord.Runtime.View.InGame.Music;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Enemy
{
    /// <summary>
    ///     TODO 敵砲弾のコントローラーを初期化する。
    ///     将来リファクタリングする予想。毎回初期化は負荷が重いので、キャッシュや再利用などの手段にあわせて実装したい。
    /// </summary>
    public class ShellInitializer : MonoBehaviour , IShellInitializer
    {
        private void Awake()
        {
            ServiceLocator.RegisterInstance<IShellInitializer>(this);
        }
        public ShellController InitAndGetShellController(ShellView shellView, EnemyAIController enemyAIController)
        {
            if(!_musicSyncInitializer) _musicSyncInitializer = FindFirstObjectByType<MusicSyncInitializer>();
            if(!_musicSyncView) _musicSyncView = FindAnyObjectByType<MusicSyncView>();

            if (_musicSyncView.MusicSyncViewModel == null)
            {
                Debug.LogError("MusicSyncViewが見つかりません。", this);
                return null;
            }
            IMusicActionScheduler musicActionScheduler = new MusicSchedulerAdaptor(_musicSyncView.MusicSyncViewModel, _musicSyncInitializer.MusicSyncService);
            ShellReservationUsecase reservationUsecase = new ShellReservationUsecase(musicActionScheduler);
            ShellController controller = new ShellController(shellView, reservationUsecase, enemyAIController);
            return controller;
        }

        private MusicSyncInitializer _musicSyncInitializer;
        private MusicSyncView _musicSyncView;
    }
}
