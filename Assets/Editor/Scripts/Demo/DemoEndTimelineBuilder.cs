using System;
using System.IO;
using System.Linq;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

namespace KillChord.Demo.Editor
{
    /// <summary>
    ///     体験版終了デモムービーのTimelineアセットとアニメーションクリップを生成するエディタ機能です。
    /// </summary>
    internal static class DemoEndTimelineBuilder
    {
        /// <summary> 生成するTimelineアセットのパスです。 </summary>
        public const string TIMELINE_ASSET_PATH =
            "Assets/Arts/Animation/Timelines/DemoEndMovie.playable";

        /// <summary> カメラの表示切り替えトラック名です。 </summary>
        public const string CAMERA_ACTIVATION_TRACK_NAME = "Camera Activation";

        /// <summary> Dollyカメラのスプライン進行トラック名です。 </summary>
        public const string DOLLY_POSITION_TRACK_NAME = "Dolly Position";

        /// <summary> Dollyカメラの注視点トラック名です。 </summary>
        public const string DOLLY_TARGET_TRACK_NAME = "Dolly Target";

        /// <summary> 暗転トラック名です。 </summary>
        public const string BLACKOUT_TRACK_NAME = "Blackout";

        /// <summary> Soldier14のアニメーショントラック名です。 </summary>
        public const string SOLDIER_TRACK_NAME = "Soldier14";

        /// <summary> ムービー全体の長さです。 </summary>
        public const float MOVIE_DURATION_SECONDS = 9.0f;

        /// <summary> 戦闘開始演出と同じカメラ移動にかける秒数です。 </summary>
        public const float CAMERA_MOVE_DURATION_SECONDS = 7.0f;

        /// <summary>
        ///     Timelineアセットを生成します。既に存在する場合はそのまま返します。
        /// </summary>
        /// <returns> 生成または取得したTimelineアセットです。 </returns>
        public static TimelineAsset CreateOrLoad()
        {
            TimelineAsset existingTimeline =
                AssetDatabase.LoadAssetAtPath<TimelineAsset>(TIMELINE_ASSET_PATH);
            if (existingTimeline != null)
            {
                return existingTimeline;
            }

            EnsureDirectory(TIMELINE_ASSET_PATH);

            TimelineAsset timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            AssetDatabase.CreateAsset(timeline, TIMELINE_ASSET_PATH);
            timeline.editorSettings.frameRate = TIMELINE_FRAME_RATE;

            CreateCameraActivationTrack(timeline);
            CreateDollyPositionTrack(timeline);
            CreateDollyTargetTrack(timeline);
            CreateBlackoutTrack(timeline);
            CreateSoldierTrack(timeline);

            EditorUtility.SetDirty(timeline);
            AssetDatabase.SaveAssets();

            // ここで ImportAsset を挟むと生成直後のインスタンスが破棄され、
            // 返した参照が破棄済みオブジェクトになるため再インポートはしない。
            return timeline;
        }

        /// <summary>
        ///     名前が一致する出力トラックを取得します。
        /// </summary>
        /// <param name="timeline"> 検索対象のTimelineアセットです。 </param>
        /// <param name="trackName"> 取得したいトラック名です。 </param>
        /// <returns> 見つかったトラックです。存在しない場合はnullです。 </returns>
        public static TrackAsset FindTrack(TimelineAsset timeline, string trackName)
        {
            if (timeline == null)
            {
                return null;
            }

            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (track.name == trackName)
                {
                    return track;
                }
            }

            return null;
        }

        private const double TIMELINE_FRAME_RATE = 60.0;

        private const string DOLLY_POSITION_CLIP_PATH =
            "Assets/Arts/Animation/Clips/DemoEnd_DollyPosition.anim";

        private const string BLACKOUT_CLIP_PATH =
            "Assets/Arts/Animation/Clips/DemoEnd_Blackout.anim";

        private const string DOLLY_TARGET_CLIP_PATH =
            "Assets/Arts/Animation/Clips/DollyCam_StageStart.anim";

        private const string SPLINE_POSITION_PROPERTY = "m_SplineSettings.Position";

        private const string CANVAS_GROUP_ALPHA_PROPERTY = "m_Alpha";

        private const float DOLLY_TARGET_HEIGHT_OFFSET = -0.09f;

