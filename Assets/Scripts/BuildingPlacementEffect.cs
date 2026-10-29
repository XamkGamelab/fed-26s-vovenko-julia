using UnityEngine;

[RequireComponent(typeof(ProductionBuilding))]
public class BuildingPlacementEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem placementEffectPrefab;
    [SerializeField] private Vector3 effectOffset = new Vector3(0f, -0.45f, 0f);

    private ProductionBuilding building;

    private void Awake()
    {
        building = GetComponent<ProductionBuilding>();
    }

    private void OnEnable()
    {
        GameEventSystem.OnBuildingPlaced += HandleBuildingPlaced;
    }

    private void OnDisable()
    {
        GameEventSystem.OnBuildingPlaced -= HandleBuildingPlaced;
    }

    private void HandleBuildingPlaced(ProductionBuilding placedBuilding)
    {
        if (placedBuilding != building || placementEffectPrefab == null)
            return;

        ParticleSystem effect = Instantiate(
            placementEffectPrefab,
            transform.position + effectOffset,
            Quaternion.identity
        );

        effect.Play();

        ParticleSystem.MainModule main = effect.main;
        float cleanupTime = main.duration + main.startLifetime.constantMax;

        Destroy(effect.gameObject, cleanupTime);
    }
}