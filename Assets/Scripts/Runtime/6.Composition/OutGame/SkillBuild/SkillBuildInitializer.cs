using KillChord.Runtime.Adaptor.OutGame.SkillBuild;
using KillChord.Runtime.Application.OutGame.SkillBuild;
using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.Domain.Player;
using KillChord.Runtime.InfraStructure;
using KillChord.Runtime.InfraStructure.OutGame.SkillBuild;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.OutGame.SkillBuild;
using SymphonyFrameWork.System.ServiceLocate;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.Composition.OutGame.SkillBuild
{
    /// <summary>
    ///     改造画面の依存解決と初期化を行うクラス。
    /// </summary>
    public sealed class SkillBuildInitializer : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("UI Document コンポーネント。")]
        private UIDocument _uiDocument;

        [SerializeField]
        [Tooltip("デバッグ用のスキルビルドリポジトリです。")]
        private SkillBuildRepositoryDebug _skillBuildRepositoryDebug;

        [SerializeField]
        [Tooltip("デバッグ用の入手済みスキルリポジトリです。")]
        private OwnedSkillRepositoryDebug _ownedSkillRepositoryDebug;

        [SerializeField]
        [Tooltip("スキル要素のテンプレート UXML（Skill.uxml）です。")]
        private VisualTreeAsset _skillElementTemplate;

        [SerializeField]
        [Tooltip("デバッグ用のスキルビルドデバッガーです。")]
        private SkillBuildDebugger _skillBuildDebugger;

        private SkillBuildScreenView _skillBuildScreenView;
        private SkillBuildViewModel _skillBuildViewModel;
        private SkillBuildController _skillBuildController;
        private SkillBuildDefinition _skillBuildDefinition;

        /// <summary>
        ///     スキル編成機能の初期化を行う。
        /// </summary>
        private void Start()
        {
            Initialize();
        }

        /// <summary>
        ///     登録した依存関係を解放する。
        /// </summary>
        private void OnDestroy()
        {
            DisposeComponents();
            // SkillBuildDefinition はゲーム全体の擬似セーブデータとして存在し続けるため解除しない
        }

        /// <summary>
        ///     スキル編成機能の依存解決を行う。
        /// </summary>
        private void Initialize()
        {
            if (_uiDocument == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(SkillBuildInitializer)}] UIDocument が設定されていません。", this);
#endif
                return;
            }

            if (_skillBuildRepositoryDebug == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(SkillBuildInitializer)}] SkillBuildRepositoryDebug が設定されていません。", this);
#endif
                return;
            }

            if (_ownedSkillRepositoryDebug == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(SkillBuildInitializer)}] OwnedSkillRepositoryDebug が設定されていません。", this);
#endif
                return;
            }

            if (_skillElementTemplate == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(SkillBuildInitializer)}] SkillElementTemplate が設定されていません。", this);
#endif
                return;
            }

            if (!ServiceLocator.TryGetInstance(out OutGameUIEvent outGameUIEvent))
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(SkillBuildInitializer)}] OutGameUIEvent が取得できませんでした。", this);
#endif
                return;
            }

            if (!ServiceLocator.TryGetInstance(out _skillBuildScreenView))
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(SkillBuildInitializer)}] SkillBuildScreenView が取得できませんでした。", this);
#endif
                return;
            }

            // デバッグ用のリポジトリから装備スキルと入手済みスキルを取得する。
            // TODO : セーブシステムが実装されたら、本番用のリポジトリを介して取得するように変更する。
            IReadOnlyList<EquippedSkill> loadedEquippedSkills = _skillBuildRepositoryDebug.GetEquippedSkills();
            IReadOnlyList<EquippedSkill> ownedSkills = _ownedSkillRepositoryDebug.GetOwnedSkills();
            SkillData[] ownedSkillData = BuildOwnedSkills(ownedSkills);

            if (!ServiceLocator.TryGetInstance(out _skillBuildDefinition))
            {
                _skillBuildDefinition = new SkillBuildDefinition(ToArray(loadedEquippedSkills));
                ServiceLocator.RegisterInstance(_skillBuildDefinition);
            }
            else
            {
                _skillBuildDefinition.EnsureSlotCount(
                    loadedEquippedSkills.Count > SkillBuildDefinition.INITIAL_SLOT_COUNT
                        ? loadedEquippedSkills.Count
                        : SkillBuildDefinition.INITIAL_SLOT_COUNT);
            }

            SkillBuildUseCase skillBuildUseCase = new(_skillBuildDefinition);
            _skillBuildViewModel = new(outGameUIEvent);
            SkillBuildPresenter skillBuildPresenter = new(_skillBuildViewModel);
            _skillBuildController = new(skillBuildUseCase, _skillBuildViewModel, ownedSkillData);

            _skillBuildScreenView.InitializeSkillList(_skillElementTemplate);
            _skillBuildScreenView.Bind(_skillBuildViewModel);

            skillBuildPresenter.Push(_skillBuildDefinition.EquippedSkills, ownedSkillData);

            SkillElementDragAndDropSetup skillElementDragAndDropSetup = new(_uiDocument, _skillBuildViewModel);

#if UNITY_EDITOR
            if (_skillBuildDebugger != null)
            {
                _skillBuildDebugger.Initialize(_skillBuildDefinition);
            }
#endif
        }

        /// <summary>
        ///     初期表示用の入手済みスキル一覧を構築する。
        /// </summary>
        /// <param name="ownedSkills"> 現在所持しているスキル一覧。 </param>
        /// <returns> 入手済みスキル一覧。 </returns>
        private SkillData[] BuildOwnedSkills(IReadOnlyList<EquippedSkill> ownedSkills)
        {
            SkillData[] result = new SkillData[ownedSkills.Count];

            for (int i = 0; i < ownedSkills.Count; i++)
            {
                result[i] = ownedSkills[i].SkillData;
            }

            return result;
        }

        /// <summary>
        ///     読み取り専用一覧を配列へ変換する。
        /// </summary>
        /// <param name="equippedSkills"> 変換元のスキル一覧。 </param>
        /// <returns> 配列化したスキル一覧。 </returns>
        private EquippedSkill[] ToArray(IReadOnlyList<EquippedSkill> equippedSkills)
        {
            EquippedSkill[] result = new EquippedSkill[equippedSkills.Count];

            for (int i = 0; i < equippedSkills.Count; i++)
            {
                result[i] = equippedSkills[i];
            }

            return result;
        }

        /// <summary>
        ///     生成したコンポーネントを解放する。
        /// </summary>
        private void DisposeComponents()
        {
            _skillBuildController?.Dispose();
            _skillBuildController = null;

            if (_skillBuildScreenView != null)
            {
                _skillBuildScreenView.Unbind();
                _skillBuildScreenView = null;
            }

            _skillBuildViewModel?.Dispose();
            _skillBuildViewModel = null;
        }

        /// <summary>
        ///     ServiceLocator へ登録したサービスを解除する。
        /// </summary>
        private void UnregisterServices()
        {
            ServiceLocator.UnregisterInstance<SkillBuildDefinition>();
        }
    }
}
