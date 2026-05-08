using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Application.InGame.Enemy;
using KillChord.Runtime.Composition.InGame.Music;
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
    ///     砲兵の砲弾の依存関係を構築する。
    /// </summary>
    public class ShellInitializer : MonoBehaviour, IShellInitializer
    {
        private void Awake()
        {
            ServiceLocator.RegisterInstance<IShellInitializer>(this);
        }
        /// <summary>
        ///     砲兵の砲弾の依存関係を構築する。
        /// </summary>
        /// <param name="shellView"></param>
        /// <param name="enemyBattleState"></param>
        /// <returns></returns>
        public void Initialize(ShellView shellView, EnemyBattleState enemyBattleState, EnemyMoveView enemyMoveView)
        {
            if (!_musicSyncInitializer) _musicSyncInitializer = FindFirstObjectByType<MusicSyncInitializer>();
            if (!_musicSyncView) _musicSyncView = FindAnyObjectByType<MusicSyncView>();

            if (_musicSyncView.MusicSyncState == null)
            {
                throw new ArgumentNullException("MusicSyncStateが見つかりません。");
            }
            IMusicActionScheduler musicActionScheduler = new MusicSchedulerAdaptor(_musicSyncView.MusicSyncState, _musicSyncInitializer.MusicSyncService);
            ShellAttackSpec attackSpec = ShellFactory.CreateAttackSpec(_attackData);
            EnemyMusicSpec musicSpec = ShellFactory.CreateMusicSpec(_musicData);

            ShellEntity entity = new ShellEntity(attackSpec, musicSpec, enemyBattleState.CurrentAttack);

            ShellReservationUsecase reservationUsecase = new ShellReservationUsecase(entity, musicActionScheduler);
            ShellAttackUsecase attackUsecase = new ShellAttackUsecase();

            ShellSpecPresenter shellSpecPresenter = new ShellSpecPresenter(entity);
            ShellController controller = new ShellController(
                entity,
                shellView,
                reservationUsecase,
                enemyBattleState.Attacker,
                enemyBattleState.Target,
                attackUsecase);

            shellView.Initialize(enemyMoveView.GetTargetTransform().position, controller, shellSpecPresenter);
        }

        [SerializeField] private ShellAttackData _attackData;
        [SerializeField] private EnemyMusicData _musicData;

        private MusicSyncInitializer _musicSyncInitializer;
        private MusicSyncView _musicSyncView;
    }
}
