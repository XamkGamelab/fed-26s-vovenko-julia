using System;

public static class GameEventSystem
{
    public static event Action<GameState> OnGameStateChanged;

    public static void GameStateChanged(GameState newState)
    {
        OnGameStateChanged?.Invoke(newState);
    }
}