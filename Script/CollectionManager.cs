using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionManager : MonoBehaviour
{
    public Image[] tileImages;
    public Sprite questionMarkSprite;
    public Sprite[] tileSprites;
    public TileUnlockPopup tileUnlockPopup;

    public const string CollectionKey = "UnlockedTiles";

    void Start()
    {
        if (!PlayerPrefs.HasKey(CollectionKey))
        {
            // Domy�lnie odblokowany tylko kafelek "2"
            string defaultTiles = "100000000000000000";
            PlayerPrefs.SetString(CollectionKey, defaultTiles);
            PlayerPrefs.Save();
        }

        LoadCollection(); // Za�aduj odblokowane kafelki na starcie
    }

    public void LoadCollection()
    {
        string savedTiles = PlayerPrefs.GetString(CollectionKey, "100000000000000000");

        for (int i = 0; i < tileImages.Length; i++)
        {
            tileImages[i].sprite = savedTiles[i] == '1' ? tileSprites[i] : questionMarkSprite;
        }
    }

    public void UnlockTile(int tileIndex)
    {
        string savedTiles = PlayerPrefs.GetString(CollectionKey, "100000000000000000");
        char[] tilesArray = savedTiles.ToCharArray();

        if (tilesArray[tileIndex] == '0') // Je�li kafelek jeszcze nieodblokowany
        {
            tilesArray[tileIndex] = '1';
            PlayerPrefs.SetString(CollectionKey, new string(tilesArray));
            PlayerPrefs.Save();
            LoadCollection(); // Aktualizuj UI

            int tileValue = GetTileValue(tileIndex); // Pobieramy warto�� liczbow� kafelka
            tileUnlockPopup.ShowPopup(tileIndex, tileValue); // Wywo�ujemy gratulacje
        }
    }
    private int GetTileValue(int index)
    {
        int[] tileValues = { 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536, 131072, 262144 };
        return tileValues[index];
    }
}
