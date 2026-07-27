using KillChord.Runtime.Adaptor.OutGame.Skill;
using KillChord.Runtime.Adaptor.OutGame.SkillBuild;
using KillChord.Runtime.Application.OutGame.SkillBuild;
using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.Domain.Player;
using KillChord.Runtime.InfraStructure;
using KillChord.Runtime.InfraStructure.Addressables;
using KillChord.Runtime.InfraStructure.OutGame.SkillBuild;
using KillChord.Runtime.Utility.Identity;
using KillChord.Runtime.Utility.OutGame.Savedata;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.OutGame.SkillBuild;
using SymphonyFrameWork.System.ServiceLocate;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.Composition.OutGame.SkillBuild
{
    /// <summary>
    ///     改造画面の依存解決と初期化を行うクラス。
    /// </summary>
    public sealed class SkillBuildInitializer : OutGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(SkillBuildInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 130;

        private const string SKILL_BUILD_CONTAINER_NAME = "SkillBuildContainer";

        [SerializeField]
        [Tooltip("UI Document コンポーネント。")]
        private UIDocument _uiDocument;

        [SerializeField, SourceDataAddress]
        [Tooltip("セーブデータから入手済みスキルを取得するリポジトリの Addressables キーです。")]
        private string _ownedSkillRepositoryKey;

        [SerializeField, SourceDataAddress]
        [Tooltip("セーブデータから装備済みスキルを取得するリポジトリの Addressables キーです。")]
        private string _skillBuildRepositoryKey;

        [SerializeField]
        [Tooltip("スキル要素のテンプレート UXML（Skill.uxml）です。")]
        private VisualTreeAsset _skillElementTemplate;

        private SkillBuildScreenView _skillBuildScreenView;
        private SkillBuildViewModel _skillBuildViewModel;
        private SkillBuildController _skillBuildController;
        private SkillBuildDefinition _skillBuildDefinition;
        private SkillBuildPresenter _skillBuildPresenter;
        private OutGameUIEvent _outGameUIEvent;
        private SkillElementDragAndDropSetup _skillElementDragAndDropSetup;
        private OwnedSkillRepository _loadedOwnedSkillRepository;
        private SkillBuildRepository _loadedSkillBuildRepository;
        private IReadOnlyList<EquippedSkill> _loadedEquippedSkills;
        private SkillTemplate[] _loadedOwnedSkillTemplates;
        private int _loadedOwnedPoints;
        private SavedataSystem _savedataSystem;
        private bool _isInitialized;
        private bool _isSubscribed;

        /// <summary>
        ///     非同期のリソースロードを行います。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        public override async Awaitable<bool> ResourceLoadAsync(CancellationToken cancellationToken)
        {
            _loadedOwnedSkillRepository = await _ownedSkillRepositoryKey.LoadAssetAsync<OwnedSkillRepository>(this, destroyCancellationToken);
            _loadedSkillBuildRepository = await _skillBuildRepositoryKey.LoadAssetAsync<SkillBuildRepository>(this, destroyCancellationToken);

            if (!ValidateLoadedRepositories())
            {
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out SavedataSystem savedataSystem))
            {
                Debug.LogError($"[{nameof(SkillBuildInitializer)}] SavedataSystem が取得できませんでした。", this);
                return false;
            }

            _savedataSystem = savedataSystem;
            _loadedOwnedSkillRepository.Initialize(_savedataSystem);
            _loadedSkillBuildRepository.Initialize(_savedataSystem);

            _loadedEquippedSkills = await GetEquippedSkillsAsync();
            IReadOnlyList<EquippedSkill> ownedSkills = await GetOwnedSkillsAsync();
            _loadedOwnedSkillTemplates = BuildOwnedSkills(ownedSkills);
            _loadedOwnedPoints = await GetOwnedPointsAsync();

            return _loadedEquippedSkills != null && _loadedOwnedSkillTemplates != null;
        }

        /// <summary>
        ///     システムを構築します。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            return Initialize();
        }

        /// <summary>
        ///     他モジュールとの結合を行います。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Ready()
        {
            Subscribe();
            return _isInitialized;
        }

        /// <summary>
        ///     登録済みサービスやイベント購読を解除します。
        /// </summary>
        public override void Shutdown()
        {
            Unsubscribe();
            DisposeComponents();

            _ownedSkillRepositoryKey.ReleaseLoadedAsset(this);
            _skillBuildRepositoryKey.ReleaseLoadedAsset(this);
            _loadedOwnedSkillRepository = null;
            _loadedSkillBuildRepository = null;
            _loadedEquippedSkills = null;
            _loadedOwnedSkillTemplates = null;
            _loadedOwnedPoints = 0;
            _savedataSystem = null;
            _outGameUIEvent = null;
            _isInitialized = false;
            _isSubscribed = false;
        }

        /// <summary>
        ///     スキル編成機能の依存解決を行います。
        /// </summary>
        /// <returns> 初期化に成功した場合はtrue。 </returns>
        private bool Initialize()
        {
            if (_uiDocument == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(SkillBuildInitializer)}] UIDocument が設定されていません。", this);
