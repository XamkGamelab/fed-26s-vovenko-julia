using System;
using System.IO;
using UnityEngine;

public class PersistenceManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private DayNightCycle dayNightCycle;

    private string SavePath =>
        Path.Combine(Application.persistentDataPath, "savegame.json");

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindAnyObjectByType<GameManager>();

        if (dayNightCycle == null)
            dayNightCycle = FindAnyObjectByType<DayNightCycle>();
    }

    public void SaveGame()
    {
        SimulationManager simulation =
            gameManager != null ? gameManager.CurrentSimulationManager : null;

        if (simulation == null || simulation.Resources == null)
        {
            Debug.LogWarning("Start a game before saving.");
            return;
        }

        GameSaveData saveData = new GameSaveData
        {
            resourcesJson = simulation.Resources.CaptureState(),
            simulationJson = simulation.CaptureState(),
            dayNightJson = dayNightCycle != null
                ? dayNightCycle.CaptureState()
                : ""
        };

        try
        {
            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"Game saved to: {SavePath}");
        }
        catch (Exception exception)
        {
            Debug.LogError($"Could not save the game: {exception.Message}");
        }
    }

    public void LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning($"Save file not found: {SavePath}");
            return;
        }

        GameSaveData saveData;

        try
        {
            string json = File.ReadAllText(SavePath);
            saveData = JsonUtility.FromJson<GameSaveData>(json);
        }
        catch (Exception exception)
        {
            Debug.LogError($"Could not read the save file: {exception.Message}");
            return;
        }

        if (saveData == null ||
            string.IsNullOrWhiteSpace(saveData.resourcesJson) ||
            string.IsNullOrWhiteSpace(saveData.simulationJson))
        {
            Debug.LogError("Save file is missing required data.");
            return;
        }

        SimulationManager simulation =
            gameManager.StartFreshSimulationForLoad();

        if (simulation == null || simulation.Resources == null)
        {
            Debug.LogError("Could not start a simulation for loading.");
            return;
        }

        simulation.Resources.RestoreState(saveData.resourcesJson);
        simulation.RestoreState(saveData.simulationJson);

        if (dayNightCycle != null &&
            !string.IsNullOrWhiteSpace(saveData.dayNightJson))
        {
            dayNightCycle.RestoreState(saveData.dayNightJson);
        }
    }
}