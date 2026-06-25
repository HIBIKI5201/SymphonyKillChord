using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.View.InGame.Music;
using KillChord.Runtime.View.InGame.Sequence;
using KillChord.Runtime.View.Persistent.Music;
using SymphonyFrameWork.System.ServiceLocate;
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
        ///　    ゲームプレイを開始し、音楽同期機能を有効にする。
        /// </summary>
        public void StartGameplay()
        {
            if (_musicPlayer == null)
            {
                return;
            }

            _musicPlayer.MusicVM.UpdateMusicCue(_testCue);
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
        [Tooltip("テスト用のキュー名。")]
        [SerializeField] private string _testCue;
        [Tooltip("テスト用のBPM。")]
        [SerializeField] private double _testBpm;
        [Tooltip("ジャスト判定の閾値。")]
        [SerializeField] private float _justTimingThreshold;

        private MusicPlayer _musicPlayer;
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