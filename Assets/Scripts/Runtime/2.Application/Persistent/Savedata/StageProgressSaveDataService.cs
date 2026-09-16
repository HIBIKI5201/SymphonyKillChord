using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.Domain.OutGame.Resource;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.Domain.Persistent.Savedata;
using SymphonyFrameWork.System.SaveSystem;
using System;
using System.Collections.Generic;
using System.Text;
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
        ///     ステージクリア結果を保存する。
        /// </summary>
        /// <param name="stageId"> クリアしたステージId。 </param>
        /// <param name="firstClearReward"> 初回クリア時にのみ付与する報酬。 </param>
        /// <param name="clearReward"> クリアするたびに付与する成功報酬。 </param>
        /// <param name="result"> 今回のサブミッション評価結果。 </param>
        /// <param name="isTutorial"> チュートリアルステージの場合はtrueです。 </param>
        /// <returns> セーブ内容が変化した場合はtrue。 </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async ValueTask<bool> SaveClearAsync(
            StageId stageId,
            StageReward firstClearReward,
            StageReward clearReward,
            MissionEvaluationResult result,
            bool isTutorial)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            List<string> achievedEvaluationIds = BuildAchievedEvaluationIds(result);
            SaveData saveData = SaveStore.IsLoaded<SaveData>()
                ? SaveStore.Get<SaveData>()
                : await SaveStore.LoadAsync<SaveData>();
            SaveDataSnapshot snapshot = new(saveData);
            bool isFirstClear = !saveData.StageProgress.IsStageCleared(stageId.Value);
            bool stageProgressChanged =
                saveData.StageProgress.RecordClear(stageId.Value, achievedEvaluationIds);
            bool tutorialChanged = isTutorial && saveData.Tutorial.CompleteBattle();

            // 成功報酬は毎回付与するため、進行状況に変化が無くても報酬があれば保存する。
            if (!stageProgressChanged && !tutorialChanged && !isFirstClear && clearReward.IsEmpty)
            {
                return false;
            }

            await SaveAndLogRewardAsync(saveData, snapshot, stageId, firstClearReward, clearReward, isFirstClear);
            return true;
        }

        /// <summary>
        ///     ミッション評価を持たないステージのクリア結果を保存する。
        /// </summary>
        /// <param name="stageId"> クリアしたステージID。 </param>
        /// <param name="firstClearReward"> 初回クリア時にのみ付与する報酬。 </param>
        /// <param name="clearReward"> クリアするたびに付与する成功報酬。 </param>
        /// <param name="completesOpeningScenario"> オープニングチュートリアルを完了する場合はtrueです。 </param>
        /// <returns> セーブ内容が変化した場合はtrue。 </returns>
        public async ValueTask<bool> SaveClearAsync(
            StageId stageId,
            StageReward firstClearReward,
            StageReward clearReward,
            bool completesOpeningScenario)
        {
            SaveData saveData = SaveStore.IsLoaded<SaveData>()
                ? SaveStore.Get<SaveData>()
                : await SaveStore.LoadAsync<SaveData>();
            SaveDataSnapshot snapshot = new(saveData);
            bool isFirstClear = !saveData.StageProgress.IsStageCleared(stageId.Value);
            bool stageProgressChanged =
                saveData.StageProgress.RecordClear(stageId.Value, Array.Empty<string>());
            bool tutorialChanged = completesOpeningScenario
                && saveData.Tutorial.Phase == TutorialPhase.NotStarted
                && saveData.Tutorial.CompleteOpeningScenario();

            // 成功報酬は毎回付与するため、進行状況に変化が無くても報酬があれば保存する。
            if (!stageProgressChanged && !tutorialChanged && !isFirstClear && clearReward.IsEmpty)
            {
                return false;
            }

            await SaveAndLogRewardAsync(saveData, snapshot, stageId, firstClearReward, clearReward, isFirstClear);
            return true;
        }

        /// <summary>
        ///     保存済みのステージ進行状況を読み込む。
        ///     現時点では呼び出し元を作成しない。
        /// </summary>
        public async ValueTask<StageProgressData> LoadAsync()
        {
            SaveData saveData = SaveStore.IsLoaded<SaveData>()
                ? SaveStore.Get<SaveData>()
                : await SaveStore.LoadAsync<SaveData>();
            return saveData.StageProgress;
        }

        private const string LEGACY_POINT_MIGRATED_JSON = "{\"_isLegacyPointMigrated\":true}";
        private const string LEGACY_POINT_NOT_MIGRATED_JSON = "{\"_isLegacyPointMigrated\":false}";

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
        ///     報酬のリソースを所持数へ加算します。
        /// </summary>
        /// <param name="inventory"> 報酬を加算するリソースの所持数。</param>
        /// <param name="reward"> 加算するステージ報酬。</param>
        private static void GrantReward(ResourceInventoryData inventory, StageReward reward)
        {
            IReadOnlyList<GameResourceAmount> items = reward.Items;
            for (int i = 0; i < items.Count; i++)
            {
                inventory.Add(items[i].ResourceId, items[i].Amount);
            }
        }

        /// <summary>
        ///     クリア報酬を反映して保存し、結果をログへ出力します。
        ///     初回クリア時は初回クリア報酬と成功報酬の両方、2回目以降は成功報酬のみを付与します。
        /// </summary>
        /// <param name="saveData"> 保存するセーブデータ。</param>
        /// <param name="snapshot"> 保存に失敗した場合へ戻すための変更前状態。</param>
        /// <param name="stageId"> クリアしたステージID。</param>
        /// <param name="firstClearReward"> 初回クリア時にのみ付与する報酬。</param>
        /// <param name="clearReward"> クリアするたびに付与する成功報酬。</param>
        /// <param name="isFirstClear"> 初回クリアの場合はtrue。</param>
        private static async ValueTask SaveAndLogRewardAsync(
            SaveData saveData,
            SaveDataSnapshot snapshot,
            StageId stageId,
            StageReward firstClearReward,
            StageReward clearReward,
            bool isFirstClear)
        {
            try
            {
                ResourceInventoryData inventory = saveData.ResourceInventory;
                if (isFirstClear)
                {
                    GrantReward(inventory, firstClearReward);
                }

                GrantReward(inventory, clearReward);

                await SaveStore.SaveAsync<SaveData>();

                Debug.Log(
                    $"<color=#FFFF00>[{nameof(StageProgressSaveDataService)}] "
                    + "ステージクリア報酬の付与に成功しました。"
                    + $" StageId: {stageId.Value},"
                    + $" FirstClearReward: {(isFirstClear ? FormatReward(firstClearReward) : "対象外")},"
                    + $" ClearReward: {FormatReward(clearReward)}</color>");
            }
            catch (Exception exception)
            {
                // SaveStore が返すキャッシュ参照を、クリア記録前の状態へ戻す。
                snapshot.Restore(saveData);
                Debug.LogError(
                    $"[{nameof(StageProgressSaveDataService)}] ステージクリア報酬の付与または保存に失敗しました。"
                    + $" StageId: {stageId.Value} / {exception}");
                throw;
            }
        }

        /// <summary>
        ///     ログ出力用に報酬の内容を文字列へ変換します。
        /// </summary>
        /// <param name="reward"> 変換する報酬。</param>
        /// <returns> 「リソースID:+数量」をカンマ区切りで並べた文字列。報酬なしの場合は「なし」。</returns>
        private static string FormatReward(StageReward reward)
        {
            if (reward.IsEmpty)
            {
                return "なし";
            }

            IReadOnlyList<GameResourceAmount> items = reward.Items;
            StringBuilder builder = new();
            for (int i = 0; i < items.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(items[i].ResourceId.Value);
                builder.Append(":+");
                builder.Append(items[i].Amount);
            }

            return builder.ToString();
        }

        /// <summary>
        ///     保存に失敗した場合にキャッシュを戻すための、セーブデータの変更前状態。
        /// </summary>
        private readonly struct SaveDataSnapshot
        {
            /// <summary>
            ///     セーブデータの現在の内容を複製して保持する。
            /// </summary>
            /// <param name="saveData"> 複製元のセーブデータ。 </param>
            internal SaveDataSnapshot(SaveData saveData)
            {
                // ResourceInventory プロパティを介さずJSONから複製し、移行前の所持数と移行フラグも復元可能にする。
                SaveDataSnapshotState snapshotState =
                    JsonUtility.FromJson<SaveDataSnapshotState>(JsonUtility.ToJson(saveData))
                    ?? new SaveDataSnapshotState();
                _resourceInventoryJson = JsonUtility.ToJson(snapshotState.ResourceInventory);
                _isLegacyPointMigrated = snapshotState.IsLegacyPointMigrated;
                _stageProgressJson = JsonUtility.ToJson(saveData.StageProgress);
                _tutorialJson = JsonUtility.ToJson(saveData.Tutorial);
                _skillBuildJson = JsonUtility.ToJson(saveData.SkillBuild);
                _skillUnlockJson = JsonUtility.ToJson(saveData.SkillUnlock);
            }

            /// <summary>
            ///     保持している内容をセーブデータへ書き戻す。
            ///     <para>
            ///         StageProgressとTutorialは変更を打ち消すDomain APIを持たないため、
            ///         各データのインスタンスを保ったままJSONで上書きする。
            ///         インスタンスを差し替えないのは、参照を保持している呼び出し元が
            ///         古いインスタンスを見続けることを防ぐため。
            ///     </para>
            /// </summary>
            /// <param name="saveData"> 書き戻す先のセーブデータ。 </param>
            internal void Restore(SaveData saveData)
            {
                // 復元用アクセスで旧ポイント移行が再実行されないよう、一時的に移行済みとして扱う。
                JsonUtility.FromJsonOverwrite(LEGACY_POINT_MIGRATED_JSON, saveData);
                JsonUtility.FromJsonOverwrite(_resourceInventoryJson, saveData.ResourceInventory);
                JsonUtility.FromJsonOverwrite(_stageProgressJson, saveData.StageProgress);
                JsonUtility.FromJsonOverwrite(_tutorialJson, saveData.Tutorial);
                JsonUtility.FromJsonOverwrite(_skillBuildJson, saveData.SkillBuild);
                JsonUtility.FromJsonOverwrite(_skillUnlockJson, saveData.SkillUnlock);
                JsonUtility.FromJsonOverwrite(
                    _isLegacyPointMigrated
                        ? LEGACY_POINT_MIGRATED_JSON
                        : LEGACY_POINT_NOT_MIGRATED_JSON,
                    saveData);
            }

            private readonly string _resourceInventoryJson;
            private readonly bool _isLegacyPointMigrated;
            private readonly string _stageProgressJson;
            private readonly string _tutorialJson;
            private readonly string _skillBuildJson;
            private readonly string _skillUnlockJson;
        }

        /// <summary>
        ///     SaveData の非公開シリアライズフィールドから、移行前の状態を読み取るための一時データ。
        /// </summary>
        [Serializable]
        private sealed class SaveDataSnapshotState
        {
            /// <summary> 移行を発生させずに取得したリソース所持数。 </summary>
            internal ResourceInventoryData ResourceInventory => _resourceInventory ?? new ResourceInventoryData();

            /// <summary> 旧形式ポイントを移行済みの場合はtrue。 </summary>
            internal bool IsLegacyPointMigrated => _isLegacyPointMigrated;

            [SerializeField, Tooltip("移行を発生させずに複製したリソース所持数。")]
            private ResourceInventoryData _resourceInventory;

            [SerializeField, Tooltip("旧形式ポイントを移行済みの場合はtrue。")]
            private bool _isLegacyPointMigrated;
        }
    }
}
