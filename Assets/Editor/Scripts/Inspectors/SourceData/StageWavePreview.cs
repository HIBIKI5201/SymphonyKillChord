using KillChord.Editor.SourceDataProvider.Core;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.Inspectors.SourceData
{
    /// <summary>
    ///     ステージの所属ツリーと同じvariantから参照Waveのマップを表示します。
    /// </summary>
    internal static class StageWavePreview
    {
        /// <summary>
        ///     参照Waveを一意に解決し、シーンマップとWaveを開く導線を描画します。
        /// </summary>
        public static void Draw(ScriptableObject stage, ScriptableObject tree)
        {
            // ステージが参照している Wave の ID を取得する。
            using SerializedObject serializedStage = new(stage);
            string waveId = serializedStage.FindProperty(WAVE_ID_PROPERTY)?.FindPropertyRelative(ID_PROPERTY)?.stringValue;
            if (string.IsNullOrWhiteSpace(waveId))
            {
                EditorGUILayout.HelpBox("参照するWaveが未設定です。", MessageType.Info);
                return;
            }
            HashSet<ScriptableObject> waves = new();
            if (!SourceDataProviderSettings.instance.TryGetCollectionMapping(WAVE_COLLECTION, out var mapping))
            {
                EditorGUILayout.HelpBox("Wave Collectionが未登録です。", MessageType.Warning);
                return;
            }
            // 所属するツリーと同じ種別の Wave リポジトリから、ID が一致する Wave を探す。
            foreach (GameDataVariant variant in Enum.GetValues(typeof(GameDataVariant)))
            {
                if (!SourceDataProviderRepositoryResolver.TryResolveAsset(TREE_KEY, variant, out ScriptableObject candidateTree, out _, out _)
                    || candidateTree != tree
                    || !SourceDataProviderRepositoryResolver.TryResolveAsset(mapping.SourceAssetAddressableKey, variant,
                        out ScriptableObject repository, out _, out _)) { continue; }
                using SerializedObject serializedRepository = new(repository);
                SerializedProperty array = serializedRepository.FindProperty(mapping.PropertyPath);
                if (array == null || !array.isArray) { continue; }
                for (int i = 0; i < array.arraySize; i++)
                {
                    if (array.GetArrayElementAtIndex(i).objectReferenceValue is ScriptableObject wave
                        && SourceCollectionElementDisplay.GetAuthoringId(wave, WAVE_COLLECTION) == waveId)
                    {
                        waves.Add(wave);
                    }
                }
            }
            // 一意に決まった Wave について、開くボタンとスポーン位置のマップを表示する。
            if (waves.Count != 1)
            {
                EditorGUILayout.HelpBox($"所属ツリーの参照Wave「{waveId}」を一意に解決できません（{waves.Count}件）。", MessageType.Warning);
                return;
            }
            foreach (ScriptableObject wave in waves)
            {
                if (GUILayout.Button($"Waveを開く: {wave.name}"))
                {
                    Selection.activeObject = wave;
                    EditorGUIUtility.PingObject(wave);
                    GUIUtility.ExitGUI();
                }
                using SerializedObject serializedWave = new(wave);
                string sceneName = serializedWave.FindProperty(SCENE_PROPERTY)?.stringValue;
                if (string.IsNullOrWhiteSpace(sceneName))
                {
                    EditorGUILayout.HelpBox("参照WaveのBattle Scene Nameが未設定です。", MessageType.Warning);
                }
                else { BattleSceneMapRenderer.Draw(sceneName); }
            }
        }

        private const string WAVE_ID_PROPERTY = "_enemyWaveDefinitionId";
        private const string ID_PROPERTY = "_id";
        private const string WAVE_COLLECTION = "Wave";
        private const string TREE_KEY = "StageTreeAsset";
        private const string SCENE_PROPERTY = "_battleSceneName";
    }
}
