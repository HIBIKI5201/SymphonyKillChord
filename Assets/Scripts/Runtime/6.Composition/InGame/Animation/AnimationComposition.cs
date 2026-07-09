using KillChord.Runtime.Adaptor.InGame.Animation;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.InfraStructure;
using KillChord.Runtime.View;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    /// <summary>
    ///     キャラクターアニメーションの依存関係を構築する。
    /// </summary>
    public sealed class AnimationComposition
    {
        /// <summary>
        ///     キャラクターアニメーションを初期化して返す。
        /// </summary>
        /// <param name="view"> アニメーション再生View。 </param>
        /// <param name="asset"> クリップ定義アセット。 </param>
        /// <param name="musicSyncState"> BPM参照元。 </param>
        /// <returns> 初期化済みのView側依存群。 </returns>
        public ICharacterAnimationViewContext Init(
            CharacterAnimationView view,
            CharacterAnimationCatalogAsset asset,
            MusicSyncState musicSyncState)
        {
            var baseClipTypes = (CharacterAnimationClipType[])Enum.GetValues(typeof(CharacterAnimationClipType));
            var baseClips = new AnimationClip[baseClipTypes.Length];
            var oneShotIndices = new Dictionary<string, int>();
            var combinedClips = new List<AnimationClip>(baseClips.Length);

            if (asset != null && asset.Entries != null)
            {
                for (int i = 0; i < asset.Entries.Count; i++)
                {
                    CharacterAnimationCatalogEntry entry = asset.Entries[i];
                    if (entry.Clip == null)
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(entry.Key))
                    {
                        baseClips[(int)entry.ClipType] = entry.Clip;
                    }
                }
            }

            for (int i = 0; i < baseClips.Length; i++)
            {
                combinedClips.Add(baseClips[i]);
            }

            if (asset != null && asset.Entries != null)
            {
                for (int i = 0; i < asset.Entries.Count; i++)
                {
                    CharacterAnimationCatalogEntry entry = asset.Entries[i];
                    if (entry.Clip == null || string.IsNullOrWhiteSpace(entry.Key))
                    {
                        continue;
                    }

                    oneShotIndices[entry.Key] = combinedClips.Count;
                    combinedClips.Add(entry.Clip);
                }
            }

            var clipLengths = new float[combinedClips.Count];
            for (int i = 0; i < combinedClips.Count; i++)
            {
                clipLengths[i] = combinedClips[i] != null
                    ? combinedClips[i].length
                    : 0f;
            }

            CharacterAnimationViewModel viewModel = new CharacterAnimationViewModel();
            CharacterAnimationPlaybackMap playbackMap = new CharacterAnimationPlaybackMap(
                attack: (int)CharacterAnimationClipType.Attack,
                dodge: (int)CharacterAnimationClipType.Dodge,
                clipLengths: clipLengths,
                damage: -1,
                oneShotIndices: oneShotIndices);
            CharacterAnimationSignal signal = new CharacterAnimationSignal(playbackMap);
            CharacterAnimationViewContext context = new CharacterAnimationViewContext(viewModel, signal);

            view.Initialize(context, combinedClips.ToArray(), musicSyncState);
            return context;
        }
    }
}
