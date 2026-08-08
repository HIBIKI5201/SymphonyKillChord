using KillChord.Runtime.Utility.Constant;
using UnityEngine;

namespace KillChord.Runtime.View.Persistent.Input
{
    [CreateAssetMenu(fileName = nameof(MobileStickFlickInputConfig),
        menuName = PathConst.CREATE_ASSET_MENU_PATH
        + "Input/"
        + nameof(MobileStickFlickInputConfig))]
    public sealed class MobileStickFlickInputConfig : ScriptableObject
    {
        /// <summary> フリックとして扱う最小距離をスティック可動範囲に対する割合で取得する。 </summary>
        public float MinFlickDistanceRate => _minFlickDistanceRate;
        /// <summary> フリックとして扱う押下から解放までの最大秒数を取得する。 </summary>
        public float MaxFlickDuration => _maxFlickDuration;

        [Header("フリック判定条件")]
        [SerializeField, Range(0f, 1f), Tooltip("フリックとして扱う最小距離をスティック可動範囲に対する割合で指定する。")]
        private float _minFlickDistanceRate = 0.8f;

        [SerializeField, Min(0f), Tooltip("フリックとして扱う押下から解放までの最大秒数。")]
        private float _maxFlickDuration = 0.2f;
    }
}
