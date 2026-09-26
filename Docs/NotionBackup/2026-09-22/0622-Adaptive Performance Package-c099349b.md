# Adaptive Performance Package

- id: 32f7c2c6-cc02-80f1-a929-e383c099349b
- path: Symphony Kill Chord / システム概要 / Adaptive Performance Package
- last_edited: 2026-04-05T19:43:48.568Z

- 概要: Adaptive Performance Packageについての説明
- カテゴリー: 開発用


## 説明
　Adaptive PerformanceはUnityのPackageであり、モバイルデバイスの温度とバッテリー残量を取得して、パフォーマンスの適応をランタイムに行うことができる。

### 参考資料
公式説明書：
‣
公式説明書 その２：
‣
APIドキュメント：
‣
その他：
‣
‣

## 詳細
　Adaptive PerformanceはUnityのPackageであり、モバイルデバイスの温度とバッテリー残量を取得して、パフォーマンスの適応をランタイムに行うことができる。
　注意すべきなのは、Adaptive PerformanceのAPIは、ゲーム自体の制御しかできなく、モバイルデバイスからデバイス情報を取得する機能がない。正確に動作させるには、モバイルデバイス情報を取得して渡てくれるProviderが必要。Providerと併用して、初めてパフォーマンス適応が機能する。
　※iOSはAndroidと違い、デバイス情報を提供する仕組みなどはまともに用意されていないらしく、正式的なProviderは現時点存在しない。つまり、Adaptive PerformanceはiOS端末でできることはPCとあまり変わらない。

#### Packageのインストール
  　Unity EditorのWindow → Package Management → Package Managerを開き、「adaptive performance」を検索すればPackageが表示されるので、それをインストールする。
　同時に出てくる「Adaptive Performance Android」は、Androidデバイス用のProviderなので、それもインストールする。
  [image: image.png] attachment:4b51c524-b40c-4c38-8c54-358e3dc7d752:image.png

#### 設定
  　※ここの記載は**Unity 6000.3.10f.1**に基づいた内容です。
  　Edit → Project Settingsを開き、Adaptive Performanceで設定を行う。
　「Enable Adaptive Performance」をチェックして、対象デバイスのタブで使いたいProviderを選択する。今回は「Adaptive Performance Android」がインストールされているので、「Android Provider」がここで出現する。これをチェックすると、内部で自動的にADPF（Android Dynamic Performance Framework）からデバイス情報を取得するようになる。
  [image: image.png] attachment:6c89b8cd-ee38-4a51-88a0-82d8046b8ed1:image.png
  　File → Build ProfilesにてAndroidのBuild Profileを一つ作成して、Add Settingボタンをクリックし、「Adaptive Performance Settings」を選択する
  [image: image.png] attachment:90eb30f4-7ada-43d5-869c-43f456119139:image.png
  　ここで、パフォーマンス最適化関連の項目を設定できる。
  [image: image.png] attachment:0be6cab4-2587-44c9-89e7-ac8ba4b49598:image.png

#### Samsung Developerが公開したサンプルコード
  ‣
  上記ページからダウンロードしたコードです。アカウント作成など手間がかかるので、ここで貼っておきます。ちなみにサンプルプロジェクトもあります。
