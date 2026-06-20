using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace DevelopProducts.Pause
{
    /// <summary>
    ///     全体の時間管理を行うクラス
    /// </summary>
    public class TimeScaleManager
    {
        /// <summary>
        ///     ITimeScalableを継承しているオブジェクトを登録するメソッド
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defaultScale"></param>
        public void Register(ITimeScalable obj, float defaultScale)
        {
            if (_timeScalableData.ContainsKey(obj))
                return;

            var handler = new TimeScaleHandler(baseScale: defaultScale);
            var data = new TimeScaleData(handler: handler);

            _timeScalableData[obj] = handler;
            obj.Inject(data: data);
        }
        /// <summary>
        ///     登録解除
        /// </summary>
        /// <param name="obj"></param>
        public void Unregister(ITimeScalable obj)
        {
            _timeScalableData.Remove(obj);
        }
        /// <summary>
        ///    登録済みの全オブジェクトを列挙する
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ITimeScalable> GetAll() => _timeScalableData.Keys;
        #region　タイプ
        /// <summary>
        ///     指定タイプのオブジェクトをポーズする
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public PauseToken PauseByType(TimeScaleType type)
        {
            var tokens = _timeScalableData
                .Where(kvp => kvp.Key.TimeScaleType.HasFlag(flag: type))
                .Select(kvp => kvp.Value.Pause())
                .ToList();

            // 全オブジェクトのトークンをまとめたトークン
            // ポーズの一括解除が行える
            return new PauseToken(() => tokens.ForEach(token => token.Dispose()));
        }
        /// <summary>
        ///     指定タイプのオブジェクトにスケールを適応する
        /// </summary>
        /// <param name="type"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public void SetScaleByType(TimeScaleType type, float scale)
        {
            foreach (var (key, value) in _timeScalableData.Where(kvp => kvp.Key.TimeScaleType.HasFlag(type)))
            {
                value.SetScale(scale: scale);
            }
        }
        #endregion

        #region 個別
        /// <summary>
        ///     指定されたIDのオブジェクトをポーズさせる
        /// </summary>
        /// <returns></returns>
        public PauseToken PauseById(int instanceId)
        {
            var entry =  _timeScalableData.First(kvp => kvp.Key.InstanceId == instanceId);

            return entry.Value != null ? entry.Value.Pause() : new PauseToken(() => { }); 
        }
        /// <summary>
        ///     指定されたIDのオブジェクトにスケールを積む
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public void SetScaleById(int instanceId, float scale)
        {
            var obj = _timeScalableData.FirstOrDefault(kvp => kvp.Key.InstanceId == instanceId);
            if (obj.Value == null)
                return;

            obj.Value.SetScale(scale: scale);
        }
        #endregion

        #region 全体
        /// <summary>
        ///     全体をポーズさせる
        /// </summary>
        /// <returns></returns>
        public PauseToken PauseAll()
        {
            var tokens = _timeScalableData.Values.Select(h => h.Pause()).ToList();

            return new PauseToken(() => tokens.ForEach(token => token.Dispose()));
        }
        /// <summary>
        ///     全体に同じタイムスケールを積む
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        public void SelectScaleAll(float scale)
        {
            foreach (var value in _timeScalableData.Values)
            {
                value.SetScale(scale: scale);
            }
        }
        #endregion
        private Dictionary<ITimeScalable, TimeScaleHandler> _timeScalableData = new();
    }
}
