using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [Header("Configuración del Tablero")]
    public int gridWidth = 10;
    public int gridHeight = 10;
    public float cellSize = 1.0f;

    [Header("Visualización")]
    public GameObject cellPrefab;
    public Material normalCellMaterial;
    public Material playerCellMaterial;
    public Material enemyCellMaterial;
    public Material bossCellMaterial; 
    public float cellYOffset = -0.1f;

    [Header("Jugador")]
    public GameObject playerPrefab;

    [Header("Enemigos")]
    public GameObject enemyPrefab;
    public int enemyCount = 3;
    public int minDistanceFromPlayer = 3;

    [Header("Boss")]
    public GameObject bossPrefab; 

    private Vector3[,] gridPositions;
    private GameObject[,] cellObjects;
    private bool[,] occupiedCells;

    private CellType[,] cellTypes;

    private PlayerController playerController;
    private List<EnemyController> enemies = new List<EnemyController>();
    private BossController bossController;

    public enum CellType
    {
        Empty,
        Player,
        Enemy,
        Boss 
    }

    void Start()
    {
        GenerateGrid();
        InitializePlayer();
        GenerateBoss();
        GenerateEnemies();
    }

    void GenerateGrid()
    {
        gridPositions = new Vector3[gridWidth, gridHeight];
        cellObjects = new GameObject[gridWidth, gridHeight];
        occupiedCells = new bool[gridWidth, gridHeight];
        cellTypes = new CellType[gridWidth, gridHeight];

        float offsetX = (gridWidth - 1) * cellSize * 0.5f;
        float offsetZ = (gridHeight - 1) * cellSize * 0.5f;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector3 worldPos = new Vector3(
                    x * cellSize - offsetX,
                    0,
                    z * cellSize - offsetZ
                );

                gridPositions[x, z] = worldPos;
                cellTypes[x, z] = CellType.Empty;

                if (cellPrefab != null)
                {
                    Vector3 cellPos = new Vector3(worldPos.x, cellYOffset, worldPos.z);
                    GameObject cell = Instantiate(cellPrefab, cellPos, Quaternion.identity);
                    cell.name = $"Cell_{x}_{z}";
                    cell.transform.parent = transform;

                    if (normalCellMaterial != null)
                    {
                        Renderer renderer = cell.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            renderer.material = normalCellMaterial;
                        }
                    }

                    cellObjects[x, z] = cell;
                }
            }
        }
    }

    void InitializePlayer()
    {
        if (playerPrefab != null)
        {
            int startX = gridWidth / 2;
            int startZ = 0; //Abajo del mapa

            Vector3 playerPosition = GetWorldPosition(startX, startZ);
            GameObject playerObj = Instantiate(playerPrefab, playerPosition, Quaternion.identity);
            playerObj.name = "Player";

            playerController = playerObj.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.SetGridManager(this);
                playerController.SetInitialGridPosition(startX, startZ);
                SetCellOccupied(startX, startZ, true, CellType.Player);
            }

        }
        else
        {
            Debug.LogError("¡No se ha asignado el Player Prefab en el GridManager!");
        }
    }

    void GenerateBoss()
    {
        if (bossPrefab == null)
        {
            Debug.LogWarning("No se ha asignado el Boss Prefab en el GridManager.");
            return;
        }

        //Jugador está en el centro abajo, boss va en el centro arriba
        int bossX = gridWidth / 2;
        int bossZ = gridHeight - 1;

        Vector3 bossWorldPos = GetWorldPosition(bossX, bossZ);
        GameObject bossObj = Instantiate(bossPrefab, bossWorldPos, Quaternion.identity);
        bossObj.name = "Boss";

        bossController = bossObj.GetComponent<BossController>();
        if (bossController != null)
        {
            bossController.SetGridManager(this);
            bossController.SetInitialGridPosition(bossX, bossZ);
            SetCellOccupied(bossX, bossZ, true, CellType.Boss);
        }
        else
        {
            Debug.LogError("El Boss Prefab no tiene el componente BossController!");
        }
    }

    void GenerateEnemies()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("¡No se ha asignado el Enemy Prefab en el GridManager!");
            return;
        }

        Vector2Int playerPos = new Vector2Int(gridWidth / 2, 0);
        Vector2Int bossPos = new Vector2Int(gridWidth / 2, gridHeight - 1); // Posición del boss para evitarla

        for (int i = 0; i < enemyCount; i++)
        {
            Vector2Int enemyPos = FindValidEnemyPosition(playerPos, bossPos);

            if (enemyPos != Vector2Int.one * -1)
            {
                Vector3 enemyWorldPos = GetWorldPosition(enemyPos.x, enemyPos.y);
                GameObject enemyObj = Instantiate(enemyPrefab, enemyWorldPos, Quaternion.identity);
                enemyObj.name = $"Enemy_{i}";

                EnemyController enemyController = enemyObj.GetComponent<EnemyController>();
                if (enemyController != null)
                {
                    enemyController.enemyName = $"Enemigo {i + 1}";
                    enemyController.SetGridManager(this);
                    enemyController.SetInitialGridPosition(enemyPos.x, enemyPos.y);
                    enemies.Add(enemyController);
                    SetCellOccupied(enemyPos.x, enemyPos.y, true, CellType.Enemy);
                }

            }
            else
            {
                Debug.LogWarning($"No se pudo encontrar posición válida para el enemigo {i}");
            }
        }
    }

    Vector2Int FindValidEnemyPosition(Vector2Int playerPosition, Vector2Int bossPosition)
    {
        int maxAttempts = 100;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            int randomX = Random.Range(0, gridWidth);
            int randomZ = Random.Range(0, gridHeight);

            Vector2Int candidatePos = new Vector2Int(randomX, randomZ);

            if (IsCellFree(randomX, randomZ))
            {
                float distanceToPlayer = Vector2Int.Distance(candidatePos, playerPosition);
                float distanceToBoss = Vector2Int.Distance(candidatePos, bossPosition);

                //Asegurar que el enemigo este lejos del player y no obstruya al boss
                if (distanceToPlayer >= minDistanceFromPlayer && distanceToBoss >= 1)
                {
                    return candidatePos;
                }
            }
        }

        return Vector2Int.one * -1;
    }

    public Vector3 GetWorldPosition(int gridX, int gridZ)
    {
        if (IsValidGridPosition(gridX, gridZ))
        {
            return gridPositions[gridX, gridZ];
        }
        return Vector3.zero;
    }

    public bool IsValidGridPosition(int gridX, int gridZ)
    {
        return gridX >= 0 && gridX < gridWidth && gridZ >= 0 && gridZ < gridHeight;
    }

    public bool IsCellFree(int gridX, int gridZ)
    {
        if (!IsValidGridPosition(gridX, gridZ))
            return false;

        return !occupiedCells[gridX, gridZ];
    }

    public void SetCellOccupied(int gridX, int gridZ, bool occupied, CellType cellType = CellType.Empty)
    {
        if (IsValidGridPosition(gridX, gridZ))
        {
            occupiedCells[gridX, gridZ] = occupied;
            cellTypes[gridX, gridZ] = occupied ? cellType : CellType.Empty;

            UpdateCellHighlight(gridX, gridZ);
        }
    }

    void UpdateCellHighlight(int gridX, int gridZ)
    {
        if (IsValidGridPosition(gridX, gridZ) && cellObjects[gridX, gridZ] != null)
        {
            Renderer renderer = cellObjects[gridX, gridZ].GetComponent<Renderer>();
            if (renderer != null)
            {
                Material materialToUse = normalCellMaterial;

                switch (cellTypes[gridX, gridZ])
                {
                    case CellType.Player:
                        materialToUse = playerCellMaterial;
                        break;
                    case CellType.Enemy:
                        materialToUse = enemyCellMaterial != null ? enemyCellMaterial : normalCellMaterial;
                        break;
                    case CellType.Boss:
                        materialToUse = bossCellMaterial != null ? bossCellMaterial : normalCellMaterial;
                        break;
                    case CellType.Empty:
                    default:
                        materialToUse = normalCellMaterial;
                        break;
                }

                renderer.material = materialToUse;
            }
        }
    }

    public Vector2Int GetPlayerPosition()
    {
        if (playerController != null)
        {
            return playerController.GetGridPosition();
        }
        return Vector2Int.zero;
    }

    public Vector2Int GetBossPosition()
    {
        if (bossController != null)
        {
            return bossController.GetGridPosition();
        }
        return Vector2Int.zero;
    }

    public BossController GetBossController()
    {
        return bossController;
    }

    void OnDrawGizmos()
    {
        if (gridPositions != null)
        {
            Gizmos.color = Color.blue;
            for (int x = 0; x < gridWidth; x++)
            {
                for (int z = 0; z < gridHeight; z++)
                {
                    Gizmos.DrawWireCube(gridPositions[x, z], Vector3.one * cellSize * 0.8f);
                }
            }

            if (Application.isPlaying)
            {
                //Posición del jugador
                Gizmos.color = Color.green;
                Vector3 playerPos = GetWorldPosition(gridWidth / 2, 0);
                Gizmos.DrawWireCube(playerPos, Vector3.one * cellSize);

                //Posición del boss
                Gizmos.color = Color.red;
                Vector3 bossPos = GetWorldPosition(gridWidth / 2, gridHeight - 1);
                Gizmos.DrawWireCube(bossPos, Vector3.one * cellSize);
            }
        }
    }
}