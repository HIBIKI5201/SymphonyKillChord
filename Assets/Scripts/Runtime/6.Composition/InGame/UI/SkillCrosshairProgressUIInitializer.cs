using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.View.InGame.Skill;
using SymphonyFrameWork.System.ServiceLocate;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.UI
{
    /// <summary>
    ///     クロスヘア上に配置するリズムコマンドUI（拍子アイコンの点灯/消灯のみを持つ専用View）を初期化するクラス。
    ///     下部の入力進捗UI（<see cref="Skill.SkillInputProgressUIInitializer"/>）とはViewを完全に分離しており、
    ///     クールダウン表現やリセット演出などクロスヘアに不要な責務を持ち込まない。
    /// </summary>
    public sealed class SkillCrosshairProgressUIInitializer : MonoBehaviour
    {
        /// <summary> クロスヘア上のView群の表示権を管理するコントローラー。 </summary>
        public SkillCrosshairProgressController Controller => _controller;

        private void Awake()
        {
            if (_uiConfig == null)
            {
                Debug.LogError($"[{nameof(SkillCrosshairProgressUIInitializer)}] {nameof(_uiConfig)} が未設定です。", this);
                return;
            }
            if (_viewPrefab == null || _stepViewPrefab == null || _viewRoots == null || _viewRoots.Length == 0)
            {
                Debug.LogError($"[{nameof(SkillCrosshairProgressUIInitializer)}] Prefabまたは配置先Transformが未設定です。", this);
                return;
            }

            _viewSetting = _uiConfig.Create();
            _controller = new SkillCrosshairProgressController();
            ServiceLocator.RegisterInstance(this, LocateTypeEnum.Locator);
            _isRegistered = true;
        }

        private void Update()
        {
            _controller?.Tick();
        }

        private void OnDestroy()
        {
            if (_isRegistered)
                ServiceLocator.UnregisterInstance(this);
        }

        /// <summary>
        ///     スキル定義データを指定し、クロスヘア上に表示するリズムコマンドUIを配置先ごとに生成する。
        /// </summary>
        /// <param name="definition"> 対象のスキル定義。 </param>
        /// <returns> 生成した全Viewを束ねたView。生成できるViewが無い場合はnull。 </returns>
        public ISkillCrosshairProgressView CreateCrosshairProgressView(SkillDefinition definition)
        {
            List<ISkillCrosshairProgressView> views = new List<ISkillCrosshairProgressView>(_viewRoots.Length);
            for (int rootIndex = 0; rootIndex < _viewRoots.Length; rootIndex++)
            {
                Transform viewRoot = _viewRoots[rootIndex];
                if (viewRoot == null)
                {
                    continue;
                }

                views.Add(CreateView(definition, viewRoot));
            }

            if (views.Count == 0)
            {
                return null;
            }

            return new SkillCrosshairProgressViewGroup(views.ToArray());
        }

        /// <summary>
        ///     指定した配置先へリズムコマンドUIを1つ生成する。
        /// </summary>
        /// <param name="definition"> 対象のスキル定義。 </param>
        /// <param name="viewRoot"> 生成先の親Transform。 </param>
        /// <returns> 生成したView。 </returns>
        private SkillCrosshairProgressView CreateView(SkillDefinition definition, Transform viewRoot)
        {
            SkillCrosshairProgressView view = Instantiate(_viewPrefab, viewRoot);
            SkillCrosshairStepView[] stepViews = new SkillCrosshairStepView[definition.SkillPattern.Signatures.Length];
            for (int i = 0; i < definition.SkillPattern.Signatures.Length; i++)
            {
                BeatType beatType = definition.SkillPattern.Signatures[i];
                SkillCrosshairStepView stepView = Instantiate(_stepViewPrefab, view.StepRoot);
                stepView.transform.SetAsLastSibling();
                SkillBeatVisualSetting setting = _viewSetting.GetSetting((int)beatType);
                stepView.Initialize(setting);
                stepViews[i] = stepView;
            }
            view.SetSteps(stepViews);
            view.SetVisible(false);
            return view;
        }

        [SerializeField, Tooltip("拍子ごとのアイコン・色設定。下部用と同じアセットを流用可（AnimationSettingは未使用）。")]
        private SkillInputProgressUIConfig _uiConfig;
        [SerializeField, Tooltip("クロスヘア専用のViewのprefab。")]
        private SkillCrosshairProgressView _viewPrefab;
        [SerializeField, Tooltip("クロスヘア専用の拍子表示Prefab。")]
        private SkillCrosshairStepView _stepViewPrefab;
        [SerializeField, Tooltip("クロスヘア上のViewを配置する親Transform（HUDEnemyHealthViewの子想定）。指定した数だけ同じ表示を生成する。")]
        private Transform[] _viewRoots;

        private SkillInputProgressViewSetting _viewSetting;
        private SkillCrosshairProgressController _controller;
        private bool _isRegistered;
    }
}
