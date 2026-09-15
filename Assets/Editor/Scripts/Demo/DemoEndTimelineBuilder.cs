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

        /// <summary> 銃身横カメラの表示切り替えトラック名です。 </summary>
        public const string SIDE_CAMERA_ACTIVATION_TRACK_NAME = "Side Camera Activation";

        /// <summary> Dollyカメラのスプライン進行トラック名です。 </summary>
        public const string DOLLY_POSITION_TRACK_NAME = "Dolly Position";

        /// <summary> Dollyカメラの注視点トラック名です。 </summary>
        public const string DOLLY_TARGET_TRACK_NAME = "Dolly Target";

        /// <summary> 暗転トラック名です。 </summary>
        public const string BLACKOUT_TRACK_NAME = "Blackout";

        /// <summary> Soldier14のアニメーショントラック名です。 </summary>
        public const string SOLDIER_TRACK_NAME = "Soldier14";

        /// <summary> Symphonyのアニメーショントラック名です。 </summary>
        public const string SYMPHONY_TRACK_NAME = "Symphony";

        /// <summary> 発砲通知トラック名です。 </summary>
        public const string GUNSHOT_SIGNAL_TRACK_NAME = "Gunshot Signal";

        /// <summary> ムービー全体の長さです。 </summary>
        public const float MOVIE_DURATION_SECONDS = 13.0f;

        /// <summary> 戦闘開始演出と同じカメラ移動にかける秒数です。 </summary>
        public const float CAMERA_MOVE_DURATION_SECONDS = 7.0f;

        /// <summary> 14号からSymphonyへカメラを振り始める秒数です。 </summary>
        public const float CAMERA_TURN_START_SECONDS = 7.0f;

        /// <summary> Symphonyを正面に捉える秒数です。 </summary>
        public const float CAMERA_TURN_END_SECONDS = 7.733333f;

        /// <summary> 銃身横カメラへ切り替える秒数です。 </summary>
        public const float SIDE_CAMERA_CUT_SECONDS = 11.3f;

        /// <summary> 発砲する秒数です。 </summary>
        public const float GUNSHOT_SECONDS = 12.0f;

        /// <summary>
        ///     Timelineアセットを生成し、現在の演出構成へ更新します。
        /// </summary>
        /// <returns> 生成または取得したTimelineアセットです。 </returns>
        public static TimelineAsset CreateOrLoad()
        {
            TimelineAsset existingTimeline =
                AssetDatabase.LoadAssetAtPath<TimelineAsset>(TIMELINE_ASSET_PATH);
            EnsureDirectory(TIMELINE_ASSET_PATH);

            TimelineAsset timeline = existingTimeline;
            if (timeline == null)
            {
                timeline = ScriptableObject.CreateInstance<TimelineAsset>();
                AssetDatabase.CreateAsset(timeline, TIMELINE_ASSET_PATH);
            }

            ClearTracks(timeline);
            timeline.editorSettings.frameRate = TIMELINE_FRAME_RATE;

            CreateCameraActivationTrack(timeline);
            CreateSideCameraActivationTrack(timeline);
            CreateDollyPositionTrack(timeline);
            CreateDollyTargetTrack(timeline);
            CreateBlackoutTrack(timeline);
            CreateSoldierTrack(timeline);
            CreateSymphonyTrack(timeline);
            CreateGunshotSignalTrack(timeline);

            EditorUtility.SetDirty(timeline);
            AssetDatabase.SaveAssets();

            // ここで ImportAsset を挟むと生成直後のインスタンスが破棄され、
            // 返した参照が破棄済みオブジェクトになるため再インポートはしない。
            return timeline;
        }

        /// <summary>
        ///     発砲通知に使用するSignalAssetを取得します。
        /// </summary>
        /// <returns> 発砲通知用SignalAssetです。 </returns>
        public static SignalAsset LoadGunshotSignal()
        {
            return AssetDatabase.LoadAssetAtPath<SignalAsset>(GUNSHOT_SIGNAL_ASSET_PATH);
        }

        /// <summary>
        ///     武器表示通知に使用するSignalAssetを取得します。
        /// </summary>
        /// <returns> 武器表示通知用SignalAssetです。 </returns>
        public static SignalAsset LoadWeaponRevealSignal()
        {
            return AssetDatabase.LoadAssetAtPath<SignalAsset>(WEAPON_REVEAL_SIGNAL_ASSET_PATH);
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

        private const string DOLLY_TARGET_CLIP_PATH =
            "Assets/Arts/Animation/Clips/DemoEnd_DollyTarget.anim";

        private const string BLACKOUT_IN_CLIP_PATH =
            "Assets/Arts/Animation/Clips/DemoEnd_Blackout_In.anim";

        private const string BLACKOUT_OUT_CLIP_PATH =
            "Assets/Arts/Animation/Clips/DemoEnd_Blackout_Out.anim";

        private const string GUNSHOT_SIGNAL_ASSET_PATH =
            "Assets/Arts/Animation/Timelines/Signals/DemoEndGunshot.signal";

        private const string WEAPON_REVEAL_SIGNAL_ASSET_PATH =
            "Assets/Arts/Animation/Timelines/Signals/DemoEndWeaponReveal.signal";

        private const string SPLINE_POSITION_PROPERTY = "m_SplineSettings.Position";

        private const string CANVAS_GROUP_ALPHA_PROPERTY = "m_Alpha";

        private const string LOCAL_POSITION_X_PROPERTY = "m_LocalPosition.x";
        private const string LOCAL_POSITION_Y_PROPERTY = "m_LocalPosition.y";
        private const string LOCAL_POSITION_Z_PROPERTY = "m_LocalPosition.z";

        private const float DOLLY_TARGET_HEIGHT_OFFSET = -0.09f;

        private const float BLACKOUT_IN_DURATION_SECONDS = 0.6f;
        private const float BLACKOUT_OUT_START_SECONDS = 11.8f;
        private const float BLACKOUT_OUT_DURATION_SECONDS = 0.6f;

        private const float SYMPHONY_AIM_START_SECONDS = 8.5f;
        private const float SYMPHONY_SHOT_START_SECONDS = GUNSHOT_SECONDS;
        private const int SYMPHONY_SHOT_FRAME_COUNT = 12;
        private const float SYMPHONY_CLIP_BLEND_SECONDS = 0.2f;
        private const double SYMPHONY_AIM_TIME_SCALE = 0.01;

        private const string SOLDIER_IDLE_CLIP_PATH =
            "Assets/AssetStoreTools/Kevin Iglesias/Human Animations/Animations/Female/Idles/HumanF@MilitaryIdle01.fbx";

        private const string SYMPHONY_AIM_CLIP_PATH =
            "Assets/Arts/Animation/Clips/Sympnonhy_HGpose_0615_3.fbx";

        /// <summary>
        ///     Timelineに残っている旧演出トラックを削除します。
        /// </summary>
        /// <param name="timeline"> 更新対象のTimelineアセットです。 </param>
        private static void ClearTracks(TimelineAsset timeline)
        {
            TrackAsset[] tracks = timeline.GetRootTracks().ToArray();
            for (int i = 0; i < tracks.Length; i++)
            {
                timeline.DeleteTrack(tracks[i]);
            }
        }

        /// <summary>
        ///     カメラの表示切り替えトラックを生成します。
        /// </summary>
        /// <param name="timeline"> 追加先のTimelineアセットです。 </param>
        private static void CreateCameraActivationTrack(TimelineAsset timeline)
        {
            ActivationTrack track =
                timeline.CreateTrack<ActivationTrack>(null, CAMERA_ACTIVATION_TRACK_NAME);

            track.postPlaybackState = ActivationTrack.PostPlaybackState.Inactive;

            TimelineClip clip = track.CreateDefaultClip();
            clip.start = 0.0;
            clip.duration = SIDE_CAMERA_CUT_SECONDS;
        }

        /// <summary>
        ///     発砲直前から銃身を横から映すカメラを有効化するトラックを生成します。
        /// </summary>
        /// <param name="timeline"> 追加先のTimelineアセットです。 </param>
        private static void CreateSideCameraActivationTrack(TimelineAsset timeline)
        {
            ActivationTrack track =
                timeline.CreateTrack<ActivationTrack>(null, SIDE_CAMERA_ACTIVATION_TRACK_NAME);
            track.postPlaybackState = ActivationTrack.PostPlaybackState.Active;

            TimelineClip clip = track.CreateDefaultClip();
            clip.start = SIDE_CAMERA_CUT_SECONDS;
            clip.duration = MOVIE_DURATION_SECONDS - SIDE_CAMERA_CUT_SECONDS;
        }

        /// <summary>
        ///     戦闘開始演出と同じ進行カーブでスプライン位置を動かすトラックを生成します。
        /// </summary>
        /// <param name="timeline"> 追加先のTimelineアセットです。 </param>
        private static void CreateDollyPositionTrack(TimelineAsset timeline)
        {
            // StageStart.playable と同じキーで、0から1まで加速しながら進む。
            AnimationCurve curve = CreateSmoothCurve(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(1.0f, 0.0f),
                new Keyframe(3.5f, 0.56f),
                new Keyframe(5.5f, 0.79f),
                new Keyframe(CAMERA_MOVE_DURATION_SECONDS, 1.0f),
                new Keyframe(SIDE_CAMERA_CUT_SECONDS, 1.0f));

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
            timelineClip.duration = SIDE_CAMERA_CUT_SECONDS;
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

            AnimationClip clip = CreateOrLoadClip(DOLLY_TARGET_CLIP_PATH);
            SetTransformCurve(
                clip,
                LOCAL_POSITION_X_PROPERTY,
                new Keyframe(0.0f, 0.006f),
                new Keyframe(CAMERA_MOVE_DURATION_SECONDS, 0.006f),
                new Keyframe(GetCameraTurnTime(0.25f), 0.481f),
                new Keyframe(GetCameraTurnTime(0.5f), 0.73f),
                new Keyframe(GetCameraTurnTime(0.75f), 0.544f),
                new Keyframe(CAMERA_TURN_END_SECONDS, 0.0f),
                new Keyframe(SIDE_CAMERA_CUT_SECONDS, 0.0f));
            SetTransformCurve(
                clip,
                LOCAL_POSITION_Y_PROPERTY,
                new Keyframe(0.0f, 0.0f),
                new Keyframe(1.0f, 0.0f),
                new Keyframe(4.016667f, 1.15f),
                new Keyframe(5.5f, 1.15f),
                new Keyframe(CAMERA_MOVE_DURATION_SECONDS, 1.267f),
                new Keyframe(GetCameraTurnTime(0.25f), 1.29f),
                new Keyframe(GetCameraTurnTime(0.5f), 1.33f),
                new Keyframe(GetCameraTurnTime(0.75f), 1.35f),
                new Keyframe(CAMERA_TURN_END_SECONDS, 1.37f),
                new Keyframe(SIDE_CAMERA_CUT_SECONDS, 1.37f));
            SetTransformCurve(
                clip,
                LOCAL_POSITION_Z_PROPERTY,
                new Keyframe(0.0f, -0.007f),
                new Keyframe(CAMERA_MOVE_DURATION_SECONDS, -0.007f),
                new Keyframe(GetCameraTurnTime(0.25f), -0.107f),
                new Keyframe(GetCameraTurnTime(0.5f), -0.588f),
                new Keyframe(GetCameraTurnTime(0.75f), -1.132f),
                new Keyframe(CAMERA_TURN_END_SECONDS, -1.388f),
                new Keyframe(SIDE_CAMERA_CUT_SECONDS, -1.388f));
            EditorUtility.SetDirty(clip);

            TimelineClip timelineClip = track.CreateClip(clip);
            timelineClip.start = 0.0;
            timelineClip.duration = SIDE_CAMERA_CUT_SECONDS;
        }

        /// <summary>
        ///     開幕の明転と、顔が見えた後の暗転を行うトラックを生成します。
        /// </summary>
        /// <param name="timeline"> 追加先のTimelineアセットです。 </param>
        private static void CreateBlackoutTrack(TimelineAsset timeline)
        {
            AnimationClip blackoutInClip = CreateOrLoadClip(BLACKOUT_IN_CLIP_PATH);
            AnimationCurve blackoutInCurve = new(
                new Keyframe(0.0f, 1.0f),
                new Keyframe(0.2f, 1.0f),
                new Keyframe(BLACKOUT_IN_DURATION_SECONDS, 0.0f));
            EditorCurveBinding alphaBinding = EditorCurveBinding.FloatCurve(
                string.Empty,
                typeof(CanvasGroup),
                CANVAS_GROUP_ALPHA_PROPERTY);
            AnimationUtility.SetEditorCurve(blackoutInClip, alphaBinding, blackoutInCurve);
            EditorUtility.SetDirty(blackoutInClip);

            AnimationClip blackoutOutClip = CreateOrLoadClip(BLACKOUT_OUT_CLIP_PATH);
            AnimationCurve blackoutOutCurve = new(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(BLACKOUT_OUT_DURATION_SECONDS, 1.0f));
            AnimationUtility.SetEditorCurve(blackoutOutClip, alphaBinding, blackoutOutCurve);
            EditorUtility.SetDirty(blackoutOutClip);

            AnimationTrack track =
                timeline.CreateTrack<AnimationTrack>(null, BLACKOUT_TRACK_NAME);
            TimelineClip blackoutInTimelineClip = track.CreateClip(blackoutInClip);
            blackoutInTimelineClip.start = 0.0;
            blackoutInTimelineClip.duration = BLACKOUT_IN_DURATION_SECONDS;

            TimelineClip blackoutOutTimelineClip = track.CreateClip(blackoutOutClip);
            blackoutOutTimelineClip.start = BLACKOUT_OUT_START_SECONDS;
            blackoutOutTimelineClip.duration = BLACKOUT_OUT_DURATION_SECONDS;
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
        ///     Symphonyが14号へ銃を向け、発砲するアニメーショントラックを生成します。
        /// </summary>
        /// <param name="timeline"> 追加先のTimelineアセットです。 </param>
        private static void CreateSymphonyTrack(TimelineAsset timeline)
        {
            AnimationTrack track =
                timeline.CreateTrack<AnimationTrack>(null, SYMPHONY_TRACK_NAME);
            track.trackOffset = TrackOffset.ApplySceneOffsets;

            AnimationClip idleClip = LoadHumanoidClip(SOLDIER_IDLE_CLIP_PATH);
            if (idleClip != null)
            {
                TimelineClip idleTimelineClip = track.CreateClip(idleClip);
                idleTimelineClip.start = 0.0;
                idleTimelineClip.duration = SYMPHONY_AIM_START_SECONDS;
                if (idleTimelineClip.asset is AnimationPlayableAsset idlePlayableAsset)
                {
                    idlePlayableAsset.loop = AnimationPlayableAsset.LoopMode.On;
                }
            }

            AnimationClip aimClip = LoadHumanoidClip(SYMPHONY_AIM_CLIP_PATH);
            if (aimClip != null)
            {
                TimelineClip aimTimelineClip = track.CreateClip(aimClip);
                aimTimelineClip.start = SYMPHONY_AIM_START_SECONDS - SYMPHONY_CLIP_BLEND_SECONDS;
                aimTimelineClip.duration =
                    SYMPHONY_SHOT_START_SECONDS - aimTimelineClip.start;
                aimTimelineClip.easeInDuration = SYMPHONY_CLIP_BLEND_SECONDS;
                // HGPose冒頭の構えをほぼ静止させ、アイドルからのブレンドで銃を上げる。
                // 同クリップ冒頭にある反動は、下の発砲クリップで通常速度再生する。
                aimTimelineClip.timeScale = SYMPHONY_AIM_TIME_SCALE;
                if (aimTimelineClip.asset is AnimationPlayableAsset aimPlayableAsset)
                {
                    aimPlayableAsset.loop = AnimationPlayableAsset.LoopMode.Off;
                }
            }

            AnimationClip shotClip = LoadHumanoidClip(SYMPHONY_AIM_CLIP_PATH);
            if (shotClip == null)
            {
                Debug.LogWarning(
                    $"[{nameof(DemoEndTimelineBuilder)}] " +
                    $"{SYMPHONY_AIM_CLIP_PATH} が見つかりません。発砲モーションを生成できませんでした。");
                return;
            }

            double shotDuration = SYMPHONY_SHOT_FRAME_COUNT / shotClip.frameRate;
            double shotBlendDuration = 1.0 / shotClip.frameRate;
            TimelineClip shotTimelineClip = track.CreateClip(shotClip);
            // 構えクリップと1フレーム重ね、射撃開始時にウェイトが0となって
            // Animatorの基準姿勢が露出することを防ぐ。
            shotTimelineClip.start = SYMPHONY_SHOT_START_SECONDS - shotBlendDuration;
            shotTimelineClip.duration = shotDuration;
            shotTimelineClip.clipIn = 0.0;
            shotTimelineClip.timeScale = 1.0;
            shotTimelineClip.easeInDuration = shotBlendDuration;
            if (shotTimelineClip.asset is AnimationPlayableAsset shotPlayableAsset)
            {
                shotPlayableAsset.loop = AnimationPlayableAsset.LoopMode.Off;
            }
        }

        /// <summary>
        ///     発砲エフェクトとSEを起動するSignalトラックを生成します。
        /// </summary>
        /// <param name="timeline"> 追加先のTimelineアセットです。 </param>
        private static void CreateGunshotSignalTrack(TimelineAsset timeline)
        {
            SignalAsset weaponRevealSignal =
                CreateOrLoadSignal(WEAPON_REVEAL_SIGNAL_ASSET_PATH);
            SignalAsset signal = CreateOrLoadSignal(GUNSHOT_SIGNAL_ASSET_PATH);
            SignalTrack track =
                timeline.CreateTrack<SignalTrack>(null, GUNSHOT_SIGNAL_TRACK_NAME);

            SignalEmitter revealEmitter =
                track.CreateMarker<SignalEmitter>(SIDE_CAMERA_CUT_SECONDS);
            revealEmitter.asset = weaponRevealSignal;
            revealEmitter.emitOnce = true;

            SignalEmitter emitter = track.CreateMarker<SignalEmitter>(GUNSHOT_SECONDS);
            emitter.asset = signal;
            emitter.emitOnce = true;
        }

        /// <summary>
        ///     Transformのfloatカーブを設定します。
        /// </summary>
        /// <param name="clip"> 設定先のAnimationClipです。 </param>
        /// <param name="propertyName"> Transformのプロパティ名です。 </param>
        /// <param name="keyframes"> 設定するキーフレームです。 </param>
        private static void SetTransformCurve(
            AnimationClip clip,
            string propertyName,
            params Keyframe[] keyframes)
        {
            EditorCurveBinding binding = EditorCurveBinding.FloatCurve(
                string.Empty,
                typeof(Transform),
                propertyName);
            AnimationUtility.SetEditorCurve(clip, binding, CreateSmoothCurve(keyframes));
        }

        /// <summary>
        ///     キー間の速度変化が連続する、オーバーシュートを抑えたカーブを生成します。
        /// </summary>
        /// <param name="keyframes"> カーブを構成するキーフレームです。 </param>
        /// <returns> Clamped Auto接線を設定したカーブです。 </returns>
        private static AnimationCurve CreateSmoothCurve(params Keyframe[] keyframes)
        {
            AnimationCurve curve = new(keyframes);
            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(
                    curve,
                    i,
                    AnimationUtility.TangentMode.ClampedAuto);
                AnimationUtility.SetKeyRightTangentMode(
                    curve,
                    i,
                    AnimationUtility.TangentMode.ClampedAuto);
            }

            return curve;
        }

        /// <summary>
        ///     振り向き区間内の正規化時刻をTimeline時刻へ変換します。
        /// </summary>
        /// <param name="normalizedTime"> 振り向き区間内の0から1の時刻です。 </param>
        /// <returns> Timeline上の時刻です。 </returns>
        private static float GetCameraTurnTime(float normalizedTime)
        {
            return Mathf.Lerp(
                CAMERA_TURN_START_SECONDS,
                CAMERA_TURN_END_SECONDS,
                normalizedTime);
        }

        /// <summary>
        ///     SignalAssetを生成または取得します。
        /// </summary>
        /// <param name="assetPath"> SignalAssetのパスです。 </param>
        /// <returns> 生成または取得したSignalAssetです。 </returns>
        private static SignalAsset CreateOrLoadSignal(string assetPath)
        {
            SignalAsset existingSignal = AssetDatabase.LoadAssetAtPath<SignalAsset>(assetPath);
            if (existingSignal != null)
            {
                return existingSignal;
            }

            EnsureDirectory(assetPath);
            SignalAsset signal = ScriptableObject.CreateInstance<SignalAsset>();
            signal.name = Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.CreateAsset(signal, assetPath);
            return signal;
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
