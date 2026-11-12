using System;
using UnityEngine;
using TMPro;

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

    [Header("Building Placement Hint")]
    [SerializeField] private TMP_Text buildingHintText;

    private void OnEnable()
    {
        GameEventSystem.OnGameStateChanged += HandleGameStateChanged;
        GameEventSystem.OnResourcesChanged += HandleResourcesChanged;
        GameEventSystem.OnBuildingSelected += HandleBuildingSelected;
    }
    private void OnDisable()
    {
        GameEventSystem.OnGameStateChanged -= HandleGameStateChanged;
        GameEventSystem.OnResourcesChanged -= HandleResourcesChanged;
        GameEventSystem.OnBuildingSelected -= HandleBuildingSelected;
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
    }

    private void HandleBuildingSelected(BuildingType type)
    {
        if (buildingHintText == null)
            return;

        switch (type)
        {
            case BuildingType.Woodcutter:
                buildingHintText.text = "Place the Woodcutter near trees.";
                break;

            case BuildingType.Quarry:
                buildingHintText.text = "Place the Quarry near a stone deposit.";
                break;
        }
    }

}