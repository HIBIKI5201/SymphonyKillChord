using KillChord.Runtime.Adaptor.InGame.Skill;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill
{
    /// <summary>
    ///     クロスヘア（ロックオンHUD）上にリズムコマンドの拍子アイコンを表示するView。
    ///     下部の入力進行UI（RowView）とは異なり、クールダウン表現やリセット演出は持たない専用実装。
    /// </summary>
    public sealed class SkillCrosshairProgressView : MonoBehaviour, ISkillCrosshairProgressView
    {
        public event Action OnUpdate;
        /// <summary> StepViewを並べる親Transform。 </summary>
        public Transform StepRoot => _stepRoot;

        /// <summary>
        ///     生成済みStepViewを設定する。
        /// </summary>
        /// <param name="stepViews"> 設定するStepViewのリスト。 </param>
        public void SetSteps(SkillCrosshairStepView[] stepViews)
        {
            _stepViews = stepViews;
        }

        /// <inheritdoc />
        public void UpdateSteps(SkillInputProgressUpdateDTO dto)
        {
            if (_stepViews == null)
            {
                return;
            }

            if (dto.PatternMatchCount < 0 || dto.PatternMatchCount > _stepViews.Length)
            {
                Debug.LogError(
                    $"[{nameof(SkillCrosshairProgressView)}] 入力進捗とスキルの拍子パターン定義が整合していません。入力進捗：{dto.PatternMatchCount}, 拍子パターン長：{_stepViews.Length}",
                    this);
                return;
            }

            for (int i = 0; i < dto.PatternMatchCount; i++)
            {
                _stepViews[i].SetStepOn();
            }

            for (int i = dto.PatternMatchCount; i < _stepViews.Length; i++)
            {
                _stepViews[i].SetStepOff();
            }
        }

        /// <inheritdoc />
        public void SetVisible(bool visible)
        {
            if (gameObject.activeSelf != visible)
            {
                gameObject.SetActive(visible);
            }
        }
        void Update()
        {
            OnUpdate?.Invoke();
        }
        private void OnDestroy()
        {
            OnUpdate = null;
        }

        [SerializeField, Tooltip("StepViewを並べる親Transform。")]
        private Transform _stepRoot;

        private SkillCrosshairStepView[] _stepViews;
    }
}
