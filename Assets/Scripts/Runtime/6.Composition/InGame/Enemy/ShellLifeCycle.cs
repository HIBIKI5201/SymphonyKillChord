using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Application.InGame.Enemy;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Composition.InGame.Music;
using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.InfraStructure.InGame.Enemy;
using KillChord.Runtime.View.InGame.Enemy;
using KillChord.Runtime.View.InGame.Music;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Enemy
{
    /// <summary>
    ///     砲弾のライフサイクルを管理するクラス。
    /// </summary>
    public class ShellLifeCycle : MonoBehaviour, IShellLifeCycle
    {
        /// <summary>
        ///     砲弾の依存関係を構築する。
        /// </summary>
        /// <param name="shellView"></param>
        /// <param name="enemyBattleState"></param>
        /// <returns></returns>
        public void Initialize(Action<ShellLifeCycle> releaseCallback)
        {
            if (!_musicSyncInitializer) _musicSyncInitializer = FindFirstObjectByType<MusicSyncInitializer>();
            if (!_musicSyncView) _musicSyncView = FindAnyObjectByType<MusicSyncView>();

            if (_musicSyncView.MusicSyncState == null)
            {
                throw new ArgumentNullException("MusicSyncStateが見つかりません。");
            }
            if (!_playerInitializer) _playerInitializer = ServiceLocator.GetInstance<PlayerInitializer>();
            IMusicActionScheduler musicActionScheduler = new MusicSchedulerAdaptor(_musicSyncView.MusicSyncState, _musicSyncInitializer.MusicSyncService);
            ShellAttackSpec attackSpec = ShellFactory.CreateAttackSpec(_attackData);
            EnemyMusicSpec musicSpec = ShellFactory.CreateMusicSpec(_musicData);

            ShellEntity entity = new ShellEntity(attackSpec, musicSpec, null);

            ShellReservationUsecase reservationUsecase = new ShellReservationUsecase(entity, musicActionScheduler);
            ShellAttackUsecase attackUsecase = new ShellAttackUsecase();

            ShellSpecPresenter shellSpecPresenter = new ShellSpecPresenter(entity);
            ShellController controller = new ShellController(
                entity,
                _view,
                reservationUsecase,
                null,
                null,
                attackUsecase);
            _controller = controller;

            _view.Initialize(_playerInitializer.transform, shellSpecPresenter, Deactivate);
            _releaseCallback = releaseCallback;
        }

        /// <summary>
        ///     有効化処理。
        /// </summary>
        /// <param name="enemyBattleState"></param>
        public void Activate(EnemyBattleState enemyBattleState)
        {
            gameObject.SetActive(true);
            _controller.Activate(enemyBattleState);
            _view.Activate();
        }

        /// <summary>
        ///     無効化処理。
        /// </summary>
        public void Deactivate()
        {
            _controller.Deactivate();
            _view.Deactivate();
            gameObject.SetActive(false);
            _releaseCallback.Invoke(this);
        }

        [SerializeField] private ShellView _view;
        [SerializeField] private ShellAttackData _attackData;
        [SerializeField] private EnemyMusicData _musicData;

        private PlayerInitializer _playerInitializer;
        private MusicSyncInitializer _musicSyncInitializer;
        private MusicSyncView _musicSyncView;
        private Action<ShellLifeCycle> _releaseCallback;
        private ShellController _controller;
    }
}
