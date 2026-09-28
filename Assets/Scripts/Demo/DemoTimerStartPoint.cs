using UnityEngine;

namespace KillChord.Demo
{
    /// <summary>
    ///     体験版の全体タイマーを開始する地点です。
    /// </summary>
    public enum DemoTimerStartPoint
    {
        /// <summary> 冒頭シナリオから開始します。 </summary>
        [InspectorName("シナリオ")]
        OpeningScenario = 0,

        /// <summary> チュートリアル戦闘から開始します。 </summary>
        [InspectorName("チュートリアル")]
        TutorialBattle = 1,

        /// <summary> ホームチュートリアルから開始します。 </summary>
        [InspectorName("ホーム")]
        Home = 2,
    }
}
