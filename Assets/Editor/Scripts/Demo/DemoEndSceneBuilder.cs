using KillChord.Demo.End;
using System.IO;
using TMPro;
using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace KillChord.Demo.Editor
{
    /// <summary>
    ///     体験版終了シーンの演出構成を生成するエディタ機能です。
    ///     戦闘開始演出と同じDollyリグを複製し、Timelineと案内UIを配線します。
    /// </summary>
    internal static class DemoEndSceneBuilder
    {
        /// <summary>
        ///     体験版終了シーンを再構築します。
        /// </summary>
        [MenuItem("KillChord/Demo/Build Demo End Scene")]
        public static void Build()
        {
            if (!EditorUtility.DisplayDialog(
                    "体験版終了シーンの再構築",
                    $"{SCENE_PATH} の内容をすべて置き換えます。よろしいですか？",
                    "実行する",
                    "やめる"))
            {
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            Execute();
        }

        /// <summary>
        ///     確認ダイアログを挟まずに体験版終了シーンを再構築します。
        /// </summary>
        internal static void Execute()
        {
            // OpenSceneは未参照アセットをアンロードするため、アセットの取得はシーンを開いた後に行う。
            Scene scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
            ClearScene(scene);

            DemoEndSequenceConfig config = CreateOrLoadConfig();
            TimelineAsset timeline = DemoEndTimelineBuilder.CreateOrLoad();
            if (config == null || timeline == null)
            {
                Debug.LogError(
                    $"[{nameof(DemoEndSceneBuilder)}] 設定アセットまたはTimelineアセットを準備できませんでした。");
                return;
            }

            DemoEndBgmView bgmView = BuildSceneRoot(config, out GameObject sceneRoot);
            MovieRig movieRig = BuildMovieRig(timeline);
            DemoEndView endView = BuildCanvas(out Animator blackoutAnimator);

            BindTimeline(movieRig, timeline, blackoutAnimator);
            WireSequenceInitializer(sceneRoot, config, movieRig.MovieView, endView, bgmView);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log(
                $"[{nameof(DemoEndSceneBuilder)}] 体験版終了シーンを再構築しました。" +
                $" Timeline: {DemoEndTimelineBuilder.TIMELINE_ASSET_PATH}");
        }

        private const string SCENE_PATH = "Assets/Level/Scenes/Demo/DemoEnd.unity";
        private const string CONFIG_ASSET_PATH =
            "Assets/Level/Data/Demo/DemoEndSequenceConfig.asset";
        private const string SOLDIER_MODEL_PATH =
            "Assets/Arts/Models/Soldier14/Soldier14_0808.fbx";
        private const string TITLE_LOGO_PATH =
            "Assets/Arts/Images/Sprites/Title/title_logo.png";
        private const string TITLE_BACKGROUND_PATH =
            "Assets/Arts/Images/Sprites/Title/Title_Background.png";
        private const string QR_CODE_PATH =
            "Assets/Arts/Images/Sprites/Demo/KillChored_HomePageQR[0914].png";
        private const string FONT_ASSET_PATH =
            "Assets/Arts/Fonts/KosugiMaru-Regular SDF.asset";

        private const string THANKS_MESSAGE_TEXT =
            "体験版をプレイしていただき、ありがとうございます。";
        private const string QR_CAPTION_TEXT = "製品版の最新情報はこちら";
        private const string PROMPT_TEXT = "攻撃ボタンでタイトルへ戻る";

        private const int CANVAS_SORTING_ORDER = 100;
        private const float CAMERA_FIELD_OF_VIEW = 60.0f;
        private const float CAMERA_NEAR_CLIP_PLANE = 0.01f;
        private const int CAMERA_PRIORITY = 10;
        private const float SOLDIER_FACING_ANGLE_Y = 180.0f;

        private const float TITLE_ART_ASPECT_RATIO = 1920.0f / 1080.0f;

        private const float MESSAGE_FONT_SIZE = 32.0f;
        private const float CAPTION_FONT_SIZE = 24.0f;
        private const float PROMPT_FONT_SIZE = 28.0f;

        /// <summary>
        ///     タイトル画面のロゴ配置を実測して求めたアンカー下限です。
        ///     Title.uxml の TitleLogo は横幅いっぱいで右下方向へずらして配置されています。
        /// </summary>
        private static readonly Vector2 TITLE_LOGO_ANCHOR_MIN = new(0.0255f, 0.0848f);

        /// <summary> タイトル画面のロゴ配置を実測して求めたアンカー上限です。 </summary>
        private static readonly Vector2 TITLE_LOGO_ANCHOR_MAX = new(1.0255f, 0.9898f);

        /// <summary>
        ///     Stage_02内でムービーリグを置く位置です。
        ///     PlayerSpawnPointのある原点は建物の内部で絵にならないため、開けた区画へ寄せています。
        /// </summary>
        private static readonly Vector3 MOVIE_ROOT_POSITION = new(-10.0f, 0.0f, -16.0f);

        /// <summary> 戦闘開始演出のDollyスプラインと同じ制御点です。 </summary>
        private static readonly Vector3[] SPLINE_KNOT_POSITIONS =
        {
            new(-0.448f, 0.177f, 0.536f),
            new(0.088f, 1.149f, 0.381f),
            new(0.351f, 1.26f, -0.043f),
            new(0.0f, 1.43f, -0.588f),
        };

        /// <summary>
        ///     演出設定アセットを生成または取得します。
        /// </summary>
        /// <returns> 生成または取得した設定アセットです。 </returns>
        private static DemoEndSequenceConfig CreateOrLoadConfig()
        {
            DemoEndSequenceConfig existingConfig =
                AssetDatabase.LoadAssetAtPath<DemoEndSequenceConfig>(CONFIG_ASSET_PATH);
            if (existingConfig != null)
            {
                return existingConfig;
            }

            string directoryPath = Path.GetDirectoryName(CONFIG_ASSET_PATH);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                AssetDatabase.Refresh();
            }

            DemoEndSequenceConfig config =
                ScriptableObject.CreateInstance<DemoEndSequenceConfig>();
            AssetDatabase.CreateAsset(config, CONFIG_ASSET_PATH);
            AssetDatabase.SaveAssets();
            return config;
        }

        /// <summary>
        ///     シーン内のルートオブジェクトをすべて削除します。
        /// </summary>
        /// <param name="scene"> 対象のシーンです。 </param>
        private static void ClearScene(Scene scene)
        {
            GameObject[] rootObjects = scene.GetRootGameObjects();
            for (int i = 0; i < rootObjects.Length; i++)
            {
                Object.DestroyImmediate(rootObjects[i]);
            }
        }

        /// <summary>
        ///     初期化モジュールを持つシーンルートを生成します。
        /// </summary>
        /// <param name="config"> 演出設定アセットです。 </param>
        /// <param name="sceneRoot"> 生成したシーンルートです。 </param>
        /// <returns> 生成したBGM再生Viewです。 </returns>
        private static DemoEndBgmView BuildSceneRoot(
            DemoEndSequenceConfig config,
            out GameObject sceneRoot)
        {
            sceneRoot = new GameObject("DemoEndSceneRoot");

            DemoEndSceneInitializer sceneInitializer =
                sceneRoot.AddComponent<DemoEndSceneInitializer>();
            SetObjectField(sceneInitializer, "_config", config);

            sceneRoot.AddComponent<DemoEndSequenceInitializer>();
            return sceneRoot.AddComponent<DemoEndBgmView>();
        }

        /// <summary>
        ///     Timelineと戦闘開始演出と同じDollyリグを生成します。
        /// </summary>
        /// <param name="timeline"> 再生するTimelineアセットです。 </param>
        /// <returns> 生成したムービーリグの参照一式です。 </returns>
        private static MovieRig BuildMovieRig(TimelineAsset timeline)
        {
            GameObject movieRoot = new("Movie");
            movieRoot.transform.position = MOVIE_ROOT_POSITION;

            GameObject directorObject = new("MovieDirector");
            directorObject.transform.SetParent(movieRoot.transform, false);

            PlayableDirector director = directorObject.AddComponent<PlayableDirector>();
            director.playableAsset = timeline;
            director.playOnAwake = false;
            director.extrapolationMode = DirectorWrapMode.None;

            DemoEndMovieView movieView = directorObject.AddComponent<DemoEndMovieView>();
            SetObjectField(movieView, "_director", director);

            GameObject splineObject = new("Dolly Spline");
            splineObject.transform.SetParent(movieRoot.transform, false);
            SplineContainer splineContainer = splineObject.AddComponent<SplineContainer>();
            splineObject.AddComponent<CinemachineSplineSmoother>();
            BuildSpline(splineContainer);

            GameObject targetObject = new("DollyCameraTarget");
            targetObject.transform.SetParent(splineObject.transform, false);
            targetObject.transform.localPosition = new Vector3(0.006f, 0.0f, -0.007f);
            Animator targetAnimator = targetObject.AddComponent<Animator>();

            GameObject cameraObject = new("Dolly CinemachineCamera");
            cameraObject.transform.SetParent(movieRoot.transform, false);
            cameraObject.transform.localPosition = SPLINE_KNOT_POSITIONS[0];

            CinemachineCamera cinemachineCamera = cameraObject.AddComponent<CinemachineCamera>();
            cinemachineCamera.Priority = CAMERA_PRIORITY;
            cinemachineCamera.Target.TrackingTarget = targetObject.transform;
            cinemachineCamera.Lens.FieldOfView = CAMERA_FIELD_OF_VIEW;
            cinemachineCamera.Lens.NearClipPlane = CAMERA_NEAR_CLIP_PLANE;

            CinemachineSplineDolly splineDolly =
                cameraObject.AddComponent<CinemachineSplineDolly>();
            splineDolly.Spline = splineContainer;
            splineDolly.PositionUnits = PathIndexUnit.Normalized;
            splineDolly.CameraPosition = 0.0f;

            cameraObject.AddComponent<CinemachineRotationComposer>();
            Animator cameraAnimator = cameraObject.AddComponent<Animator>();

            Animator soldierAnimator = BuildSoldier(movieRoot.transform);

            return new MovieRig(
                director,
                movieView,
                cameraObject,
                cameraAnimator,
                targetAnimator,
                soldierAnimator);
        }

        /// <summary>
        ///     戦闘開始演出と同じ制御点でスプラインを構築します。
        /// </summary>
        /// <param name="splineContainer"> 構築対象のスプラインコンテナです。 </param>
        private static void BuildSpline(SplineContainer splineContainer)
        {
            Spline spline = splineContainer.Spline;
            spline.Clear();

            for (int i = 0; i < SPLINE_KNOT_POSITIONS.Length; i++)
            {
                Vector3 position = SPLINE_KNOT_POSITIONS[i];
                spline.Add(
                    new BezierKnot(new float3(position.x, position.y, position.z)),
                    TangentMode.AutoSmooth);
            }
        }

        /// <summary>
        ///     Soldier14モデルを配置します。
        /// </summary>
        /// <param name="parent"> 配置先の親Transformです。 </param>
        /// <returns> 配置したモデルのAnimatorです。存在しない場合はnullです。 </returns>
        private static Animator BuildSoldier(Transform parent)
        {
            GameObject soldierModel =
                AssetDatabase.LoadAssetAtPath<GameObject>(SOLDIER_MODEL_PATH);
            if (soldierModel == null)
            {
                Debug.LogWarning(
                    $"[{nameof(DemoEndSceneBuilder)}] {SOLDIER_MODEL_PATH} が見つかりません。" +
                    "Soldier14の配置をスキップします。");
                return null;
            }

            GameObject soldierInstance =
                (GameObject)PrefabUtility.InstantiatePrefab(soldierModel);
            soldierInstance.name = "Soldier14";
            soldierInstance.transform.SetParent(parent, false);

            // カメラの到達位置が背面側のため、正面を向かせて顔が映るようにする。
            soldierInstance.transform.localRotation =
                Quaternion.Euler(0.0f, SOLDIER_FACING_ANGLE_Y, 0.0f);

            Animator animator = soldierInstance.GetComponent<Animator>();
            if (animator == null)
            {
                animator = soldierInstance.AddComponent<Animator>();
            }

            return animator;
        }

        /// <summary>
        ///     暗転と案内UIを持つCanvasを生成します。
        /// </summary>
        /// <param name="blackoutAnimator"> 暗転をアニメートするAnimatorです。 </param>
        /// <returns> 生成した終了画面Viewです。 </returns>
        private static DemoEndView BuildCanvas(out Animator blackoutAnimator)
        {
            GameObject canvasObject = new(
                "DemoEndCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = CANVAS_SORTING_ORDER;

            CanvasScaler canvasScaler = canvasObject.GetComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920.0f, 1080.0f);

            RectTransform blackout = CreateStretchedUiObject("Blackout", canvasObject.transform);
            Image blackoutImage = blackout.gameObject.AddComponent<Image>();
            blackoutImage.color = Color.black;
            blackoutImage.raycastTarget = false;
            CanvasGroup blackoutCanvasGroup = blackout.gameObject.AddComponent<CanvasGroup>();
            blackoutCanvasGroup.alpha = 1.0f;
            blackoutCanvasGroup.blocksRaycasts = false;
            blackoutCanvasGroup.interactable = false;
            blackoutAnimator = blackout.gameObject.AddComponent<Animator>();

            RectTransform endUi = CreateStretchedUiObject("EndUi", canvasObject.transform);
            CanvasGroup endUiCanvasGroup = endUi.gameObject.AddComponent<CanvasGroup>();
            endUiCanvasGroup.alpha = 0.0f;
            endUiCanvasGroup.blocksRaycasts = false;
            endUiCanvasGroup.interactable = false;

            CreateTitleBackground(endUi);
            CreateTitleLogo(endUi);
            CreateQrCode(endUi);
            CreateText(
                "Message",
                endUi,
                new Vector2(0.08f, 0.10f),
                new Vector2(0.62f, 0.18f),
                THANKS_MESSAGE_TEXT,
                MESSAGE_FONT_SIZE);

            RectTransform prompt = CreateText(
                "Prompt",
                endUi,
                new Vector2(0.08f, 0.04f),
                new Vector2(0.62f, 0.10f),
                PROMPT_TEXT,
                PROMPT_FONT_SIZE);
            CanvasGroup promptCanvasGroup = prompt.gameObject.AddComponent<CanvasGroup>();
            promptCanvasGroup.alpha = 0.0f;

            DemoEndView endView = canvasObject.AddComponent<DemoEndView>();
            SetObjectField(endView, "_blackoutCanvasGroup", blackoutCanvasGroup);
            SetObjectField(endView, "_endUiCanvasGroup", endUiCanvasGroup);
            SetObjectField(endView, "_promptCanvasGroup", promptCanvasGroup);

            return endView;
        }

        /// <summary>
        ///     タイトル画面と同じ背景イラストを、同じ切り取り方で敷きます。
        /// </summary>
        /// <param name="parent"> 配置先の親RectTransformです。 </param>
        private static void CreateTitleBackground(RectTransform parent)
        {
            RectTransform background = CreateStretchedUiObject("Background", parent);

            // UI Toolkit側の scale-and-crop と同じ見え方にするため、親を覆う比率で拡大する。
            background.anchorMin = new Vector2(0.5f, 0.5f);
            background.anchorMax = new Vector2(0.5f, 0.5f);
            background.pivot = new Vector2(0.5f, 0.5f);
            background.anchoredPosition = Vector2.zero;

            Image image = background.gameObject.AddComponent<Image>();
            image.raycastTarget = false;
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(TITLE_BACKGROUND_PATH);

            AspectRatioFitter fitter = background.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = TITLE_ART_ASPECT_RATIO;
        }

        /// <summary>
        ///     タイトル画面と同じ位置とサイズでロゴを配置します。
        /// </summary>
        /// <param name="parent"> 配置先の親RectTransformです。 </param>
        private static void CreateTitleLogo(RectTransform parent)
        {
            // Title.uxml の TitleLogo を実行時に計測した比率をそのまま使う。
            RectTransform logo = CreateUiObject(
                "TitleLogo",
                parent,
                TITLE_LOGO_ANCHOR_MIN,
                TITLE_LOGO_ANCHOR_MAX);

            Image image = logo.gameObject.AddComponent<Image>();
            image.raycastTarget = false;
            image.preserveAspect = true;
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(TITLE_LOGO_PATH);
        }

        /// <summary>
        ///     差し替え用のQRコード枠と説明文を配置します。
        /// </summary>
        /// <param name="parent"> 配置先の親RectTransformです。 </param>
        private static void CreateQrCode(RectTransform parent)
        {
            RectTransform qrCode = CreateUiObject(
                "QrCode",
                parent,
                new Vector2(0.72f, 0.20f),
                new Vector2(0.90f, 0.52f));

            Image image = qrCode.gameObject.AddComponent<Image>();
            image.raycastTarget = false;
            image.preserveAspect = true;
            image.color = Color.white;
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(QR_CODE_PATH);

            CreateText(
                "QrCaption",
                parent,
                new Vector2(0.66f, 0.14f),
                new Vector2(0.96f, 0.20f),
                QR_CAPTION_TEXT,
                CAPTION_FONT_SIZE);
        }

        /// <summary>
        ///     TextMeshProのテキストを配置します。
        /// </summary>
        /// <param name="name"> 生成するオブジェクト名です。 </param>
        /// <param name="parent"> 配置先の親RectTransformです。 </param>
        /// <param name="anchorMin"> アンカーの最小値です。 </param>
        /// <param name="anchorMax"> アンカーの最大値です。 </param>
        /// <param name="text"> 表示するテキストです。 </param>
        /// <param name="fontSize"> フォントサイズです。 </param>
        /// <returns> 生成したRectTransformです。 </returns>
        private static RectTransform CreateText(
            string name,
            RectTransform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            string text,
            float fontSize)
        {
            RectTransform rectTransform = CreateUiObject(name, parent, anchorMin, anchorMax);

            TextMeshProUGUI textComponent =
                rectTransform.gameObject.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.alignment = TextAlignmentOptions.Center;
            textComponent.raycastTarget = false;

            TMP_FontAsset fontAsset =
                AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_ASSET_PATH);
            if (fontAsset != null)
            {
                textComponent.font = fontAsset;
            }

            return rectTransform;
        }

        /// <summary>
        ///     親いっぱいに広がるUIオブジェクトを生成します。
        /// </summary>
        /// <param name="name"> 生成するオブジェクト名です。 </param>
        /// <param name="parent"> 配置先の親Transformです。 </param>
        /// <returns> 生成したRectTransformです。 </returns>
        private static RectTransform CreateStretchedUiObject(string name, Transform parent)
        {
            GameObject uiObject = new(name, typeof(RectTransform));
            RectTransform rectTransform = uiObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            return rectTransform;
        }

        /// <summary>
        ///     アンカーを指定してUIオブジェクトを生成します。
        /// </summary>
        /// <param name="name"> 生成するオブジェクト名です。 </param>
        /// <param name="parent"> 配置先の親Transformです。 </param>
        /// <param name="anchorMin"> アンカーの最小値です。 </param>
        /// <param name="anchorMax"> アンカーの最大値です。 </param>
        /// <returns> 生成したRectTransformです。 </returns>
        private static RectTransform CreateUiObject(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            RectTransform rectTransform = CreateStretchedUiObject(name, parent);
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            return rectTransform;
        }

        /// <summary>
        ///     Timelineの各トラックへシーン内オブジェクトを割り当てます。
        /// </summary>
        /// <param name="movieRig"> ムービーリグの参照一式です。 </param>
        /// <param name="timeline"> 割り当て先のTimelineアセットです。 </param>
        /// <param name="blackoutAnimator"> 暗転をアニメートするAnimatorです。 </param>
        private static void BindTimeline(
            MovieRig movieRig,
            TimelineAsset timeline,
            Animator blackoutAnimator)
        {
            BindTrack(
                movieRig.Director,
                timeline,
                DemoEndTimelineBuilder.CAMERA_ACTIVATION_TRACK_NAME,
                movieRig.CameraObject);
            BindTrack(
                movieRig.Director,
                timeline,
                DemoEndTimelineBuilder.DOLLY_POSITION_TRACK_NAME,
                movieRig.CameraAnimator);
            BindTrack(
                movieRig.Director,
                timeline,
                DemoEndTimelineBuilder.DOLLY_TARGET_TRACK_NAME,
                movieRig.TargetAnimator);
            BindTrack(
                movieRig.Director,
                timeline,
                DemoEndTimelineBuilder.BLACKOUT_TRACK_NAME,
                blackoutAnimator);
            BindTrack(
                movieRig.Director,
                timeline,
                DemoEndTimelineBuilder.SOLDIER_TRACK_NAME,
                movieRig.SoldierAnimator);
        }

        /// <summary>
        ///     名前で引いたトラックへ割り当て先を設定します。
        /// </summary>
        /// <param name="director"> 設定先のPlayableDirectorです。 </param>
        /// <param name="timeline"> 対象のTimelineアセットです。 </param>
        /// <param name="trackName"> 割り当てるトラック名です。 </param>
        /// <param name="value"> 割り当てるオブジェクトです。 </param>
        private static void BindTrack(
            PlayableDirector director,
            TimelineAsset timeline,
            string trackName,
            Object value)
        {
            TrackAsset track = DemoEndTimelineBuilder.FindTrack(timeline, trackName);
            if (track == null || value == null)
            {
                Debug.LogWarning(
                    $"[{nameof(DemoEndSceneBuilder)}] トラック {trackName} を割り当てられませんでした。" +
                    $" track={(track == null ? "null" : "ok")}" +
                    $" value={(value == null ? "null" : value.name)}");
                return;
            }

            director.SetGenericBinding(track, value);
        }

        /// <summary>
        ///     シーケンス制御モジュールへViewの参照を設定します。
        /// </summary>
        /// <param name="sceneRoot"> シーンルートオブジェクトです。 </param>
        /// <param name="config"> 演出設定アセットです。 </param>
        /// <param name="movieView"> デモムービーViewです。 </param>
        /// <param name="endView"> 終了画面Viewです。 </param>
        /// <param name="bgmView"> BGM再生Viewです。 </param>
        private static void WireSequenceInitializer(
            GameObject sceneRoot,
            DemoEndSequenceConfig config,
            DemoEndMovieView movieView,
            DemoEndView endView,
            DemoEndBgmView bgmView)
        {
            DemoEndSequenceInitializer sequenceInitializer =
                sceneRoot.GetComponent<DemoEndSequenceInitializer>();

            SetObjectField(sequenceInitializer, "_config", config);
            SetObjectField(sequenceInitializer, "_movieView", movieView);
            SetObjectField(sequenceInitializer, "_endView", endView);
            SetObjectField(sequenceInitializer, "_bgmView", bgmView);
        }

        /// <summary>
        ///     SerializeFieldへオブジェクト参照を設定します。
        /// </summary>
        /// <param name="target"> 設定先のコンポーネントです。 </param>
        /// <param name="fieldName"> 設定するフィールド名です。 </param>
        /// <param name="value"> 設定する値です。 </param>
        private static void SetObjectField(Object target, string fieldName, Object value)
        {
            SerializedObject serializedObject = new(target);
            SerializedProperty property = serializedObject.FindProperty(fieldName);
            if (property == null)
            {
                Debug.LogWarning(
                    $"[{nameof(DemoEndSceneBuilder)}] " +
                    $"{target.GetType().Name} に {fieldName} が見つかりません。");
                return;
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        ///     Timelineへ割り当てるムービーリグの参照一式です。
        /// </summary>
        private readonly struct MovieRig
        {
            /// <summary>
            ///     ムービーリグの参照一式を生成します。
            /// </summary>
            /// <param name="director"> Timelineを再生するPlayableDirectorです。 </param>
            /// <param name="movieView"> デモムービーViewです。 </param>
            /// <param name="cameraObject"> Dollyカメラのオブジェクトです。 </param>
            /// <param name="cameraAnimator"> Dollyカメラのアニメーターです。 </param>
            /// <param name="targetAnimator"> 注視点のアニメーターです。 </param>
            /// <param name="soldierAnimator"> Soldier14のアニメーターです。 </param>
            public MovieRig(
                PlayableDirector director,
                DemoEndMovieView movieView,
                GameObject cameraObject,
                Animator cameraAnimator,
                Animator targetAnimator,
                Animator soldierAnimator)
            {
                Director = director;
                MovieView = movieView;
                CameraObject = cameraObject;
                CameraAnimator = cameraAnimator;
                TargetAnimator = targetAnimator;
                SoldierAnimator = soldierAnimator;
            }

            /// <summary> Timelineを再生するPlayableDirectorです。 </summary>
            public PlayableDirector Director { get; }

            /// <summary> デモムービーViewです。 </summary>
            public DemoEndMovieView MovieView { get; }

            /// <summary> Dollyカメラのオブジェクトです。 </summary>
            public GameObject CameraObject { get; }

            /// <summary> Dollyカメラのアニメーターです。 </summary>
            public Animator CameraAnimator { get; }

            /// <summary> 注視点のアニメーターです。 </summary>
            public Animator TargetAnimator { get; }

            /// <summary> Soldier14のアニメーターです。 </summary>
            public Animator SoldierAnimator { get; }
        }
    }
}
