using GoogleMobileAds.Samples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject menuPanel;
    public GameObject gameUIPanel;
    public GameObject collectionPanel;
    public Slider segmentDurationSlider;
    public Slider hexSettingsSlider; // Nowy slider

    [Header("External References")]
    public TileMover tileMover;
    public GameResetManager gameResetManager;
    public GameUI gameUI;
    public GameOverScreen gameOverScreen;
    public HexGridGenerator hexGridGenerator; // Nowy skrypt do generowania mapy
    public CollectionManager collectionManager;

    private const string SegmentDurationKey = "SegmentDuration"; // Klucz dla PlayerPrefs segmentDuration
    private const string HexSettingsKey = "HexSettings"; // Klucz dla hexSize i prefabu

    private void Start()
    {
        menuPanel.SetActive(false);
        collectionPanel.SetActive(false);
        // --- Inicjalizacja segmentDuration ---
        float savedDuration = PlayerPrefs.HasKey(SegmentDurationKey)
            ? PlayerPrefs.GetFloat(SegmentDurationKey)
            : 0.08f; // Domyœlna wartoœæ

        tileMover.segmentDuration = savedDuration;

        if (!PlayerPrefs.HasKey(SegmentDurationKey))
        {
            segmentDurationSlider.value = 8;
            UpdateSegmentDuration();
        }
        else
        {
            segmentDurationSlider.value = Mathf.Round((0.16f - savedDuration) * 100);
        }

        // --- Inicjalizacja hexSettings ---
        int savedHexIndex = PlayerPrefs.HasKey(HexSettingsKey)
            ? PlayerPrefs.GetInt(HexSettingsKey)
            : 1; // Domyœlnie œrodkowy prefab (1)

        hexSettingsSlider.value = savedHexIndex;
        UpdateHexSettings();
    }

    // Aktualizacja segmentDuration
    public void UpdateSegmentDuration()
    {
        float value = segmentDurationSlider.value;
        float newDuration = (16f - value) / 100f;

        tileMover.segmentDuration = newDuration;
        PlayerPrefs.SetFloat(SegmentDurationKey, newDuration);
        PlayerPrefs.Save();
    }

    // Aktualizacja hexSize i prefabu
    public void UpdateHexSettings()
    {
        int index = (int)hexSettingsSlider.value;

        float[] hexSizes = { 0.54f, 0.6f, 0.66f }; // Rozmiary hexów
        hexGridGenerator.SetHexPrefab(index); // Ustawienie prefabu
        hexGridGenerator.hexSize = hexSizes[index]; // Ustawienie hexSize

        PlayerPrefs.SetInt(HexSettingsKey, index);
        PlayerPrefs.Save();


        
    }

    // Powrót do interfejsu gry
    public void ReturnToGameUI()
    {
        menuPanel.SetActive(false);
        gameUIPanel.SetActive(true);
        Time.timeScale = 1f;
    }

    // Restart gry
    public void RestartGame()
    {
        Time.timeScale = 1f;
        gameUI.ResetAll();
        gameUI.wasPausedByAd = true;
        menuPanel.SetActive(false);
        gameResetManager.ResetGame();
    }

    // Funkcja do w³¹czania menu
    public void ShowMenu()
    {
        menuPanel.SetActive(true);
        gameUIPanel.SetActive(false);
        Time.timeScale = 0f;
    }
    public void ShowCollection()
    {
        menuPanel.SetActive(false);
        collectionPanel.SetActive(true);
    }

    public void CloseCollection()
    {
        collectionPanel.SetActive(false);
        menuPanel.SetActive(true);
    }
}
