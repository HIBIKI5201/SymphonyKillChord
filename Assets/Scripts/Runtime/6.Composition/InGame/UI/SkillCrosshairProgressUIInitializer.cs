using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Composition.InGame.Bootstrap;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.View.InGame.Skill;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.UI
{
    /// <summary>
    ///     クロスヘア上に配置するリズムコマンドUI（拍子アイコンの点灯/消灯のみを持つ専用View）を初期化するクラス。
    ///     下部の入力進捗UI（<see cref="Skill.SkillInputProgressUIInitializer"/>）とはViewを完全に分離しており、
    ///     クールダウン表現やリセット演出などクロスヘアに不要な責務を持ち込まない。
    /// </summary>
    public sealed class SkillCrosshairProgressUIInitializer : InGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(SkillCrosshairProgressUIInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 440;

        /// <summary> クロスヘア上のView群の表示権を管理するコントローラー。 </summary>
        public SkillCrosshairProgressController Controller => _controller;

        /// <summary>
        ///     クロスヘア用のViewSettingとControllerを生成し、サービスとして登録します。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            if (_uiConfig == null)
            {
                Debug.LogError($"[{nameof(SkillCrosshairProgressUIInitializer)}] {nameof(_uiConfig)} が未設定です。", this);
                return false;
            }
            if (_viewPrefab == null || _stepViewPrefab == null || _viewRoot == null)
            {
                Debug.LogError($"[{nameof(SkillCrosshairProgressUIInitializer)}] Prefabまたは配置先Transformが未設定です。", this);
                return false;
            }

            _viewSetting = _uiConfig.Create();
            _controller = new SkillCrosshairProgressController();
            ServiceLocator.RegisterInstance(this, LocateTypeEnum.Locator);
            _isRegistered = true;
            return true;
        }

        /// <summary>
        ///     登録済みサービスを解除します。
        /// </summary>
        public override void Shutdown()
        {
            if (_isRegistered)
            {
                ServiceLocator.UnregisterInstance(this);
            }

            if (_progressView is not null)
            {
                _progressView.OnUpdate -= _controller.Tick;
            }
        }

        /// <summary>
        ///     スキル定義データを指定し、クロスヘア上に表示するリズムコマンドUIを配置先ごとに生成する。
        /// </summary>
        /// <param name="definition"> 対象のスキル定義。 </param>
        /// <returns> 生成した全Viewを束ねたView。生成できるViewが無い場合はnull。 </returns>
        public ISkillCrosshairProgressView CreateCrosshairProgressView(SkillDefinition definition)
        {
            if (_viewRoot == null)
                return null;

            SkillCrosshairProgressView view = Instantiate(_viewPrefab, _viewRoot);
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


            if (_progressView is null)
            {
                _progressView = view;
                _progressView.OnUpdate += _controller.Tick;
            }


            return view;
        }

        [SerializeField, Tooltip("拍子ごとのアイコン・色設定。下部用と同じアセットを流用可（AnimationSettingは未使用）。")]
        private SkillInputProgressUIConfig _uiConfig;
        [SerializeField, Tooltip("クロスヘア専用のViewのprefab。")]
        private SkillCrosshairProgressView _viewPrefab;
        [SerializeField, Tooltip("クロスヘア専用の拍子表示Prefab。")]
        private SkillCrosshairStepView _stepViewPrefab;
        [SerializeField, Tooltip("クロスヘア上のViewを配置する親Transform（HUDEnemyHealthViewの子想定）。指定した数だけ同じ表示を生成する。")]
        private Transform _viewRoot;

        private SkillInputProgressViewSetting _viewSetting;
        private SkillCrosshairProgressController _controller;
        private SkillCrosshairProgressView _progressView = null;
        private bool _isRegistered;
    }
}
