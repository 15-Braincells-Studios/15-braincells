
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using Project.Scripts.Core;

public class SettingsMenuManager : MonoBehaviour
{
    [Header("Settings Panels")] [SerializeField]
    private GameObject audioSettingsPanel;

    [SerializeField] private GameObject graphicsSettingsPanel;
    [SerializeField] private GameObject gameplaySettingsPanel;
    [SerializeField] private GameObject accessibilitySettingsPanel;

    [Header("Audio Settings")] [SerializeField]
    private AudioMixer audioMixer;

    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider soundEffectsSlider;
    [SerializeField] private Slider dialogueSlider;

    [Header("Graphics Settings")] [SerializeField]
    private TMP_Dropdown windowModeDropdown;

    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown qualityDropdown;

    [Header("Gameplay Settings")] [SerializeField]
    private Slider cameraSensitivitySlider;

    [Header("Accessibility Settings")] [SerializeField]
    private Slider backgroundOpacitySlider;
        
    [Header("Localization")]

    [SerializeField] private TMP_Text audioButtonText;
    [SerializeField] private TMP_Text graphicsButtonText;
    [SerializeField] private TMP_Text gameplayButtonText;
    [SerializeField] private TMP_Text accessibilityButtonText;

    [SerializeField] private TMP_Text settingsTitleText;
    [SerializeField] private TMP_Text masterVolumeText;
    [SerializeField] private TMP_Text musicVolumeText;
    [SerializeField] private TMP_Text soundEffectsText;
    [SerializeField] private TMP_Text dialogueText;

    private Resolution[] resolutions;

    private void Start()
    {
        Localization.OnLanguageChanged += UpdateAllText;

        SetupResolutionDropdown();
        SetupQualityDropdown();

        LoadSettings();

        ShowAudioSettings();

        UpdateAllText();
    }

    private void OnDestroy()
    {
        Localization.OnLanguageChanged -= UpdateAllText;
    }

    // CATEGORY SWITCHING

    public void ShowAudioSettings()
    {
        audioSettingsPanel.SetActive(true);
        graphicsSettingsPanel.SetActive(false);
        gameplaySettingsPanel.SetActive(false);
        accessibilitySettingsPanel.SetActive(false);
    }

    public void ShowGraphicsSettings()
    {
        audioSettingsPanel.SetActive(false);
        graphicsSettingsPanel.SetActive(true);
        gameplaySettingsPanel.SetActive(false);
        accessibilitySettingsPanel.SetActive(false);
    }

    public void ShowGameplaySettings()
    {
        audioSettingsPanel.SetActive(false);
        graphicsSettingsPanel.SetActive(false);
        gameplaySettingsPanel.SetActive(true);
        accessibilitySettingsPanel.SetActive(false);
    }

    public void ShowAccessibilitySettings()
    {
        audioSettingsPanel.SetActive(false);
        graphicsSettingsPanel.SetActive(false);
        gameplaySettingsPanel.SetActive(false);
        accessibilitySettingsPanel.SetActive(true);
    }

    // AUDIO SETTINGS

    private void SetMixerVolume(string parameter, float value)
    {
        // Convert slider value (0-1) to decibels.
        float volume = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;

        audioMixer.SetFloat(parameter, volume);
    }

