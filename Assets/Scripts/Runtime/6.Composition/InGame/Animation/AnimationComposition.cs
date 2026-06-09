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
             MusicSyncState musicSyncState, out CharacterAnimationIndices indices)
        {
            CharacterAnimationClipRepository repository = new CharacterAnimationClipRepository(asset);

            // Compositionでクリップ配列を解決する
            var states = (CharacterAnimationState[])Enum.GetValues(typeof(CharacterAnimationState));
            var clips = new AnimationClip[states.Length];
            for (int i = 0; i < states.Length; i++)
            {
                repository.TryFindByState(states[i], out clips[i]);
            }

            // Domain enum -> int のマッピング
            var indexMap = new Dictionary<CharacterAnimationState, int>
            {
                { CharacterAnimationState.Idle, 0 },
                { CharacterAnimationState.Walk, 1 },
                { CharacterAnimationState.Dodge, 2 },
                { CharacterAnimationState.Attack, 3 },
            };

            // Adaptor 層のインデックス設定オブジェクトを作成
            indices = new CharacterAnimationIndices(
                attack: indexMap[CharacterAnimationState.Attack],
                dodge: indexMap[CharacterAnimationState.Dodge],
                damage: -1  // 今後実装
            );

            // Application層を作成
            ICharacterAnimationApplication application = new CharacterAnimationApplication();

            // Adaptor層を作成
            ICharacterAnimationController controller = new CharacterAnimationController(application, musicSyncState);

            // Viewを初期化
            view.Initialize(controller, clips);

            return controller;
        }
    }
}
