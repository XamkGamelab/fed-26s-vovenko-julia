using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameState CurrentState { get; private set; }

    [SerializeField] private GameObject simulationManagerPrefab;

    private GameObject simulationManagerInstance;

    private void Start()
    {
        ChangeState(GameState.MainMenu);
    }

    public void ChangeState(GameState newState)
    {
        GameState previousState = CurrentState;
        CurrentState = newState;

        if (previousState != GameState.Gameplay &&
            newState == GameState.Gameplay)
        {
            StartSimulation();
        }

        if (previousState == GameState.Gameplay &&
            newState == GameState.MainMenu)
        {
            StopSimulation();
        }

        GameEventSystem.GameStateChanged(newState);
    }

    public void StartGame()
    {
        ChangeState(GameState.Gameplay);
    }

    private void StartSimulation()
    {
        if (simulationManagerInstance != null)
            return;

        simulationManagerInstance = Instantiate(simulationManagerPrefab);

        SimulationManager simulationManager =
            simulationManagerInstance.GetComponent<SimulationManager>();

        simulationManager.Initialize();
    }

    private void StopSimulation()
    {
        if (simulationManagerInstance != null)
        {
            Destroy(simulationManagerInstance);
            simulationManagerInstance = null;
        }
    }
}