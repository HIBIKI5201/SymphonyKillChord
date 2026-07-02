using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.InfraStructure.InGame.Music;
using KillChord.Runtime.View.InGame.Music;
using KillChord.Runtime.View.InGame.Sequence;
using KillChord.Runtime.View.Persistent.Music;
using SymphonyFrameWork.System.ServiceLocate;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Music
{
    /// <summary>
    ///     音楽同期機能の初期化を行うクラス。
    /// </summary>
    public class MusicSyncInitializer : MonoBehaviour, IGameplayControllable
    {
        /// <summary> 音楽同期コントローラー。 </summary>
        public MusicSyncController MusicSyncController { get; private set; }
        /// <summary> 音楽同期サービス。 </summary>
        public MusicSyncService MusicSyncService { get; private set; }
        /// <summary> 音楽同期の状態。 </summary>
        public MusicSyncState MusicSyncState { get; private set; }

        /// <summary>
        ///     音楽同期機能を初期化する。
        /// </summary>
        public void Initialize()
        {
            MusicSyncState = new();
            _musicPlayer = ServiceLocator.GetInstance<MusicPlayer>();
            MusicSyncService = new MusicSyncService(new RhythmDefinition(_testBpm, _justTimingThreshold), RhythmJustService.Instance.TriggerJustHit);
            MusicSyncController = new(MusicSyncState, MusicSyncService);
            _musicSyncView.Bind(
                _musicPlayer,
                MusicSyncState,
                MusicSyncController,
                _testBpm
            );

            ServiceLocator.RegisterInstance<IMusicSyncService>(MusicSyncService);
            ServiceLocator.RegisterInstance<MusicSyncState>(MusicSyncState);

            _isRegistered = true;
        }

        /// <summary>
        ///     装備スキルに応じたBGMのキュー名を解決して保持する。
        ///     カタログが未設定の場合は解決を行わず、テスト用キューへフォールバックする。
        /// </summary>
        /// <param name="token"> キャンセルトークン。 </param>
        public async ValueTask PrepareBgmAsync(CancellationToken token)
        {
            if (_skillBgmCatalog == null)
            {
                Debug.LogWarning(
                    $"[{nameof(MusicSyncInitializer)}] {nameof(SkillBgmCatalogAsset)}が未設定です。テスト用キューを使用します。",
                    this);
                return;
            }

            token.ThrowIfCancellationRequested();

            SkillBgmResolveService resolveService = new SkillBgmResolveService(_skillBgmCatalog.ToDomain());
            _bgmCue = await resolveService.ResolveCueAsync();
        }

        /// <summary>
        ///　    ゲームプレイを開始し、音楽同期機能を有効にする。
        /// </summary>
        public void StartGameplay()
        {
            if (_musicPlayer == null)
            {
                return;
            }

            // 装備スキルから解決したキューを優先し、未解決の場合はテスト用キューを使用する
            string cueName = string.IsNullOrEmpty(_bgmCue) ? _testCue : _bgmCue;
            _musicPlayer.MusicVM.UpdateMusicCue(cueName);
        }

        /// <summary>
        ///    ゲームプレイを停止し、音楽同期機能を無効にする。
        /// </summary>
        public void StopGameplay()
        {
            if (_musicPlayer == null)
            {
                return;
            }

            _musicPlayer.MusicVM.UpdateMusicCue(string.Empty);
        }

        [Tooltip("音楽同期View。")]
        [SerializeField] private MusicSyncView _musicSyncView;
        [Tooltip("装備スキルとBGMの対応カタログ。")]
        [SerializeField] private SkillBgmCatalogAsset _skillBgmCatalog;
        [Tooltip("テスト用のキュー名。")]
        [SerializeField] private string _testCue;
        [Tooltip("テスト用のBPM。")]
        [SerializeField] private double _testBpm;
        [Tooltip("ジャスト判定の閾値。")]
        [SerializeField] private float _justTimingThreshold;

        private MusicPlayer _musicPlayer;
        private string _bgmCue;
        private bool _isRegistered;

        private void OnDestroy()
        {
            if (!_isRegistered)
            {
                return;
            }

            ServiceLocator.UnregisterInstance<IMusicSyncService>();
            ServiceLocator.UnregisterInstance<MusicSyncState>();

            _isRegistered = false;
        }
    }
}