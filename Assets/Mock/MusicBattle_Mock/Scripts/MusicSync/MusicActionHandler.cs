using SymphonyFrameWork.Attribute;
using System;
using System.Collections.Generic;
using System.Text;
using Mock.MusicBattle.Utility;
using UnityEngine;

namespace Mock.MusicBattle.MusicSync
{
    /// <summary>
    ///     アクションの予約や発火を管理するクラス。
    /// </summary>
    [DisallowMultipleComponent]
    public class MusicActionHandler : MonoBehaviour
    {
        #region Publicメソッド
        /// <summary>
        /// 予約アクションを登録する。
        /// </summary>
        /// <param name="barTimingInfo">小節タイミング</param>
        /// <param name="action">実行するアクション</param>
        /// <param name="cancelToken">キャンセルトークン</param>
        public void RegisterAction(BarTimingInfo barTimingInfo, Action action)
        {
            _debugLog.Clear();
            _debugLog.AppendLine("アクション予約受付た。");
            _debugLog.AppendLine($"小節フラグ：{barTimingInfo.BarFlg}, 拍子スケール：{barTimingInfo.TimeSignature}, 拍数：{barTimingInfo.TargetBeat}");
            _debugLog.AppendLine($"現在拍数：{_musicBuffer.CurrentBeat}");

            double executeBeat = _musicBuffer.ConvertBarTimingInfoToBeat(barTimingInfo);
            _debugLog.AppendLine($"予約アクション発火拍数：{executeBeat}");

            ScheduledAction scheduledAction = new ScheduledAction(executeBeat, action);
            _scheduledActions.Enqueue(scheduledAction);
            Debug.Log(_debugLog.ToString());
        }
        #endregion

        [SerializeField] private CriMusicBuffer _musicBuffer;

        /// <summary>予約アクションのキュー</summary>
        private PriorityQueue<ScheduledAction> _scheduledActions = new PriorityQueue<ScheduledAction>(
            Comparer<ScheduledAction>.Create((a, b) => a.ExecuteBeat.CompareTo(b.ExecuteBeat))
            );

        private StringBuilder _debugLog = new StringBuilder();

        [Header("デバッグ用")]
        [SerializeField, Tooltip("定期アクションを起こす単位拍数")]
        private double _onBeatUnit = 1d;
        [SerializeField, ReadOnly, Tooltip("定期アクションを起こす次の拍数")]
        private double _targetBeat = 0;


        public event Action OnBeat;

        #region ライフサイクル

        void Update()
        {
            CheckOnBeat();
            TriggerRegistedActions();
        }

        #endregion


        #region Privateメソッド
        /// <summary>
        /// 単位拍数ごとにイベントを発火する。
        /// </summary>
        private void CheckOnBeat()
        {
            double currentBeat = _musicBuffer.CurrentBeat;
            if (currentBeat >= _targetBeat)
            {
                OnBeat?.Invoke();
                _targetBeat += _onBeatUnit;
            }
        }
        /// <summary>
        /// 拍数判定して、予約されたアクションを発火する。
        /// </summary>
        private void TriggerRegistedActions()
        {
            // 現在拍数
            double currentBeat = _musicBuffer.CurrentBeat;
            while (_scheduledActions.Count > 0)
            {
                ScheduledAction item = _scheduledActions.Peek();
                _debugLog.Clear();
                if (item != null && item.ExecuteBeat > currentBeat)
                {
                    // 予約アクションの中、発火タイミングの最小値も達していない場合、繰り返し終了
                    _debugLog.AppendLine("アクション発火拍：" + item.ExecuteBeat);
                    _debugLog.AppendLine("発火できるアクション無し、ループ終了。");
                    break;
                }
                else
                {
                    // 発火タイミングに達している場合、アクション発火
                    _debugLog.AppendLine("アクション発火拍：" + item.ExecuteBeat);
                    _debugLog.AppendLine("アクション発火。");
                    item.Action?.Invoke();
                    _scheduledActions.Dequeue();
                }
                Debug.Log(_debugLog.ToString());
            }
        }
        #endregion
    }
}
