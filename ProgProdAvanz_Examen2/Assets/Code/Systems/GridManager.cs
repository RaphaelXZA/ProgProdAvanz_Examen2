using UnityEngine;

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
    public float cellYOffset = -0.1f;

    [Header("Jugador")]
    public GameObject playerPrefab;

    private Vector3[,] gridPositions;
    private GameObject[,] cellObjects;
    private PlayerController playerController;

    void Start()
    {
        GenerateGrid();
        InitializePlayer();
    }

    void GenerateGrid()
    {
        gridPositions = new Vector3[gridWidth, gridHeight];
        cellObjects = new GameObject[gridWidth, gridHeight];

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
            //Posición inicial del jugador: abajo y al centro
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
            }
        }
        else
        {
            Debug.LogError("¡No se ha asignado el prefab del Player al GridManager!");
        }
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

    public void HighlightCell(int gridX, int gridZ, bool highlight)
    {
        if (IsValidGridPosition(gridX, gridZ) && cellObjects[gridX, gridZ] != null)
        {
            Renderer renderer = cellObjects[gridX, gridZ].GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = highlight ? playerCellMaterial : normalCellMaterial;
            }
        }
    }
    void OnDrawGizmos()
    {
        if (gridPositions != null)
        {
            Gizmos.color = Color.grey;
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
