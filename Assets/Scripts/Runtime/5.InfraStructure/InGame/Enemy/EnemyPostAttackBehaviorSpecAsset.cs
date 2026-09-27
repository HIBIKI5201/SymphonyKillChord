using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.Utility.Constant;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Enemy
{
    /// <summary>
    ///     敵の攻撃後の行動（抽選の重みと移動量）を保持するScriptableObject。
    ///     射程外の接近と障害物の迂回はこの抽選より優先される。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(EnemyPostAttackBehaviorSpecAsset), menuName = PathConst.CREATE_ASSET_MENU_PATH + "Enemy/" + nameof(EnemyPostAttackBehaviorSpecAsset))]
    public class EnemyPostAttackBehaviorSpecAsset : ScriptableObject
    {
        /// <summary>
        ///     ドメイン層の攻撃後行動仕様を生成する。
        /// </summary>
        /// <returns> 攻撃後行動仕様。 </returns>
        public EnemyPostAttackBehaviorSpec ToSpec()
        {
            return new EnemyPostAttackBehaviorSpec(
                _stayWeight,
                _regroupWeight,
                _obstacleApproachWeight,
                _regroupDistanceMin,
                _regroupDistanceMax,
                _obstacleApproachRatio,
                _overrideArrivalThreshold);
        }

        [SerializeField, Min(0f), Tooltip("その場に留まり再攻撃する重み。")]
        private float _stayWeight = 0.5f;
        [SerializeField, Min(0f), Tooltip("近くの味方に合流する重み。")]
        private float _regroupWeight = 0.25f;
        [SerializeField, Min(0f), Tooltip("近くの障害物に接近する重み。")]
        private float _obstacleApproachWeight = 0.25f;
        [SerializeField, Min(0f), Tooltip("合流時、味方から離れる最小距離(m)。")]
        private float _regroupDistanceMin = 2f;
        [SerializeField, Min(0f), Tooltip("合流時、味方から離れる最大距離(m)。")]
        private float _regroupDistanceMax = 3f;
        [SerializeField, Range(0f, 1f), Tooltip("障害物へ近づく割合。自身と障害物の距離をこの割合だけ縮める。")]
        private float _obstacleApproachRatio = 0.3f;
        [SerializeField, Min(0f), Tooltip("上書き移動先への到達とみなす距離(m)。")]
        private float _overrideArrivalThreshold = 0.5f;
    }
}
