
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class SimulationManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject worldCursorPrefab;
    [SerializeField] private GameObject cameraPrefab;

    private GameObject worldCursorInstance;
    private GameObject cameraInstance;

    // Gameplay systems
    private PlayerResources playerResources;
    private BuildingGrid buildingGrid;

    private List<ProductionBuilding> productionBuildings =
        new List<ProductionBuilding>();

    private bool isPaused;

    public PlayerResources Resources => playerResources;
    public BuildingGrid Grid => buildingGrid;

    public void Initialize()
    {
        // Initialize gameplay systems
        playerResources = new PlayerResources();
        buildingGrid = new BuildingGrid();

        // Create cursor
        worldCursorInstance = Instantiate(
            worldCursorPrefab,
            Vector3.zero,
            Quaternion.identity,
            transform
        );

        // Create camera
        cameraInstance = Instantiate(
            cameraPrefab,
            transform
        );

        CinemachineCamera cinemachineCamera =
            cameraInstance.GetComponent<CinemachineCamera>();

        if (cinemachineCamera != null)
        {
            cinemachineCamera.Target.TrackingTarget =
                worldCursorInstance.transform;
        }

        Debug.Log("Simulation initialized");
    }

    private void OnEnable()
    {
        GameEventSystem.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameEventSystem.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState state)
    {
        isPaused = state != GameState.Gameplay;
    }

    private void Update()
    {
        if (isPaused || playerResources == null)
            return;

        foreach (ProductionBuilding building in productionBuildings)
        {
            if (building != null)
            {
                building.Simulate(
                    Time.deltaTime,
                    playerResources
                );
            }
        }
    }

    public void RegisterBuilding(ProductionBuilding building)
    {
        if (building != null &&
            !productionBuildings.Contains(building))
        {
            productionBuildings.Add(building);
        }
    }


    public bool TryPlaceBuilding(BuildingData data, Vector2Int gridPosition)
    {

        if (isPaused)
            return false;

        if (data == null || data.Prefab == null ||
            playerResources == null || buildingGrid == null)
            return false;

        // Check if the required cells are occupied
        if (buildingGrid.IsOccupied(gridPosition, data.Size))
        {
            Debug.Log("Cannot place building: cells occupied.");
            return false;
        }

        // Check construction cost
        if (!playerResources.HasEnough(data.CostType, data.Cost))
        {
            Debug.Log("Cannot place building: not enough resources.");
            return false;
        }

        // Create building at the selected grid position
        Vector3 worldPosition = new Vector3(
            gridPosition.x + (data.Size.x - 1) * 0.5f,
            0.5f,
            gridPosition.y + (data.Size.y - 1) * 0.5f
        );

        GameObject instance = Instantiate(
            data.Prefab,
            worldPosition,
            Quaternion.identity,
            transform
        );

        ProductionBuilding building =
            instance.GetComponent<ProductionBuilding>();

        if (building == null)
        {
            Destroy(instance);
            Debug.LogError("Building prefab is missing ProductionBuilding!");
            return false;
        }

        building.BeginConstruction();

        if (!playerResources.TrySpend(data.CostType, data.Cost))
        {
            Destroy(instance);
            return false;
        }

        // Mark cells as occupied
        buildingGrid.SetCellOccupiedState(
            gridPosition,
            data.Size,
            building
        );

        // Register building for production
        RegisterBuilding(building);
        GameEventSystem.BuildingPlaced(building);

        Debug.Log($"Placed {data.Type} at {gridPosition}");

        return true;
    }

}