```C#
using UnityEngine;

using UnityEngine.AdaptivePerformance;


public class AdaptivePerformanceController : MonoBehaviour
{
    IAdaptivePerformance ap = null;
    IDevicePerformanceControl perfCtrl;
    IThermalStatus thermalStatus;

    public float LOD_level = 0.0f;  // 0.6 ~ 3.0

    float startTime = 0.0f;

    float lastChangeTimeStamp = 0.0f;
    float lastGpuLevelRaiseTimeStamp = 0.0f;
    float lastCpuLevelRaiseTimeStamp = 0.0f;
    float targetFrameRateHitTimestamp = 0.0f;

    bool preferRaiseLOD = false;

    bool active = false;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        ap = Holder.Instance;
        if (ap != null)
        {
            perfCtrl = ap.DevicePerformanceControl;
            thermalStatus = ap.ThermalStatus;
            ap.DevicePerformanceControl.AutomaticPerformanceControl = false;

            perfCtrl.CpuLevel = 0;
            perfCtrl.GpuLevel = 0;
        }
        SetStatus(true);
    }

    public void SetStatus(bool enabled)
    {
        if (!ap.Active)
            return;

        active = enabled;
        Debug.Log("Start " + active);

        if (active)
        {
            Application.targetFrameRate = 60;
            ap.ThermalStatus.ThermalEvent += OnThermalEvent;
            StartGameMode();
        }
        else
        {
            Application.targetFrameRate = 120;
            ap.ThermalStatus.ThermalEvent -= OnThermalEvent;
        }

        startTime = Time.time;
        lastChangeTimeStamp = startTime;
        targetFrameRateHitTimestamp = startTime;
    }

    public void StartGameMode()
    {
        if (!active)
            return;

        perfCtrl.CpuLevel = 2;
        perfCtrl.GpuLevel = 1;
    }

    public void StartMenuMode()
    {
        if (!active)
            return;

        perfCtrl.CpuLevel = 0;
        perfCtrl.GpuLevel = 0;
    }

    private void UpdateTargetFrameRate(WarningLevel warningLevel)
    {
        if (!active)
            return;

        int targetFps = 60;
        switch (warningLevel)
        {
            case WarningLevel.NoWarning:
                targetFps = 60;
                break;
            case WarningLevel.ThrottlingImminent:
                targetFps = 30;
                break;
            case WarningLevel.Throttling:
                targetFps = 28;
                break;
        }

        if (Application.targetFrameRate != targetFps)
        {
            Application.targetFrameRate = targetFps;
            lastChangeTimeStamp = Time.time;
            Debug.Log("target framerate: " + targetFps);
        }
    }

    /* void OnThermalEvent(ThermalMetrics ev)
    {
        UpdateTargetFrameRate(ev.WarningLevel);
    }*/

    void OnThermalEvent(ThermalMetrics ev)
    {
        switch (ev.WarningLevel)
        {
            case WarningLevel.NoWarning:
                Application.targetFrameRate = 60;
                break;
            case WarningLevel.ThrottlingImminent:
                Application.targetFrameRate = 30;
                break;
            case WarningLevel.Throttling:
                Application.targetFrameRate = 15;
                break;

        }
    }

    void OnBottleneckChange(object obj, PerformanceBottleneckChangeEventArgs ev)
    {
        if (ev.PerformanceBottleneck == PerformanceBottleneck.TargetFrameRate)
            targetFrameRateHitTimestamp = Time.time;
    }

    bool CanLowerLOD()
    {
        return QualitySettings.lodBias > 1.0f;
    }

    void LowerLOD()
    {
        if (CanLowerLOD())
        {
            QualitySettings.lodBias = Mathf.Max(0.6f, QualitySettings.lodBias - 0.3f);
            lastChangeTimeStamp = Time.time;
            Debug.Log($"[ADP] Lower lodBias={QualitySettings.lodBias}");
        }
    }

    bool CanRaiseLOD()
    {
        return QualitySettings.lodBias < 3.0f;
    }

    void RaiseLOD()
    {
        if (CanRaiseLOD())
        {
            QualitySettings.lodBias = Mathf.Min(3.0f, QualitySettings.lodBias + 0.3f);
            lastChangeTimeStamp = Time.time;
            Debug.Log($"[ADP] Raise lodBias={QualitySettings.lodBias}");
            preferRaiseLOD = false;
        }
    }

    void LowerFramerate(WarningLevel level)
    {
        UpdateTargetFrameRate(level);
    }

    // Update is called once per frame
    void Update()
    {
        if (!active)
            return;

        if (LOD_level != 0.0f)
        {
            Debug.LogErrorFormat("[ADP] {0} : set LOD_level[{1}]", Time.time, LOD_level);
            QualitySettings.lodBias = LOD_level;
        }

        var timestamp = Time.time;

        // after a change wait at least 5s before making additional changes
        if (timestamp - lastChangeTimeStamp < 5.0f)
            return;


        ThermalMetrics thermals = thermalStatus.ThermalMetrics; // IThermalStatus.ThermalMetrics.WarningLevel

        // prefer raising CPU/GPU levels only as long as device is reasonably cool
        bool preferRaiseLevels = (thermals.WarningLevel == WarningLevel.NoWarning);


        switch (ap.PerformanceStatus.PerformanceMetrics.PerformanceBottleneck)  // IAdaptivePerformance.PerformanceStatus.PerformanceMetrics.PerformanceBottleneck
        {
            case PerformanceBottleneck.GPU:
                Debug.LogFormat("[ADP] PerformanceBottleneck.GPU ");
                LowerLOD();  //A lower value leads to a shorter view distance before a lower resolution LOD is picked.
                break;
            case PerformanceBottleneck.CPU:
                Debug.LogFormat("[ADP] PerformanceBottleneck.CPU ");
                break;
            case PerformanceBottleneck.TargetFrameRate:
                Debug.LogFormat("[ADP] PerformanceBottleneck.TargetFrameRate ");
                break;
            case PerformanceBottleneck.Unknown:
                Debug.LogFormat("[ADP] PerformanceBottleneck.Unknown");
                break;
        }
    }

}

```

