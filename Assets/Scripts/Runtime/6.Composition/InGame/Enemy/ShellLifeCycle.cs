using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Application.InGame.Enemy;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Composition.InGame.Music;
using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.InfraStructure.Addressables;
using KillChord.Runtime.InfraStructure.InGame.Enemy;
using KillChord.Runtime.Utility.Identity;
using KillChord.Runtime.View.InGame.Character;
using KillChord.Runtime.View.InGame.Enemy;
using KillChord.Runtime.View.InGame.Music;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Enemy
{
    /// <summary>
    ///     砲弾のライフサイクルを管理するクラス。
    /// </summary>
    public class ShellLifeCycle : MonoBehaviour, IShellLifeCycle
    {
        /// <summary>
        ///     砲弾用 Addressables アセットをロードします。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        public async Task<bool> LoadAddressableAssetsAsync(CancellationToken cancellationToken)
        {
            try
            {
                _loadedAttackData = await _attackDataKey.LoadAssetAsync<ShellAttackSpecAsset>(this, cancellationToken);
                _loadedMusicData = await _musicDataKey.LoadAssetAsync<EnemyMusicSpecAsset>(this, cancellationToken);
            }
            catch (Exception ex) { Debug.LogException(ex, this); }

            return _loadedAttackData != null && _loadedMusicData != null;
        }

        /// <summary>
        ///     ロード済みアセット参照を別インスタンスへコピーします。
        /// </summary>
        /// <param name="source"> コピー元です。 </param>
        public void CopyLoadedAssetsFrom(ShellLifeCycle source)
        {
            _loadedAttackData = source._loadedAttackData;
            _loadedMusicData = source._loadedMusicData;
        }

        /// <summary>
        ///     砲弾の依存関係を構築する。
        /// </summary>
        /// <param name="releaseCallback"> 砲弾をObject Poolへ戻す際に呼び出すコールバック。 </param>
        /// <param name="shellExplosionEffectView"> 爆発エフェクトを再生するパーティクルView。 </param>
        /// <param name="shellExplosionSoundView"> 爆発SEを再生する外部所有のView。 </param>
        public void Initialize(Action<ShellLifeCycle> releaseCallback, ReusableParticleSystemView shellExplosionEffectView, ReusableSoundEffectView shellExplosionSoundView)
        {
            if (!_musicSyncInitializer) _musicSyncInitializer = FindFirstObjectByType<MusicSyncInitializer>();
            if (!_musicSyncView) _musicSyncView = FindAnyObjectByType<MusicSyncView>();

            if (_musicSyncView.MusicSyncState == null)
            {
                throw new ArgumentNullException("MusicSyncStateが見つかりません。");
            }
            if (_playerModuleContainer == null)
            {
                _playerModuleContainer = ServiceLocator.GetInstance<PlayerModuleContainer>();
            }

            if (_playerModuleContainer == null || _playerModuleContainer.PlayerView == null)
            {
                throw new ArgumentNullException(nameof(_playerModuleContainer), "PlayerModuleContainerが見つかりません。");
            }
            IMusicActionScheduler musicActionScheduler = new MusicSchedulerAdaptor(_musicSyncView.MusicSyncState, _musicSyncInitializer.MusicSyncService);
            ShellAttackSpec attackSpec = ShellFactory.CreateAttackSpec(_loadedAttackData);
            MusicSyncSpec musicSpec = ShellFactory.CreateMusicSpec(_loadedMusicData);

            ShellEntity entity = new ShellEntity(attackSpec, musicSpec, null);

            ShellReservationUsecase reservationUsecase = new ShellReservationUsecase(entity, musicActionScheduler);
            _reservationUsecase = reservationUsecase;
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

            _view.Initialize(
                _playerModuleContainer.PlayerView.transform,
                shellSpecPresenter,
                Deactivate,
                shellExplosionEffectView,
                shellExplosionSoundView,
                GetDetonateApproach);
            _releaseCallback = releaseCallback;
        }

        /// <summary>
        ///     有効化処理。
        /// </summary>
        /// <param name="enemyBattleState"> 砲弾の発射元となる敵の戦闘状態。 </param>
        public void Activate(EnemyBattleState enemyBattleState)
        {
            ReleaseIndicatorOwner();
            gameObject.SetActive(true);

            // Viewの有効化に失敗した(=攻撃対象を失っている)場合、着弾予告SE・爆発予約を
            // 仕込まずに即座にプールへ戻す。表示・ダメージが伴わないまま音だけが再生される事故を防ぐため。
            if (!_view.TryActivate())
            {
                Deactivate();
                return;
            }

            _indicatorOwner = enemyBattleState;
            _indicatorOwnerGeneration = enemyBattleState.BeginShellIndicator();
            _controller.Activate(enemyBattleState);
        }

        /// <summary>
        ///     無効化処理。
        /// </summary>
        public void Deactivate()
        {
            _controller.Deactivate();
            _view.Deactivate();
            ReleaseIndicatorOwner();
            gameObject.SetActive(false);
            _releaseCallback.Invoke(this);
        }


        [SerializeField] private ShellView _view;
        [SerializeField, SourceDataAddress, Tooltip("砲弾攻撃仕様の Addressables キーです。")] private string _attackDataKey;
        [SerializeField, SourceDataAddress, Tooltip("砲弾音楽仕様の Addressables キーです。")] private string _musicDataKey;

        private PlayerModuleContainer _playerModuleContainer;
        private MusicSyncInitializer _musicSyncInitializer;
        private MusicSyncView _musicSyncView;
        private Action<ShellLifeCycle> _releaseCallback;
        private ShellController _controller;
        private ShellReservationUsecase _reservationUsecase;
        private ShellAttackSpecAsset _loadedAttackData;
        private EnemyMusicSpecAsset _loadedMusicData;
        private EnemyBattleState _indicatorOwner;
        private uint _indicatorOwnerGeneration;

        /// <summary>
        ///     表示開始時の発射元へ一度だけ終了を通知する。
        ///     再利用された敵の新しい世代には古い砲弾の終了を反映しない。
        /// </summary>
        private void ReleaseIndicatorOwner()
        {
            EnemyBattleState owner = _indicatorOwner;
            _indicatorOwner = null;
            owner?.EndShellIndicator(_indicatorOwnerGeneration);
        }

        /// <summary>
        ///     直接無効化された場合も、表示中の砲弾として残さない。
        /// </summary>
        private void OnDisable()
        {
            ReleaseIndicatorOwner();
        }

        /// <summary>
        ///     予約済みの爆発時刻までの残り時間から、0〜1の接近進捗を算出します。
        ///     区間の長さ（拍）はShellMusicConstants.DETONATE_LEAD_BEAT_COUNTを使用し、
        ///     着弾予告SEの再生タイミング（ShellReservationUsecase側）と同じ値で揃える。
        /// </summary>
        /// <returns> 0〜1の進捗。予約が無い場合や算出できない場合は0。 </returns>
        private float GetDetonateApproach()
        {
            if (_reservationUsecase == null || !_reservationUsecase.HasDetonateReservation)
            {
                return 0f;
            }

            MusicSyncState musicSyncState = _musicSyncView != null ? _musicSyncView.MusicSyncState : null;
            if (musicSyncState == null)
            {
                return 0f;
            }

            return musicSyncState.GetNormalizedApproach(
                _reservationUsecase.DetonateExecutionTime,
                ShellMusicConstants.DETONATE_LEAD_BEAT_COUNT);
        }

        /// <summary>
        ///     ロード済みアセットを解放します。
        /// </summary>
        private void OnDestroy()
        {
            ReleaseIndicatorOwner();
            _attackDataKey.ReleaseLoadedAsset(this);
            _musicDataKey.ReleaseLoadedAsset(this);
            _loadedAttackData = null;
            _loadedMusicData = null;
        }
    }
}

