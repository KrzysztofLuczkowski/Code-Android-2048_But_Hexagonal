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
    public TileSpawner tileSpawner; // Referencja do TileSpawner (do generowania kafelk�w)
    public HexGridGenerator hexGrid; // Referencja do HexGrid (gdzie przechowywane s� HexCell)
    public InterstitialAdController interstitialAdController;

    // Ta metoda resetuje wszystkie zmienne zwi�zane z gr�
    public void ResetGame()
    {
        
        // Zresetowanie wyniku, liczby ruch�w i czasu
        gameUI.ResetAll();
        gameUI.isWaitingForFirstMove = true;

        // Usuni�cie wszystkich istniej�cych kafelk�w
        DestroyAllHexCells();

        // Wyczy�� list� kom�rek
        hexGrid.ClearCells();

        // Ponowne wygenerowanie planszy
        hexGrid.GenerateGrid();

        // Od�wie�enie kafelk�w na planszy
        tileSpawner.SpawnTile2();

        //interstitialAdController.LoadAd();
        Time.timeScale = 1f;

    }

    // Metoda odpowiedzialna za usuni�cie wszystkich kafelk�w
    private void DestroyAllHexCells()
    {
        foreach (var cell in hexGrid.cells)
        {
            if (cell != null)
            {
                Destroy(cell.gameObject); // Usuni�cie obiektu HexCell
            }
        }
    }

}