#endif
                return false;
            }

            if (_skillElementTemplate == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(SkillBuildInitializer)}] SkillElementTemplate が設定されていません。", this);
#endif
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out _outGameUIEvent))
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(SkillBuildInitializer)}] OutGameUIEvent が取得できませんでした。", this);
#endif
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out _skillBuildScreenView))
            {
                _skillBuildScreenView = CreateSkillBuildScreenView();
                if (_skillBuildScreenView == null)
                {
#if UNITY_EDITOR
                    Debug.LogError($"[{nameof(SkillBuildInitializer)}] SkillBuildScreenView が取得できませんでした。", this);
#endif
                    return false;
                }
            }

            if (!ServiceLocator.TryGetInstance(out _skillBuildDefinition))
            {
                _skillBuildDefinition = new SkillBuildDefinition(ToArray(_loadedEquippedSkills));
                ServiceLocator.RegisterInstance(_skillBuildDefinition);
            }
            else
            {
                _skillBuildDefinition.EnsureSlotCount(
                    _loadedEquippedSkills.Count > SkillBuildDefinition.INITIAL_SLOT_COUNT
                        ? _loadedEquippedSkills.Count
                        : SkillBuildDefinition.INITIAL_SLOT_COUNT);
            }

            SkillBuildUseCase skillBuildUseCase =
                new(_skillBuildDefinition, _loadedSkillBuildRepository);
            _skillBuildController = new(skillBuildUseCase, _loadedOwnedSkillTemplates);
            _skillBuildViewModel = new(_skillBuildController);
            SkillDisplayTextFormatter textFormatter =
                new(new SkillEffectDescriptionFormatter());
            _skillBuildPresenter = new(_skillBuildViewModel, textFormatter);

            _skillElementDragAndDropSetup = new SkillElementDragAndDropSetup(_uiDocument, _skillBuildViewModel);

            _skillBuildScreenView.InitializeSkillList(_skillElementTemplate, _skillElementDragAndDropSetup.SetupDraggable);
            _skillBuildScreenView.Bind(_skillBuildViewModel);
            _skillBuildPresenter.Push(
                _skillBuildDefinition.EquippedSkills,
                _loadedOwnedSkillTemplates,
                _loadedOwnedPoints);

            _isInitialized = true;
            return true;
        }

        /// <summary>
        ///     ロード済みリポジトリの妥当性を検証します。
        /// </summary>
        /// <returns> 有効な場合はtrue。 </returns>
        private bool ValidateLoadedRepositories()
        {
            if (_loadedSkillBuildRepository == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(SkillBuildInitializer)}] SkillBuildRepository が設定されていません。", this);
#endif
                return false;
            }

            if (_loadedOwnedSkillRepository == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(SkillBuildInitializer)}] OwnedSkillRepository が設定されていません。", this);
