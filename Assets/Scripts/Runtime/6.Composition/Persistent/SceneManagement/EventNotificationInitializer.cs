using KillChord.Runtime.Composition.Persistent.Bootstrap;
using KillChord.Runtime.View.Persistent.Load;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.Persistent.SceneManagement
{
    /// <summary>
    ///     常駐通知Viewを入力初期化前に公開し、常駐終了時に解除します。
    /// </summary>
    public sealed class EventNotificationInitializer : PersistentInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(EventNotificationInitializer);

        /// <summary> 入力モジュールより前に通知を登録します。 </summary>
        public override int Order => 40;

        /// <summary>
        ///     表示の必須参照を確認し、通知を公開します。
        /// </summary>
        public override bool Build()
        {
            if (_view == null)
            {
                Debug.LogError($"[{nameof(EventNotificationInitializer)}] 常駐通知Viewが未設定です。", this);
                return false;
            }
            if (!_view.Initialize()) { return false; }
            return ServiceLocator.RegisterInstance(_view);
        }

        /// <summary>
        ///     表示を取り消し、自身が登録したViewだけを解除します。
        /// </summary>
        public override void Shutdown()
        {
            if (_view != null) { _view.Cancel(); }
            if (ServiceLocator.TryGetInstance(out EventNotificationView registered)
                && ReferenceEquals(registered, _view))
            {
                ServiceLocator.UnregisterInstance<EventNotificationView>();
            }
        }

        [SerializeField, Tooltip("シーンをまたいで中央通知を表示するViewです。")]
        private EventNotificationView _view;
    }
}
