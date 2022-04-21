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
    [SerializeField] private GameObject pause_overlay_panel_;
    private bool is_paused_ = false;
    [SerializeField] private GameObject gameover_overlay_panel_;
    private bool is_gameover_ = false;
    [SerializeField] private Slider ui_hp_bar_;
    [SerializeField] private Text ui_score_;
    private int score_ = 0;

    [SerializeField] private AudioClip click_sfx_;
    private AudioSource audio_source_;

    private Rect screen_;
    private Rect safe_area_;
    private RectTransform back_btn_rect_transform_;

    private Player.ThirdPersonController player_;

    private SaveFlag save_flag_;

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

        player_ = FindObjectOfType<Player.ThirdPersonController>();
        save_flag_ = FindObjectOfType<SaveFlag>();
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
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(next_level_);
    }

    public void DoLoadSave()
    {
        if (save_flag_ != null)
        {
            DontDestroyOnLoad(save_flag_.gameObject);
        }
        DoLoadNextLevel();
    }

    /// <summary>
    /// Loads prev level
    /// </summary>
    public void DoLoadPrevLevel()
    {
        audio_source_.PlayOneShot(click_sfx_);
        StartCoroutine(Delay());
        Time.timeScale = 1.0f;
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

    public void DoPauseGame()
    {
        pause_overlay_panel_.SetActive(true );
        is_paused_ = true;
        player_.SetPlayerInputEnabled(false);
        Time.timeScale = 0.0f;
    }

    public void DoResumeGame()
    {
        Time.timeScale = 1.0f;
        pause_overlay_panel_.SetActive(false);
        is_paused_ = false;
        player_.SetPlayerInputEnabled(true);
    }

    public void DoTogglePauseGame()
    {
        if (is_gameover_)
        {
            return;
        }

        if (is_paused_)
        {
            DoResumeGame();
        }
        else
        {
            DoPauseGame();
        }
    }

    public void DoGameOver()
    {
        is_gameover_ = true;
        gameover_overlay_panel_.SetActive(true);
        player_.SetPlayerInputEnabled(false);
        Time.timeScale = 0.0f;
    }

    public bool IsGameOver()
    {
        return is_gameover_;
    }

    public void DoSaveData()
    {
        player_.DoSaveData();
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
