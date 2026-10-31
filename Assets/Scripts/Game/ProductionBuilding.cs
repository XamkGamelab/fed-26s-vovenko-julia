using UnityEngine;

public class ProductionBuilding : Building
{
    private float productionTimer;
    private float constructionTimer;

    public bool IsUnderConstruction { get; private set; }
    public float ConstructionProgress { get; private set; }

    public void BeginConstruction()
    {
        constructionTimer = 0f;
        ConstructionProgress = 0f;
        IsUnderConstruction = true;
    }

    public void Simulate(float deltaTime, PlayerResources playerResources)
    {
        if (Data == null || playerResources == null)
            return;

        if (IsUnderConstruction)
        {
            float buildTime = Mathf.Max(Data.BuildTime, 0.01f);
            constructionTimer += deltaTime;
            ConstructionProgress = Mathf.Clamp01(constructionTimer / buildTime);

            GameEventSystem.ConstructionProgressChanged(
                this,
                ConstructionProgress
            );

            if (ConstructionProgress < 1f)
                return;

            IsUnderConstruction = false;
            GameEventSystem.BuildingConstructionCompleted(this);
            return;
        }

        float interval = Data.ProductionInterval;

        if (interval <= 0f)
            return;

        productionTimer += deltaTime;

        while (productionTimer >= interval)
        {
            productionTimer -= interval;

            playerResources.Add(
                Data.ProducedResource,
                Data.ProductionAmount
            );

            GameEventSystem.ProductionCompleted(this);

            Debug.Log(
                $"{Data.Type} produced {Data.ProductionAmount} " +
                $"{Data.ProducedResource}"
            );
        }
    }
}