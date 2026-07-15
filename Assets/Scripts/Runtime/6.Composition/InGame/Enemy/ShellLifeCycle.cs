using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Application.InGame.Enemy;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Composition.InGame.Music;
using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.InfraStructure.Addressables;
using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.InfraStructure.InGame.Enemy;
using KillChord.Runtime.View.InGame.Enemy;
using KillChord.Runtime.View.InGame.Music;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using KillChord.Runtime.View.InGame.Player;

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
            EnemyMusicSpec musicSpec = ShellFactory.CreateMusicSpec(_loadedMusicData);

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

            _view.Initialize(_playerModuleContainer.PlayerView.transform, shellSpecPresenter, Deactivate);
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
        [SerializeField, Tooltip("砲弾攻撃仕様の Addressables キーです。")] private string _attackDataKey;
        [SerializeField, Tooltip("砲弾音楽仕様の Addressables キーです。")] private string _musicDataKey;

        private PlayerModuleContainer _playerModuleContainer;
        private MusicSyncInitializer _musicSyncInitializer;
        private MusicSyncView _musicSyncView;
        private Action<ShellLifeCycle> _releaseCallback;
        private ShellController _controller;
        private ShellAttackSpecAsset _loadedAttackData;
        private EnemyMusicSpecAsset _loadedMusicData;

        /// <summary>
        ///     ロード済みアセットを解放します。
        /// </summary>
        private void OnDestroy()
        {
            _attackDataKey.ReleaseLoadedAsset(this);
            _musicDataKey.ReleaseLoadedAsset(this);
            _loadedAttackData = null;
            _loadedMusicData = null;
        }
    }
}

