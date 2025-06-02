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
    public float cellYOffset = -0.1f;

    [Header("Jugador")]
    public GameObject playerPrefab;

    [Header("Enemigos")]
    public GameObject enemyPrefab;
    public int enemyCount = 3;
    public int minDistanceFromPlayer = 3;

    private Vector3[,] gridPositions;
    private GameObject[,] cellObjects;
    private bool[,] occupiedCells;

    private CellType[,] cellTypes;

    private PlayerController playerController;
    private List<EnemyController> enemies = new List<EnemyController>();

    public enum CellType
    {
        Empty,
        Player,
        Enemy
    }

    void Start()
    {
        GenerateGrid();
        InitializePlayer();
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
            int startZ = 0;

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

    void GenerateEnemies()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("¡No se ha asignado el Enemy Prefab en el GridManager!");
            return;
        }

        Vector2Int playerPos = new Vector2Int(gridWidth / 2, 0);

        for (int i = 0; i < enemyCount; i++)
        {
            Vector2Int enemyPos = FindValidEnemyPosition(playerPos);

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

    Vector2Int FindValidEnemyPosition(Vector2Int playerPosition)
    {
        int maxAttempts = 100;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            int randomX = Random.Range(0, gridWidth);
            int randomZ = Random.Range(0, gridHeight);

            Vector2Int candidatePos = new Vector2Int(randomX, randomZ);

            if (IsCellFree(randomX, randomZ))
            {
                float distance = Vector2Int.Distance(candidatePos, playerPosition);
                if (distance >= minDistanceFromPlayer)
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
        }
    }
}