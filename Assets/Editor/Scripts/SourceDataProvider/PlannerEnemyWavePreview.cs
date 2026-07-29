using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.SourceDataProvider
{
    /// <summary>
    ///     敵Wave定義リポジトリと個別Wave定義の概要を表示します。
    /// </summary>
    internal static class PlannerEnemyWavePreview
    {
        /// <summary>
        ///     リポジトリに登録された敵Wave定義一覧を描画します。
        /// </summary>
        /// <param name="repositoryProperty"> Wave定義アセットの参照配列です。 </param>
        /// <param name="elementLimit"> 表示する最大要素数です。 </param>
        public static void DrawRepository(
            SerializedProperty repositoryProperty,
            int elementLimit)
        {
            for (int i = 0; i < Mathf.Min(repositoryProperty.arraySize, elementLimit); i++)
            {
                SerializedProperty element = repositoryProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                if (element.propertyType != SerializedPropertyType.ObjectReference)
                {
                    EditorGUILayout.HelpBox(
                        "Wave Collectionの要素がアセット参照ではありません。"
                        + " Source Data Provider設定のProperty Pathを確認してください。",
                        MessageType.Error);
                }
                else if (element.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox(
                        $"EnemyWaveDefinitionAssetが未設定です。Index: {i}",
                        MessageType.Warning);
                }
                else
                {
                    Draw(element.objectReferenceValue, elementLimit);
                }
                EditorGUILayout.EndVertical();
            }

            if (repositoryProperty.arraySize > elementLimit)
            {
                EditorGUILayout.HelpBox(
                    $"残り {repositoryProperty.arraySize - elementLimit} 件はInspector側で確認してください。",
                    MessageType.None);
            }
        }

        /// <summary>
        ///     Wave定義アセットの概要とWave構成を描画します。
        /// </summary>
        /// <param name="target"> Wave定義アセットです。 </param>
        /// <param name="elementLimit"> 表示する最大Wave数です。 </param>
        public static void Draw(UnityEngine.Object target, int elementLimit)
        {
            if (!TryCreateSerializedDefinition(target, out SerializedObject serializedDefinition))
            {
                EditorGUILayout.HelpBox(
                    "EnemyWaveDefinitionAssetのWave定義を取得できません。",
                    MessageType.Warning);
                return;
            }

            SerializedProperty idProperty = serializedDefinition.FindProperty(
                ID_PROPERTY_NAME);
            SerializedProperty idValueProperty = idProperty?.FindPropertyRelative(
                DATA_ID_VALUE_PROPERTY_NAME);
            SerializedProperty loopProperty = serializedDefinition.FindProperty(
                LOOP_PROPERTY_NAME);
            SerializedProperty loopStartProperty = serializedDefinition.FindProperty(
                LOOP_START_PROPERTY_NAME);
            string id = idValueProperty?.stringValue;

            EditorGUILayout.LabelField(
                string.IsNullOrWhiteSpace(id) ? target.name : id,
                EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField("Asset", target, target.GetType(), false);
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                EditorGUILayout.HelpBox("Wave定義IDが未設定です。", MessageType.Warning);
            }

            bool isLoop = loopProperty?.boolValue ?? false;
            string loopLabel = isLoop
                ? $"あり（開始Index: {loopStartProperty?.intValue ?? 0}）"
                : "なし";
            EditorGUILayout.LabelField($"ループ: {loopLabel}");

            int waveDefinitionIdHash = idProperty?.FindPropertyRelative(DATA_ID_HASH_PROPERTY_NAME)?.intValue ?? 0;
            DrawDefinition(
                target,
                waveDefinitionIdHash,
                serializedDefinition,
                serializedDefinition.FindProperty(WAVES_PROPERTY_NAME),
                elementLimit);
        }

        /// <summary>
        ///     Wave定義アセットからナビゲーション表示名を生成します。
        /// </summary>
        /// <param name="target"> Wave定義アセット候補です。 </param>
        /// <param name="label"> 生成した表示名です。 </param>
        /// <returns> Wave定義アセットとして表示名を生成できた場合はtrueです。 </returns>
        public static bool TryBuildItemLabel(
            UnityEngine.Object target,
            out string label)
        {
            label = string.Empty;
            if (!TryCreateSerializedDefinition(target, out SerializedObject serializedDefinition))
            {
                return false;
            }

            SerializedProperty idProperty = serializedDefinition.FindProperty(ID_PROPERTY_NAME);
            SerializedProperty idValueProperty = idProperty?.FindPropertyRelative(
                DATA_ID_VALUE_PROPERTY_NAME);
            label = string.IsNullOrWhiteSpace(idValueProperty?.stringValue)
                ? $"<ID未設定> ({target.name})"
                : $"{idValueProperty.stringValue} ({target.name})";
            return true;
        }

        /// <summary>
        ///     1つの敵Wave定義アセットが持つWave構成を描画します。
        /// </summary>
        /// <param name="target"> Wave定義アセットです。 </param>
        /// <param name="waveDefinitionIdHash"> Wave定義アセット自体のIDハッシュです。 </param>
        /// <param name="serializedDefinition"> Wave定義アセットのSerializedObjectです。 </param>
        /// <param name="wavesProperty"> Wave配列プロパティです。 </param>
        /// <param name="elementLimit"> 表示する最大Wave数です。 </param>
        private static void DrawDefinition(
            UnityEngine.Object target,
            int waveDefinitionIdHash,
            SerializedObject serializedDefinition,
            SerializedProperty wavesProperty,
            int elementLimit)
        {
            if (wavesProperty == null || !wavesProperty.isArray)
            {
                EditorGUILayout.HelpBox("Wave定義を取得できません。", MessageType.Warning);
                return;
            }

            float totalDuration = 0f;
            for (int i = 0; i < wavesProperty.arraySize; i++)
            {
                SerializedProperty wave = wavesProperty.GetArrayElementAtIndex(i);
                SerializedProperty durationProperty = wave.FindPropertyRelative(
                    WAVE_DURATION_PROPERTY_NAME);
                if (durationProperty != null)
                {
                    totalDuration += durationProperty.floatValue;
                }
            }

            EditorGUILayout.LabelField(
                $"Wave数: {wavesProperty.arraySize} / Total Duration: {totalDuration:0.##} sec",
                EditorStyles.miniBoldLabel);
            for (int i = 0; i < Mathf.Min(wavesProperty.arraySize, elementLimit); i++)
            {
                SerializedProperty wave = wavesProperty.GetArrayElementAtIndex(i);
                SerializedProperty detailsProperty = wave.FindPropertyRelative(
                    WAVE_DETAILS_PROPERTY_NAME);
                SerializedProperty durationProperty = wave.FindPropertyRelative(
                    WAVE_DURATION_PROPERTY_NAME);
                SerializedProperty stageEffectsProperty = wave.FindPropertyRelative(
                    WAVE_STAGE_EFFECTS_PROPERTY_NAME);

                int enemyTypeCount = detailsProperty?.arraySize ?? 0;
                int spawnCount = CountEnemies(detailsProperty);
                int stageEffectCount = stageEffectsProperty?.arraySize ?? 0;
                float duration = durationProperty?.floatValue ?? 0f;
                float progress = totalDuration > 0f ? duration / totalDuration : 0f;

                EditorGUILayout.LabelField($"Wave {i + 1}", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField(
                    $"敵種類: {enemyTypeCount} / 総数: {spawnCount} / 演出: {stageEffectCount}");
                Rect rect = GUILayoutUtility.GetRect(18f, 18f, GUILayout.ExpandWidth(true));
                EditorGUI.ProgressBar(rect, progress, $"{duration:0.##} sec");

                DrawSpawnPointCandidates(target, waveDefinitionIdHash, serializedDefinition, wave, i);
            }

            if (wavesProperty.arraySize > elementLimit)
            {
                EditorGUILayout.HelpBox(
                    $"残り {wavesProperty.arraySize - elementLimit} WaveはInspector側で確認してください。",
                    MessageType.None);
            }
        }

        /// <summary>
        ///     Waveのスポーン候補地マップと編集UIを描画します。
        /// </summary>
        /// <param name="target"> Wave定義アセットです。 </param>
        /// <param name="waveDefinitionIdHash"> Wave定義アセット自体のIDハッシュです。 </param>
        /// <param name="serializedDefinition"> Wave定義アセットのSerializedObjectです。 </param>
        /// <param name="waveProperty"> 対象Waveのプロパティです。 </param>
        /// <param name="waveIndex"> 対象WaveのCollectionインデックスです。 </param>
        private static void DrawSpawnPointCandidates(
            UnityEngine.Object target,
            int waveDefinitionIdHash,
            SerializedObject serializedDefinition,
            SerializedProperty waveProperty,
            int waveIndex)
        {
            SerializedProperty candidatesProperty = waveProperty.FindPropertyRelative(
                SPAWN_POINT_CANDIDATES_PROPERTY_NAME);
            if (candidatesProperty == null || !candidatesProperty.isArray)
            {
                return;
            }

            EditorGUILayout.LabelField("スポーン候補地", EditorStyles.miniBoldLabel);

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

            if (!TryFindBattleSceneNames(waveDefinitionIdHash, out List<string> sceneNames))
            {
                EditorGUILayout.HelpBox(
                    "対応するステージシーンを特定できません。"
                    + "BattleStageAssetのEnemyWaveDefinitionIdを確認してください。",
                    MessageType.Warning);
                DrawReadOnlyCandidateList(candidatesProperty);
                return;
            }

            string resolvedSceneName;
            if (sceneNames.Count == 1)
            {
                resolvedSceneName = sceneNames[0];
            }
            else
            {
                EditorGUILayout.HelpBox(
                    $"複数のステージシーンが見つかりました（{sceneNames.Count}件）。プレビューするシーンを選択してください。",
                    MessageType.Warning);
                (int, int) selectionKey = (target.GetInstanceID(), waveIndex);
                _sceneSelectionByWave.TryGetValue(selectionKey, out int selectedIndex);
                selectedIndex = Mathf.Clamp(selectedIndex, 0, sceneNames.Count - 1);
                selectedIndex = EditorGUILayout.Popup("プレビュー対象シーン", selectedIndex, sceneNames.ToArray());
                _sceneSelectionByWave[selectionKey] = selectedIndex;
                resolvedSceneName = sceneNames[selectedIndex];
            }

            bool mapDrawn = PlannerBattleSceneMapRenderer.Draw(
                resolvedSceneName,
                currentHashes,
                spawnPoint => ToggleCandidate(serializedDefinition, target, candidatesProperty, spawnPoint));
            if (!mapDrawn)
            {
                DrawReadOnlyCandidateList(candidatesProperty);
            }
        }

        /// <summary>
        ///     スポーン候補地の選択状態をトグルし、保存します。
        /// </summary>
        /// <param name="serializedDefinition"> Wave定義アセットのSerializedObjectです。 </param>
        /// <param name="target"> Wave定義アセットです。 </param>
        /// <param name="candidatesProperty"> スポーン候補地配列プロパティです。 </param>
        /// <param name="spawnPoint"> クリックされたスポーンポイントです。 </param>
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
        ///     選択中のスポーン候補地を読み取り専用テキストで表示します。
        /// </summary>
        /// <param name="candidatesProperty"> スポーン候補地配列プロパティです。 </param>
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
        ///     指定Wave定義IDを使用しているBattleStageAssetのシーン名一覧を取得します。
        /// </summary>
        /// <param name="waveDefinitionIdHash"> Wave定義アセットのIDハッシュです。 </param>
        /// <param name="sceneNames"> 取得したシーン名一覧（重複なし、昇順）です。 </param>
        /// <returns> 1件以上見つかった場合はtrueです。 </returns>
        private static bool TryFindBattleSceneNames(int waveDefinitionIdHash, out List<string> sceneNames)
        {
            sceneNames = new List<string>();
            if (waveDefinitionIdHash == 0)
            {
                return false;
            }

            IReadOnlyList<SourceDataIDOption> stageOptions =
                SourceDataProviderRepositoryResolver.GetOptions(STAGE_ASSET_COLLECTION_KEY);
            HashSet<int> visitedStageInstanceIds = new();
            HashSet<string> distinctSceneNames = new(StringComparer.Ordinal);
            for (int i = 0; i < stageOptions.Count; i++)
            {
                UnityEngine.Object stageAsset = stageOptions[i].Source;
                if (stageAsset == null || !visitedStageInstanceIds.Add(stageAsset.GetInstanceID()))
                {
                    continue;
                }

                SerializedObject serializedStage = new(stageAsset);
                SerializedProperty stageWaveHashProperty = serializedStage
                    .FindProperty(ENEMY_WAVE_DEFINITION_ID_PROPERTY_NAME)
                    ?.FindPropertyRelative(DATA_ID_HASH_PROPERTY_NAME);
                if (stageWaveHashProperty == null || stageWaveHashProperty.intValue != waveDefinitionIdHash)
                {
                    continue;
                }

                SerializedProperty sceneNameProperty = serializedStage.FindProperty(BATTLE_SCENE_NAME_PROPERTY_NAME);
                if (sceneNameProperty != null
                    && sceneNameProperty.propertyType == SerializedPropertyType.String
                    && !string.IsNullOrEmpty(sceneNameProperty.stringValue))
                {
                    distinctSceneNames.Add(sceneNameProperty.stringValue);
                }
            }

            sceneNames.AddRange(distinctSceneNames);
            sceneNames.Sort(StringComparer.Ordinal);
            return sceneNames.Count > 0;
        }

        /// <summary>
        ///     Wave定義アセットのSerializedObjectを生成します。
        /// </summary>
        /// <param name="target"> Wave定義アセット候補です。 </param>
        /// <param name="serializedDefinition"> 生成したSerializedObjectです。 </param>
        /// <returns> Wave配列を持つScriptableObjectの場合はtrueです。 </returns>
        private static bool TryCreateSerializedDefinition(
            UnityEngine.Object target,
            out SerializedObject serializedDefinition)
        {
            serializedDefinition = null;
            if (target is not ScriptableObject definitionAsset)
            {
                return false;
            }

            serializedDefinition = new SerializedObject(definitionAsset);
            if (serializedDefinition.FindProperty(WAVES_PROPERTY_NAME) != null)
            {
                return true;
            }

            serializedDefinition.Dispose();
            serializedDefinition = null;
            return false;
        }

        /// <summary>
        ///     Wave内の敵総数を集計します。
        /// </summary>
        /// <param name="detailsProperty"> Wave詳細配列です。 </param>
        /// <returns> 敵総数です。 </returns>
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
                SerializedProperty amountProperty = detail.FindPropertyRelative(
                    WAVE_ENEMY_AMOUNT_PROPERTY_NAME);
                if (amountProperty != null)
                {
                    count += amountProperty.intValue;
                }
            }

            return count;
        }

        private const string ID_PROPERTY_NAME = "_id";
        private const string DATA_ID_VALUE_PROPERTY_NAME = "_id";
        private const string DATA_ID_HASH_PROPERTY_NAME = "_hashId";
        private const string WAVES_PROPERTY_NAME = "_waves";
        private const string LOOP_PROPERTY_NAME = "_loop";
        private const string LOOP_START_PROPERTY_NAME = "_loopStart";
        private const string WAVE_DETAILS_PROPERTY_NAME = "Details";
        private const string WAVE_DURATION_PROPERTY_NAME = "WaveDuration";
        private const string WAVE_STAGE_EFFECTS_PROPERTY_NAME = "StageEffects";
        private const string WAVE_ENEMY_AMOUNT_PROPERTY_NAME = "EnemyAmount";
        private const string SPAWN_POINT_CANDIDATES_PROPERTY_NAME = "SpawnPointCandidates";
        private const string STAGE_ASSET_COLLECTION_KEY = "StageAsset";
        private const string ENEMY_WAVE_DEFINITION_ID_PROPERTY_NAME = "_enemyWaveDefinitionId";
        private const string BATTLE_SCENE_NAME_PROPERTY_NAME = "_battleSceneName";

        private static readonly Dictionary<(int, int), int> _sceneSelectionByWave = new();
    }
}
