using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using UnityEngine.Audio;

public class MenuUIManager : MonoBehaviour
{
    public GameObject mainMenuUI;
    public GameObject optionsMenuUI;
    public GameObject controlsMenuUI;
    public GameObject creditsMenuUI;

    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown graphicsDropdown;
    public Slider maxFramerateSlider;
    public TMP_Text maxFramerateText;
    public Slider soundSlider;
    public Slider musicSlider;

    public AudioMixer gameAudioMixer;


    void Start()
    {
        resolutionDropdown.onValueChanged.AddListener(UpdateResolution);
        graphicsDropdown.onValueChanged.AddListener(UpdateGraphicsQuality);
        maxFramerateSlider.onValueChanged.AddListener(UpdateMaxFramerate);
        soundSlider.onValueChanged.AddListener(UpdateSound);
        musicSlider.onValueChanged.AddListener(UpdateMusic);

        soundSlider.minValue = 0.001f;
        musicSlider.minValue = 0.001f;

        Resolution currentRes = new Resolution();
        Resolution[] resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> resOptions = new List<TMP_Dropdown.OptionData>();
        foreach(Resolution res in resolutions)
        {
            if(Screen.currentResolution.width == res.width && Screen.currentResolution.height == res.height)
            {
                currentRes = res;
            }
            TMP_Dropdown.OptionData resOption = new TMP_Dropdown.OptionData();
            resOption.text = $"{res.width}x{res.height}";
            resOptions.Add(resOption);
        }
        resolutionDropdown.AddOptions(resOptions);
        string savedResolution = PlayerPrefs.GetString("Options_Resolution", "");
        if(savedResolution == "")
        {
            resolutionDropdown.value = resolutionDropdown.options.FindIndex(x => x.text == $"{currentRes.width}x{currentRes.height}");
        }else
        {
            resolutionDropdown.value = resolutionDropdown.options.FindIndex(x => x.text == savedResolution);
        }
        UpdateResolution(resolutionDropdown.value);

        string[] qualityLevels = QualitySettings.names;

        graphicsDropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> graphicsOptions = new List<TMP_Dropdown.OptionData>();
        foreach(string quality in qualityLevels)
        {
            TMP_Dropdown.OptionData qualityOption = new TMP_Dropdown.OptionData();
            qualityOption.text = quality;
            graphicsOptions.Add(qualityOption);
        }
        graphicsDropdown.AddOptions(graphicsOptions);
        int savedGraphics = PlayerPrefs.GetInt("Options_Graphics", -1);
        if(savedGraphics == -1)
        {
            graphicsDropdown.value = graphicsDropdown.options.FindIndex(x => x.text == qualityLevels[QualitySettings.GetQualityLevel()]);
        }else
        {
            graphicsDropdown.value = savedGraphics;
        }
        UpdateGraphicsQuality(graphicsDropdown.value);


        int savedMaxFramerate = PlayerPrefs.GetInt("Options_MaxFramerate", -1);
        if(savedMaxFramerate == -1)
        {
            maxFramerateSlider.value = 60;
        }else
        {
            maxFramerateSlider.value = savedMaxFramerate;
        }
        UpdateMaxFramerate(maxFramerateSlider.value);

        float savedSound = PlayerPrefs.GetFloat("Options_Sound", 1f);
        soundSlider.value = savedSound;
        UpdateSound(soundSlider.value);

        float savedMusic = PlayerPrefs.GetFloat("Options_Music", 1f);
        musicSlider.value = savedMusic;
        UpdateMusic(musicSlider.value);


    }

    public void UpdateGraphicsQuality(int val)
    {
        QualitySettings.SetQualityLevel(val);
        PlayerPrefs.SetInt("Options_Graphics", val);
    }

    public void UpdateResolution(int val)
    {
        string res = resolutionDropdown.options[val].text;
        string[] resVals = res.Split("x");
        Screen.SetResolution(Convert.ToInt32(resVals[0]), Convert.ToInt32(resVals[1]), true);
        PlayerPrefs.SetString("Options_Resolution", res);
    }

    public void UpdateMaxFramerate(float val)
    {
        int maxFPS = Convert.ToInt32(val);
        Application.targetFrameRate = maxFPS;
        maxFramerateText.text = maxFPS.ToString();
        PlayerPrefs.SetInt("Options_MaxFramerate", maxFPS);
    }

    public void UpdateSound(float val)
    {
        PlayerPrefs.SetFloat("Options_Sound", val);
        float db = Mathf.Log10(val) * 20f;
        gameAudioMixer.SetFloat("SoundVal", db);
    }

    public void UpdateMusic(float val)
    {
        PlayerPrefs.SetFloat("Options_Music", val);
        float db = Mathf.Log10(val) * 20f;
        gameAudioMixer.SetFloat("MusicVal", db);
    }



    public void PlayGame()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void OpenOptionsMenu()
    {
        mainMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);
        creditsMenuUI.SetActive(false);
        controlsMenuUI.SetActive(false);
    }

    public void OpenControlsMenu()
    {
        mainMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
        creditsMenuUI.SetActive(false);
        controlsMenuUI.SetActive(true);
    }

    public void OpenCreditsMenu()
    {
        mainMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
        creditsMenuUI.SetActive(true);
        controlsMenuUI.SetActive(false);
    }

    public void BackToMainMenu()
    {
        mainMenuUI.SetActive(true);
        optionsMenuUI.SetActive(false);
        creditsMenuUI.SetActive(false);
        controlsMenuUI.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }   
}
