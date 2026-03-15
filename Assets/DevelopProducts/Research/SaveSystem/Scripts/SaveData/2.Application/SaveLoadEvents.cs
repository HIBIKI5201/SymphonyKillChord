using System;
namespace Research.SaveSystem
{
    /// <summary>
    ///     セーブとロードの開始終了イベントを管理するクラス。
    /// </summary>
    public class SaveLoadEvents
    {
        /// <summary>セーブ開始時に発火するイベント</summary>
        public event Action OnSaveStart;
        /// <summary>セーブ終了時に発火するイベント</summary>
        public event Action OnSaveEnd;
        /// <summary>ロード開始時に発火するイベント</summary>
        public event Action OnLoadStart;
        /// <summary>ロード終了時に発火するイベント</summary>
        public event Action OnLoadEnd;
        /// <summary>
        ///     セーブ開始時のイベントを発火する。
        /// </summary>
        public void InvokeSaveStart()
        {
            OnSaveStart?.Invoke();
        }

        /// <summary>
        ///     セーブ終了時のイベントを発火する。
        /// </summary>
        public void InvokeSaveEnd()
        {
            OnSaveEnd?.Invoke();
        }

        /// <summary>
        ///     ロード開始時のイベントを発火する。
        /// </summary>
        public void InvokeLoadStart()
        {
            OnLoadStart?.Invoke();
        }

        /// <summary>
        ///     ロード終了時のイベントを発火する。
        /// </summary>
        public void InvokeLoadEnd()
        {
            OnLoadEnd?.Invoke();
        }
    }
}
