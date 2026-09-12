using UnityEngine;

namespace KillChord.Runtime.View.InGame.Camera
{
    /// <summary>
    ///     カメラシェイク1種類分の揺れ方を設定するScriptableObject。
    ///     LitMotionのPunchモーションの設定値として使用する。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(CameraShakeConfig), menuName = "KillChord/InGame/CameraShakeConfig")]
    public sealed class CameraShakeConfig : ScriptableObject
    {
        /// <summary> エフェクト実行の優先順位。高い数字ほど優先される。 </summary>
        public int Priority => _priority;

        /// <summary> 揺れの継続時間(秒)。 </summary>
        public float Duration => _duration;

        /// <summary> X(左右)方向の振動の強さの抽選範囲。xが最小値、yが最大値。 </summary>
        public Vector2 StrengthRangeX => _strengthRangeX;

        /// <summary> Y(上下)方向の振動の強さの抽選範囲。xが最小値、yが最大値。 </summary>
        public Vector2 StrengthRangeY => _strengthRangeY;

        /// <summary> Z(前後)方向の振動の強さの抽選範囲。xが最小値、yが最大値。 </summary>
        public Vector2 StrengthRangeZ => _strengthRangeZ;

        /// <summary> 継続時間中の振動回数の抽選範囲。xが最小値、yが最大値。1未満を指定した場合は1として扱う。 </summary>
        public Vector2Int Frequency => Vector2Int.Max(_frequency, Vector2Int.one);

        [SerializeField, Tooltip("エフェクト実行の優先順位（高い数字ほど優先）")]
        private int _priority;

        [Min(0f)]
        [SerializeField, Tooltip("揺れの継続時間(秒)。0以下の場合はシェイクしません。")]
        private float _duration;

        [SerializeField, Tooltip("X(左右)方向の振動の強さ(メートル)の抽選範囲。x=最小値, y=最大値。")]
        private Vector2 _strengthRangeX;

        [SerializeField, Tooltip("Y(上下)方向の振動の強さ(メートル)の抽選範囲。x=最小値, y=最大値。")]
        private Vector2 _strengthRangeY;

        [SerializeField, Tooltip("Z(前後)方向の振動の強さ(メートル)の抽選範囲。x=最小値, y=最大値。")]
        private Vector2 _strengthRangeZ;

        [SerializeField, Tooltip("継続時間中の振動回数の抽選範囲。x=最小値, y=最大値。大きいほど細かく振動します。1未満を指定した場合は1として扱われます。")]
        private Vector2Int _frequency;
    }
}
