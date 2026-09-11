using KillChord.Runtime.Utility.Identity;
using SymphonyFrameWork.Attribute;
using UnityEngine;

namespace KillChord.Demo
{
    /// <summary>
    ///     体験版の制限時間と遷移先を定義します。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(DemoExperienceConfig), menuName = "KillChord/Demo/Experience Config")]
    public sealed class DemoExperienceConfig : ScriptableObject
    {
        /// <summary> ホーム滞在可能時間です。 </summary>
        public float HomeTimeLimitSeconds => Mathf.Max(0.0f, _homeTimeLimitSeconds);

        /// <summary> 体験開始からの全体制限時間です。 </summary>
        public float OverallTimeLimitSeconds => Mathf.Max(0.0f, _overallTimeLimitSeconds);

        /// <summary> ホーム期限切れ時に強制選択するステージIDです。 </summary>
        public int ForcedStageId => _forcedStageId.Id;

        /// <summary> 完了時に体験版終了画面へ進む最終ステージIDです。 </summary>
        public int FinalStageId => _finalStageId.Id;

        /// <summary> 体験版終了シーン名です。 </summary>
        public string EndSceneName => _endSceneName;

        [SerializeField, Min(0.0f), Tooltip("初めてホームへ来てから強制出撃するまでの秒数です。")]
        private float _homeTimeLimitSeconds = 180.0f;

        [SerializeField, Min(0.0f), Tooltip("初めてホームへ来てから体験終了までの秒数です。戦闘中も進みます。")]
        private float _overallTimeLimitSeconds = 900.0f;

        [SerializeField, SourceDataCollection("StageAsset"), Tooltip("ホーム期限切れ後に出撃させるステージです。")]
        private DataID _forcedStageId;

        [SerializeField, SourceDataCollection("StageAsset"), Tooltip("クリア時に体験を終了する最後のステージです。")]
        private DataID _finalStageId;

        [SerializeField, SceneNameSelector, Tooltip("演出と案内を表示する体験版終了専用シーンです。")]
        private string _endSceneName = "DemoEnd";
    }
}
