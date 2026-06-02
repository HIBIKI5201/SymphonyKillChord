using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;
using KillChord.Runtime.InfraStructure;
using KillChord.Runtime.View;
using System;
using System.Collections.Generic;
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

            var indices = new Dictionary<CharacterAnimationState, int>
            {
                { CharacterAnimationState.Idle, 0 },
                { CharacterAnimationState.Walk, 1 },
                { CharacterAnimationState.Dodge, 2 },
                { CharacterAnimationState.Attack, 3 },
            };

            // Application層を作成
            ICharacterAnimationApplication application = new CharacterAnimationApplication();

            // Adaptor層を作成
            ICharacterAnimationController controller = new CharacterAnimationController(application, musicSyncState);

            // View: AdaptorとClip配列を受け取って初期化する
            view.Initialize(controller, clips);

            return controller;
        }

        /// <summary> 指定した状態の再生インデックスを返す。 </summary>
        public int GetAnimationIndex(CharacterAnimationState state)
        {
            var indices = new Dictionary<CharacterAnimationState, int>
            {
                { CharacterAnimationState.Idle, 0 },
                { CharacterAnimationState.Walk, 1 },
                { CharacterAnimationState.Dodge, 2 },
                { CharacterAnimationState.Attack, 3 },
            };

            return indices.TryGetValue(state, out int index) ? index : -1;
        }
    }
}
