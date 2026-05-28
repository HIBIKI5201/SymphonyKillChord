using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;
using KillChord.Runtime.InfraStructure;
using KillChord.Runtime.View;
using System;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    public class AnimationComposition : MonoBehaviour
    {
        public ICharacterAnimationController Init(CharacterAnimationView view, CharacterAnimationCatalogAsset asset,
             MusicSyncState musicSyncState)
        {
            CharacterAnimationClipRepository repository = new CharacterAnimationClipRepository(asset);

            // Compositionでクリップ配列を解決する（ViewはRepositoryを知らない）
            var states = (CharacterAnimationState[])Enum.GetValues(typeof(CharacterAnimationState));
            var clips = new AnimationClip[states.Length];
            for (int i = 0; i < states.Length; i++)
            {
                repository.TryFindByState(states[i], out clips[i]);
            }

            // Application: ブレンド計算を担うサービス
            ICharacterAnimationApplication application = new CharacterAnimationApplication();

            // Adaptor: ApplicationとMusicSyncStateを橋渡しする
            ICharacterAnimationController controller = new CharacterAnimationController(application, musicSyncState);

            // View: AdaptorとClip配列を受け取って初期化する
            view.Initialize(controller, clips);

            return controller;
        }
    }
}
