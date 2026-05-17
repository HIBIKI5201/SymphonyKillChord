using KillChord.Runtime.Adaptor.InGame.UI;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     敵のHP表示関連View。
    /// </summary>
    public class EnemyHealthView : MonoBehaviour
    {
        public void Initialize(IHealthHudPresenter presenter)
        {
            _presenter = presenter;
        }

        private void OnDestroy()
        {
            _presenter?.Dispose();
        }

        private IHealthHudPresenter _presenter;
    }
}
