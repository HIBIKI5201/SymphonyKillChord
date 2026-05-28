using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View.InGame.Skill
{
    /// <summary>
    ///     スキル入力進行UIの拍子ごとの表示を管理するクラス。
    /// </summary>
    public class SkillInputProgressStepView : MonoBehaviour
    {
        /// <summary>
        ///     表示データを反映する。
        /// </summary>
        public void Apply(in SkillInputProgressStepData data)
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.color = data.Color;
            }

            if (_iconImage != null)
            {
                _iconImage.sprite = data.Icon;
                _iconImage.enabled = data.Icon != null;
            }

            if (_beatText != null)
            {
                _beatText.text = data.BeatType.ToString();
                _beatText.enabled = data.Icon == null;
            }

            if (_activeEffect != null)
            {
                _activeEffect.SetActive(data.IsActive);
            }
        }

        [SerializeField, Tooltip(" 背景色を反映するImage。 ")]
        private Image _backgroundImage;

        [SerializeField, Tooltip(" アイコンを表示するImage。 ")]
        private Image _iconImage;

        // 色で十分だったから使わなくてもいいかも。
        [SerializeField, Tooltip(" アイコン未設定時に拍子番号を表示するText。 ")]
        private TMP_Text _beatText;

        [SerializeField, Tooltip(" 入力済み時に表示する発光用オブジェクト。 ")]
        private GameObject _activeEffect;
    }
}
