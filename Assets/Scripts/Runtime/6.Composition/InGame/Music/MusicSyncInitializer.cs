using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Composition.InGame.Bootstrap;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.InfraStructure.InGame.Music;
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
    public class MusicSyncInitializer : InGameInitializationModuleBase, IGameplayControllable
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(MusicSyncInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 200;

        /// <summary> 音楽同期コントローラー。 </summary>
        public MusicSyncController MusicSyncController { get; private set; }
        /// <summary> 音楽同期サービス。 </summary>
        public MusicSyncService MusicSyncService { get; private set; }
        /// <summary> 音楽同期の状態。 </summary>
        public MusicSyncState MusicSyncState { get; private set; }

        /// <summary>
        ///     音楽同期機能を初期化してContainerを登録する。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            if (_musicSyncView == null || _rhythmJudgmentDefinitionAsset == null)
            {
                Debug.LogError($"[{nameof(MusicSyncInitializer)}] {nameof(_musicSyncView)} または {nameof(_rhythmJudgmentDefinitionAsset)} が設定されていません。", this);
                return false;
            }

            Initialize();
            _moduleContainer = new MusicSyncModuleContainer(
                MusicSyncController,
                MusicSyncService,
                MusicSyncState);
            ServiceLocator.RegisterInstance(_moduleContainer);
            _isModuleRegistered = true;
            return _musicPlayer != null
                && MusicSyncService != null
                && MusicSyncState != null;
        }

        /// <summary>
        ///     音楽同期機能を初期化する。
        /// </summary>
        public void Initialize()
        {
            MusicSyncState = new();
            _musicPlayer = ServiceLocator.GetInstance<MusicPlayer>();
            MusicSyncService = new MusicSyncService(
                new RhythmDefinition(_testBpm, _testBeatOffsetSeconds),
                _rhythmJudgmentDefinitionAsset.ToDefinition(),
                RhythmJustService.Instance.TriggerJustHit);
            MusicSyncController = new(MusicSyncState, MusicSyncService);
            _musicSyncView.Bind(
                _musicPlayer,
                MusicSyncState,
                MusicSyncController,
                _testBpm,
                _testBeatOffsetSeconds
            );

            ServiceLocator.RegisterInstance<IMusicSyncService>(MusicSyncService);
            ServiceLocator.RegisterInstance<MusicSyncState>(MusicSyncState);

            _isRegistered = true;
        }

        /// <summary>
        ///     登録済みContainerを解除する。
        /// </summary>
        public override void Shutdown()
        {
            if (!_isModuleRegistered)
            {
                return;
            }

            ServiceLocator.UnregisterInstance<MusicSyncModuleContainer>();
            _moduleContainer = null;
            _isModuleRegistered = false;
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
        [Tooltip("音源先頭から最初の小節頭までのオフセット秒数。負数で判定を前倒しできます。")]
        [SerializeField] private double _testBeatOffsetSeconds;
        [Tooltip("リズム判定定義アセット。")]
        [SerializeField] private RhythmJudgmentDefinitionAsset _rhythmJudgmentDefinitionAsset;

        private MusicPlayer _musicPlayer;
        private bool _isRegistered;
        private bool _isModuleRegistered;
        private MusicSyncModuleContainer _moduleContainer;

        private void OnDestroy()
        {
            if (!_isRegistered)
            {
                return;
            }

            ServiceLocator.UnregisterInstance<IMusicSyncService>();
            ServiceLocator.UnregisterInstance<MusicSyncState>();

            // 拍の通知ストリームを破棄する。購読側は先にShutdownで解除されている想定。
            MusicSyncState?.Dispose();

            _isRegistered = false;
        }
    }
}