#endif
                return false;
            }

            return true;
        }

        /// <summary>
        ///     初期表示用の入手済みスキル一覧を構築します。
        /// </summary>
        /// <param name="ownedSkills"> 現在所持しているスキル一覧。 </param>
        /// <returns> 入手済みスキル一覧です。 </returns>
        private SkillTemplate[] BuildOwnedSkills(IReadOnlyList<EquippedSkill> ownedSkills)
        {
            SkillTemplate[] result = new SkillTemplate[ownedSkills.Count];

            for (int i = 0; i < ownedSkills.Count; i++)
            {
                result[i] = ownedSkills[i].SkillTemplate;
            }

            return result;
        }

        /// <summary>
        ///     入手済みスキル一覧を取得します。
        /// </summary>
        /// <returns> 入手済みスキル一覧です。 </returns>
        private async ValueTask<IReadOnlyList<EquippedSkill>> GetOwnedSkillsAsync()
        {
            return await _loadedOwnedSkillRepository.LoadOwnedSkillsAsync();
        }

        /// <summary>
        ///     装備済みスキル一覧を取得します。
        /// </summary>
        /// <returns> 装備済みスキル一覧です。 </returns>
        private async ValueTask<IReadOnlyList<EquippedSkill>> GetEquippedSkillsAsync()
        {
            return await _loadedSkillBuildRepository.GetEquippedSkills();
        }

        /// <summary>
        ///     所持ポイントを取得します。
        ///     <para> スキルのレベルアップに使用できるポイントです。 </para>
        /// </summary>
        /// <returns> 現在の所持ポイントです。 </returns>
        private async ValueTask<int> GetOwnedPointsAsync()
        {
            SaveData saveData = await _savedataSystem.LoadAsync<SaveData>();
            return saveData.SkillBuild.SkillLevelupPoint;
        }

        /// <summary>
        ///     入手済みスキル一覧を再取得して画面へ反映します。
        /// </summary>
        /// <param name="resetsDetailToDefault"> 更新後に装備スロット1を既定表示する場合は true。 </param>
        private async void RefreshOwnedSkills(bool resetsDetailToDefault)
        {
            try
            {
                IReadOnlyList<EquippedSkill> ownedSkills = await GetOwnedSkillsAsync();
                IReadOnlyList<EquippedSkill> equippedSkills = await _loadedSkillBuildRepository.LoadSkillBuild();
                int ownedPoints = await GetOwnedPointsAsync();
                SkillTemplate[] ownedSkillData = BuildOwnedSkills(ownedSkills);
                _skillBuildDefinition.UpdateEquippedSkills(ToArray(equippedSkills));
                _skillBuildController?.UpdateOwnedSkills(ownedSkillData);
                _skillBuildPresenter?.Push(
                    _skillBuildDefinition.EquippedSkills,
                    ownedSkillData,
                    ownedPoints);
                if (resetsDetailToDefault)
                {
                    _skillBuildViewModel?.ResetDetailToDefault();
                }
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"[{nameof(SkillBuildInitializer)}] 入手済みスキル一覧の更新に失敗しました: {exception}", this);
            }
        }

        /// <summary>
        ///     読み取り専用一覧を配列へ変換します。
        /// </summary>
        /// <param name="equippedSkills"> 変換元のスキル一覧。 </param>
        /// <returns> 配列化したスキル一覧です。 </returns>
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
        ///     改造画面の View をその場で生成します。
        /// </summary>
        /// <returns> 生成した View。生成できない場合は null です。 </returns>
        private SkillBuildScreenView CreateSkillBuildScreenView()
        {
            VisualElement skillBuildRoot = _uiDocument.rootVisualElement.Q<VisualElement>(SKILL_BUILD_CONTAINER_NAME);
            if (skillBuildRoot == null)
            {
                return null;
            }

            return new SkillBuildScreenView(skillBuildRoot, _outGameUIEvent);
        }

        /// <summary>
        ///     生成したコンポーネントを解放します。
        /// </summary>
        private void DisposeComponents()
        {
            _skillBuildController = null;

            if (_skillBuildScreenView != null)
            {
                _skillBuildScreenView.Unbind();
                _skillBuildScreenView = null;
            }

            _skillBuildViewModel?.Dispose();
            _skillBuildViewModel = null;
            _skillBuildPresenter = null;
            _skillElementDragAndDropSetup = null;
        }

        /// <summary>
        ///     イベントを購読します。
        /// </summary>
        private void Subscribe()
        {
            if (!_isInitialized || _outGameUIEvent == null || _isSubscribed)
            {
                return;
            }

            _outGameUIEvent.OnOwnedSkillChanged += HandleOwnedSkillChangedHandler;
            _outGameUIEvent.OnShownSkillBuildScreen += HandleShownSkillBuildScreenHandler;
            _isSubscribed = true;
        }

        /// <summary>
        ///     イベント購読を解除します。
        /// </summary>
        private void Unsubscribe()
        {
            if (_outGameUIEvent == null || !_isSubscribed)
            {
                return;
            }

            _outGameUIEvent.OnOwnedSkillChanged -= HandleOwnedSkillChangedHandler;
            _outGameUIEvent.OnShownSkillBuildScreen -= HandleShownSkillBuildScreenHandler;
            _isSubscribed = false;
        }

        /// <summary>
        ///     入手済みスキル更新イベントを処理します。
        /// </summary>
        private void HandleOwnedSkillChangedHandler()
        {
            RefreshOwnedSkills(false);
        }

        /// <summary>
        ///     改造画面表示イベントを処理します。
        /// </summary>
        private void HandleShownSkillBuildScreenHandler()
        {
            RefreshOwnedSkills(true);
        }
    }
}
