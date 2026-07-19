using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

// 设置界面控制器。
// 负责把 UI 控件、AudioMixer、Screen 设置和 GameSettingsStore(settings.json) 同步起来。
public class GameSettingController : MonoBehaviour
{
    // AudioMixer 暴露参数名，需要和 Mixer 面板里的 Exposed Parameters 一致。
    private const string MasterMixerParameter = "MasterMixer";
    private const string MusicMixerParameter = "MusicMixer";
    private const string VFMixerParameter = "VFMixer";

    public static GameSettingController instance;

    [Header("Language")]
    public Dropdown languageDropDown;

    [Header("Resolution")]
    public Dropdown resolutionDropDown;
    [SerializeField] private Resolution[] resolutions;

    [Header("Fullscreen")]
    public Toggle fullScreenController;

    [Header("Controller Vibration")]
    public Toggle controllerVibraton;
    public bool isVibration;

    [Header("Audio")]
    public AudioMixer masterMixer;
    public AudioMixer musicMixer;
    public AudioMixer vfMixer;
    public Slider master;
    public Slider music;
    public Slider vf;

    private const float FpsSampleInterval = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool showDebugFps = true;
    private float fps;
    private GUIStyle style;

    private void Awake()
    {
        // 设置界面全局入口；Controller 会读取这里的 isVibration 判断是否震动。
        instance = this;
        Application.targetFrameRate = 280;
    }

    private void Start()
    {
        // 先加载 JSON 设置，再根据设置初始化语言、分辨率、全屏、震动和音量 UI。
        GameSettingsStore.Load();

        InitializeLanguageDropDown();
        InitialzieResolutionDropDown();
        statusCheck();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        style = new GUIStyle();
        style.fontSize = 30;
        style.normal.textColor = Color.white;
        if (showDebugFps)
        {
            StartCoroutine(SampleFps());
        }
#endif
    }

    /// <summary>
    /// 绘制仅用于调试或开发构建的即时模式界面。
    /// </summary>
    /// <remarks>
    /// 使用注意：由 Unity 按生命周期或消息规则自动调用，不要从普通业务代码直接调用。
    /// </remarks>
    private void OnGUI()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // 简单 FPS 显示，用于测试性能。
        if (showDebugFps && style != null)
        {
            GUI.Label(new Rect(0, Screen.height - 40, 200, 200), "FPS: " + fps.ToString("f2"), style);
        }
#endif
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>
    /// 通过固定时间窗口内的 Time.frameCount 差值统计 FPS，不需要独立 Update。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 GameSettingController 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private IEnumerator SampleFps()
    {
        // 通过固定时间窗口内的 Time.frameCount 差值统计 FPS，不需要独立 Update。
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(FpsSampleInterval);
        while (enabled && showDebugFps)
        {
            int startFrame = Time.frameCount;
            float startTime = Time.realtimeSinceStartup;
            yield return wait;

            float elapsed = Time.realtimeSinceStartup - startTime;
            fps = elapsed > 0f ? (Time.frameCount - startFrame) / elapsed : 0f;
        }
    }
#endif

    /// <summary>
    /// Dropdown 改变时，根据索引找到去重后的 Resolution。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 GameSettingController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void SetResolution(int resolutionIndex)
    {
        // Dropdown 改变时，根据索引找到去重后的 Resolution。
        if (!TryGetResolution(resolutionIndex, out Resolution resolution))
        {
            return;
        }

        // 应用分辨率并写入 settings.json。
        GameSettingsData settings = GameSettingsStore.Current;
        Screen.SetResolution(resolution.width, resolution.height, settings.fullscreen);
        settings.resolutionWidth = resolution.width;
        settings.resolutionHeight = resolution.height;
        GameSettingsStore.Save();
    }

