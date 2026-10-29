using UnityEngine;

[RequireComponent(typeof(ProductionBuilding))]
public class BuildingConstructionVisual : MonoBehaviour
{
    [SerializeField] private Transform visualRoot;
    [SerializeField, Min(0f)] private float riseHeight = 1f;

    private ProductionBuilding building;
    private Vector3 completedLocalPosition;

    private void Awake()
    {
        building = GetComponent<ProductionBuilding>();

        if (visualRoot != null)
            completedLocalPosition = visualRoot.localPosition;
    }

    private void OnEnable()
    {
        GameEventSystem.OnBuildingPlaced += HandleBuildingPlaced;
        GameEventSystem.OnConstructionProgressChanged += HandleProgressChanged;
        GameEventSystem.OnBuildingConstructionCompleted += HandleConstructionCompleted;
    }

    private void OnDisable()
    {
        GameEventSystem.OnBuildingPlaced -= HandleBuildingPlaced;
        GameEventSystem.OnConstructionProgressChanged -= HandleProgressChanged;
        GameEventSystem.OnBuildingConstructionCompleted -= HandleConstructionCompleted;
    }

    private void HandleBuildingPlaced(ProductionBuilding placedBuilding)
    {
        if (placedBuilding == building)
            ApplyProgress(0f);
    }

    private void HandleProgressChanged(
        ProductionBuilding changedBuilding,
        float progress)
    {
        if (changedBuilding == building)
            ApplyProgress(progress);
    }

    private void HandleConstructionCompleted(ProductionBuilding completedBuilding)
    {
        if (completedBuilding == building)
            ApplyProgress(1f);
    }

    private void ApplyProgress(float progress)
    {
        if (visualRoot == null)
            return;

        progress = Mathf.Clamp01(progress);

        visualRoot.localPosition =
            completedLocalPosition +
            Vector3.up * ((progress - 1f) * riseHeight);
    }
}