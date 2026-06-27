using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.InfraStructure.InGame.Skill;
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
            if (_skillInputProgressViewConfigAsset == null)
            {
                Debug.LogError("スキル入力進行UIの表示設定が未設定です。");
                return;
            }
            _inputProgressViewConfig = _skillInputProgressViewConfigAsset.Create();
            ServiceLocator.RegisterInstance(this, LocateType.Locator);
        }

        private void OnDestroy()
        {
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
            SkillInputProgressStepView[] stepViews = new SkillInputProgressStepView[definition.SkillPattern.Signatures.Length];
            for (int i = 0; i < definition.SkillPattern.Signatures.Length; i++)
            {
                BeatType beatType = definition.SkillPattern.Signatures[i];
                SkillInputProgressStepView stepView = Instantiate(_stepViewPrefab, rowView.StepRoot);
                stepView.transform.SetAsLastSibling();
                SkillBeatVisualSetting setting = _inputProgressViewConfig.GetSetting((int)beatType);
                stepView.Initialize(setting);
                stepViews[i] = stepView;
            }
            rowView.SetSteps(stepViews);
            return rowView;
        }

        [SerializeField, Tooltip("スキル入力進行UIの設定情報Asset")]
        private SkillInputProgressViewConfigAsset _skillInputProgressViewConfigAsset;
        [SerializeField, Tooltip("行のprefab")]
        private SkillInputProgressRowView _rowViewPrefab;
        [SerializeField, Tooltip("入力拍子のprefab")]
        private SkillInputProgressStepView _stepViewPrefab;
        [SerializeField, Tooltip("RowViewを並べる親Transform。")]
        private Transform _rowRoot;

        private SkillInputProgressViewconfig _inputProgressViewConfig;
    }
}
