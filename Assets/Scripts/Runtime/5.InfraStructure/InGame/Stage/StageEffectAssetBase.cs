using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Stage;
using SymphonyFrameWork.Attribute;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Stage
{
    /// <summary>
    ///     Wave開始時に実行するステージ演出を入力するAsset基底です。
    /// </summary>
    [Serializable]
    public abstract class StageEffectAssetBase : ISerializationCallbackReceiver
    {
        /// <summary>
        ///     ステージ演出定義を生成します。
        /// </summary>
        /// <returns> 生成したステージ演出定義です。 </returns>
        public abstract IStageEffectDefinition Create();

        /// <summary>
        ///     シリアライズ前にサマリーを更新します。
        /// </summary>
        public void OnBeforeSerialize()
        {
            _inspectorSummary = BuildSummary();
        }

        /// <summary>
        ///     デシリアライズ後の処理を行います。
        /// </summary>
        public void OnAfterDeserialize()
        {
        }

        /// <summary>
        ///     指定種類のステージ演出定義を生成します。
        /// </summary>
        /// <param name="kind"> 演出種類です。 </param>
        /// <returns> 生成した定義です。 </returns>
        protected IStageEffectDefinition CreateDefinition(StageEffectKind kind)
        {
            if (_barFlag > 1 || _timeSignature <= 0d || _targetBeat <= 0d)
            {
                throw new InvalidOperationException(
                    $"ステージ演出の音楽同期値が不正です。EffectId: {_effectId}");
            }

            EnemyMusicSpec musicSpec = new(
                _barFlag,
                _timeSignature,
                _targetBeat);
            return new StageEffectDefinition(_effectId, kind, musicSpec);
        }

        /// <summary>
        ///     インスペクター表示用サマリーを生成します。
        /// </summary>
        /// <returns> サマリーです。 </returns>
        protected abstract string BuildSummary();

        /// <summary> 演出IDです。 </summary>
        protected string EffectId => _effectId;

        [SerializeField, Tooltip("StageEffectView側の演出と対応するIDです。")]
        private string _effectId;

        [SerializeField, Range(0, 1), Tooltip("0は現在小節、1は次小節で実行します。")]
        private byte _barFlag;

        [SerializeField, Min(1f), Tooltip("演出を同期する拍子です。")]
        private double _timeSignature = 4d;

        [SerializeField, Min(0.01f), Tooltip("小節内で演出を実行する拍です。")]
        private double _targetBeat = 1d;

        [SerializeField, TextArea(2, 4), ReadOnly, Tooltip("設定内容の要約です。")]
        private string _inspectorSummary;
    }
}