    /// <summary>
    /// Unity 返回的 Screen.resolutions 可能同宽高不同刷新率，因此先按宽高去重。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 GameSettingController 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void InitialzieResolutionDropDown()
    {
        // Unity 返回的 Screen.resolutions 可能同宽高不同刷新率，因此先按宽高去重。
        resolutions = GetUniqueResolutions(Screen.resolutions);
        if (resolutions == null || resolutions.Length == 0)
        {
            resolutions = new[] { Screen.currentResolution };
        }

        resolutionDropDown.ClearOptions();

        List<string> options = new List<string>();
        for (int i = 0; i < resolutions.Length; i++)
        {
            options.Add(resolutions[i].width + "x" + resolutions[i].height);
        }

        resolutionDropDown.AddOptions(options);

        // 根据 JSON 中保存的宽高，反查当前 Dropdown 应该选中哪一项。
        GameSettingsData settings = GameSettingsStore.Current;
        int savedResolutionIndex = FindResolutionIndex(settings.resolutionWidth, settings.resolutionHeight);
        if (savedResolutionIndex < 0)
        {
            savedResolutionIndex = FindResolutionIndex(Screen.currentResolution.width, Screen.currentResolution.height);
            if (savedResolutionIndex < 0)
            {
                savedResolutionIndex = 0;
            }

            settings.resolutionWidth = resolutions[savedResolutionIndex].width;
            settings.resolutionHeight = resolutions[savedResolutionIndex].height;
            GameSettingsStore.Save();
        }

        // 初始化 UI 时使用 WithoutNotify，避免触发 Dropdown.onValueChanged 导致重复保存。
        resolutionDropDown.SetValueWithoutNotify(savedResolutionIndex);
        resolutionDropDown.RefreshShownValue();
        Screen.SetResolution(resolutions[savedResolutionIndex].width, resolutions[savedResolutionIndex].height, settings.fullscreen);
    }

    /// <summary>
    /// 去掉相同宽高的重复分辨率，避免 Dropdown 显示多个一样的 1920x1080。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 GameSettingController 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private Resolution[] GetUniqueResolutions(Resolution[] source)
    {
        // 去掉相同宽高的重复分辨率，避免 Dropdown 显示多个一样的 1920x1080。
        if (source == null || source.Length == 0)
        {
            return source;
        }

        List<Resolution> unique = new List<Resolution>();
        HashSet<string> seen = new HashSet<string>();

        for (int i = 0; i < source.Length; i++)
        {
            string key = source[i].width + "x" + source[i].height;
            if (seen.Contains(key))
            {
                continue;
            }

            seen.Add(key);
            unique.Add(source[i]);
        }

        return unique.ToArray();
    }

    /// <summary>
    /// 根据 Unity Localization 当前可用语言动态生成 Dropdown 选项。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 GameSettingController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void InitializeLanguageDropDown()
    {
        // 根据 Unity Localization 当前可用语言动态生成 Dropdown 选项。
        languageDropDown.ClearOptions();
        var locales = LocalizationSettings.AvailableLocales.Locales;
        foreach (var locale in locales)
        {
            languageDropDown.options.Add(new Dropdown.OptionData(locale.LocaleName));
        }

        if (locales.Count == 0)
        {
            return;
        }

        // JSON 中保存的是语言索引，超出范围时回到 0。
        GameSettingsData settings = GameSettingsStore.Current;
        int languageIndex = settings.languageIndex;
        if (languageIndex < 0 || languageIndex >= locales.Count)
        {
            languageIndex = 0;
            settings.languageIndex = languageIndex;
            GameSettingsStore.Save();
        }

        // 初始化 Dropdown 时不触发保存事件。
        LocalizationSettings.SelectedLocale = locales[languageIndex];
        languageDropDown.SetValueWithoutNotify(languageIndex);
        languageDropDown.RefreshShownValue();
    }

    /// <summary>
    /// Dropdown 改变时切换语言，并保存到 JSON。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 GameSettingController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void SetLanguage(int index)
    {
        // Dropdown 改变时切换语言，并保存到 JSON。
        var locales = LocalizationSettings.AvailableLocales.Locales;
        if (index < 0 || index >= locales.Count)
        {
            return;
        }

        LocalizationSettings.SelectedLocale = locales[index];
        GameSettingsStore.Current.languageIndex = index;
        GameSettingsStore.Save();
    }

    /// <summary>
    /// 统一应用 JSON 中的设置到当前运行时状态和 UI 控件。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 GameSettingController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void statusCheck()
    {
        // 统一应用 JSON 中的设置到当前运行时状态和 UI 控件。
        judgeFullScreen();
        judgeVibrate();
        InitVolume();
    }

    /// <summary>
    /// 初始化震动 Toggle。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 GameSettingController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void judgeVibrate()
    {
        // 初始化震动 Toggle。
        isVibration = GameSettingsStore.Current.vibration;
        if (controllerVibraton != null)
        {
            controllerVibraton.SetIsOnWithoutNotify(isVibration);
        }
    }

