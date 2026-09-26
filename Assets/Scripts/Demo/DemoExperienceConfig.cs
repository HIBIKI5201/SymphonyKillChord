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

        /// <summary> 全体タイマーの開始地点です。 </summary>
        public DemoTimerStartPoint OverallTimerStartPoint => _overallTimerStartPoint;

        /// <summary> 体験版終了シーン名です。 </summary>
        public string EndSceneName => _endSceneName;

        /// <summary>
        ///     体験版セッションをリセットするタイトルシーン名です。
        ///     DemoEndSequenceConfigの遷移先と同じシーンを設定します。
        /// </summary>
        public string TitleSceneName => _titleSceneName;

        [SerializeField, Min(0.0f), Tooltip("ホームへ入ってから強制出撃するまでの秒数です。ホームへ戻るたびにリセットされます。")]
        private float _homeTimeLimitSeconds = 90.0f;

        [SerializeField, Min(0.0f), Tooltip("選択した開始地点から体験終了までの秒数です。ホームや戦闘中も進みます。")]
        private float _overallTimeLimitSeconds = 300.0f;

        [SerializeField, Tooltip("全体タイマーを開始する地点です。選択した地点より後から再開する場合は、再開地点で開始します。")]
        private DemoTimerStartPoint _overallTimerStartPoint = DemoTimerStartPoint.TutorialBattle;

        [SerializeField, SceneNameSelector, Tooltip("演出と案内を表示する体験版終了専用シーンです。")]
        private string _endSceneName = "DemoEnd";

        [SerializeField, SceneNameSelector, Tooltip("体験版セッションをリセットするタイトルシーンです。DemoEndSequenceConfigと同じシーンを設定します。")]
        private string _titleSceneName = "Title";
    }
}
