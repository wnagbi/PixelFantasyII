using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class GameSettingController : MonoBehaviour
{
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
        instance = this;
        Application.targetFrameRate = 280;
    }

    private void Start()
    {
        GameSettingsStore.Load();

        InitializeLanguageDropDown();
        InitialzieResolutionDropDown();
        statusCheck();

        prevTime = Time.realtimeSinceStartup;
        style = new GUIStyle();
        style.fontSize = 30;
        style.normal.textColor = Color.white;
    }

    // private void OnGUI()
    // {
    //     GUI.Label(new Rect(0, Screen.height - 40, 200, 200), "FPS: " + fps.ToString("f2"), style);
    // }

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
        if (!TryGetResolution(resolutionIndex, out Resolution resolution))
        {
            return;
        }

        GameSettingsData settings = GameSettingsStore.Current;
        Screen.SetResolution(resolution.width, resolution.height, settings.fullscreen);
        settings.resolutionWidth = resolution.width;
        settings.resolutionHeight = resolution.height;
        GameSettingsStore.Save();
    }

    private Resolution[] GetUniqueResolutions(Resolution[] source) 
    {
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
    private void InitialzieResolutionDropDown()
    {
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

        resolutionDropDown.SetValueWithoutNotify(savedResolutionIndex);
        resolutionDropDown.RefreshShownValue();

        Screen.SetResolution(
            resolutions[savedResolutionIndex].width,
            resolutions[savedResolutionIndex].height,
            settings.fullscreen
        );
    }
    // private void InitialzieResolutionDropDown()
    // {
    //     resolutions = Screen.resolutions;
    //     if (resolutions == null || resolutions.Length == 0)
    //     {
    //         resolutions = new[] { Screen.currentResolution };
    //     }

    //     resolutionDropDown.ClearOptions();

    //     List<string> options = new List<string>();
    //     for (int i = 0; i < resolutions.Length; i++)
    //     {
    //         string key = resolutions[i].width + "x" + resolutions[i].height;
    //         if (options.Contains(key))
    //         {
    //             continue;
    //         }
    //         options.Add(key);
            
    //     }

    //     resolutionDropDown.AddOptions(options);

    //     GameSettingsData settings = GameSettingsStore.Current;
    //     int savedResolutionIndex = FindResolutionIndex(settings.resolutionWidth, settings.resolutionHeight);
    //     if (savedResolutionIndex < 0)
    //     {
    //         savedResolutionIndex = FindResolutionIndex(Screen.currentResolution.width, Screen.currentResolution.height);
    //         if (savedResolutionIndex < 0)
    //         {
    //             savedResolutionIndex = 0;
    //         }

    //         settings.resolutionWidth = resolutions[savedResolutionIndex].width;
    //         settings.resolutionHeight = resolutions[savedResolutionIndex].height;
    //         GameSettingsStore.Save();
    //     }

    //     resolutionDropDown.SetValueWithoutNotify(savedResolutionIndex);
    //     resolutionDropDown.RefreshShownValue();
    //     Screen.SetResolution(resolutions[savedResolutionIndex].width, resolutions[savedResolutionIndex].height, settings.fullscreen);
    // }

    public void InitializeLanguageDropDown()
    {
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

        GameSettingsData settings = GameSettingsStore.Current;
        int languageIndex = settings.languageIndex;
        if (languageIndex < 0 || languageIndex >= locales.Count)
        {
            languageIndex = 0;
            settings.languageIndex = languageIndex;
            GameSettingsStore.Save();
        }

        LocalizationSettings.SelectedLocale = locales[languageIndex];
        languageDropDown.SetValueWithoutNotify(languageIndex);
        languageDropDown.RefreshShownValue();
    }

    public void SetLanguage(int index)
    {
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
        judgeFullScreen();
        judgeVibrate();
        InitVolume();
    }

    public void judgeVibrate()
    {
        isVibration = GameSettingsStore.Current.vibration;
        if (controllerVibraton != null)
        {
            controllerVibraton.SetIsOnWithoutNotify(isVibration);
        }
    }

    public void ToggleVibrate(bool isVibrate)
    {
        isVibration = isVibrate;
        GameSettingsStore.Current.vibration = isVibrate;
        GameSettingsStore.Save();
    }

    public void judgeFullScreen()
    {
        bool fullscreen = GameSettingsStore.Current.fullscreen;
        Screen.fullScreen = fullscreen;
        if (fullScreenController != null)
        {
            fullScreenController.SetIsOnWithoutNotify(fullscreen);
        }
    }

    public void ToggleFullScreen(bool isFullScreen)
    {
        GameSettingsData settings = GameSettingsStore.Current;
        settings.fullscreen = isFullScreen;
        Screen.fullScreen = isFullScreen;
        Screen.SetResolution(settings.resolutionWidth, settings.resolutionHeight, isFullScreen);
        GameSettingsStore.Save();
    }

    public void InitVolume()
    {
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
        SetMixerValue(masterMixer, MasterMixerParameter, volume);
        GameSettingsStore.Current.masterVolume = volume;
        GameSettingsStore.Save();
    }

    public void SetMusicMixerVolume(float volume)
    {
        SetMixerValue(musicMixer, MusicMixerParameter, volume);
        GameSettingsStore.Current.musicVolume = volume;
        GameSettingsStore.Save();
    }

    public void SetVFMixerVolume(float volume)
    {
        SetMixerValue(vfMixer, VFMixerParameter, volume);
        GameSettingsStore.Current.vfVolume = volume;
        GameSettingsStore.Save();
    }

    private bool TryGetResolution(int resolutionIndex, out Resolution resolution)
    {
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
        if (mixer != null)
        {
            mixer.SetFloat(parameterName, value);
        }
    }
}
