using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameState CurrentState { get; private set; }

    private void Start()
    {
        ChangeState(GameState.MainMenu);
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        GameEventSystem.GameStateChanged(newState);
    }

    public void StartGame()
    {
        ChangeState(GameState.Gameplay);
    }
}