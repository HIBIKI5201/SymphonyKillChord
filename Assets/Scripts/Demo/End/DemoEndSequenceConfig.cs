using SymphonyFrameWork.Attribute;
using UnityEngine;

namespace KillChord.Demo.End
{
    /// <summary>
    ///     体験版終了演出の再生設定を定義します。
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(DemoEndSequenceConfig),
        menuName = "KillChord/Demo/End Sequence Config")]
    public sealed class DemoEndSequenceConfig : ScriptableObject
    {
        /// <summary> 背景として重ねるステージシーン名です。 </summary>
        public string BackgroundSceneName => _backgroundSceneName;

        /// <summary> 演出終了後に戻るタイトルシーン名です。 </summary>
        public string TitleSceneName => _titleSceneName;

        /// <summary> 演出中に再生する戦闘BGMのCue名です。 </summary>
        public string BattleBgmCueName => _battleBgmCueName;

        /// <summary> Timeline終了後に案内UIをフェードインさせる秒数です。 </summary>
        public float EndUiFadeInDuration => Mathf.Max(0.0f, _endUiFadeInDuration);

        /// <summary> 案内UI表示からBGMのフェードアウトを開始するまでの秒数です。 </summary>
        public float BgmFadeOutDelaySeconds => Mathf.Max(0.0f, _bgmFadeOutDelaySeconds);

        /// <summary> BGMをフェードアウトさせる秒数です。 </summary>
        public float BgmFadeOutDuration => Mathf.Max(0.0f, _bgmFadeOutDuration);

        /// <summary> 案内UI表示から攻撃入力を受け付けるまでの秒数です。 </summary>
        public float InputAcceptDelaySeconds => Mathf.Max(0.0f, _inputAcceptDelaySeconds);

        [Header("Scene")]
        [SerializeField, SceneNameSelector, Tooltip("背景として重ねるステージシーン名です。")]
        private string _backgroundSceneName = "Stage_02";

        [SerializeField, SceneNameSelector, Tooltip("演出終了後に戻るタイトルシーン名です。")]
        private string _titleSceneName = "Title";

        [Header("BGM")]
        [SerializeField, Tooltip("演出中に再生する戦闘BGMのCue名です。")]
        private string _battleBgmCueName = "BGM_Ingame";

        [SerializeField, Min(0.0f), Tooltip("案内UI表示からBGMのフェードアウトを開始するまでの秒数です。")]
        private float _bgmFadeOutDelaySeconds = 10.0f;

        [SerializeField, Min(0.0f), Tooltip("BGMをフェードアウトさせる秒数です。")]
        private float _bgmFadeOutDuration = 3.0f;

        [Header("UI")]
        [SerializeField, Min(0.0f), Tooltip("Timeline終了後に案内UIをフェードインさせる秒数です。")]
        private float _endUiFadeInDuration = 1.0f;

        [SerializeField, Min(0.0f), Tooltip("案内UI表示から攻撃入力を受け付けるまでの秒数です。")]
        private float _inputAcceptDelaySeconds = 1.0f;
    }
}
