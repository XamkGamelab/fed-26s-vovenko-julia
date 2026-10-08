
using UnityEngine;

public class ProductionBuilding : Building
{
    private float productionTimer;

    public void Simulate(float deltaTime, PlayerResources playerResources)
    {
        if (Data == null || playerResources == null)
            return;

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

            Debug.Log(
                $"{Data.Type} produced {Data.ProductionAmount} " +
                $"{Data.ProducedResource}"
            );
        }
    }
}
