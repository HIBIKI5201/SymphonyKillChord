using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.Utility.OutGame.Savedata;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        /// <param name="result"> 今回のサブミッション評価結果。 </param>
        /// <param name="isTutorial"> チュートリアルステージの場合はtrueです。 </param>
        /// <returns> セーブ内容が変化した場合はtrue。 </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async ValueTask<bool> SaveClearAsync(
            StageId stageId,
            MissionEvaluationResult result,
            bool isTutorial)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            List<string> achievedEvaluationIds = BuildAchievedEvaluationIds(result);
            SaveData saveData = await _savedataSystem.LoadAsync<SaveData>();
            bool stageProgressChanged =
                saveData.StageProgress.RecordClear(stageId.Value, achievedEvaluationIds);
            bool tutorialChanged = isTutorial && saveData.Tutorial.Complete();

            if (!stageProgressChanged && !tutorialChanged)
            {
                return false;
            }

            await _savedataSystem.SaveAsync(saveData);
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

        private readonly SavedataSystem _savedataSystem;
    }
}
