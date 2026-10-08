
using System;

public static class GameEventSystem
{
    public static event Action<GameState> OnGameStateChanged;

    public static event Action<ResourceType, int> OnResourcesChanged;

    public static void GameStateChanged(GameState newState)
    {
        OnGameStateChanged?.Invoke(newState);
    }

    public static void ResourcesChanged(ResourceType type, int amount)
    {
        OnResourcesChanged?.Invoke(type, amount);
    }
}