    public void SetMasterVolume(float value)
    {
        SetMixerVolume("MasterVolume", value);
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    public void SetMusicVolume(float value)
    {
        SetMixerVolume("MusicVolume", value);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSoundEffectsVolume(float value)
    {
        SetMixerVolume("SFXVolume", value);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    public void SetDialogueVolume(float value)
    {
        SetMixerVolume("DialogueVolume", value);
        PlayerPrefs.SetFloat("DialogueVolume", value);
    }

    // GRAPHICS SETTINGS

    private void SetupResolutionDropdown()
    {
        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        System.Collections.Generic.List<string> options =
            new System.Collections.Generic.List<string>();

        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option =
                resolutions[i].width + " x " +
                resolutions[i].height;

            if (!options.Contains(option))
            {
                options.Add(option);
            }

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = options.IndexOf(option);
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void SetupQualityDropdown()
    {
        qualityDropdown.ClearOptions();

        qualityDropdown.AddOptions(
            new System.Collections.Generic.List<string>(
                QualitySettings.names
            )
        );

        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.RefreshShownValue();
    }

    public void SetWindowMode(int index)
    {
        switch (index)
        {
            case 0:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;

            case 1:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;

            case 2:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
        }

        PlayerPrefs.SetInt("WindowMode", index);
    }

    public void SetResolution(int index)
    {
        if (resolutions == null || resolutions.Length == 0)
            return;

        // Use the unique resolutions displayed in the dropdown.
        string selected = resolutionDropdown.options[index].text;

        string[] dimensions = selected.Split('x');

        int width = int.Parse(dimensions[0].Trim());
        int height = int.Parse(dimensions[1].Trim());

        Screen.SetResolution(
            width,
            height,
            Screen.fullScreenMode
        );

        PlayerPrefs.SetInt("ResolutionWidth", width);
        PlayerPrefs.SetInt("ResolutionHeight", height);
    }

    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);

        PlayerPrefs.SetInt("Quality", index);
    }

    // GAMEPLAY SETTINGS

    public void SetCameraSensitivity(float value)
    {
        PlayerPrefs.SetFloat("CameraSensitivity", value);

        // Your camera controller can read this value.
    }

    // ACCESSIBILITY SETTINGS

    public void SetBackgroundOpacity(float value)
    {
        PlayerPrefs.SetFloat("BackgroundOpacity", value);

        // Apply this to your subtitle background image.
    }

    public void SetSubtitleSize(int size)
    {
        PlayerPrefs.SetInt("SubtitleSize", size);
    }


    // LOAD SAVED SETTINGS

    private void LoadSettings()
    {
        // AUDIO
        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float music = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);
        float dialogue = PlayerPrefs.GetFloat("DialogueVolume", 1f);

        masterVolumeSlider.SetValueWithoutNotify(master);
        musicVolumeSlider.SetValueWithoutNotify(music);
        soundEffectsSlider.SetValueWithoutNotify(sfx);
        dialogueSlider.SetValueWithoutNotify(dialogue);

        SetMixerVolume("MasterVolume", master);
        SetMixerVolume("MusicVolume", music);
        SetMixerVolume("SFXVolume", sfx);
        SetMixerVolume("DialogueVolume", dialogue);

        // WINDOW MODE
        int windowMode = PlayerPrefs.GetInt("WindowMode", 0);

        windowModeDropdown.SetValueWithoutNotify(windowMode);
        SetWindowMode(windowMode);

        // RESOLUTION
        int savedWidth = PlayerPrefs.GetInt(
            "ResolutionWidth",
            Screen.currentResolution.width
        );

        int savedHeight = PlayerPrefs.GetInt(
            "ResolutionHeight",
            Screen.currentResolution.height
        );

        for (int i = 0; i < resolutionDropdown.options.Count; i++)
        {
            string option = resolutionDropdown.options[i].text;
            string[] dimensions = option.Split('x');

            int width = int.Parse(dimensions[0].Trim());
            int height = int.Parse(dimensions[1].Trim());

            if (width == savedWidth && height == savedHeight)
            {
                resolutionDropdown.SetValueWithoutNotify(i);

                Screen.SetResolution(
                    width,
                    height,
                    Screen.fullScreenMode
                );

                break;
            }
        }

        // QUALITY
        int quality = PlayerPrefs.GetInt(
            "Quality",
            QualitySettings.GetQualityLevel()
        );

        quality = Mathf.Clamp(
            quality,
            0,
            QualitySettings.names.Length - 1
        );

        qualityDropdown.SetValueWithoutNotify(quality);
        QualitySettings.SetQualityLevel(quality);

        // GAMEPLAY
        float sensitivity = PlayerPrefs.GetFloat(
            "CameraSensitivity",
            1f
        );

        cameraSensitivitySlider.SetValueWithoutNotify(sensitivity);

        // ACCESSIBILITY
        float opacity = PlayerPrefs.GetFloat(
            "BackgroundOpacity",
            1f
        );

        backgroundOpacitySlider.SetValueWithoutNotify(opacity);
    }

    public void UpdateAllText()
    {
        settingsTitleText.text = Localization.Get("settings_title");

        audioButtonText.text = Localization.Get("settings_audio");
        graphicsButtonText.text = Localization.Get("settings_graphics");
        gameplayButtonText.text = Localization.Get("settings_gameplay");
        accessibilityButtonText.text = Localization.Get("settings_accessibility");

        masterVolumeText.text = Localization.Get("settings_master_volume");
        musicVolumeText.text = Localization.Get("settings_music_volume");
        soundEffectsText.text = Localization.Get("settings_sfx");
        dialogueText.text = Localization.Get("settings_dialogue");
    }
}
