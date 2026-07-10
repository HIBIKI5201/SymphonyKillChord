using KillChord.Runtime.Adaptor.OutGame.SkillBuild;
using KillChord.Runtime.Application.OutGame.SkillBuild;
using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.Domain.Player;
using KillChord.Runtime.InfraStructure;
using KillChord.Runtime.InfraStructure.Addressables;
using KillChord.Runtime.InfraStructure.OutGame.SkillBuild;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.OutGame.SkillBuild;
using SymphonyFrameWork.System.ServiceLocate;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        [Tooltip("セーブデータから入手済みスキルを取得するリポジトリの Addressables キーです。")]
        private string _ownedSkillRepositoryKey;

        [SerializeField]
        [Tooltip("セーブデータから装備済みスキルを取得するリポジトリの Addressables キーです。")]
        private string _skillBuildRepositoryKey;

        [SerializeField]
        [Tooltip("スキル要素のテンプレート UXML（Skill.uxml）です。")]
        private VisualTreeAsset _skillElementTemplate;

        [Header("デバッグ用")]
        [SerializeField]
        private bool _isDebugMode = false;

        [SerializeField]
        [Tooltip("デバッグ用のスキルビルドデバッガーです。")]
        private SkillBuildDebugger _skillBuildDebugger;

        [SerializeField]
        [Tooltip("デバッグ用の入手済みスキルリポジトリの Addressables キーです。")]
        private string _ownedSkillRepositoryDebugKey;

        [SerializeField]
        [Tooltip("デバッグ用のスキルビルドリポジトリの Addressables キーです。")]
        private string _skillBuildRepositoryDebugKey;

        private SkillBuildScreenView _skillBuildScreenView;
        private SkillBuildViewModel _skillBuildViewModel;
        private SkillBuildController _skillBuildController;
        private SkillBuildDefinition _skillBuildDefinition;
        private SkillBuildPresenter _skillBuildPresenter;
        private OutGameUIEvent _outGameUIEvent;
        private SkillElementDragAndDropSetup _skillElementDragAndDropSetup;
        private OwnedSkillRepository _loadedOwnedSkillRepository;
        private SkillBuildRepository _loadedSkillBuildRepository;
        private OwnedSkillRepositoryDebug _loadedOwnedSkillRepositoryDebug;
        private SkillBuildRepositoryDebug _loadedSkillBuildRepositoryDebug;

        /// <summary>
        ///     スキル編成機能の初期化を行う。
        /// </summary>
        private async void Start()
        {
            await InitializeAsync();
        }

        /// <summary>
        ///     登録した依存関係を解放する。
        /// </summary>
        private void OnDestroy()
        {
            Unsubscribe();
            DisposeComponents();
            // SkillBuildDefinition はゲーム全体の擬似セーブデータとして存在し続けるため解除しない
            _ownedSkillRepositoryKey.ReleaseLoadedAsset(this);
            _skillBuildRepositoryKey.ReleaseLoadedAsset(this);
            _ownedSkillRepositoryDebugKey.ReleaseLoadedAsset(this);
            _skillBuildRepositoryDebugKey.ReleaseLoadedAsset(this);
            _loadedOwnedSkillRepository = null;
            _loadedSkillBuildRepository = null;
            _loadedOwnedSkillRepositoryDebug = null;
            _loadedSkillBuildRepositoryDebug = null;
        }

        /// <summary>
        ///     スキル編成機能の依存解決を行う。
        /// </summary>
        private async Task InitializeAsync()
        {
            _loadedOwnedSkillRepository = await _ownedSkillRepositoryKey.LoadAssetAsync<OwnedSkillRepository>(this, destroyCancellationToken);
            _loadedSkillBuildRepository = await _skillBuildRepositoryKey.LoadAssetAsync<SkillBuildRepository>(this, destroyCancellationToken);

            if (_isDebugMode)
            {
                _loadedOwnedSkillRepositoryDebug =
                    await _ownedSkillRepositoryDebugKey.LoadAssetAsync<OwnedSkillRepositoryDebug>(this, destroyCancellationToken);
                _loadedSkillBuildRepositoryDebug =
                    await _skillBuildRepositoryDebugKey.LoadAssetAsync<SkillBuildRepositoryDebug>(this, destroyCancellationToken);
            }

            if (_uiDocument == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(SkillBuildInitializer)}] UIDocument が設定されていません。", this);
#endif
                return;
            }

            if (!_isDebugMode && _loadedSkillBuildRepository == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(SkillBuildInitializer)}] SkillBuildRepository が設定されていません。", this);
#endif
                return;
            }

            if (!_isDebugMode && _loadedOwnedSkillRepository == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(SkillBuildInitializer)}] OwnedSkillRepository が設定されていません。", this);
