using KillChord.Runtime.Domain.Persistent.Savedata;
using SymphonyFrameWork.System.SaveSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using static KillChord.Editor.AIDebugPlay.AIDebugJson;

namespace KillChord.Editor.AIDebugPlay
{
    /// <summary>
    ///     メモリに読み込み済みのセーブをコピーし、装備順と進捗を観測する。
    /// </summary>
    internal static class AIDebugSaveSnapshot
    {
        /// <summary>
        ///     ロードや保存を起こさずに、キャッシュのコピーから状態を取得する。
        /// </summary>
        internal static object Read()
        {
            if (!SaveStore.IsLoaded<SaveData>()) { throw new InvalidOperationException("SaveData is not loaded"); }
            // SaveDataのgetterはnull時に値を生成するため、必ずコピーを参照する。
            var copy = JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(SaveStore.Get<SaveData>()));
            var stages = new List<object>();
            foreach (var stage in copy.StageProgress.ClearDatas)
            {
                stages.Add(Object(("stageId", stage.StageId), ("achievedEvaluationIds", stage.AchievedEvaluationIds)));
            }
            return Object(("source", "inMemoryCopy"), ("diskPersistenceVerified", false),
                ("equippedSkillIds", copy.SkillBuild.EquipmentSkillIDs),
                ("skillLevelupPoint", copy.SkillBuild.SkillLevelupPoint),
                ("researchPoint", copy.SkillUnlock.ResearchPoint),
                ("unlockedSkillIds", copy.SkillUnlock.UnlockedSkillIds),
                ("unlockedSkillNodeIds", copy.SkillUnlock.UnlockedSkillNodeIds),
                ("tutorialPhase", copy.Tutorial.Phase.ToString()), ("clearedStages", stages),
                ("audio", Object(("bgm", copy.AudioSettings.BgmVolume),
                    ("soundEffect", copy.AudioSettings.SoundEffectVolume), ("voice", copy.AudioSettings.VoiceVolume))));
        }
    }
}
