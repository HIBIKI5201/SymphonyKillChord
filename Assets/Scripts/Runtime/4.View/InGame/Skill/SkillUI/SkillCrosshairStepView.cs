using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View.InGame.Skill
{
    /// <summary>
    ///     クロスヘア上のリズムコマンドUIの拍子ごとの表示を管理するクラス。
    ///     下部の入力進行UIと異なり、クールダウン表現やアニメーションは持たず、点灯/消灯のみを扱う。
    /// </summary>
    public sealed class SkillCrosshairStepView : MonoBehaviour
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="data"> 拍子ごとの表示設定。 </param>
        public void Initialize(in SkillBeatVisualSetting data)
        {
            if (_iconImage != null)
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

        [SerializeField, Tooltip("背景色を反映するImage。")]
        private Image _backgroundImage;

        [SerializeField, Tooltip("アイコンを表示するImage。")]
        private Image _iconImage;

        private Color _onColor;
        private Color _offColor;
    }
}
