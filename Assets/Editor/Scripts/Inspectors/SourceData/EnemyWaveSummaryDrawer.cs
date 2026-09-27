using KillChord.Editor.SourceDataProvider.Core;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.Inspectors.SourceData
{
    /// <summary>
    ///     敵Wave定義の概要とスポーン候補地マップを描画します。
    /// </summary>
    internal static class EnemyWaveSummaryDrawer
    {
        /// <summary>
        ///     敵Wave定義のWave一覧とスポーン候補地マップを描画します。
        /// </summary>
        /// <param name="serializedDefinition"> 描画対象のシリアライズ済みWave定義です。 </param>
        /// <param name="target"> 描画対象のWave定義アセットです。 </param>
        /// <param name="selectedWave"> マップを表示するWaveのindexです。 </param>
        public static void DrawWaveSummary(SerializedObject serializedDefinition, UnityEngine.Object target, ref int selectedWave)
        {
            SerializedProperty wavesProperty = serializedDefinition.FindProperty(WAVES_PROPERTY_NAME);
            DrawDefinition(wavesProperty, PREVIEW_ELEMENT_LIMIT);
            if (wavesProperty == null || !wavesProperty.isArray || wavesProperty.arraySize == 0) { return; }
            selectedWave = Mathf.Clamp(selectedWave, 0, wavesProperty.arraySize - 1);
            int nextWave = EditorGUILayout.IntSlider("マップを表示するWave", selectedWave + 1, 1, wavesProperty.arraySize) - 1;
            if (nextWave != selectedWave)
            {
                selectedWave = nextWave;
                GUIUtility.ExitGUI();
            }
            DrawSpawnPointCandidates(target, serializedDefinition, wavesProperty.GetArrayElementAtIndex(selectedWave));
        }

        private const int PREVIEW_ELEMENT_LIMIT = 20;

        private const string DATA_ID_VALUE_PROPERTY_NAME = "_id";
        private const string DATA_ID_HASH_PROPERTY_NAME = "_hashId";
        private const string WAVES_PROPERTY_NAME = "_waves";
        private const string WAVE_DETAILS_PROPERTY_NAME = "Details";
        private const string WAVE_DURATION_PROPERTY_NAME = "WaveDuration";
        private const string WAVE_STAGE_EFFECTS_PROPERTY_NAME = "StageEffects";
        private const string WAVE_ENEMY_AMOUNT_PROPERTY_NAME = "EnemyAmount";
        private const string WAVE_ENEMY_DEFINITION_ID_PROPERTY_NAME = "EnemyDefinitionId";
        private const string SPAWN_POINT_CANDIDATES_PROPERTY_NAME = "SpawnPointCandidates";
        private const string BATTLE_SCENE_NAME_PROPERTY_NAME = "_battleSceneName";

        /// <summary>
        ///     Wave定義の一覧と各Waveの概要を描画します。
        /// </summary>
        /// <param name="wavesProperty"> Wave配列のプロパティです。 </param>
        /// <param name="elementLimit"> 描画するWave数の上限です。 </param>
        private static void DrawDefinition(
            SerializedProperty wavesProperty,
            int elementLimit)
        {
            if (wavesProperty == null || !wavesProperty.isArray)
            {
                EditorGUILayout.HelpBox("Wave定義を取得できません。", MessageType.Warning);
                return;
            }

            // 全ウェーブの合計時間を求める。
            float totalDuration = 0f;
            for (int i = 0; i < wavesProperty.arraySize; i++)
            {
                SerializedProperty wave = wavesProperty.GetArrayElementAtIndex(i);
                SerializedProperty durationProperty = wave.FindPropertyRelative(WAVE_DURATION_PROPERTY_NAME);
                if (durationProperty != null)
                {
                    totalDuration += durationProperty.floatValue;
                }
            }

            EditorGUILayout.LabelField(
                $"Wave数: {wavesProperty.arraySize} / Total Duration: {totalDuration:0.##} sec",
                EditorStyles.miniBoldLabel);
            // 上限数までのウェーブについて、敵の構成と時間の割合を描画する。
            for (int i = 0; i < Mathf.Min(wavesProperty.arraySize, elementLimit); i++)
            {
                SerializedProperty wave = wavesProperty.GetArrayElementAtIndex(i);
                SerializedProperty detailsProperty = wave.FindPropertyRelative(WAVE_DETAILS_PROPERTY_NAME);
                SerializedProperty durationProperty = wave.FindPropertyRelative(WAVE_DURATION_PROPERTY_NAME);
                SerializedProperty stageEffectsProperty = wave.FindPropertyRelative(WAVE_STAGE_EFFECTS_PROPERTY_NAME);

                int enemyDefinitionCount = detailsProperty?.arraySize ?? 0;
                int spawnCount = CountEnemies(detailsProperty);
                int stageEffectCount = stageEffectsProperty?.arraySize ?? 0;
                float duration = durationProperty?.floatValue ?? 0f;
                float progress = totalDuration > 0f ? duration / totalDuration : 0f;

                EditorGUILayout.LabelField($"Wave {i + 1}", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField(
                    $"敵データ: {enemyDefinitionCount} / 総数: {spawnCount} / 演出: {stageEffectCount}");
                DrawEnemyDefinitions(detailsProperty);
                Rect rect = GUILayoutUtility.GetRect(18f, 18f, GUILayout.ExpandWidth(true));
                EditorGUI.ProgressBar(rect, progress, $"{duration:0.##} sec");
            }

            // 表示しきれなかったウェーブがあれば案内を出す。
            if (wavesProperty.arraySize > elementLimit)
            {
                EditorGUILayout.HelpBox(
                    $"残り {wavesProperty.arraySize - elementLimit} Waveの詳細は上のWaves配列で確認できます。マップは下で選択できます。",
                    MessageType.None);
            }
        }

        /// <summary>
        ///     Waveのスポーン候補地マップを描画します。
        /// </summary>
        /// <param name="target"> 編集対象のWave定義アセットです。 </param>
        /// <param name="serializedDefinition"> 編集対象のシリアライズ済みWave定義です。 </param>
        /// <param name="waveProperty"> 描画するWaveのプロパティです。 </param>
        private static void DrawSpawnPointCandidates(
            UnityEngine.Object target,
            SerializedObject serializedDefinition,
            SerializedProperty waveProperty)
        {
            SerializedProperty candidatesProperty = waveProperty.FindPropertyRelative(SPAWN_POINT_CANDIDATES_PROPERTY_NAME);
            if (candidatesProperty == null || !candidatesProperty.isArray)
            {
                return;
            }

            EditorGUILayout.LabelField("スポーン候補地", EditorStyles.miniBoldLabel);

            // 現在選ばれている候補地のハッシュを集める。
            HashSet<int> currentHashes = new();
            for (int i = 0; i < candidatesProperty.arraySize; i++)
            {
                SerializedProperty hashProperty = candidatesProperty.GetArrayElementAtIndex(i)
                    .FindPropertyRelative(DATA_ID_HASH_PROPERTY_NAME);
                if (hashProperty != null)
                {
                    currentHashes.Add(hashProperty.intValue);
                }
            }

            // 対象シーンが未設定の場合は、読み取り専用の一覧だけを表示する。
            SerializedProperty battleSceneNameProperty = serializedDefinition.FindProperty(BATTLE_SCENE_NAME_PROPERTY_NAME);
            string resolvedSceneName = battleSceneNameProperty == null ? string.Empty : battleSceneNameProperty.stringValue;
            if (string.IsNullOrWhiteSpace(resolvedSceneName))
            {
                EditorGUILayout.HelpBox(
                    "対象のステージシーンが未設定です。Wave定義アセットのBattle Scene Nameを設定してください。",
                    MessageType.Warning);
                DrawReadOnlyCandidateList(candidatesProperty);
                return;
            }

            // マップ上をクリックして候補地を切り替えられるようにする。描画できない場合は一覧で代用する。
            bool mapDrawn = BattleSceneMapRenderer.Draw(
                resolvedSceneName,
                currentHashes,
                spawnPoint => ToggleCandidate(serializedDefinition, target, candidatesProperty, spawnPoint));
            if (!mapDrawn)
            {
                DrawReadOnlyCandidateList(candidatesProperty);
            }
        }

        /// <summary>
        ///     指定したスポーンポイントを候補地へ登録または候補地から解除します。
        /// </summary>
        /// <param name="serializedDefinition"> 編集対象のシリアライズ済みWave定義です。 </param>
        /// <param name="target"> 編集対象のWave定義アセットです。 </param>
        /// <param name="candidatesProperty"> スポーン候補地配列のプロパティです。 </param>
        /// <param name="spawnPoint"> 登録状態を切り替えるスポーンポイントです。 </param>
        private static void ToggleCandidate(
            SerializedObject serializedDefinition,
            UnityEngine.Object target,
            SerializedProperty candidatesProperty,
            BattleSceneDataReader.SpawnPointInfo spawnPoint)
        {
            int existingIndex = -1;
            for (int i = 0; i < candidatesProperty.arraySize; i++)
            {
                SerializedProperty hashProperty = candidatesProperty.GetArrayElementAtIndex(i)
                    .FindPropertyRelative(DATA_ID_HASH_PROPERTY_NAME);
                if (hashProperty != null && hashProperty.intValue == spawnPoint.HashId)
                {
                    existingIndex = i;
                    break;
                }
            }

            // serializedDefinition.ApplyModifiedProperties()がUndoエントリを自動で積むため、
            // ここでUndo.RecordObjectを重ねて呼ぶとUndoが2回分登録され、Ctrl+Zを2回要求してしまう。
            if (existingIndex >= 0)
            {
                candidatesProperty.DeleteArrayElementAtIndex(existingIndex);
            }
            else
            {
                int newIndex = candidatesProperty.arraySize;
                candidatesProperty.InsertArrayElementAtIndex(newIndex);
                SerializedProperty newElement = candidatesProperty.GetArrayElementAtIndex(newIndex);
                newElement.FindPropertyRelative(DATA_ID_VALUE_PROPERTY_NAME).stringValue = spawnPoint.Id;
                newElement.FindPropertyRelative(DATA_ID_HASH_PROPERTY_NAME).intValue = spawnPoint.HashId;
            }

            serializedDefinition.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssetIfDirty(target);
        }

        /// <summary>
        ///     現在選択されているスポーン候補地を読み取り専用で表示します。
        /// </summary>
        /// <param name="candidatesProperty"> スポーン候補地配列のプロパティです。 </param>
        private static void DrawReadOnlyCandidateList(SerializedProperty candidatesProperty)
        {
            if (candidatesProperty.arraySize == 0)
            {
                EditorGUILayout.LabelField("選択中の候補地: なし（全スポーンポイント対象）");
                return;
            }

            List<string> ids = new();
            for (int i = 0; i < candidatesProperty.arraySize; i++)
            {
                SerializedProperty idProperty = candidatesProperty.GetArrayElementAtIndex(i)
                    .FindPropertyRelative(DATA_ID_VALUE_PROPERTY_NAME);
                ids.Add(string.IsNullOrEmpty(idProperty?.stringValue) ? "<不明>" : idProperty.stringValue);
            }

            EditorGUILayout.LabelField($"選択中の候補地: {string.Join(", ", ids)}");
        }

        /// <summary>
        ///     Wave詳細に設定された敵の総数を計算します。
        /// </summary>
        /// <param name="detailsProperty"> Wave詳細配列のプロパティです。 </param>
        /// <returns> 設定された敵の総数です。 </returns>
        private static int CountEnemies(SerializedProperty detailsProperty)
        {
            if (detailsProperty == null || !detailsProperty.isArray)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < detailsProperty.arraySize; i++)
            {
                SerializedProperty detail = detailsProperty.GetArrayElementAtIndex(i);
                SerializedProperty amountProperty = detail.FindPropertyRelative(WAVE_ENEMY_AMOUNT_PROPERTY_NAME);
                if (amountProperty != null)
                {
                    count += amountProperty.intValue;
                }
            }

            return count;
        }

        /// <summary>
        ///     Wave詳細に設定された敵定義と出現数を描画します。
        /// </summary>
        /// <param name="detailsProperty"> Wave詳細配列のプロパティです。 </param>
        private static void DrawEnemyDefinitions(SerializedProperty detailsProperty)
        {
            if (detailsProperty == null || !detailsProperty.isArray)
            {
                return;
            }

            for (int i = 0; i < detailsProperty.arraySize; i++)
            {
                SerializedProperty detail = detailsProperty.GetArrayElementAtIndex(i);
                SerializedProperty definitionIdProperty = detail.FindPropertyRelative(WAVE_ENEMY_DEFINITION_ID_PROPERTY_NAME);
                SerializedProperty definitionIdValueProperty = definitionIdProperty?.FindPropertyRelative(DATA_ID_VALUE_PROPERTY_NAME);
                SerializedProperty amountProperty = detail.FindPropertyRelative(WAVE_ENEMY_AMOUNT_PROPERTY_NAME);
                string definitionId = string.IsNullOrWhiteSpace(definitionIdValueProperty?.stringValue)
                    ? "<未設定>"
                    : definitionIdValueProperty.stringValue;
                EditorGUILayout.LabelField(
                    $"  {definitionId} × {amountProperty?.intValue ?? 0}",
                    EditorStyles.miniLabel);
            }
        }
    }
}