    /// <summary>
    /// 用户改变震动开关时保存到 JSON。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 GameSettingController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void ToggleVibrate(bool isVibrate)
    {
        // 用户改变震动开关时保存到 JSON。
        isVibration = isVibrate;
        GameSettingsStore.Current.vibration = isVibrate;
        GameSettingsStore.Save();
    }

    /// <summary>
    /// 初始化全屏状态和 Toggle。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 GameSettingController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void judgeFullScreen()
    {
        // 初始化全屏状态和 Toggle。
        bool fullscreen = GameSettingsStore.Current.fullscreen;
        Screen.fullScreen = fullscreen;
        if (fullScreenController != null)
        {
            fullScreenController.SetIsOnWithoutNotify(fullscreen);
        }
    }

    /// <summary>
    /// 用户改变全屏开关时应用屏幕设置并保存。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 GameSettingController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void ToggleFullScreen(bool isFullScreen)
    {
        // 用户改变全屏开关时应用屏幕设置并保存。
        GameSettingsData settings = GameSettingsStore.Current;
        settings.fullscreen = isFullScreen;
        Screen.fullScreen = isFullScreen;
        Screen.SetResolution(settings.resolutionWidth, settings.resolutionHeight, isFullScreen);
        GameSettingsStore.Save();
    }

    /// <summary>
    /// 从 JSON 读取三路音量，应用到 Mixer 和 Slider。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 GameSettingController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void InitVolume()
    {
        // 从 JSON 读取三路音量，应用到 Mixer 和 Slider。
        GameSettingsData settings = GameSettingsStore.Current;
        SetMixerValue(masterMixer, MasterMixerParameter, settings.masterVolume);
        SetMixerValue(musicMixer, MusicMixerParameter, settings.musicVolume);
        SetMixerValue(vfMixer, VFMixerParameter, settings.vfVolume);

        if (master != null)
        {
            master.SetValueWithoutNotify(settings.masterVolume);
        }

        if (music != null)
        {
            music.SetValueWithoutNotify(settings.musicVolume);
        }

        if (vf != null)
        {
            vf.SetValueWithoutNotify(settings.vfVolume);
        }
    }

    /// <summary>
    /// 主音量 Slider 回调。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 GameSettingController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void SetMaserMixerVolume(float volume)
    {
        // 主音量 Slider 回调。
        SetMixerValue(masterMixer, MasterMixerParameter, volume);
        GameSettingsStore.Current.masterVolume = volume;
        GameSettingsStore.Save();
    }

    /// <summary>
    /// 音乐音量 Slider 回调。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 GameSettingController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void SetMusicMixerVolume(float volume)
    {
        // 音乐音量 Slider 回调。
        SetMixerValue(musicMixer, MusicMixerParameter, volume);
        GameSettingsStore.Current.musicVolume = volume;
        GameSettingsStore.Save();
    }

    /// <summary>
    /// 音效音量 Slider 回调。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 GameSettingController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void SetVFMixerVolume(float volume)
    {
        // 音效音量 Slider 回调。
        SetMixerValue(vfMixer, VFMixerParameter, volume);
        GameSettingsStore.Current.vfVolume = volume;
        GameSettingsStore.Save();
    }

    /// <summary>
    /// 尝试执行 TryGetResolution，并通过返回值表示本次操作是否成功。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 GameSettingController 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private bool TryGetResolution(int resolutionIndex, out Resolution resolution)
    {
        // 防御非法索引，避免 Dropdown 和 resolutions 数组不同步时报错。
        resolution = default;
        if (resolutions == null || resolutionIndex < 0 || resolutionIndex >= resolutions.Length)
        {
            return false;
        }

        resolution = resolutions[resolutionIndex];
        return true;
    }

    /// <summary>
    /// 用保存的宽高反查当前分辨率数组索引。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 GameSettingController 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private int FindResolutionIndex(int width, int height)
    {
        // 用保存的宽高反查当前分辨率数组索引。
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == width && resolutions[i].height == height)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Mixer 为空时跳过，方便某些场景不挂完整音频设置。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 GameSettingController 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void SetMixerValue(AudioMixer mixer, string parameterName, float value)
    {
        // Mixer 为空时跳过，方便某些场景不挂完整音频设置。
        if (mixer != null)
        {
            mixer.SetFloat(parameterName, value);
        }
    }
}
