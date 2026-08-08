using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.Utility.OutGame.Savedata;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.Application.Persistent.Savedata
{
    /// <summary>
    ///     ステージ進行状況の保存と読み込みを行うサービス。
    /// </summary>
    public class StageProgressSaveDataService
    {
        /// <summary>
        ///     ステージ進行保存サービスを生成します。
        /// </summary>
        /// <param name="savedataSystem"> 使用するセーブシステムです。 </param>
        public StageProgressSaveDataService(SavedataSystem savedataSystem)
        {
            _savedataSystem = savedataSystem ?? throw new ArgumentNullException(nameof(savedataSystem));
        }

        /// <summary>
        ///     ステージクリア結果を保存する。
        /// </summary>
        /// <param name="stageId"> クリアしたステージId。 </param>
        /// <param name="reward"> 初回クリア時に付与する報酬。 </param>
        /// <param name="result"> 今回のサブミッション評価結果。 </param>
        /// <param name="isTutorial"> チュートリアルステージの場合はtrueです。 </param>
        /// <returns> セーブ内容が変化した場合はtrue。 </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async ValueTask<bool> SaveClearAsync(
            StageId stageId,
            StageReward reward,
            MissionEvaluationResult result,
            bool isTutorial)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            List<string> achievedEvaluationIds = BuildAchievedEvaluationIds(result);
            SaveData saveData = await _savedataSystem.LoadAsync<SaveData>();
            bool isFirstClear = !saveData.StageProgress.IsStageCleared(stageId.Value);
            bool stageProgressChanged =
                saveData.StageProgress.RecordClear(stageId.Value, achievedEvaluationIds);
            bool tutorialChanged = isTutorial && saveData.Tutorial.Complete();

            if (!stageProgressChanged && !tutorialChanged && !isFirstClear)
            {
                return false;
            }

            await SaveAndLogRewardAsync(saveData, stageId, reward, isFirstClear);
            return true;
        }

        /// <summary>
        ///     ミッション評価を持たないステージのクリア結果を保存する。
        /// </summary>
        /// <param name="stageId"> クリアしたステージID。 </param>
        /// <param name="reward"> 初回クリア時に付与する報酬。 </param>
        /// <returns> セーブ内容が変化した場合はtrue。 </returns>
        public async ValueTask<bool> SaveClearAsync(StageId stageId, StageReward reward)
        {
            SaveData saveData = await _savedataSystem.LoadAsync<SaveData>();
            bool isFirstClear = !saveData.StageProgress.IsStageCleared(stageId.Value);
            bool stageProgressChanged =
                saveData.StageProgress.RecordClear(stageId.Value, Array.Empty<string>());
            if (!stageProgressChanged)
            {
                return false;
            }

            await SaveAndLogRewardAsync(saveData, stageId, reward, isFirstClear);
            return true;
        }

        /// <summary>
        ///     保存済みのステージ進行状況を読み込む。
        ///     現時点では呼び出し元を作成しない。
        /// </summary>
        public async ValueTask<StageProgressData> LoadAsync()
        {
            SaveData saveData = await _savedataSystem.LoadAsync<SaveData>();
            return saveData.StageProgress;
        }

        /// <summary>
        ///     達成済みのサブミッションIDを抽出する。
        /// </summary>
        private static List<string> BuildAchievedEvaluationIds(MissionEvaluationResult evaluationResult)
        {
            List<string> achievedEvaluationIds = new();
            MissionEvaluationProgress[] progresses = evaluationResult.Progresses;

            for (int i = 0; i < progresses.Length; i++)
            {
                MissionEvaluationProgress progress = progresses[i];

                if (!progress.IsSucceeded)
                {
                    continue;
                }

                string evaluationId = progress.EvaluationId.Value;

                if (string.IsNullOrWhiteSpace(evaluationId)
                    || achievedEvaluationIds.Contains(evaluationId))
                {
                    continue;
                }

                achievedEvaluationIds.Add(
                    evaluationId);
            }

            return achievedEvaluationIds;
        }

        /// <summary>
        ///     初回クリア報酬をセーブデータへ加算します。
        /// </summary>
        /// <param name="saveData"> 報酬を加算するセーブデータ。</param>
        /// <param name="reward"> 加算するステージ報酬。</param>
        private static void GrantReward(SaveData saveData, StageReward reward)
        {
            int skillLevelupPoint = checked(
                saveData.SkillBuild.SkillLevelupPoint + reward.SkillBuildPoint);
            int researchPoint = checked(
                saveData.SkillUnlock.ResearchPoint + reward.SkillUnlockPoint);

            saveData.SkillBuild.SetSkillLevelupPoint(skillLevelupPoint);
            saveData.SkillUnlock.SetResearchPoint(researchPoint);
        }

        /// <summary>
        ///     初回クリア報酬を反映して保存し、結果をログへ出力します。
        /// </summary>
        /// <param name="saveData"> 保存するセーブデータ。</param>
        /// <param name="stageId"> クリアしたステージID。</param>
        /// <param name="reward"> 初回クリア時に付与する報酬。</param>
        /// <param name="isFirstClear"> 初回クリアの場合はtrue。</param>
        private async ValueTask SaveAndLogRewardAsync(
            SaveData saveData,
            StageId stageId,
            StageReward reward,
            bool isFirstClear)
        {
            try
            {
                if (isFirstClear)
                {
                    GrantReward(saveData, reward);
                }

                await _savedataSystem.SaveAsync(saveData);

                if (isFirstClear)
                {
                    Debug.Log(
                        $"<color=#FFFF00>[{nameof(StageProgressSaveDataService)}] "
                        + "ステージクリア報酬の付与に成功しました。"
                        + $" StageId: {stageId.Value},"
                        + $" SkillBuildPoint: +{reward.SkillBuildPoint},"
                        + $" SkillUnlockPoint: +{reward.SkillUnlockPoint}</color>");
                }
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"[{nameof(StageProgressSaveDataService)}] ステージクリア報酬の付与または保存に失敗しました。"
                    + $" StageId: {stageId.Value} / {exception}");
                throw;
            }
        }

        private readonly SavedataSystem _savedataSystem;
    }
}
