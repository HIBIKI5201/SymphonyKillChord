using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.View.InGame.Skill;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Skill
{
    /// <summary>
    ///     スキル入力進行UIを初期化するクラス。
    /// </summary>
    public class SkillInputProgressUIInitializer : MonoBehaviour
    {
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
        public SkillInputProgressRowView CreateInputProgressRow(SkillDefinition definition)
        {
            SkillInputProgressRowView rowView = Instantiate(_rowViewPrefab, _rowRoot);
            rowView.Initialize(_inputProgressViewSetting.AnimationSetting);
            SkillInputProgressStepView[] stepViews = new SkillInputProgressStepView[definition.SkillPattern.Signatures.Length];
            for (int i = 0; i < definition.SkillPattern.Signatures.Length; i++)
            {
                BeatType beatType = definition.SkillPattern.Signatures[i];
                SkillInputProgressStepView stepView = Instantiate(_stepViewPrefab, rowView.StepRoot);
                stepView.transform.SetAsLastSibling();
                SkillBeatVisualSetting setting = _inputProgressViewSetting.GetSetting((int)beatType);
                stepView.Initialize(setting, _inputProgressViewSetting.AnimationSetting);
                stepViews[i] = stepView;
            }
            rowView.SetSteps(stepViews);
            return rowView;
        }

        [SerializeField, Tooltip("スキル入力進行UIの表示設定。")]
        private SkillInputProgressUIConfig _skillInputProgressUIConfig;
        [SerializeField, Tooltip("行のprefab")]
        private SkillInputProgressRowView _rowViewPrefab;
        [SerializeField, Tooltip("入力拍子のprefab")]
        private SkillInputProgressStepView _stepViewPrefab;
        [SerializeField, Tooltip("RowViewを並べる親Transform。")]
        private Transform _rowRoot;

        private SkillInputProgressViewSetting _inputProgressViewSetting;
        private bool _isRegistered;
    }
}
