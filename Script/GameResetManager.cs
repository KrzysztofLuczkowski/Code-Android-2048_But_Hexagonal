using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using GoogleMobileAds.Sample;

public class GameResetManager : MonoBehaviour
{
    public GameUI gameUI; // Referencja do GameUI
    public GameOverScreen gameOverScreen; // Referencja do GameOverScreen
    public TileSpawner tileSpawner; // Referencja do TileSpawner (do generowania kafelków)
    public HexGridGenerator hexGrid; // Referencja do HexGrid (gdzie przechowywane s¹ HexCell)
    public InterstitialAdController interstitialAdController;

    // Ta metoda resetuje wszystkie zmienne zwi¹zane z gr¹
    public void ResetGame()
    {
        
        // Zresetowanie wyniku, liczby ruchów i czasu
        gameUI.ResetAll();
        gameUI.isWaitingForFirstMove = true;

        // Usuniêcie wszystkich istniej¹cych kafelków
        DestroyAllHexCells();

        // Wyczyœæ listê komórek
        hexGrid.ClearCells();

        // Ponowne wygenerowanie planszy
        hexGrid.GenerateGrid();

        // Odœwie¿enie kafelków na planszy
        tileSpawner.SpawnTile2();

        //interstitialAdController.LoadAd();
        Time.timeScale = 1f;

    }

    // Metoda odpowiedzialna za usuniêcie wszystkich kafelków
    private void DestroyAllHexCells()
    {
        foreach (var cell in hexGrid.cells)
        {
            if (cell != null)
            {
                Destroy(cell.gameObject); // Usuniêcie obiektu HexCell
            }
        }
    }

}
