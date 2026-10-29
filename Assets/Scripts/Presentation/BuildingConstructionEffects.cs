using UnityEngine;

[RequireComponent(typeof(ProductionBuilding))]
public class BuildingConstructionEffects : MonoBehaviour
{
    [SerializeField] private ParticleSystem constructionEffectPrefab;
    [SerializeField] private ParticleSystem completionEffectPrefab;
    [SerializeField] private Vector3 effectOffset = new Vector3(0f, -0.45f, 0f);

    private ProductionBuilding building;
    private ParticleSystem activeConstructionEffect;
    private bool isGameplay = true;

    private void Awake()
    {
        building = GetComponent<ProductionBuilding>();
    }

    private void OnEnable()
    {
        GameEventSystem.OnBuildingPlaced += HandleBuildingPlaced;
        GameEventSystem.OnConstructionProgressChanged += HandleProgressChanged;
        GameEventSystem.OnBuildingConstructionCompleted += HandleConstructionCompleted;
        GameEventSystem.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameEventSystem.OnBuildingPlaced -= HandleBuildingPlaced;
        GameEventSystem.OnConstructionProgressChanged -= HandleProgressChanged;
        GameEventSystem.OnBuildingConstructionCompleted -= HandleConstructionCompleted;
        GameEventSystem.OnGameStateChanged -= HandleGameStateChanged;

        StopConstructionEffect();
    }

    private void HandleBuildingPlaced(ProductionBuilding placedBuilding)
    {
        if (placedBuilding == building && isGameplay)
            StartConstructionEffect();
    }

    private void HandleProgressChanged(
        ProductionBuilding changedBuilding,
        float progress)
    {
        if (changedBuilding != building)
            return;

        if (progress < 1f && isGameplay)
            StartConstructionEffect();
        else
            StopConstructionEffect();
    }

    private void HandleConstructionCompleted(ProductionBuilding completedBuilding)
    {
        if (completedBuilding != building)
            return;

        StopConstructionEffect();
        PlayOneShot(completionEffectPrefab);
    }

    private void HandleGameStateChanged(GameState state)
    {
        isGameplay = state == GameState.Gameplay;

        if (isGameplay && building.IsUnderConstruction)
            StartConstructionEffect();
        else
            StopConstructionEffect();
    }

    private void StartConstructionEffect()
    {
        if (constructionEffectPrefab == null || activeConstructionEffect != null)
            return;

        activeConstructionEffect = Instantiate(
            constructionEffectPrefab,
            transform.position + effectOffset,
            Quaternion.identity,
            transform
        );

        activeConstructionEffect.Play();
    }

    private void StopConstructionEffect()
    {
        if (activeConstructionEffect == null)
            return;

        activeConstructionEffect.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );

        Destroy(activeConstructionEffect.gameObject);
        activeConstructionEffect = null;
    }

    private void PlayOneShot(ParticleSystem effectPrefab)
    {
        if (effectPrefab == null)
            return;

        ParticleSystem effect = Instantiate(
            effectPrefab,
            transform.position + effectOffset,
            Quaternion.identity
        );

        effect.Play();

        ParticleSystem.MainModule main = effect.main;
        float cleanupTime = main.duration + main.startLifetime.constantMax;

        Destroy(effect.gameObject, cleanupTime);
    }
}