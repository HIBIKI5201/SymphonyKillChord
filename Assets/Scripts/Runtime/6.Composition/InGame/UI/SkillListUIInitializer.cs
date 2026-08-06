using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.View.InGame.Skill;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.UI
{
    /// <summary>
    ///     装備中スキルをコマンド全拍付きの一覧として表示するUIを初期化するクラス。
    ///     リズムGUI下部の <see cref="Skill.SkillInputProgressUIInitializer"/>（次の1拍のみ表示）や
    ///     クロスヘア用UIとは独立した、常時表示の一覧UIを担当する。
    /// </summary>
    public sealed class SkillListUIInitializer : MonoBehaviour
    {
        private void Awake()
        {
            if (_uiConfig == null)
            {
                Debug.LogError($"[{nameof(SkillListUIInitializer)}] {nameof(_uiConfig)} が未設定です。", this);
                return;
            }
            if (_rowViewPrefab == null || _stepViewPrefab == null || _rowRoot == null)
            {
                Debug.LogError($"[{nameof(SkillListUIInitializer)}] Prefabまたは配置先Transformが未設定です。", this);
                return;
            }

            _viewSetting = _uiConfig.Create();
            ServiceLocator.RegisterInstance(this, LocateType.Locator);
            _isRegistered = true;
        }

        private void OnDestroy()
        {
            if (_isRegistered)
                ServiceLocator.UnregisterInstance(this);
        }

        /// <summary>
        ///     スキル定義データを指定し、一覧へ表示する1スキル分の行を生成する。
        /// </summary>
        /// <param name="definition"> 対象のスキル定義。 </param>
        /// <returns> 生成した行View。 </returns>
        public ISkillInputProgressRowView CreateSkillListRow(SkillDefinition definition)
        {
            SkillListRowView rowView = Instantiate(_rowViewPrefab, _rowRoot);
            rowView.Initialize(_viewSetting.AnimationSetting);

            SkillListStepView[] stepViews = new SkillListStepView[definition.SkillPattern.Signatures.Length];
            for (int i = 0; i < definition.SkillPattern.Signatures.Length; i++)
            {
                BeatType beatType = definition.SkillPattern.Signatures[i];
                SkillListStepView stepView = Instantiate(_stepViewPrefab, rowView.StepRoot);
                stepView.transform.SetAsLastSibling();
                SkillBeatVisualSetting setting = _viewSetting.GetSetting((int)beatType);
                stepView.Initialize(setting, _viewSetting.AnimationSetting);
                stepViews[i] = stepView;
            }

            rowView.SetSteps(stepViews);
            return rowView;
        }

        [SerializeField, Tooltip("拍子ごとのアイコン・色設定。他UIと同じアセットを流用可。")]
        private SkillInputProgressUIConfig _uiConfig;
        [SerializeField, Tooltip("1スキル分の行Prefab。")]
        private SkillListRowView _rowViewPrefab;
        [SerializeField, Tooltip("コマンド1拍分のPrefab。")]
        private SkillListStepView _stepViewPrefab;
        [SerializeField, Tooltip("行を並べる親Transform。")]
        private Transform _rowRoot;

        private SkillInputProgressViewSetting _viewSetting;
        private bool _isRegistered;
    }
}
