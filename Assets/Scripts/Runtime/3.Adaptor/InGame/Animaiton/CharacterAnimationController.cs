using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;
using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    ///     MusicSyncStateからBPMを取得してICharacterAnimationApplicationへ橋渡しするAdaptorクラス。
    ///     ViewへはViewModelに変換して渡す。
    /// </summary>
    public sealed class CharacterAnimationController : ICharacterAnimationController
    {
        /// <summary> CharacterAnimationControllerを初期化する。 </summary>
        /// <param name="animApplication"> アニメーション処理を委譲するApplication。 </param>
        /// <param name="musicSyncState"> BPM情報を持つ音楽同期状態。 </param>
        public CharacterAnimationController(ICharacterAnimationApplication animApplication, MusicSyncState musicSyncState)
        {
            _animApplication = animApplication;
            _musicSyncState = musicSyncState;
            _weights = new float[Enum.GetValues(typeof(CharacterAnimationState)).Length];
        }

        /// <summary>
        ///     Application層の計算結果をDTOに変換して返す。
        /// </summary>
        public CharacterAnimationDTO GetDTO()
        {
            CharacterAnimationBlendData blend = _animApplication.BlendData;
            // StateのInt値をインデックスとしてウェイトを格納する
            _weights[(int)CharacterAnimationState.Idle] = blend.IdleWeight;
            _weights[(int)CharacterAnimationState.Walk] = blend.WalkWeight;

            return new CharacterAnimationDTO(_animApplication.AnimationSpeed, _weights);
        }

        /// <summary>
        ///     MusicSyncStateのBPMをApplicationへ渡し、速度ベクトルを更新する。
        /// </summary>
        /// <param name="velocity"> 2D速度ベクトル。 </param>
        public void SetVelocity(Vector2 velocity)
        {
            // MusicSyncState(Adaptor層) → ICharacterAnimationApplication(Application層) へBPMを橋渡しする
            _animApplication.SetBpm((float)_musicSyncState.Bpm);
            _animApplication.SetVelocity(velocity);
        }

        private readonly ICharacterAnimationApplication _animApplication;
        private readonly MusicSyncState _musicSyncState;
        private readonly float[] _weights;
    }
}
