using Mock.MusicBattle.Basis;
using UnityEngine;

namespace Mock.MusicBattle.MusicSync
{
    [CreateAssetMenu(
        fileName = nameof(MusicSyncConfigs),
        menuName = EditorConstraint.CREATE_ASSET_PATH + nameof(MusicSyncConfigs))]
    public class MusicSyncConfigs : ScriptableObject
    {
        #region パブリックプロパティ
        /// <summary> 入力タイミングの追いかけ処理における補正の閾値。この閾値を超えると次の拍として扱われる。 </summary>
        public double InputFixThreshold => _inputFixThreshold;
        /// <summary> 入力履歴を最大何小節保持するか。これをバー制限として古い入力は破棄される。 </summary>
        public int InputHistoryBarLimit => _inputHistoryBarLimit;
        /// <summary> 入力された拍子の履歴を最大何個保持するか。 </summary>
        public int InputSignatureHistoryLimit => _inputSignatureHistoryLimit;
        #endregion

        #region インスペクター表示フィールド
        [SerializeField, Range(0.5f, 1.0f), Tooltip("入力タイミングの追いかけ処理における補正の閾値。")]
        private double _inputFixThreshold = 0.8d;
        [SerializeField, Tooltip("入力履歴を最大何小節保持するか。")]
        private int _inputHistoryBarLimit = 4;
        [SerializeField, Tooltip("入力された拍子の履歴を最大何個保持するか。")]
        private int _inputSignatureHistoryLimit = 4;
        #endregion
    }
}
