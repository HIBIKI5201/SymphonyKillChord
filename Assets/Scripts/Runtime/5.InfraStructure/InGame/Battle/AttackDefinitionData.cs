using KillChord.Runtime.InfraStructure.InGame.Battle;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    /// <summary>
    ///     攻撃の定義を保持するScriptableObjectクラス。
    /// </summary>
    [CreateAssetMenu(fileName = "AttackDefinitionData", menuName = "KillChord/AttackDefinitionData")]
    public class AttackDefinitionData : ScriptableObject
    {
        /// <summary> 攻撃名を取得する。 </summary>
        public string AttackName => _attackName;
        /// <summary> 基本ダメージを取得する。 </summary>
        public float BaseDamage => _baseDamage;
        /// <summary> 攻撃パラメーターセットを取得する。 </summary>
        public AttackParameterSetData AttackParameterSetData => _attackParameterSetData;
        /// <summary> 攻撃パイプラインアセットを取得する。 </summary>
        public AttackPipelineAsset AttackPipelineAsset => _attackPipelineAsset;
        /// <summary> ビートタイプを使用するかどうかを取得する。 </summary>
        public bool UseBeatType => _useBeatType;
        /// <summary> ビートタイプを取得する。 </summary>
        public int BeatType => _beatType;

        [SerializeField, Tooltip("攻撃名")] private string _attackName;
        [SerializeField, Tooltip("基本ダメージ")] private float _baseDamage;
        [SerializeField, Tooltip("攻撃パラメーターセット")] private AttackParameterSetData _attackParameterSetData;
        [SerializeField, Tooltip("攻撃パイプラインアセット")] private AttackPipelineAsset _attackPipelineAsset;

        [SerializeField, Tooltip("ビートタイプを使用するかどうか")] private bool _useBeatType;
        [SerializeField, Tooltip("ビートタイプ")] private int _beatType;
    }
}
