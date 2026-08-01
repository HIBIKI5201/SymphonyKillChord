using UnityEngine;
using UnityEngine.Animations;

namespace KillChord.Runtime.View.InGame.Sequence
{
    /// <summary>
    ///     戦闘ステージが開始された時のDollyCameraの向く方向を取得するためのクラス。
    /// </summary>
    public class StageStartConstraintView : MonoBehaviour
    {
        /// <summary>
        ///     ConstraintのSourceにTransformを追加する。
        /// </summary>
        /// <param name="playerTransform"> プレイヤーの座標。 </param>
        public void AddConstraintSource(Transform playerTransform)
        {
            if (_sourceIndex != -1) { return; }

            ConstraintSource constraintSource = new()
            {
                sourceTransform = playerTransform,
                weight = SOURCE_WEIGHT,
            };
            _sourceIndex = _constraint.AddSource(constraintSource);
            _constraint.constraintActive = true;
        }
        /// <summary>
        ///     Sourceを削除する。
        /// </summary>
        public void RemoveSource()
        {
            if (_sourceIndex < 0 || _sourceIndex >= _constraint.sourceCount) { return; }
            _constraint.RemoveSource(_sourceIndex);
            _sourceIndex = -1;
        }
        private const float SOURCE_WEIGHT = 1.0f;
        [SerializeField,Tooltip("Playerのステージ開始時に動かすオブジェクトのSourceを持っているObject")]private PositionConstraint _constraint;
        private int _sourceIndex = -1;
        private void Awake()
        {
            if (_constraint == null)
            {
                Debug.LogError($"{nameof(StageStartConstraintView)}{nameof(_constraint)}がnullです。", this);
            }
        }
    }
}
