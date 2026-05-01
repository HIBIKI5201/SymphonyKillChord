using KillChord.Runtime.Domain.OutGame.Screen;
using System.Collections.Generic;

namespace KillChord.Runtime.Application.OutGame.Screen      
{
    /// <summary>
    ///     画面遷移状態を保持するクラス。
    /// </summary>
    public sealed class ScreenTransitionState
    {
        /// <summary> 現在画面 ID を取得します。 </summary>
        public ScreenId? CurrentScreenId { get; private set; }

        /// <summary>
        ///     指定画面へ遷移します。
        /// </summary>
        public void MoveTo(ScreenId nextScreenId, bool keepInHistory)
        {
            if (keepInHistory && CurrentScreenId.HasValue)
            {
                _history.Push(CurrentScreenId.Value);
            }

            CurrentScreenId = nextScreenId;
        }

        /// <summary>
        ///     戻り先画面の取得を試みます。
        /// </summary>
        public bool TryGoBack(out ScreenId previousScreenId)
        {
            if (_history.Count == 0)
            {
                previousScreenId = default;
                return false;
            }

            previousScreenId = _history.Pop();
            CurrentScreenId = previousScreenId;
            return true;
        }

        /// <summary>
        ///     履歴をクリアし、ルート画面へ戻します。
        /// </summary>
        public void Reset(ScreenId rootScreenId)
        {
            _history.Clear();
            CurrentScreenId = rootScreenId;
        }

        private readonly Stack<ScreenId> _history = new();
    }
}
