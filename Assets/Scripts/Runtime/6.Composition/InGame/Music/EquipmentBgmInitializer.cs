using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Composition.InGame.Bootstrap;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.InfraStructure.InGame.Music;
using KillChord.Runtime.View.Persistent.Music;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Music
{
    /// <summary>
    ///     装備スキル構成に応じたBGMループの初期化を行うクラス。
    ///     BGMの演出機能であり、失敗してもゲーム本体の初期化は止めない。
    /// </summary>
    public class EquipmentBgmInitializer : InGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(EquipmentBgmInitializer);

        /// <summary> 実行順です。MusicSyncInitializer(200)の後に実行する。 </summary>
        public override int Order => 250;

        /// <summary> 装備BGMサービス。 </summary>
        public EquipmentBgmService EquipmentBgmService { get; private set; }

        /// <summary> 装備BGMコントローラー。 </summary>
        public EquipmentBgmController EquipmentBgmController { get; private set; }

        /// <summary>
        ///     装備スキルからシーケンスを構築し、Containerを登録する。
        /// </summary>
        /// <returns> 常にtrue。BGM演出のためゲーム初期化は中断しない。 </returns>
        public override bool Build()
        {
            // 依存や設定の欠落で早期returnする場合でも読み込み結果を確認できるよう、
            // 装備スキルの収集とログ出力を最初に行う。
            IReadOnlyList<int> equippedSkillIds = CollectEquippedSkillIds(out string skillSource);
            LogEquippedSkills(equippedSkillIds, skillSource);

            if (_labelTable == null)
            {
                Debug.LogWarning(
                    $"[{ModuleName}] {nameof(_labelTable)} が設定されていないため、通常BGMで再生します。",
                    this);
                return true;
            }

            if (!TryResolveDependencies(out MusicPlayer musicPlayer, out MusicSyncState musicSyncState))
            {
                return true;
            }

            SkillBgmSelectorTable table = _labelTable.ToDomain();
            if (!table.HasOriginalLabel)
            {
                Debug.LogWarning(
                    $"[{ModuleName}] 対応表の原曲ラベルが未設定です。原曲の位置に空ラベルが送られるため、" +
                    $"{_labelTable.name} を確認してください。",
                    this);
            }

            BgmSelectorSequence sequence = table.CreateSequence(
                equippedSkillIds,
                out IReadOnlyList<int> unmappedSkillIds);

            if (unmappedSkillIds.Count > 0)
            {
                // 対応が無いスキルは並びから除外されるため、装備数と小節構成が食い違う。
                // 設定漏れに気付けるよう明示的に警告する。
                Debug.LogWarning(
                    $"[{ModuleName}] セレクターラベルが未登録のスキルがあります。" +
                    $"該当スキルは切り替え対象から除外されます。対象: [{DescribeSkills(unmappedSkillIds)}]",
                    this);
            }

            EquipmentBgmService = new EquipmentBgmService(sequence, _measuresPerDivision);

            if (!EquipmentBgmService.HasSequence)
            {
                // 装備スキルが無い場合はセレクターを操作せず、
                // CRIのデフォルト＝通常BGMをそのまま再生する正常系。
                Debug.Log($"[{ModuleName}] 切り替え対象のスキルが無いため、通常BGMで再生します。", this);
                EquipmentBgmService = null;
                return true;
            }

            _originalLabel = table.OriginalLabel;
            LogSequence(sequence);

            Action<int, string> onLabelApplied =
                _logSelectorSwitch ? new Action<int, string>(LogAppliedLabel) : null;
            EquipmentBgmController = new EquipmentBgmController(
                musicSyncState,
                EquipmentBgmService,
                musicPlayer,
                SELECTOR_NAME,
                onLabelApplied);

            _moduleContainer = new EquipmentBgmModuleContainer(EquipmentBgmController, EquipmentBgmService);
            ServiceLocator.RegisterInstance(_moduleContainer);
            _isModuleRegistered = true;

            return true;
        }

        /// <summary>
        ///     初期ラベルを適用し、拍ストリームの購読を開始する。
        /// </summary>
        /// <returns> 常にtrue。 </returns>
        public override bool Ready()
        {
            if (EquipmentBgmController == null)
            {
                return true;
            }

            EquipmentBgmController.Start();
            Debug.Log(
                $"<color=yellow>[{ModuleName}] BGMループ開始 → 原曲 '{EquipmentBgmService.InitialLabel}'</color>",
                this);
            return true;
        }

        /// <summary>
        ///     InGame開始時に読み込まれた装備スキル構成をConsoleへ出力する。
        /// </summary>
        /// <param name="equippedSkillIds"> 装備スキルのID列。 </param>
        /// <param name="skillSource"> 装備スキルの取得元。 </param>
        private void LogEquippedSkills(IReadOnlyList<int> equippedSkillIds, string skillSource)
        {
            if (equippedSkillIds.Count == 0)
            {
                Debug.Log(
                    $"<color=yellow>[{ModuleName}] InGame開始時の装備スキル: なし（取得元: {skillSource}）</color>",
                    this);
                return;
            }

            List<string> descriptions = new(equippedSkillIds.Count);
            for (int i = 0; i < equippedSkillIds.Count; i++)
            {
                descriptions.Add($"slot{i}: {DescribeSkill(equippedSkillIds[i])}");
            }

            Debug.Log(
                $"<color=yellow>[{ModuleName}] InGame開始時の装備スキル {equippedSkillIds.Count}個" +
                $"（取得元: {skillSource}） [{string.Join(" / ", descriptions)}]</color>",
                this);
        }

        /// <summary>
        ///     構築したシーケンスの並びをConsoleへ出力する。
        /// </summary>
        /// <param name="sequence"> 構築したシーケンス。 </param>
        private void LogSequence(BgmSelectorSequence sequence)
        {
            string[] labels = new string[sequence.Length];
            for (int i = 0; i < sequence.Length; i++)
            {
                labels[i] = sequence.ResolveLabel(i);
            }

            Debug.Log(
                $"<color=yellow>[{ModuleName}] BGMシーケンス構築: {sequence.Length}区切り" +
                $"（1区切り={_measuresPerDivision}小節） [{string.Join(", ", labels)}]</color>",
                this);
        }

        /// <summary>
        ///     切り替わったセレクターラベルをConsoleへ出力する。
        /// </summary>
        /// <param name="measureIndex"> 切り替わった時点の小節番号。 </param>
        /// <param name="label"> 適用されたセレクターラベル。 </param>
        private void LogAppliedLabel(int measureIndex, string label)
        {
            string kind = label == _originalLabel ? "原曲" : "スキル";
            int division = measureIndex / _measuresPerDivision;

            Debug.Log(
                $"<color=yellow>[{ModuleName}] セレクター切替 → {kind} '{label}' " +
                $"(小節={measureIndex}, 区切り={division})</color>",
                this);
        }

        /// <summary>
        ///     購読を解除し、登録済みContainerを解除する。
        /// </summary>
        public override void Shutdown()
        {
            EquipmentBgmController?.Dispose();
            EquipmentBgmController = null;
            EquipmentBgmService = null;

            if (!_isModuleRegistered)
            {
                return;
            }

            ServiceLocator.UnregisterInstance<EquipmentBgmModuleContainer>();
            _moduleContainer = null;
            _isModuleRegistered = false;
        }

        /// <summary>
        ///     依存オブジェクトの解決を試みる。
        /// </summary>
        /// <param name="musicPlayer"> 音楽プレイヤー。 </param>
        /// <param name="musicSyncState"> 音楽同期状態。 </param>
        /// <returns> すべて解決できた場合はtrue。 </returns>
        private bool TryResolveDependencies(out MusicPlayer musicPlayer, out MusicSyncState musicSyncState)
        {
            musicSyncState = null;

            if (!ServiceLocator.TryGetInstance(out musicPlayer))
            {
                Debug.LogWarning($"[{ModuleName}] {nameof(MusicPlayer)} が未登録のため、BGM切り替えを行いません。", this);
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out musicSyncState))
            {
                Debug.LogWarning($"[{ModuleName}] {nameof(MusicSyncState)} が未登録のため、BGM切り替えを行いません。", this);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     装備スキル構成からスキルID列（スロット順）を収集する。
        ///     SkillBuildDefinitionはOutGameで登録されるため、Title→InGame直行等では
        ///     未登録の場合がある。その場合は装備なしとして扱う。
        /// </summary>
        /// <param name="skillSource"> 装備スキルの取得元を表す文字列。 </param>
        /// <returns> 装備中スキルのID列。 </returns>
        private IReadOnlyList<int> CollectEquippedSkillIds(out string skillSource)
        {
            if (!ServiceLocator.TryGetInstance(out SkillBuildDefinition skillBuild)
                || skillBuild?.EquippedSkills == null)
            {
                skillSource = $"{nameof(SkillBuildDefinition)}未登録のため装備なし扱い";
                return Array.Empty<int>();
            }

            skillSource = nameof(SkillBuildDefinition);
            _skillDisplayNames.Clear();

            List<int> skillIds = new(skillBuild.EquippedSkills.Count);
            foreach (EquippedSkill equippedSkill in skillBuild.EquippedSkills)
            {
                if (!equippedSkill.HasSkill)
                {
                    continue;
                }

                int skillId = equippedSkill.SkillTemplate.Id.Value;
                skillIds.Add(skillId);
                _skillDisplayNames[skillId] = equippedSkill.SkillTemplate.DisplayName;
            }
            // スキルIDを昇順で並べ替えて返す
            return skillIds.OrderBy(x => x).ToList();
        }

        /// <summary>
        ///     スキルIDをログ用の表示名へ変換する。
        ///     表示名が未設定の場合は数値IDにフォールバックする。
        /// </summary>
        /// <param name="skillId"> スキルID。 </param>
        /// <returns> ログ表示用の文字列。 </returns>
        private string DescribeSkill(int skillId)
        {
            if (_skillDisplayNames.TryGetValue(skillId, out string displayName)
                && !string.IsNullOrEmpty(displayName))
            {
                return $"スキル{displayName}";
            }

            return $"id={skillId}";
        }

        /// <summary>
        ///     スキルID列をログ用の表示名へ変換して連結する。
        /// </summary>
        /// <param name="skillIds"> スキルID列。 </param>
        /// <returns> ログ表示用の文字列。 </returns>
        private string DescribeSkills(IReadOnlyList<int> skillIds)
        {
            string[] descriptions = new string[skillIds.Count];
            for (int i = 0; i < skillIds.Count; i++)
            {
                descriptions[i] = DescribeSkill(skillIds[i]);
            }

            return string.Join(", ", descriptions);
        }

        [SerializeField, Tooltip("スキルIDとCRIセレクターラベル名の対応表です。")]
        private BgmSelectorLabelTableAsset _labelTable;
        [SerializeField, Min(1), Tooltip("セレクターを切り替える1区切りの小節数です。")]
        private int _measuresPerDivision = DEFAULT_MEASURES_PER_DIVISION;
        [SerializeField, Tooltip("切り替えのたびに適用したラベルをConsoleへ出力します（動作確認用）。")]
        private bool _logSelectorSwitch = true;

        /// <summary> CRIのセレクター名。ACBの定義と完全一致させる必要がある。 </summary>
        private const string SELECTOR_NAME = "Selector_BGM";
        /// <summary> 1区切りの小節数の初期値。 </summary>
        private const int DEFAULT_MEASURES_PER_DIVISION = 4;

        private EquipmentBgmModuleContainer _moduleContainer;
        private bool _isModuleRegistered;
        private string _originalLabel;

        /// <summary> スキルIDとログ表示用の名前の対応。 </summary>
        private readonly Dictionary<int, string> _skillDisplayNames = new();
    }
}
