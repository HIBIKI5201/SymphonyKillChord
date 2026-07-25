using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.View.InGame.Music;
using KillChord.Runtime.View.InGame.Skill;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Skill
{
    /// <summary>
    ///     リズムGUI下部のスキル入力進行UIを初期化するクラス。
    /// </summary>
    public class SkillInputProgressUIInitializer : MonoBehaviour
    {
        /// <summary> 全スキルのコマンド表示を横断的に制御するコントローラー。 </summary>
        public SkillGuideProgressController GuideProgressController => _guideProgressController;

        private void Awake()
        {
            if (_skillInputProgressUIConfig == null)
            {
                Debug.LogError("スキル入力進行UIの表示設定が未設定です。");
                return;
            }
            _inputProgressViewSetting = _skillInputProgressUIConfig.Create();
            ServiceLocator.RegisterInstance(this, LocateType.Locator);
            _isRegistered = true;
        }

        private void OnDestroy()
        {
            if (_isRegistered)
                ServiceLocator.UnregisterInstance(this);
        }

        /// <summary>
        ///     スキル定義データを指定し、画面に表示する入力進捗UIを生成する。
        /// </summary>
        /// <param name="definition"></param>
        /// <returns></returns>
        public ISkillInputProgressRowView CreateInputProgressRow(SkillDefinition definition)
        {
            SkillGuideProgressView view = Instantiate(_viewPrefab, _viewRoot);
            SkillBeatVisualSetting[] stepSettings = new SkillBeatVisualSetting[definition.SkillPattern.Signatures.Length];
            for (int i = 0; i < definition.SkillPattern.Signatures.Length; i++)
            {
                BeatType beatType = definition.SkillPattern.Signatures[i];
                stepSettings[i] = _inputProgressViewSetting.GetSetting((int)beatType);
            }
            view.Initialize(stepSettings, _inputProgressViewSetting.AnimationSetting, _rhythmGuideView);
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

        private SkillInputProgressViewSetting _inputProgressViewSetting;
        private readonly SkillGuideProgressController _guideProgressController = new();
        private bool _isRegistered;
    }
}
