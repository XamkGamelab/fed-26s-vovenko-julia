using System;

public static class GameEventSystem
{
    public static event Action<GameState> OnGameStateChanged;
    public static event Action<ResourceType, int> OnResourcesChanged;

    public static event Action<ProductionBuilding> OnBuildingPlaced;
    public static event Action<ProductionBuilding, float> OnConstructionProgressChanged;
    public static event Action<ProductionBuilding> OnBuildingConstructionCompleted;

    public static event Action OnInvalidBuildingPlacement;
    public static event Action<ProductionBuilding> OnProductionCompleted;
    public static event Action<BuildingType> OnBuildingSelected;
    public static event Action<BuildingData> OnBuildingDataSelected;
    public static event Action<string> OnPlayerMessage;
    public static event Action<ProductionBuilding> OnProductionBuildingSelected;

    public static void GameStateChanged(GameState newState)
    {
        OnGameStateChanged?.Invoke(newState);
    }

    public static void ResourcesChanged(ResourceType type, int amount)
    {
        OnResourcesChanged?.Invoke(type, amount);
    }

    public static void BuildingPlaced(ProductionBuilding building)
    {
        OnBuildingPlaced?.Invoke(building);
    }

    public static void ConstructionProgressChanged(
        ProductionBuilding building,
        float progress)
    {
        OnConstructionProgressChanged?.Invoke(building, progress);
    }

    public static void BuildingConstructionCompleted(ProductionBuilding building)
    {
        OnBuildingConstructionCompleted?.Invoke(building);
    }

    public static void InvalidBuildingPlacement()
    {
        OnInvalidBuildingPlacement?.Invoke();
    }

    public static void ProductionCompleted(ProductionBuilding building)
    {
        OnProductionCompleted?.Invoke(building);
    }

    public static void BuildingSelected(BuildingType type)
    {
        OnBuildingSelected?.Invoke(type);
    }

    public static void BuildingDataSelected(BuildingData data)
    {
        OnBuildingDataSelected?.Invoke(data);
    }

    public static void PlayerMessage(string message)
    {
        OnPlayerMessage?.Invoke(message);
    }
    public static void ProductionBuildingSelected(ProductionBuilding building)
    {
        OnProductionBuildingSelected?.Invoke(building);
    }
}