        private const string SOLDIER_IDLE_CLIP_PATH =
            "Assets/AssetStoreTools/Kevin Iglesias/Human Animations/Animations/Female/Idles/HumanF@MilitaryIdle01.fbx";

        /// <summary>
        ///     カメラの表示切り替えトラックを生成します。
        /// </summary>
        /// <param name="timeline"> 追加先のTimelineアセットです。 </param>
        private static void CreateCameraActivationTrack(TimelineAsset timeline)
        {
            ActivationTrack track =
                timeline.CreateTrack<ActivationTrack>(null, CAMERA_ACTIVATION_TRACK_NAME);

            // 停止後もカメラを残し、暗転中に別のカメラへ切り替わらないようにする。
            track.postPlaybackState = ActivationTrack.PostPlaybackState.Active;

            TimelineClip clip = track.CreateDefaultClip();
            clip.start = 0.0;
            clip.duration = MOVIE_DURATION_SECONDS;
        }

        /// <summary>
        ///     戦闘開始演出と同じ進行カーブでスプライン位置を動かすトラックを生成します。
        /// </summary>
        /// <param name="timeline"> 追加先のTimelineアセットです。 </param>
        private static void CreateDollyPositionTrack(TimelineAsset timeline)
        {
            // StageStart.playable と同じキーで、0から1まで加速しながら進む。
            AnimationCurve curve = new(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(1.0f, 0.0f),
                new Keyframe(3.5f, 0.56f),
                new Keyframe(5.5f, 0.79f),
                new Keyframe(CAMERA_MOVE_DURATION_SECONDS, 1.0f));

            AnimationClip clip = CreateOrLoadClip(DOLLY_POSITION_CLIP_PATH);
            EditorCurveBinding binding = EditorCurveBinding.FloatCurve(
                string.Empty,
                typeof(CinemachineSplineDolly),
                SPLINE_POSITION_PROPERTY);
            AnimationUtility.SetEditorCurve(clip, binding, curve);
            EditorUtility.SetDirty(clip);

            AnimationTrack track =
                timeline.CreateTrack<AnimationTrack>(null, DOLLY_POSITION_TRACK_NAME);
            TimelineClip timelineClip = track.CreateClip(clip);
            timelineClip.start = 0.0;
            timelineClip.duration = CAMERA_MOVE_DURATION_SECONDS;
        }

        /// <summary>
        ///     戦闘開始演出と同じ注視点アニメーションを再生するトラックを生成します。
        /// </summary>
        /// <param name="timeline"> 追加先のTimelineアセットです。 </param>
        private static void CreateDollyTargetTrack(TimelineAsset timeline)
        {
            AnimationTrack track =
                timeline.CreateTrack<AnimationTrack>(null, DOLLY_TARGET_TRACK_NAME);

            // 注視点はSymphonyの立ち姿基準で作られており、アイドル中のSoldier14は頭がやや低い。
            // そのままだと寄りで頭頂が映るため、注視点を少し下げる。
            track.trackOffset = TrackOffset.ApplyTransformOffsets;
            track.position = new Vector3(0.0f, DOLLY_TARGET_HEIGHT_OFFSET, 0.0f);

            AnimationClip clip =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(DOLLY_TARGET_CLIP_PATH);
            if (clip == null)
            {
                Debug.LogWarning(
                    $"[{nameof(DemoEndTimelineBuilder)}] " +
                    $"{DOLLY_TARGET_CLIP_PATH} が見つかりません。トラックは空のまま生成します。");
                return;
            }

            TimelineClip timelineClip = track.CreateClip(clip);
            timelineClip.start = 0.0;
            timelineClip.duration = CAMERA_MOVE_DURATION_SECONDS;
        }

