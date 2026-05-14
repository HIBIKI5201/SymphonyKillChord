using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Application;
using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    public class CharacterAnimationController : ICharacterAnimationController
    {
        /// <summary>
        ///     MusicSyncStateからBPMを取得してPlayerAnimationApplicationへ橋渡しするAdaptorクラス。
        ///     Viewから受け取った速度もApplicationへ転送する。
        /// </summary>
        /// <param name="animApplication"> キャラクターアニメーションのApplication層インスタンス。 </param>
        /// <param name="musicSyncState"> 音楽同期状態のインスタンス。 </param>
        public CharacterAnimationController(ICharacterAnimationApplication animApplication, MusicSyncState musicSyncState)
        {
            _animApplication = animApplication;
            _musicSyncState = musicSyncState;
        }

        /// <summary> アニメーションの再生速度。 </summary>
        public float AnimationSpeed => _animApplication.AnimationSpeed;

        /// <summary> アイドルから歩きへのブレンドウェイト（0〜1）。 </summary>
        public float BlendWeight => _animApplication.BlendWeight;

        /// <summary>
        ///     MusicSyncStateのBPMをApplicationへ渡し、速度を更新する。
        ///     毎フレームViewのUpdateから呼ぶ。
        /// </summary>
        /// <param name="velocity"> 2D速度ベクトル。 </param>
        public void SetVelocity(Vector2 velocity)
        {
            _animApplication.SetBpm((float)_musicSyncState.Bpm);
            _animApplication.SetVelocity(velocity);
        }

        private readonly ICharacterAnimationApplication _animApplication;
        private readonly MusicSyncState _musicSyncState;
    }
}
