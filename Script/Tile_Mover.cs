using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMover : MonoBehaviour
{
    [Tooltip("Referencja do komponentu generuj�cego plansz�")]
    public HexGridGenerator gridGenerator;

    [Tooltip("Lista prefabrykat�w kafelk�w, ustawiona rosn�co (2, 4, 8, 16, ...)")]
    public List<GameObject> tilePrefabs;

    [Tooltip("Referencja do komponentu do spawnowania nowych kafelk�w")]
    public TileSpawner tileSpawner;

    [Tooltip("Czas trwania animacji przesuni�cia pomi�dzy kolejnymi kom�rkami")]
    public float segmentDuration = 0.08f;

    [Tooltip("Referencja do komponentu, kt�ry sprawdza mo�liwo�� ruchu")]
    public MoveValidator moveValidator;

    [Tooltip("Referencja do komponentu, kt�ry sprawdza warunki przegranej")]
    public GameOverChecker gameOverChecker;

    public CollectionManager collectionManager;
    public bool isMoving = false;
    public GameUI gameUI;

    void Update()
    {
        if (isMoving)
            return;

        Vector2Int? direction = null;

        if (Input.GetKeyDown(KeyCode.S))
            direction = new Vector2Int(0, -1);     // Northwest
        else if (Input.GetKeyDown(KeyCode.D))
            direction = new Vector2Int(1, -1);     // Northeast
        else if (Input.GetKeyDown(KeyCode.E))
            direction = new Vector2Int(1, 0);      // East
        else if (Input.GetKeyDown(KeyCode.W))
            direction = new Vector2Int(0, 1);      // Southeast
        else if (Input.GetKeyDown(KeyCode.Q))
            direction = new Vector2Int(-1, 1);     // Southwest
        else if (Input.GetKeyDown(KeyCode.A))
            direction = new Vector2Int(-1, 0);     // West

        if (direction.HasValue && moveValidator.IsMovePossible(direction.Value))
        {
            StartCoroutine(MoveTiles(direction.Value));
            gameUI.IncrementMoves();
            gameUI.PlayMoveSound();
            gameUI.isWaitingForFirstMove = false;
        }
    }

    // Klasa pomocnicza przechowuj�ca dane o ruchu kafelka
    private class TileMoveCommand
    {
        public GameObject tile;
        public List<Vector3> pathPositions;
        public HexCell originCell;
        public HexCell targetCell;
        public bool isMerge;
        public HexCell mergeTarget;
        public int tileValue;
    }

    // Coroutine obliczaj�ca ruchy, animuj�ca przesuni�cia oraz aktualizuj�ca stan planszy
    public IEnumerator MoveTiles(Vector2Int direction)
    {
        isMoving = true;
        List<TileMoveCommand> moveCommands = new List<TileMoveCommand>();
        HashSet<HexCell> mergedCells = new HashSet<HexCell>();

        // Pobieramy wszystkie kom�rki z kafelkami
        List<HexCell> cellsWithTiles = new List<HexCell>();
        foreach (HexCell cell in gridGenerator.cells)
        {
            if (cell.currentTile != null)
                cellsWithTiles.Add(cell);
        }

        // Sortujemy kom�rki � te bardziej wysuni�te w kierunku ruchu b�d� przetwarzane jako pierwsze
        cellsWithTiles.Sort((a, b) =>
        {
            int aPriority = a.q * direction.x + a.r * direction.y;
            int bPriority = b.q * direction.x + b.r * direction.y;
            return bPriority.CompareTo(aPriority);
        });

        // Przechodzimy po ka�dej kom�rce, symuluj�c ruch i natychmiast aktualizuj�c stan zaj�to�ci
        foreach (HexCell cell in cellsWithTiles)
        {
            if (cell.currentTile == null || mergedCells.Contains(cell))
                continue;

            GameObject movingTile = cell.currentTile;
            int tileValue = movingTile.GetComponent<Tile>().value;
            HexCell currentCell = cell;
            List<Vector3> pathPositions = new List<Vector3>();
            // Dodajemy pozycj� pocz�tkow� z ustalonym Z = -1
            Vector3 startPos = new Vector3(currentCell.transform.position.x, currentCell.transform.position.y, -1);
            pathPositions.Add(startPos);
            HexCell mergeTarget = null;

            // Przesuwamy si� "krok po kroku" w zadanym kierunku, obliczaj�c �cie�k�
            while (true)
            {
                int nextQ = currentCell.q + direction.x;
                int nextR = currentCell.r + direction.y;
                HexCell nextCell = gridGenerator.GetCellAt(nextQ, nextR);
                if (nextCell == null)
                    break;

                if (nextCell.currentTile != null)
                {
                    int nextTileValue = nextCell.currentTile.GetComponent<Tile>().value;
                    // Je�li trafimy na kafelek o tej samej warto�ci i nie by� jeszcze ��czony, ustawiamy merge
                    if (nextTileValue == tileValue && !mergedCells.Contains(nextCell))
                    {
                        mergeTarget = nextCell;
                    }
                    break;
                }
                else
                {
                    Vector3 nextPos = new Vector3(nextCell.transform.position.x, nextCell.transform.position.y, -1);
                    pathPositions.Add(nextPos);
                    currentCell = nextCell;
                }
            }

            // Je�li mo�liwe jest ��czenie
            if (mergeTarget != null)
            {
                TileMoveCommand cmd = new TileMoveCommand();
                cmd.tile = movingTile;
                cmd.pathPositions = new List<Vector3>(pathPositions);
                cmd.originCell = cell;
                cmd.targetCell = mergeTarget;
                cmd.isMerge = true;
                cmd.mergeTarget = mergeTarget;
                cmd.tileValue = tileValue;
                moveCommands.Add(cmd);

                mergedCells.Add(mergeTarget);
                // Aktualizujemy stan: kom�rka pocz�tkowa staje si� pusta
                cell.currentTile = null;
            }
            // Je�li kafelek mo�e si� przesun��, ale nie nast�puje merge
            else if (currentCell != cell)
            {
                TileMoveCommand cmd = new TileMoveCommand();
                cmd.tile = movingTile;
                cmd.pathPositions = new List<Vector3>(pathPositions);
                cmd.originCell = cell;
                cmd.targetCell = currentCell;
                cmd.isMerge = false;
                cmd.tileValue = tileValue;
                moveCommands.Add(cmd);

                // Aktualizujemy stan � zwalniamy kom�rk� pocz�tkow� i zajmujemy docelow�
                cell.currentTile = null;
                currentCell.currentTile = movingTile;
            }
        }

        // Uruchamiamy animacje dla wszystkich ruch�w jednocze�nie
        int animationsRunning = moveCommands.Count;
        foreach (TileMoveCommand cmd in moveCommands)
        {
            StartCoroutine(AnimateTile(cmd.tile, cmd.pathPositions, segmentDuration, cmd, () => { animationsRunning--; }));
        }

        yield return new WaitUntil(() => animationsRunning == 0);

        // Po zako�czeniu animacji aktualizujemy ostateczny stan planszy
        foreach (TileMoveCommand cmd in moveCommands)
        {
            if (cmd.originCell != null)
                cmd.originCell.currentTile = null;

            if (cmd.isMerge)
            {
                if (cmd.tile != null)
                    Destroy(cmd.tile);
                if (cmd.mergeTarget != null && cmd.mergeTarget.currentTile != null)
                    Destroy(cmd.mergeTarget.currentTile);

                GameObject newTilePrefab = GetNextTilePrefab(cmd.tileValue);
                if (newTilePrefab != null && cmd.mergeTarget != null)
                {
                    GameObject newTile = Instantiate(newTilePrefab, cmd.mergeTarget.transform.position, Quaternion.identity, cmd.mergeTarget.transform);
                    newTile.transform.position = new Vector3(newTile.transform.position.x, newTile.transform.position.y, -1);
                    cmd.mergeTarget.currentTile = newTile;
                    gameUI.AddScore(cmd.tileValue * 2);

                    // Sprawdzamy, czy kafelek jest ju� odblokowany
                    int tileIndex = GetTileIndex(cmd.tileValue * 2); // Tutaj wywo�ujesz GetTileIndex
                    if (tileIndex != -1) // Je�li znaleziono poprawny indeks
                    {
                        // Uzyskujemy dost�p do CollectionKey poprzez CollectionManager
                        string savedTiles = PlayerPrefs.GetString(CollectionManager.CollectionKey, "100000000000000000");

                        if (savedTiles[tileIndex] == '0') // Je�li kafelek jeszcze nie jest odblokowany
                        {
                            // U�ywamy metody UnlockTile z CollectionManager
                            collectionManager.UnlockTile(tileIndex); // To jest metoda w Twoim CollectionManager
                        }
                    }
                }
            }
            else
            {
                if (cmd.targetCell != null)
                {
                    cmd.targetCell.currentTile = cmd.tile;
                    cmd.tile.transform.position = new Vector3(cmd.targetCell.transform.position.x, cmd.targetCell.transform.position.y, -1);
                }
            }
        }
       
        tileSpawner.SpawnTile2();
        tileSpawner.SpawnTile2();
        gameOverChecker.CheckGameOver();
        isMoving = false;
    }

    // Coroutine animuj�ca przesuwanie pojedynczego kafelka po wyliczonej �cie�ce
    IEnumerator AnimateTile(GameObject tile, List<Vector3> path, float segmentDuration, TileMoveCommand moveCommand, System.Action onComplete)
    {
        if (path.Count < 2)
        {
            // Je�li kafelek ju� jest na miejscu, po��czymy go natychmiast
            if (moveCommand != null && moveCommand.isMerge)
            {
                GameObject targetTile = moveCommand.mergeTarget.currentTile;
                if (targetTile != null)
                {
                    Vector3 mergePosition = targetTile.transform.position;
                    // Przesuwamy kafelek o jedno pole, zanim wejdzie pod drugi
                    float moveTime = segmentDuration;
                    float elapsedMove = 0f;

                    // Animacja przesuni�cia o jedno pole
                    while (elapsedMove < moveTime)
                    {
                        tile.transform.position = Vector3.Lerp(tile.transform.position, mergePosition, elapsedMove / moveTime);
                        elapsedMove += Time.deltaTime;
                        yield return null;
                    }
                    tile.transform.position = mergePosition;
                }
            }
            onComplete?.Invoke();
            yield break;
        }

        for (int i = 1; i < path.Count; i++)
        {
            Vector3 start = path[i - 1];
            Vector3 end = path[i];
            float elapsed = 0f;

            while (elapsed < segmentDuration)
            {
                tile.transform.position = Vector3.Lerp(start, end, elapsed / segmentDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            tile.transform.position = end;
        }

        // Je�li kafelek si� ��czy, dodatkowa animacja na�o�enia si� na inny kafelek
        if (moveCommand != null && moveCommand.isMerge)
        {
            GameObject targetTile = moveCommand.mergeTarget.currentTile;
            if (targetTile != null)
            {
                Vector3 mergePosition = targetTile.transform.position;
                float mergeTime = segmentDuration;
                float elapsedMerge = 0f;

                while (elapsedMerge < mergeTime)
                {
                    tile.transform.position = Vector3.Lerp(tile.transform.position, mergePosition, elapsedMerge / mergeTime);
                    elapsedMerge += Time.deltaTime;
                    yield return null;
                }
                tile.transform.position = mergePosition;
            }
        }

        onComplete?.Invoke();
    }



    GameObject GetNextTilePrefab(int currentValue)
    {
        for (int i = 0; i < tilePrefabs.Count - 1; i++)
        {
            Tile tileComponent = tilePrefabs[i].GetComponent<Tile>();
            if (tileComponent != null && tileComponent.value == currentValue)
            {
                return tilePrefabs[i + 1];
            }
        }
        return null;
 
    }

    private int GetTileIndex(int tileValue)
    {
        int[] tileValues = { 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536, 131072, 262144 };
        for (int i = 0; i < tileValues.Length; i++)
        {
            if (tileValues[i] == tileValue)
                return i;
        }
        return -1; // Je�li warto�� nie jest w tablicy, zwraca -1
    }

}
