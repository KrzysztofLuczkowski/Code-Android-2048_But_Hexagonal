using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGridGenerator : MonoBehaviour
{
    public GameObject[] hexPrefabs; // Tablica prefab�w heks�w
    public GameObject hexTilePrefab; // Zmienna na aktualnie wybrany prefab
    public float hexSize = 0.6f; // Domy�lny hexSize
    public int radius = 2; // Promie� heksagonu (dla planszy 19 p�l)

    public List<HexCell> cells = new List<HexCell>(); // Lista wszystkich p�l planszy

    [Tooltip("Referencja do komponentu do spawnowania kafelk�w")]
    public TileSpawner tileSpawner;

    private const string HexSettingsKey = "HexSettings"; // Klucz dla Prefabu i hexSize

    void Start()
    {
        // Odczytanie zapisanych ustawie� z PlayerPrefs, lub ustawienie domy�lnych warto�ci
        int savedHexIndex = PlayerPrefs.HasKey(HexSettingsKey)
            ? PlayerPrefs.GetInt(HexSettingsKey)
            : 1; // Domy�lnie prefab o indeksie 1 i hexSize 0.6

        hexSize = savedHexIndex == 0 ? 0.54f : savedHexIndex == 1 ? 0.6f : 0.66f;
        hexTilePrefab = hexPrefabs[savedHexIndex]; // Przypisanie odpowiedniego prefabrykatu

        // Generowanie planszy
        GenerateGrid();
        tileSpawner.SpawnTile2(); // Spawning tiles

        
    }

    public void GenerateGrid()
    {
        for (int q = -radius; q <= radius; q++)
        {
            int r1 = Mathf.Max(-radius, -q - radius);
            int r2 = Mathf.Min(radius, -q + radius);
            for (int r = r1; r <= r2; r++)
            {
                Vector2 pos = HexToWorld(q, r);
                GameObject hexTile = Instantiate(hexTilePrefab, pos, Quaternion.identity, transform);

                // Pobierz komponent HexCell z prefabrykaty pola
                HexCell cell = hexTile.GetComponent<HexCell>();
                if (cell != null)
                {
                    cell.q = q;
                    cell.r = r;
                    cells.Add(cell);
                }
            }
        }
    }

    // Przelicza wsp�rz�dne axial (q, r) na pozycj� w �wiecie 2D
    Vector2 HexToWorld(int q, int r)
    {
        float x = hexSize * (3f / 2f * q);
        float y = hexSize * (Mathf.Sqrt(3) / 2f * q + Mathf.Sqrt(3) * r);
        return new Vector2(x, y);
    }

    public HexCell GetCellAt(int q, int r)
    {
        foreach (HexCell cell in cells)
        {
            if (cell.q == q && cell.r == r)
                return cell;
        }
        return null;
    }

    public void ClearCells()
    {
        cells.Clear(); // Wyczy�� list� przechowuj�c� kom�rki
    }

    public void SetHexPrefab(int index)
    {
        if (index >= 0 && index < hexPrefabs.Length)
        {
            hexTilePrefab = hexPrefabs[index];
            
        }
        
    }

}
