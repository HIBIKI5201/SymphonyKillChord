using KillChord.Runtime.Utility.Identity;
using SymphonyFrameWork.Attribute;
using UnityEngine;

namespace KillChord.Demo
{
    /// <summary>
    ///     体験版の制限時間、終了条件、遷移先を定義します。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(DemoExperienceConfig), menuName = "KillChord/Demo/Experience Config")]
    public sealed class DemoExperienceConfig : ScriptableObject
    {
        /// <summary> ホームタイマーと全体タイマーを使用する場合はtrueです。 </summary>
        public bool IsTimerEnabled => _isTimerEnabled;

        /// <summary> ホーム滞在可能時間です。 </summary>
        public float HomeTimeLimitSeconds => Mathf.Max(0.0f, _homeTimeLimitSeconds);

        /// <summary> 体験開始からの全体制限時間です。 </summary>
        public float OverallTimeLimitSeconds => Mathf.Max(0.0f, _overallTimeLimitSeconds);

        /// <summary> 全体タイマーの開始地点です。 </summary>
        public DemoTimerStartPoint OverallTimerStartPoint => _overallTimerStartPoint;

        /// <summary>
        ///     クリアすると体験版終了へ進むステージIDの数値です。
        ///     0の場合はステージツリー上の最終バトルステージを使用します。
        /// </summary>
        public int EndStageIdValue => _endStageId.Id;

        /// <summary> 体験版終了シーンへ到達した時点でセーブデータを削除する場合はtrueです。 </summary>
        public bool IsSaveDataResetOnEnd => _isSaveDataResetOnEnd;

        /// <summary> 体験版終了シーン名です。 </summary>
        public string EndSceneName => _endSceneName;

        /// <summary>
        ///     体験版セッションをリセットするタイトルシーン名です。
        ///     DemoEndSequenceConfigの遷移先と同じシーンを設定します。
        /// </summary>
        public string TitleSceneName => _titleSceneName;

        [Header("Timer")]
        [SerializeField, Tooltip("オフにすると、ホームタイマーと全体タイマーを表示・計時せず、時間切れによる強制出撃や体験版終了も行いません。")]
        private bool _isTimerEnabled = true;

        [SerializeField, Min(0.0f), Tooltip("ホームへ入ってから強制出撃するまでの秒数です。ホームへ戻るたびにリセットされます。")]
        private float _homeTimeLimitSeconds = 90.0f;

        [SerializeField, Min(0.0f), Tooltip("選択した開始地点から体験終了までの秒数です。ホームや戦闘中も進みます。")]
        private float _overallTimeLimitSeconds = 300.0f;

        [SerializeField, Tooltip("全体タイマーを開始する地点です。選択した地点より後から再開する場合は、再開地点で開始します。")]
        private DemoTimerStartPoint _overallTimerStartPoint = DemoTimerStartPoint.TutorialBattle;

        [Header("End")]
        [SerializeField, SourceDataCollection("StageAsset")]
        [Tooltip("クリアすると体験版終了シーンへ進むバトルステージです。未設定の場合はステージツリー上の最終バトルステージを使用します。")]
        private DataID _endStageId;

        [SerializeField, Tooltip("オフにすると、体験版終了シーンへ到達してもセーブデータを削除せず、次回はタイトルから続きを再開できます。")]
        private bool _isSaveDataResetOnEnd = true;

        [SerializeField, SceneNameSelector, Tooltip("演出と案内を表示する体験版終了専用シーンです。")]
        private string _endSceneName = "DemoEnd";

        [SerializeField, SceneNameSelector, Tooltip("体験版セッションをリセットするタイトルシーンです。DemoEndSequenceConfigと同じシーンを設定します。")]
        private string _titleSceneName = "Title";
    }
}
