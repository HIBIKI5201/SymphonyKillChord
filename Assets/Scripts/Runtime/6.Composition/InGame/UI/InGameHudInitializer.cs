using KillChord.Runtime.Adaptor.InGame.UI;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.View.InGame.UI;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.UI
{
    /// <summary>
    ///     InGameのHUDを初期化するクラス。
    /// </summary>
    public class InGameHudInitializer : MonoBehaviour
    {
        /// <summary>
        ///     HPバーHUDの初期化。
        /// </summary>
        /// <param name="entity"></param>
        public void InitializeHpHud(CharacterEntity entity)
        {
            HealthHudViewModel hudViewModel = new HealthHudViewModel(entity.CurrentHealth.Value, entity.MaxHealth.Value);
            _healthHudView.Bind(hudViewModel);
        }

        [SerializeField] private HealthHudView _healthHudView;
    }
}
