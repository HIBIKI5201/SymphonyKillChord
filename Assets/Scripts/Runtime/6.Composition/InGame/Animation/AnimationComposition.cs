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
    /// <summary>
    ///     キャラクターアニメーションのCompositionクラス。
    /// </summary>
    public class AnimationComposition : MonoBehaviour
    {
        /// <summary> キャラクターアニメーションを初期化して返す。 </summary>
        /// <param name="view">CharacterAnimationViewのインスタンス</param>
        /// <param name="asset">CharacterAnimationCatalogAssetのインスタンス</param>
        /// <param name="musicSyncState">MusicSyncStateのインスタンス</param>
        /// <param name="indices">CharacterAnimationIndicesの出力パラメータ</param>
        /// <returns>初期化されたICharacterAnimationControllerのインスタンス</returns>
        public ICharacterAnimationController Init(CharacterAnimationView view, CharacterAnimationCatalogAsset asset,
             MusicSyncState musicSyncState, out CharacterAnimationIndices indices)
        {
            CharacterAnimationClipRepository repository = new CharacterAnimationClipRepository(asset);
            // RepositoryからDomain enumに対応するAnimationClipを取得。
            var states = (CharacterAnimationState[])Enum.GetValues(typeof(CharacterAnimationState));
            var baseClips = new AnimationClip[states.Length];
            // RepositoryからDomain enumに対応するAnimationClipを取得。
            for (int i = 0; i < states.Length; i++)
            {
                repository.TryFindByState(states[i], out baseClips[i]);
            }

            // Domain enum -> int のマッピング。
            var indexMap = new Dictionary<CharacterAnimationState, int>
            {
                { CharacterAnimationState.Idle, 0 },
                { CharacterAnimationState.Walk, 1 },
                { CharacterAnimationState.Dodge, 2 },
                { CharacterAnimationState.Attack, 3 },
            };

            var oneShotIndices = new Dictionary<string, int>();
            var combinedClips = new List<AnimationClip>(baseClips.Length);

            for (int i = 0; i < baseClips.Length; i++)
            {
                combinedClips.Add(baseClips[i]);
            }

            float[] clipDurations = new float[combinedClips.Count];
            for (int i = 0; i < combinedClips.Count; i++)
            {
                clipDurations[i] = combinedClips[i] != null
                    ? combinedClips[i].length
                    : 0f;
            }

            // AssetからOneShotアニメーションを取得して、Domain enumに対応するAnimationClipリストに追加。
            if (asset != null && asset.Entries != null)
            {
                foreach (var entry in asset.Entries)
                {
                    if (entry.Clip == null)
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(entry.Key))
                    {
                        continue;
                    }

                    oneShotIndices[entry.Key] = combinedClips.Count;
                    combinedClips.Add(entry.Clip);
                }
            }

            // Adaptor 層のインデックス設定オブジェクトを作成。
            indices = new CharacterAnimationIndices(
                attack: indexMap[CharacterAnimationState.Attack],
                dodge: indexMap[CharacterAnimationState.Dodge],
                damage: -1,
                oneShotIndices: oneShotIndices
            );

            // Application層を作成
            ICharacterAnimationApplication application = new CharacterAnimationApplication();

            // Adaptor層を作成
            ICharacterAnimationController controller = new CharacterAnimationController(application, musicSyncState, combinedClips.Count, clipDurations);

            // Viewを初期化
            view.Initialize(controller, combinedClips.ToArray());

            return controller;
        }
    }
}