        /// <summary>
        ///     開幕の明転と、顔が見えた後の暗転を行うトラックを生成します。
        /// </summary>
        /// <param name="timeline"> 追加先のTimelineアセットです。 </param>
        private static void CreateBlackoutTrack(TimelineAsset timeline)
        {
            // 黒画面から明けた後、カメラ移動の完了に合わせて暗転し直す。
            AnimationCurve curve = new(
                new Keyframe(0.0f, 1.0f),
                new Keyframe(0.2f, 1.0f),
                new Keyframe(0.6f, 0.0f),
                new Keyframe(CAMERA_MOVE_DURATION_SECONDS, 0.0f),
                new Keyframe(8.5f, 1.0f),
                new Keyframe(MOVIE_DURATION_SECONDS, 1.0f));

            AnimationClip clip = CreateOrLoadClip(BLACKOUT_CLIP_PATH);
            EditorCurveBinding binding = EditorCurveBinding.FloatCurve(
                string.Empty,
                typeof(CanvasGroup),
                CANVAS_GROUP_ALPHA_PROPERTY);
            AnimationUtility.SetEditorCurve(clip, binding, curve);
            EditorUtility.SetDirty(clip);

            AnimationTrack track =
                timeline.CreateTrack<AnimationTrack>(null, BLACKOUT_TRACK_NAME);
            TimelineClip timelineClip = track.CreateClip(clip);
            timelineClip.start = 0.0;
            timelineClip.duration = MOVIE_DURATION_SECONDS;
        }

        /// <summary>
        ///     Soldier14のアイドルモーションを再生するトラックを生成します。
        /// </summary>
        /// <param name="timeline"> 追加先のTimelineアセットです。 </param>
        private static void CreateSoldierTrack(TimelineAsset timeline)
        {
            AnimationTrack track =
                timeline.CreateTrack<AnimationTrack>(null, SOLDIER_TRACK_NAME);

            // 既定のオフセットではクリップのルート姿勢が優先され、
            // シーンで向けたカメラ側への向きが打ち消されるため、シーンの姿勢を基準にする。
            track.trackOffset = TrackOffset.ApplySceneOffsets;

            AnimationClip clip = LoadHumanoidClip(SOLDIER_IDLE_CLIP_PATH);
            if (clip == null)
            {
                Debug.LogWarning(
                    $"[{nameof(DemoEndTimelineBuilder)}] " +
                    $"{SOLDIER_IDLE_CLIP_PATH} からクリップを取得できませんでした。トラックは空のまま生成します。");
                return;
            }

            TimelineClip timelineClip = track.CreateClip(clip);
            timelineClip.start = 0.0;
            timelineClip.duration = MOVIE_DURATION_SECONDS;

            // 素材のアイドルはムービー尺より短いため、明示的にループさせる。
            if (timelineClip.asset is AnimationPlayableAsset playableAsset)
            {
                playableAsset.loop = AnimationPlayableAsset.LoopMode.On;
            }
        }

        /// <summary>
        ///     FBXに含まれるHumanoidアニメーションクリップを取得します。
        /// </summary>
        /// <param name="assetPath"> 読み込むFBXのパスです。 </param>
        /// <returns> 取得したクリップです。存在しない場合はnullです。 </returns>
        private static AnimationClip LoadHumanoidClip(string assetPath)
        {
            // FBXのサブアセットにはプレビュー用クリップも含まれるため除外する。
            return AssetDatabase.LoadAllAssetsAtPath(assetPath)
                .OfType<AnimationClip>()
                .FirstOrDefault(clip => !clip.name.StartsWith("__preview__", StringComparison.Ordinal));
        }

        /// <summary>
        ///     アニメーションクリップアセットを生成または取得します。
        /// </summary>
        /// <param name="assetPath"> クリップアセットのパスです。 </param>
        /// <returns> 生成または取得したクリップです。 </returns>
        private static AnimationClip CreateOrLoadClip(string assetPath)
        {
            AnimationClip existingClip =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
            if (existingClip != null)
            {
                return existingClip;
            }

            EnsureDirectory(assetPath);

            AnimationClip clip = new()
            {
                name = Path.GetFileNameWithoutExtension(assetPath),
            };
            AssetDatabase.CreateAsset(clip, assetPath);
            return clip;
        }

        /// <summary>
        ///     アセットパスの親ディレクトリを作成します。
        /// </summary>
        /// <param name="assetPath"> 作成対象のアセットパスです。 </param>
        private static void EnsureDirectory(string assetPath)
        {
            string directoryPath = Path.GetDirectoryName(assetPath);
            if (string.IsNullOrEmpty(directoryPath) || Directory.Exists(directoryPath))
            {
                return;
            }

            Directory.CreateDirectory(directoryPath);
            AssetDatabase.Refresh();
        }
    }
}
