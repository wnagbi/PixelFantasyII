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

    private float timeDelta = 0.5f;
    private float prevTime;
    private float fps;
    private int frames;
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

        prevTime = Time.realtimeSinceStartup;
        style = new GUIStyle();
        style.fontSize = 30;
        style.normal.textColor = Color.white;
    }

    private void OnGUI()
    {
        // 简单 FPS 显示，用于测试性能。
        GUI.Label(new Rect(0, Screen.height - 40, 200, 200), "FPS: " + fps.ToString("f2"), style);
    }

    private void Update()
    {
        frames++;
        if (Time.realtimeSinceStartup >= prevTime + timeDelta)
        {
            fps = frames / (Time.realtimeSinceStartup - prevTime);
            prevTime = Time.realtimeSinceStartup;
            frames = 0;
        }
    }

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

    public void statusCheck()
    {
        // 统一应用 JSON 中的设置到当前运行时状态和 UI 控件。
        judgeFullScreen();
        judgeVibrate();
        InitVolume();
    }

    public void judgeVibrate()
    {
        // 初始化震动 Toggle。
        isVibration = GameSettingsStore.Current.vibration;
        if (controllerVibraton != null)
        {
            controllerVibraton.SetIsOnWithoutNotify(isVibration);
        }
    }

    public void ToggleVibrate(bool isVibrate)
    {
        // 用户改变震动开关时保存到 JSON。
        isVibration = isVibrate;
        GameSettingsStore.Current.vibration = isVibrate;
        GameSettingsStore.Save();
    }

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

    public void ToggleFullScreen(bool isFullScreen)
    {
        // 用户改变全屏开关时应用屏幕设置并保存。
        GameSettingsData settings = GameSettingsStore.Current;
        settings.fullscreen = isFullScreen;
        Screen.fullScreen = isFullScreen;
        Screen.SetResolution(settings.resolutionWidth, settings.resolutionHeight, isFullScreen);
        GameSettingsStore.Save();
    }

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

    public void SetMaserMixerVolume(float volume)
    {
        // 主音量 Slider 回调。
        SetMixerValue(masterMixer, MasterMixerParameter, volume);
        GameSettingsStore.Current.masterVolume = volume;
        GameSettingsStore.Save();
    }

    public void SetMusicMixerVolume(float volume)
    {
        // 音乐音量 Slider 回调。
        SetMixerValue(musicMixer, MusicMixerParameter, volume);
        GameSettingsStore.Current.musicVolume = volume;
        GameSettingsStore.Save();
    }

    public void SetVFMixerVolume(float volume)
    {
        // 音效音量 Slider 回调。
        SetMixerValue(vfMixer, VFMixerParameter, volume);
        GameSettingsStore.Current.vfVolume = volume;
        GameSettingsStore.Save();
    }

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

    private void SetMixerValue(AudioMixer mixer, string parameterName, float value)
    {
        // Mixer 为空时跳过，方便某些场景不挂完整音频设置。
        if (mixer != null)
        {
            mixer.SetFloat(parameterName, value);
        }
    }
}
