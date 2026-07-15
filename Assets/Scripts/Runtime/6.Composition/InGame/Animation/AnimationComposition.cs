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
            var baseEnterBlendFrameCounts = new int[baseClipTypes.Length];
            var baseExitBlendFrameCounts = new int[baseClipTypes.Length];
            var oneShotIndices = new Dictionary<string, int>();
            var combinedClips = new List<AnimationClip>(baseClips.Length);
            var combinedEnterBlendFrameCounts = new List<int>(baseClips.Length);
            var combinedExitBlendFrameCounts = new List<int>(baseClips.Length);

            int defaultEnterBlendFrameCount = asset != null
                ? asset.EnterBlendFrameCount
                : 0;
            int defaultExitBlendFrameCount = asset != null
                ? asset.ExitBlendFrameCount
                : 0;

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
                        int index = (int)entry.ClipType;
                        baseClips[index] = entry.Clip;
                        baseEnterBlendFrameCounts[index] = ResolveBlendFrameCount(
                            entry.EnterBlendFrameCount,
                            defaultEnterBlendFrameCount);
                        baseExitBlendFrameCounts[index] = ResolveBlendFrameCount(
                            entry.ExitBlendFrameCount,
                            defaultExitBlendFrameCount);
                    }
                }
            }

            for (int i = 0; i < baseClips.Length; i++)
            {
                combinedClips.Add(baseClips[i]);
                combinedEnterBlendFrameCounts.Add(baseEnterBlendFrameCounts[i]);
                combinedExitBlendFrameCounts.Add(baseExitBlendFrameCounts[i]);
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
                    combinedEnterBlendFrameCounts.Add(ResolveBlendFrameCount(
                        entry.EnterBlendFrameCount,
                        defaultEnterBlendFrameCount));
                    combinedExitBlendFrameCounts.Add(ResolveBlendFrameCount(
                        entry.ExitBlendFrameCount,
                        defaultExitBlendFrameCount));
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
                enterBlendFrameCounts: combinedEnterBlendFrameCounts.ToArray(),
                exitBlendFrameCounts: combinedExitBlendFrameCounts.ToArray(),
                damage: -1,
                oneShotIndices: oneShotIndices);
            CharacterAnimationOneShotTimingCalculator timingCalculator = new CharacterAnimationOneShotTimingCalculator();
            CharacterAnimationSignal signal = new CharacterAnimationSignal(
                playbackMap,
                () => GetAnimationSpeed(musicSyncState),
                timingCalculator);
            CharacterAnimationViewContext context = new CharacterAnimationViewContext(viewModel, signal);

            view.Initialize(context, combinedClips.ToArray(), musicSyncState);
            return context;
        }

        /// <summary>
        ///     現在のBPMからアニメーション再生速度を取得する。
        /// </summary>
        /// <param name="musicSyncState"> 音楽同期状態です。 </param>
        /// <returns> 再生速度です。 </returns>
        private static float GetAnimationSpeed(MusicSyncState musicSyncState)
        {
            const float baseBpm = 60f;

            return musicSyncState == null
                ? 1f
                : Mathf.Max(1f, (float)musicSyncState.Bpm) / baseBpm;
        }

        /// <summary>
        ///     EntryとCatalog全体設定から使用するブレンドフレーム数を解決する。
        /// </summary>
        /// <param name="entryFrameCount"> Entry側のフレーム数です。 </param>
        /// <param name="defaultFrameCount"> Catalog側の既定フレーム数です。 </param>
        /// <returns> 使用するブレンドフレーム数です。 </returns>
        private static int ResolveBlendFrameCount(int entryFrameCount, int defaultFrameCount)
        {
            return entryFrameCount > 0
                ? entryFrameCount
                : defaultFrameCount;
        }
    }
}
