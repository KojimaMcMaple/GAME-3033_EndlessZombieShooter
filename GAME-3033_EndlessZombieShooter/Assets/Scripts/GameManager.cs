using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
///  The Source file name: GameManager.cs
///  Author's name: Trung Le (Kyle Hunter)
///  Student Number: 101264698
///  Program description: Global game manager script, manages the UI and level loading
///  Date last Modified: See GitHub
///  Revision History: See GitHub
/// </summary>
public class GameManager : MonoBehaviour
{
    [SerializeField] private string next_level_;
    [SerializeField] private string prev_level_;
    [SerializeField] private GameObject overlay_panel_;
    [SerializeField] private Slider ui_hp_bar_;
    [SerializeField] private Text ui_score_;
    private int score_ = 0;

    [SerializeField] private AudioClip click_sfx_;
    private AudioSource audio_source_;

    // LAB1
    private Rect screen_;
    private Rect safe_area_;
    private RectTransform back_btn_rect_transform_;

    void Awake()
    {
        audio_source_ = GetComponent<AudioSource>();

        if (ui_score_ != null)
        {
            SetUIScoreValue(score_);
        }
        else
        {
            Debug.Log(">>> NO ui_score_!");
        }
    }

    //void Update()
    //{

    //    //// LAB1
    //    //screen_ = new Rect(0f, 0f, Screen.width, Screen.height);
    //    //safe_area_ = Screen.safeArea;
    //    //CheckOrientation();
    //}

    /// <summary>
    /// Mutator for private variable
    /// </summary>
    public void SetUIHPBarValue(float value)
    {
        ui_hp_bar_.value = value;
    }

    /// <summary>
    /// Mutator for private variable
    /// </summary>
    public void IncrementScore(int value)
    {
        score_ += value;
        SetUIScoreValue(score_);
    }

    /// <summary>
    /// Mutator for private variable
    /// </summary>
    public void SetUIScoreValue(int value)
    {
        ui_score_.text = ("Score " + value).ToString();
    }

    /// <summary>
    /// Loads next level
    /// </summary>
    public void DoLoadNextLevel()
    {
        audio_source_.PlayOneShot(click_sfx_);
        StartCoroutine(Delay());
        SceneManager.LoadScene(next_level_);
    }

    /// <summary>
    /// Loads prev level
    /// </summary>
    public void DoLoadPrevLevel()
    {
        audio_source_.PlayOneShot(click_sfx_);
        StartCoroutine(Delay());
        SceneManager.LoadScene(prev_level_);
    }

    /// <summary>
    /// Closes app
    /// </summary>
    public void DoQuitApp()
    {
        audio_source_.PlayOneShot(click_sfx_);
        StartCoroutine(Delay());
        Application.Quit();
    }

    /// <summary>
    /// Shows hidden panel
    /// </summary>
    public void DoShowOverlayPanel()
    {
        audio_source_.PlayOneShot(click_sfx_);
        overlay_panel_.SetActive(true);
    }

    /// <summary>
    /// Hides overlay panel
    /// </summary>
    public void DoHideOverlayPanel()
    {
        audio_source_.PlayOneShot(click_sfx_);
        overlay_panel_.SetActive(false);
    }

    /// <summary>
    /// General delay function for level loading, show explosion before game over, etc.
    /// </summary>
    /// <returns></returns>
    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(2.0f);
    }

    /// <summary>
    /// LAB 1
    /// </summary>
    private static void CheckOrientation()
    {
        switch (Screen.orientation)
        {
            case ScreenOrientation.Unknown:
                break;
            case ScreenOrientation.Portrait:
                break;
            case ScreenOrientation.PortraitUpsideDown:
                break;
            case ScreenOrientation.LandscapeLeft:
                break;
            case ScreenOrientation.LandscapeRight:
                break;
            case ScreenOrientation.AutoRotation:
                break;
            default:
                break;
        }
    }
}
