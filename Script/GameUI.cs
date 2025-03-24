using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Sample;

public class GameUI : MonoBehaviour
{
    public TMP_Text scoreText;
    public TMP_Text movesText;
    public TMP_Text timerText;
    public GameObject gameOverScreen;
    public GameObject gameUIPanel;
    public AudioSource moveSound;
    public GameObject tutorialPanel;
    public Button closeButton;
    public InterstitialAdController interstitialAdController;

    private int score = 0;
    private int moves = 0;
    private float gameTime = 0f;
    public bool isRunning = true;
    public bool wasPausedByAd = false;
    public bool isWaitingForFirstMove = true;
    private float lastAdTime = 0f; // Ostatni czas wyœwietlenia reklamy

    public const string tutorialKey = "TutorialShown";

    void Start()
    {
        if (PlayerPrefs.GetInt(tutorialKey, 0) == 0)
        {
            ShowTutorial();
            gameUIPanel.SetActive(false);
            Time.timeScale = 0f;
        }
        else
        {
            tutorialPanel.SetActive(false);
            gameUIPanel.SetActive(true);
            UpdateScoreText();
            UpdateMovesText();
            gameOverScreen.SetActive(false);
        }
        closeButton.onClick.AddListener(HideTutorial);
    }

    void Update()
    {
        if (isRunning == true && wasPausedByAd == false && isWaitingForFirstMove == false)
        {
            gameTime += Time.deltaTime;
            UpdateTimerText();

            int currentMinutes = Mathf.FloorToInt(gameTime / 60);
            int lastAdMinutes = Mathf.FloorToInt(lastAdTime / 60);

            // Wyœwietl reklamê co 10 minut
            if (currentMinutes % 10 == 0 && currentMinutes != lastAdMinutes && currentMinutes != 0)
            {
                if (interstitialAdController != null)
                {
                    wasPausedByAd = true;
                    interstitialAdController.ShowAd();
                    lastAdTime = gameTime; // Aktualizacja czasu ostatniej reklamy
                }
            }
        }
    }

    private void ShowTutorial()
    {
        tutorialPanel.SetActive(true);
    }

    public void HideTutorial()
    {
        Time.timeScale = 1f;
        tutorialPanel.SetActive(false);
        PlayerPrefs.SetInt(tutorialKey, 1);
        PlayerPrefs.Save();
        gameUIPanel.SetActive(true);
        UpdateScoreText();
        UpdateMovesText();
        gameOverScreen.SetActive(false);
        isRunning = true;
    }

    public void AddScore(int points)
    {
        score += points;
        UpdateScoreText();
    }

    public void IncrementMoves()
    {
        moves++;
        UpdateMovesText();
    }

    private void UpdateScoreText()
    {
        scoreText.text = $"{score}";
    }

    private void UpdateMovesText()
    {
        movesText.text = $"{moves}";
    }

    private void UpdateTimerText()
    {
        timerText.text = GetFormattedTime();
    }

    public string GetFormattedTime()
    {
        int totalSeconds = Mathf.FloorToInt(gameTime);
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;

        if (hours > 0)
            return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        else
            return $"{minutes:D2}:{seconds:D2}";
    }

    public int GetScore()
    {
        return score;
    }

    public int GetMoveCount()
    {
        return moves;
    }

    public void ShowGameOverScreen()
    {
        isRunning = false;
        gameUIPanel.SetActive(false);
    }

    public void PlayMoveSound()
    {
        if (moveSound != null)
        {
            moveSound.Play();
        }
    }

    public void ResetAll()
    {
        score = 0;
        moves = 0;
        gameTime = 0f;
        timerText.text = GetFormattedTime();
        gameUIPanel.SetActive(true);
        isRunning = true;

        UpdateScoreText();
        UpdateMovesText();
        gameOverScreen.SetActive(false);
    }
}
