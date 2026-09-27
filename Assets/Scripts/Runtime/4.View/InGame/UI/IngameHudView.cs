using R3;
using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View.InGame.UI
{
    /// <summary>
    ///     ゲーム中のHUD表示を行うViewクラス。
    /// </summary>
    public class IngameHudView : MonoBehaviour
    {
        [SerializeField, Tooltip("HP バーの Image。")] private Image _healthBarImage;

        private IngameHudViewModel _viewModel;
        
        /// <summary>
        ///     ViewModel を設定し、HP の割合の変化を購読する。
        /// </summary>
        public void Bind(IngameHudViewModel viewModel)
        {
            _viewModel = viewModel;

            _viewModel.HealthRate.Subscribe(ChangeHitPoint).RegisterTo(destroyCancellationToken);
        }

        /// <summary>
        ///     HP バーの長さを HP の割合に合わせる。
        /// </summary>
        private void ChangeHitPoint(float fillAmount)
        {
            _healthBarImage.fillAmount = Mathf.Clamp(fillAmount, 0f, 1f);
        }
    }
}