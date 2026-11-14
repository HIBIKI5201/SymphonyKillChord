using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Composition.InGame.Bootstrap;
using KillChord.Runtime.Composition.InGame.Sequence;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Persistent.Experiment;
using KillChord.Runtime.InfraStructure.Persistent.Experiment;
using KillChord.Runtime.View.InGame.Music;
using KillChord.Runtime.View.InGame.Skill;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Skill
{
    /// <summary>
    ///     リズムGUI下部のスキル入力進行UIを初期化するクラス。
    /// </summary>
    [AddComponentMenu("KillChord/Composition/UI/SkillInputProgressUIInitializer")]
    public class SkillInputProgressUIInitializer : InGameInitializationModuleBase
    {
        /// <summary> 全スキルのコマンド表示を横断的に制御するコントローラー。 </summary>
        public SkillGuideProgressController GuideProgressController => _guideProgressController;

        /// <inheritdoc/>
        public override string ModuleName => nameof(SkillInputProgressUIInitializer);

        /// <inheritdoc/>
        public override int Order => 442;

        /// <inheritdoc/>
        public override bool Build()
        {
            if (_skillInputProgressUIConfig == null)
            {
                Debug.LogError($"[{nameof(SkillInputProgressUIInitializer)}] スキル入力進行UIの表示設定が未設定です。", this);
                return false;
            }
            if (_viewPrefab == null || _viewRoot == null || _rhythmGuideView == null)
            {
                Debug.LogError($"[{nameof(SkillInputProgressUIInitializer)}] View Prefab・親Transform・リズムガイドViewのいずれかが未設定です。", this);
                return false;
            }

            _inputProgressViewSetting = _skillInputProgressUIConfig.Create();

            // 試遊の案：スキルゲージをリズムゲージの上に置き、アイコンを大きくして目立たせる。
            _isEmphasisEnabled = new GaugeGazeExperimentSettingsStore().Load()
                .IsEnabled(GaugeGazeExperimentFeature.SkillGaugeEmphasis);
            if (_isEmphasisEnabled)
            {
                SkillGuideProgressLayout.MoveAboveGauge(_viewRoot, _rhythmGuideView.transform, _emphasisGap);
            }
            ServiceLocator.RegisterInstance(this, LocateTypeEnum.Locator);
            _isRegistered = true;

            return true;
        }

        /// <inheritdoc/>
        public override void Shutdown()
        {
            if (_isRegistered)
                ServiceLocator.UnregisterInstance(this);
        }

        /// <summary>
        ///     スキル定義データを指定し、画面に表示する入力進捗UIを生成する。
        /// </summary>
        /// <param name="definition"> 表示対象のスキル定義です。 </param>
        /// <param name="skillIcon"> 表示対象のスキルアイコンです。未設定の場合はnull。 </param>
        /// <returns> 生成した入力進捗UIの行Viewです。 </returns>
        public ISkillInputProgressRowView CreateInputProgressRow(SkillDefinition definition, Sprite skillIcon)
        {
            SkillGuideProgressView view = Instantiate(_viewPrefab, _viewRoot);
            if (_isEmphasisEnabled)
            {
                view.MultiplyIconScale(_emphasisIconScale);
            }
            view.SetTimeProvider(BattlePlayTimeProvider.Create());
            SkillBeatVisualSetting[] stepSettings = new SkillBeatVisualSetting[definition.SkillPattern.Signatures.Length];
            for (int i = 0; i < definition.SkillPattern.Signatures.Length; i++)
            {
                BeatType beatType = definition.SkillPattern.Signatures[i];
                stepSettings[i] = _inputProgressViewSetting.GetSetting((int)beatType);
            }
            view.Initialize(stepSettings, _inputProgressViewSetting.AnimationSetting, _rhythmGuideView, skillIcon);
            _guideProgressController.Register(view);
            return view;
        }

        [SerializeField, Tooltip("スキル入力進行UIの表示設定。")]
        private SkillInputProgressUIConfig _skillInputProgressUIConfig;
        [SerializeField, Tooltip("1アイコン＋クールダウンゲージのView Prefab。")]
        private SkillGuideProgressView _viewPrefab;
        [SerializeField, Tooltip("Viewを配置する親Transform。")]
        private Transform _viewRoot;
        [SerializeField, Tooltip("アイコンのX座標をジャストタイミング位置に合わせるためのリズムガイドView。")]
        private ACLikeRhythmGuideView _rhythmGuideView;
        [SerializeField, Min(1f), Tooltip("試遊の案（スキルゲージを目立たせる）で、拍子アイコンに掛ける大きさの倍率。")]
        private float _emphasisIconScale = 1.5f;
        [SerializeField, Tooltip("試遊の案（スキルゲージをリズムゲージの上に置く）で、ゲージとの間に足す余白。")]
        private float _emphasisGap = 8f;

        private SkillInputProgressViewSetting _inputProgressViewSetting;
        private readonly SkillGuideProgressController _guideProgressController = new();
        private bool _isRegistered;
        private bool _isEmphasisEnabled;
    }
}
