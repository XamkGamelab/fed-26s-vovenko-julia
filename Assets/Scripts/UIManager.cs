using System;
using UnityEngine;
using TMPro;
using System.Collections;

public enum UIPanel
{
    MainMenu,
    HUD,
    Pause
}

public class UIManager : MonoBehaviour
{
    [Serializable]
    public struct UIPanelReference
    {
        public UIPanel panel;
        public GameObject canvas;
    }

    [SerializeField]
    private UIPanelReference[] panels;


    [Header("Resource Display")]
    [SerializeField] private TMP_Text woodText;
    [SerializeField] private TMP_Text stoneText;
    [SerializeField] private TMP_Text metalText;

    [Header("Building Placement Hint")]
    [SerializeField] private TMP_Text buildingHintText;

    [SerializeField, Min(0.1f)] private float playerMessageDuration = 2.5f;

    [Header("Day/Night Display")]
    [SerializeField] private TMP_Text dayNightText;
    [SerializeField] private DayNightCycle dayNightCycle;

    private string selectedBuildingHint = string.Empty;
    private Coroutine playerMessageCoroutine;

    private void OnEnable()
    {
        GameEventSystem.OnGameStateChanged += HandleGameStateChanged;
        GameEventSystem.OnResourcesChanged += HandleResourcesChanged;
        GameEventSystem.OnBuildingSelected += HandleBuildingSelected;
        GameEventSystem.OnPlayerMessage += HandlePlayerMessage;
    }
    private void OnDisable()
    {
        GameEventSystem.OnGameStateChanged -= HandleGameStateChanged;
        GameEventSystem.OnResourcesChanged -= HandleResourcesChanged;
        GameEventSystem.OnBuildingSelected -= HandleBuildingSelected;
        GameEventSystem.OnPlayerMessage -= HandlePlayerMessage;
    }


    private void HandleResourcesChanged(ResourceType type, int amount)
    {
        switch (type)
        {
            case ResourceType.Wood:
                if (woodText != null)
                    woodText.text = $"Wood: {amount}";
                break;

            case ResourceType.Stone:
                if (stoneText != null)
                    stoneText.text = $"Stone: {amount}";
                break;

            case ResourceType.Metal:
                if (metalText != null)
                    metalText.text = $"Metal: {amount}";
                break;
        }
    }

    private void HandleGameStateChanged(GameState newState)
    {
        Debug.Log("UI state changed to: " + newState);

        switch (newState)
        {
            case GameState.MainMenu:
                ShowPanel(UIPanel.MainMenu);
                break;

            case GameState.Gameplay:
                ShowPanel(UIPanel.HUD);
                RefreshResources();
                break;

            case GameState.Paused:
                ShowPanel(UIPanel.Pause);
                break;
        }
    }

    private void ShowPanel(UIPanel activePanel)
    {
        foreach (UIPanelReference panelReference in panels)
        {
            panelReference.canvas.SetActive(
                panelReference.panel == activePanel
            );
        }
    }


    private void RefreshResources()
    {
        SimulationManager simulation =
            FindAnyObjectByType<SimulationManager>();

        if (simulation == null || simulation.Resources == null)
            return;

        HandleResourcesChanged(
            ResourceType.Wood,
            simulation.Resources.GetAmount(ResourceType.Wood)
        );

        HandleResourcesChanged(
            ResourceType.Stone,
            simulation.Resources.GetAmount(ResourceType.Stone)
        );

        HandleResourcesChanged(
            ResourceType.Metal,
            simulation.Resources.GetAmount(ResourceType.Metal)
        );
    }

    private void HandleBuildingSelected(BuildingType type)
    {
        if (buildingHintText == null)
            return;

        switch (type)
        {
            case BuildingType.Woodcutter:
                selectedBuildingHint =
                    "Woodcutter selected. Place near trees.";
                break;

            case BuildingType.Quarry:
                selectedBuildingHint =
                    "Quarry selected. Place near a stone deposit.";
                break;

            case BuildingType.Smelter:
                selectedBuildingHint =
                    "Smelter selected. It needs wood and stone.";
                break;

            default:
                selectedBuildingHint = string.Empty;
                break;
        }

        if (playerMessageCoroutine == null)
            buildingHintText.text = selectedBuildingHint;
    }

    private void Update()
    {
        if (dayNightText == null || dayNightCycle == null)
            return;

        dayNightText.text = dayNightCycle.IsDay
            ? "DAY | Production active"
            : "NIGHT | Production paused";
    }
    private void HandlePlayerMessage(string message)
    {
        if (buildingHintText == null)
            return;

        if (playerMessageCoroutine != null)
            StopCoroutine(playerMessageCoroutine);

        buildingHintText.text = message;
        playerMessageCoroutine = StartCoroutine(RestoreBuildingHint());
    }

    private IEnumerator RestoreBuildingHint()
    {
        yield return new WaitForSeconds(playerMessageDuration);

        if (buildingHintText != null)
            buildingHintText.text = selectedBuildingHint;

        playerMessageCoroutine = null;
    }

}