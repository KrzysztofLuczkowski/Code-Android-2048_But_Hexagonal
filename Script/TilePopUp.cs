using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileUnlockPopup : MonoBehaviour
{
    public GameObject popupPanel; // Panel gratulacji
    public GameObject uiPanel;
    public Image tileImage; // Obraz kafelka w panelu
    public AudioSource audioSource; // Komponent do odtwarzania d�wi�ku
    public AudioClip unlockSound; // D�wi�k odblokowania
    public Sprite[] tileSprites; // Lista sprite'�w dla kafelk�w

    private const int MinTileValueForPopup = 128; // Minimalna warto�� kafelka do pokazania gratulacji

    void Start()
    {
        popupPanel.SetActive(false); // Ukrywamy panel na starcie
    }

    public void ShowPopup(int tileIndex, int tileValue)
    {
        if (tileValue < MinTileValueForPopup) return; // Ignorujemy kafelki poni�ej 128

        if (tileIndex >= 0 && tileIndex < tileSprites.Length) // Sprawdzamy, czy indeks jest poprawny
        {
            Time.timeScale = 0f;
            tileImage.sprite = tileSprites[tileIndex]; // Ustawiamy grafik� nowego kafelka
            popupPanel.SetActive(true); // Pokazujemy panel
            uiPanel.SetActive(false);
            if (audioSource != null && unlockSound != null)
            {
                audioSource.PlayOneShot(unlockSound); // Odtwarzamy d�wi�k
            }
        }
        else
        {
            Debug.LogWarning("Nieprawid�owy indeks kafelka: " + tileIndex);
        }
    }

    public void ClosePopup()
    {
        popupPanel.SetActive(false); // Zamykamy panel
        uiPanel.SetActive(true);
        Time.timeScale = 1f;
    }
}