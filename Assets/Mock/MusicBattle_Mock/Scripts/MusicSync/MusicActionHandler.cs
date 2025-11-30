using SymphonyFrameWork.Attribute;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
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
            StringBuilder debugLog = new StringBuilder();
            debugLog.AppendLine("アクション予約受付た。");
            debugLog.AppendLine($"小節フラグ：{barTimingInfo.BarFlg}, 拍子スケール：{barTimingInfo.TimeSignature}, 拍数：{barTimingInfo.TargetBeat}");
            debugLog.AppendLine($"現在拍数：{_musicBuffer.CurrentBeat}");

            double executeBeat = _musicBuffer.ConvertBarTimingInfoToBeat(barTimingInfo);
            debugLog.AppendLine($"予約アクション発火拍数：{executeBeat}");

            ScheduledAction scheduledAction = new ScheduledAction(executeBeat, action);
            _scheduledActions.Add(scheduledAction);
            _scheduledActions.Sort((a, b) => a.ExecuteBeat.CompareTo(b.ExecuteBeat));
            Debug.Log(debugLog.ToString());
        }
        #endregion

        [SerializeField]
        private CriMusicBuffer _musicBuffer;
        private List<ScheduledAction> _scheduledActions = new List<ScheduledAction>();

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
            // 削除待ちアクションリスト
            List<ScheduledAction> actionsToRemove = new List<ScheduledAction>();
            // デバッグログ出力用
            foreach (var scheduledAction in _scheduledActions)
            {
                StringBuilder debugLog = new StringBuilder();
                if (scheduledAction.ExecuteBeat > currentBeat)
                {
                    // 予約アクションの中、発火タイミングの最小値も達していない場合、繰り返し終了
                    debugLog.AppendLine("アクション発火拍：" + scheduledAction.ExecuteBeat);
                    debugLog.AppendLine("最小タイミングも達していないため、繰り返し終了。");
                    break;
                }
                else
                {
                    // 発火タイミングに達している場合、アクション発火
                    debugLog.AppendLine("アクション発火拍：" + scheduledAction.ExecuteBeat);
                    debugLog.AppendLine("アクション発火。");
                    scheduledAction.Action?.Invoke();
                    actionsToRemove.Add(scheduledAction);
                }
                Debug.Log(debugLog.ToString());
            }
            // 発火済み、キャンセル済みアクションを削除
            _scheduledActions.RemoveAll(action => actionsToRemove.Contains(action));
        }
        #endregion
    }
}
