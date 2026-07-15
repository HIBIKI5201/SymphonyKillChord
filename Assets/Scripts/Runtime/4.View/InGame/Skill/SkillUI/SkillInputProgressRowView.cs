using KillChord.Runtime.Adaptor.InGame.Skill;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View.InGame.Skill
{
    /// <summary>
    ///     スキル入力の進捗を表示する行のViewクラス。
    /// </summary>
    public class SkillInputProgressRowView : MonoBehaviour, ISkillInputProgressRowView
    {
        /// <summary> StepViewを並べる親Transform。 </summary>
        public Transform StepRoot => _stepRoot;

        /// <summary>
        ///     生成済みStepViewを設定する。
        /// </summary>
        /// <param name="stepViews"> 設定するStepViewのリスト。 </param>
        public void SetSteps(SkillInputProgressStepView[] stepViews)
        {
            _stepViews = stepViews;
        }

        /// <summary>
        ///     行内の拍子アイコンの状態を更新する。
        /// </summary>
        /// <param name="dto"></param>
        public void UpdateSteps(SkillInputProgressUpdateDTO dto)
        {
            if (dto.PatternMatchCount < 0 || dto.PatternMatchCount > _stepViews.Length)
            {
                Debug.LogError($"入力進捗とスキルの拍子パターン定義が整合していません。入力進捗：{dto.PatternMatchCount}, 拍子パターン長：{_stepViews.Length}");
                return;
            }

            // マッチした部分を光るようにする
            for (int i = 0; i < dto.PatternMatchCount; i++)
            {
                _stepViews[i].SetStepOn();
            }

            // マッチしていない部分を光らないようにする
            for (int i = dto.PatternMatchCount; i < _stepViews.Length; i++)
            {
                _stepViews[i].SetStepOff();
            }

            // スキル発動した時
            if (dto.SkillTriggeredFlg)
            {
                ProcessSkillTriggered(dto);
            }
        }

        private void FixedUpdate()
        {
            if (_isSkillCoolingDown)
            {
                float cooldownDuration = _skillReadyTimestamp - _skillTriggeredTimestamp;
                if (cooldownDuration <= 0)
                {
                    _backgroundImage.fillAmount = 1;
                    ProcessSkillReady();
                    return;
                }

                float backgroundFillAmount = (Time.unscaledTime - _skillTriggeredTimestamp)
                    / cooldownDuration;

                if (backgroundFillAmount >= 1)
                {
                    _backgroundImage.fillAmount = 1;
                    ProcessSkillReady();
                }
                else
                {
                    _backgroundImage.fillAmount = backgroundFillAmount;
                }
            }
        }

        [SerializeField, Tooltip("StepViewを並べる親Transform。")]
        private Transform _stepRoot;
        [SerializeField, Tooltip("クールダウンを表現するための背景。")]
        private Image _backgroundImage;

        private SkillInputProgressStepView[] _stepViews;
        private bool _isSkillCoolingDown;
        private float _skillTriggeredTimestamp;
        private float _skillReadyTimestamp;

        /// <summary>
        ///     スキル発動時の処理。
        /// </summary>
        /// <param name="dto"></param>
        private void ProcessSkillTriggered(SkillInputProgressUpdateDTO dto)
        {
            // TODO UI状態更新や、エフェクトなど
            _isSkillCoolingDown = true;
            _skillTriggeredTimestamp = dto.CurrentTimestamp;
            _skillReadyTimestamp = dto.SkillReadyTimestamp;
            _backgroundImage.fillAmount = 0;
        }

        /// <summary>
        ///     スキルクールダウン終了時の処理。
        /// </summary>
        private void ProcessSkillReady()
        {
            // TODO UI状態更新や、エフェクトなど
            _isSkillCoolingDown = false;
        }
    }
}
