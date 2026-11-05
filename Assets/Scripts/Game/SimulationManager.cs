
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using System;

[Serializable]
public class SimulationSaveData
{
    public List<string> buildingStates = new List<string>();
}

public class SimulationManager : MonoBehaviour, IJsonSaveable
{
    [Header("Building Data")]
    [SerializeField] private BuildingData[] buildingDataAssets;

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

        building.Initialize(data);
        building.SetGridPosition(gridPosition);
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

    public string CaptureState()
    {
        SimulationSaveData saveData = new SimulationSaveData();

        foreach (ProductionBuilding building in productionBuildings)
        {
            if (building != null)
                saveData.buildingStates.Add(building.CaptureState());
        }

        return JsonUtility.ToJson(saveData);
    }

    public void RestoreState(string json)
    {
        if (buildingGrid == null || productionBuildings == null)
        {
            Debug.LogError("Initialize SimulationManager before loading buildings.");
            return;
        }

        if (productionBuildings.Count > 0)
        {
            Debug.LogWarning("Load buildings into a fresh simulation.");
            return;
        }

        SimulationSaveData saveData;

        try
        {
            saveData = JsonUtility.FromJson<SimulationSaveData>(json);
        }
        catch (Exception exception)
        {
            Debug.LogError($"Could not load simulation data: {exception.Message}");
            return;
        }

        if (saveData == null || saveData.buildingStates == null)
        {
            Debug.LogError("Simulation save data could not be read.");
            return;
        }

        foreach (string buildingJson in saveData.buildingStates)
        {
            ProductionBuildingSaveData buildingState;

            try
            {
                buildingState =
                    JsonUtility.FromJson<ProductionBuildingSaveData>(buildingJson);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Could not read building entry: {exception.Message}");
                continue;
            }

            if (buildingState == null)
                continue;

            BuildingData data = FindBuildingData(buildingState.type);

            if (data == null || data.Prefab == null)
            {
                Debug.LogError($"No prefab configured for {buildingState.type}.");
                continue;
            }

            Vector2Int gridPosition =
                new Vector2Int(buildingState.gridX, buildingState.gridY);

            if (buildingGrid.IsOccupied(gridPosition, data.Size))
            {
                Debug.LogWarning($"Skipped overlapping building at {gridPosition}.");
                continue;
            }

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
                Debug.LogError($"Prefab for {buildingState.type} has no ProductionBuilding.");
                continue;
            }

            building.Initialize(data);
            building.SetGridPosition(gridPosition);
            building.RestoreState(buildingJson);

            buildingGrid.SetCellOccupiedState(
                gridPosition,
                data.Size,
                building
            );

            RegisterBuilding(building);
        }
    }

    private BuildingData FindBuildingData(BuildingType type)
    {
        foreach (BuildingData data in buildingDataAssets)
        {
            if (data != null && data.Type == type)
                return data;
        }

        return null;
    }
}
