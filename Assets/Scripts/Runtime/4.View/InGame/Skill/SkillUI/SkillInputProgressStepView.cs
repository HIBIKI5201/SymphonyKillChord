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
        ///     初期化処理。
        /// </summary>
        /// <param name="data"></param>
        public void Initialize(in SkillBeatVisualSetting data)
        {
            if(_iconImage != null)
            {
                _iconImage.sprite = data.Icon;
            }
            _onColor = data.ActiveColor;
            _offColor = data.NormalColor;
            SetStepOff();
        }

        /// <summary>
        ///     入力済みにする。
        /// </summary>
        public void SetStepOn()
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.color = _onColor;
            }
        }

        /// <summary>
        ///     未入力にする。
        /// </summary>
        public void SetStepOff()
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.color = _offColor;
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

        private Color _onColor; // 入力済み時の色
        private Color _offColor; // 未入力時の色
    }
}
