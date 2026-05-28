using System;
using DevelopProducts.BehaviorGraph.Runtime.Adaptor.InGame.Battle;
using DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Music;
using DevelopProducts.BehaviorGraph.Runtime.View;
using DevelopProducts.BehaviorGraph.Runtime.View.InGame.Music;
using DevelopProducts.BehaviorGraph.Runtime.View.Persistent.Music;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.Composition.InGame.Music
{
    /// <summary>
    ///     音楽同期機能の初期化を行うクラス。
    /// </summary>
    public class MusicSyncInitializer : MonoBehaviour
    {
        [SerializeField] private MusicSyncView _musicSyncView;
        [SerializeField] private string _testCue;
        [SerializeField] private int _testBpm;

        public MusicSyncController MusicSyncController;
        public MusicSyncService MusicSyncService;

        public void Initialize()
        {
            MusicSyncViewModel msvm = new();
            var mp = ServiceLocator.GetInstance<MusicPlayer>();
            _musicSyncView.Bind(
                mp,
                msvm
            );

            mp.MusicVM.UpdateMusicCue(_testCue);
            MusicSyncService = new(new(_testBpm));
            MusicSyncController = new(msvm, MusicSyncService);
            ServiceLocator.RegisterInstance<IMusicSyncService>(MusicSyncService);
        }

        public void OnDestroy()
        {
            MusicSyncController.Dispose();
        }
    }
}