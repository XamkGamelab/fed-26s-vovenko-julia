using System;

public static class GameEventSystem
{
    public static event Action<GameState> OnGameStateChanged;
    public static event Action<ResourceType, int> OnResourcesChanged;

    public static event Action<ProductionBuilding> OnBuildingPlaced;
    public static event Action<ProductionBuilding, float> OnConstructionProgressChanged;
    public static event Action<ProductionBuilding> OnBuildingConstructionCompleted;

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
}