#endif
                return;
            }

            if (_isDebugMode && _loadedSkillBuildRepositoryDebug == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(SkillBuildInitializer)}] SkillBuildRepositoryDebug が設定されていません。", this);
#endif
                return;
            }

            if (_isDebugMode && _loadedOwnedSkillRepositoryDebug == null)
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

            if (!ServiceLocator.TryGetInstance(out _outGameUIEvent))
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

            // 装備スキルと入手済みスキルを取得する。
            IReadOnlyList<EquippedSkill> loadedEquippedSkills = await GetEquippedSkillAsync();
            IReadOnlyList<EquippedSkill> ownedSkills = await GetOwnedSkillsAsync();
            SkillTemplate[] ownedSkillData = BuildOwnedSkills(ownedSkills);

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
            _skillBuildViewModel = new(_outGameUIEvent);
            _skillBuildPresenter = new(_skillBuildViewModel);
            _skillBuildController = new(skillBuildUseCase, _skillBuildViewModel, ownedSkillData);

            // ドラッグアンドドロップのセットアップを先に生成し、スキル要素生成コールバックとして渡す。
            // これにより、Push() で生成される要素にもマニピュレーターが即座にアタッチされる。
            _skillElementDragAndDropSetup = new SkillElementDragAndDropSetup(_uiDocument, _skillBuildViewModel);

            _skillBuildScreenView.InitializeSkillList(_skillElementTemplate, _skillElementDragAndDropSetup.SetupDraggable);
            _skillBuildScreenView.Bind(_skillBuildViewModel);

            _skillBuildPresenter.Push(_skillBuildDefinition.EquippedSkills, ownedSkillData);
            Subscribe();

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
        ///     入手済みスキル一覧を取得する。
        ///     デバッグモードの場合はデバッグ用リポジトリから取得する。
        /// </summary>
        /// <returns> 入手済みスキル一覧。 </returns>
        private async ValueTask<IReadOnlyList<EquippedSkill>> GetOwnedSkillsAsync()
        {
            if (!_isDebugMode)
            {
                return await _loadedOwnedSkillRepository.LoadOwnedSkillsAsync();
            }

            return await _loadedOwnedSkillRepositoryDebug.GetOwnedSkills();
        }

        /// <summary>
        ///    装備済みスキル一覧を取得する。
        ///    デバッグモードの場合はデバッグ用リポジトリから取得する。
        /// </summary>
        /// <returns></returns>
        private async ValueTask<IReadOnlyList<EquippedSkill>> GetEquippedSkillAsync()
        {
            if (!_isDebugMode)
            {
                return await _loadedSkillBuildRepository.GetEquippedSkills();
            }

            return await _loadedSkillBuildRepositoryDebug.GetEquippedSkills();
        }

        /// <summary>
        ///     入手済みスキル一覧を再取得して画面へ反映する。
        /// </summary>
        private async void RefreshOwnedSkills()
        {
            try
            {
                IReadOnlyList<EquippedSkill> ownedSkills = await GetOwnedSkillsAsync();
                SkillTemplate[] ownedSkillData = BuildOwnedSkills(ownedSkills);
                _skillBuildController?.UpdateOwnedSkills(ownedSkillData);
                _skillBuildPresenter?.Push(_skillBuildDefinition.EquippedSkills, ownedSkillData);
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"[{nameof(SkillBuildInitializer)}] 入手済みスキル一覧の更新に失敗しました: {exception}", this);
            }
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
            _skillBuildPresenter = null;
        }

        /// <summary>
        ///     イベントを購読する。
        /// </summary>
        private void Subscribe()
        {
            _outGameUIEvent.OnOwnedSkillChanged += HandleOwnedSkillChangedHandler;
            _outGameUIEvent.OnShownSkillBuildScreen += HandleShownSkillBuildScreenHandler;
        }

        /// <summary>
        ///     イベント購読を解除する。
        /// </summary>
        private void Unsubscribe()
        {
            if (_outGameUIEvent == null)
            {
                return;
            }

            _outGameUIEvent.OnOwnedSkillChanged -= HandleOwnedSkillChangedHandler;
            _outGameUIEvent.OnShownSkillBuildScreen -= HandleShownSkillBuildScreenHandler;
        }

        /// <summary>
        ///     入手済みスキル更新イベントを処理する。
        /// </summary>
        private void HandleOwnedSkillChangedHandler()
        {
            RefreshOwnedSkills();
        }

        /// <summary>
        ///     改造画面表示イベントを処理する。
        /// </summary>
        private void HandleShownSkillBuildScreenHandler()
        {
            RefreshOwnedSkills();
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

