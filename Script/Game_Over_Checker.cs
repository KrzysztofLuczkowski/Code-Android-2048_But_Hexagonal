using GoogleMobileAds.Sample;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverChecker : MonoBehaviour
{
    public MoveValidator MoveValidator;
    public GameUI gameUI;
    public GameOverScreen gameOverScreen;
    public InterstitialAdController interstitialAdController;


    public void CheckGameOver()
    {
        if (MoveValidator.IsAnyMovePossible() != true)
        {
            interstitialAdController.ShowAd();
            gameUI.ShowGameOverScreen();
            gameOverScreen.PlayGameOverSound();
            gameOverScreen.ShowGameOverScreen();
        }
    }
}