#### ChatGPTからもらったサンプルコード
```C#
// Unity 6.3 LTS + Adaptive Performance 6.0.0
// Cross-platform performance management architecture
// Android: uses Adaptive Performance
// iOS/Other: fallback to manual frame-time based system

using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.AdaptivePerformance;
#endif

/// <summary>
///     自作インタフェース。
///     Adaptive Performanceがバージョンアップする時、
///     APIやプロパティが変わる可能性があるので、
///     これがあれば対応時の手間を省ける。
/// </summary>
public interface IPerformanceProvider
{
    /// <summary>
    ///     初期化処理。
    /// </summary>
    public void Initialize();
    /// <summary>
    ///     フレーム毎実行される処理。
    /// </summary>
    /// <param name="deltaTime"></param>
    public void Tick(float deltaTime);

    int CpuLevel { get; }
    int GpuLevel { get; }
    /// <summary> 1フレームの経過時間 </summary>
    float FrameTime { get; }

    /// <summary> GPUがボトルネックに達したか </summary>
    bool IsGpuBottleneck { get; }
}

#if UNITY_ANDROID
/// <summary>
///     Android環境用のProvider。
/// </summary>
public class AndroidPerformanceProvider : IPerformanceProvider
{
    private IAdaptivePerformance ap;

    // ProviderからCPUの動作状況を取得する
    public int CpuLevel => ap?.PerformanceStatus.PerformanceMetrics.CurrentCpuLevel ?? 0;
    // ProviderからGPUの動作状況を取得する
    public int GpuLevel => ap?.PerformanceStatus.PerformanceMetrics.CurrentGpuLevel ?? 0;
    public float FrameTime { get; private set; }

    // ProviderからGPUの動作状況を取得する
    public bool IsGpuBottleneck =>
        ap != null &&
        ap.PerformanceStatus.PerformanceMetrics.PerformanceBottleneck == PerformanceBottleneck.GPU;

    public void Initialize()
    {
        // Adaptive Performanceのインスタンスを取得する
        ap = Holder.Instance;
        Debug.Log($"[AP] Android Provider Active: {ap?.Active}");
    }

    public void Tick(float deltaTime)
    {
        FrameTime = deltaTime;
    }
}
#endif

/// <summary>
///     PC/iOS環境用のProvider。
/// </summary>
public class FallbackPerformanceProvider : IPerformanceProvider
{
    private float smoothedFrameTime;

    public int CpuLevel => 0;
    public int GpuLevel => 0;
    public float FrameTime => smoothedFrameTime;

    public bool IsGpuBottleneck => smoothedFrameTime > (1f / 55f);

    public void Initialize()
    {
        smoothedFrameTime = 0;
        Debug.Log("[AP] Fallback Provider Initialized");
    }

    public void Tick(float deltaTime)
    {
        smoothedFrameTime = Mathf.Lerp(smoothedFrameTime, deltaTime, 0.1f);
    }
}

/// <summary>
///     実際にAdaptive Performanceを用いて実行状況を取得、判断して、
///     パフォーマンス制御を行うオブジェクト
/// </summary>
public class PerformanceManager : MonoBehaviour
{
    public static PerformanceManager Instance;

    private IPerformanceProvider provider;

    [Header("Targets")]
    public int targetFPS = 60;

    [Header("Dynamic Settings")]
    public float renderScale = 1.0f;
    public int qualityLevel = 2;

    private void Awake()
    {
        Instance = this;

#if UNITY_ANDROID
        provider = new AndroidPerformanceProvider();
#else
        provider = new FallbackPerformanceProvider();
#endif

        provider.Initialize();

        Application.targetFrameRate = targetFPS;
    }

    private void Update()
    {
        provider.Tick(Time.deltaTime);

        Evaluate();
    }

    /// <summary>
    ///     実行状況を評価し、パフォーマンス制御を行う
    /// </summary>
    private void Evaluate()
    {
        float frameTime = provider.FrameTime;

        // ---- Strategy ----
        // 1. Detect GPU bottleneck
        // 2. Adjust resolution
        // 3. Adjust quality

        if (provider.IsGpuBottleneck)
        {
            // GPUがボトルネックを達した場合、画質を下げる
            DecreaseQuality();
        }
        else if (frameTime < (1f / targetFPS) * 0.8f)
        {
            // フレームレートが想定フレームレートより高い場合、画質を上げる
            IncreaseQuality();
        }
    }

    /// <summary>
    ///     画質を下げる
    /// </summary>
    private void DecreaseQuality()
    {
        renderScale = Mathf.Max(0.7f, renderScale - 0.05f);
        qualityLevel = Mathf.Max(0, qualityLevel - 1);

        ApplySettings();
    }

    /// <summary>
    ///     画質を上げる
    /// </summary>
    private void IncreaseQuality()
    {
        renderScale = Mathf.Min(1.0f, renderScale + 0.05f);
        qualityLevel = Mathf.Min(QualitySettings.names.Length - 1, qualityLevel + 1);

        ApplySettings();
    }

    private void ApplySettings()
    {
        // Dynamic Resolution (URP/HDRP compatible approach may differ)
        ScalableBufferManager.ResizeBuffers(renderScale, renderScale);

        QualitySettings.SetQualityLevel(qualityLevel);
    }
}

/// <summary>
///     パフォーマンス関連のパラメータをUIで表示する
/// </summary>
public class PerformanceDebugUI : MonoBehaviour
{
    void OnGUI()
    {
        if (PerformanceManager.Instance == null) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 200));

        var pm = PerformanceManager.Instance;

        GUILayout.Label($"FPS Target: {pm.targetFPS}");
        GUILayout.Label($"Render Scale: {pm.renderScale:F2}");
        GUILayout.Label($"Quality: {pm.qualityLevel}");

        GUILayout.EndArea();
    }
}

```
