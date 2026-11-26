using System;
using UnityEngine;

[Serializable]
public class ProductionBuildingSaveData
{
    public BuildingType type;
    public int gridX;
    public int gridY;
    public float constructionTimer;
    public float productionTimer;
    public bool isUnderConstruction;
    public int upgradeLevel;
}

public class ProductionBuilding : Building, IJsonSaveable
{
    [Header("Upgrade Indicator")]
    [SerializeField] private GameObject upgradeIndicator;

    private float productionTimer;
    private float constructionTimer;
    private Vector2Int gridPosition;
    private int upgradeLevel;

    public int UpgradeLevel => upgradeLevel;
    public bool IsUnderConstruction { get; private set; }
    public float ConstructionProgress { get; private set; }

    public void SetGridPosition(Vector2Int position)
    {
        gridPosition = position;
    }

    public void BeginConstruction()
    {
        constructionTimer = 0f;
        ConstructionProgress = 0f;
        IsUnderConstruction = true;

        if (upgradeIndicator != null)
            upgradeIndicator.SetActive(false);
    }

    public void Simulate(
        float deltaTime,
        PlayerResources playerResources,
        bool isDay)
    {
        if (Data == null || playerResources == null)
            return;

        if (upgradeIndicator != null)
            upgradeIndicator.SetActive(CanUpgrade(playerResources));

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

        if (!isDay)
            return;

        float interval = GetProductionInterval();

        if (interval <= 0f)
            return;

        productionTimer += deltaTime;

        while (productionTimer >= interval)
        {
            bool hasFirstInput =
                Data.InputAmount <= 0 ||
                playerResources.HasEnough(
                    Data.InputResource,
                    Data.InputAmount
                );

            bool hasSecondInput =
                Data.SecondInputAmount <= 0 ||
                playerResources.HasEnough(
                    Data.SecondInputResource,
                    Data.SecondInputAmount
                );

            if (!hasFirstInput || !hasSecondInput)
            {
                productionTimer = interval;
                return;
            }

            if (Data.InputAmount > 0)
            {
                playerResources.TrySpend(
                    Data.InputResource,
                    Data.InputAmount
                );
            }

            if (Data.SecondInputAmount > 0)
            {
                playerResources.TrySpend(
                    Data.SecondInputResource,
                    Data.SecondInputAmount
                );
            }

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

    public int GetNextUpgradeCost()
    {
        if (Data == null || upgradeLevel >= Data.MaxUpgradeLevel)
            return 0;

        if (upgradeLevel == 0)
            return Data.FirstUpgradeMetalCost;

        if (upgradeLevel == 1)
            return Data.SecondUpgradeMetalCost;

        return 0;
    }

    public bool CanUpgrade(PlayerResources resources)
    {
        if (Data == null ||
            resources == null ||
            IsUnderConstruction ||
            upgradeLevel >= Data.MaxUpgradeLevel)
        {
            return false;
        }

        int cost = GetNextUpgradeCost();

        return cost > 0 &&
               resources.HasEnough(ResourceType.Metal, cost);
    }

    public bool TryUpgrade(PlayerResources playerResources)
    {
        if (!CanUpgrade(playerResources))
            return false;

        int cost = GetNextUpgradeCost();

        if (!playerResources.TrySpend(ResourceType.Metal, cost))
            return false;

        upgradeLevel++;
        return true;
    }

    private float GetProductionInterval()
    {
        if (Data == null)
            return 0f;

        return Data.ProductionInterval *
               Mathf.Pow(
                   Data.ProductionIntervalMultiplierPerLevel,
                   upgradeLevel
               );
    }

    public string CaptureState()
    {
        ProductionBuildingSaveData saveData =
            new ProductionBuildingSaveData
            {
                type = Data.Type,
                gridX = gridPosition.x,
                gridY = gridPosition.y,
                constructionTimer = constructionTimer,
                productionTimer = productionTimer,
                isUnderConstruction = IsUnderConstruction,
                upgradeLevel = upgradeLevel
            };

        return JsonUtility.ToJson(saveData);
    }

    public void RestoreState(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning("Building save data is empty.");
            return;
        }

        if (Data == null)
        {
            Debug.LogError("Initialize the building before restoring its state.");
            return;
        }

        ProductionBuildingSaveData saveData;

        try
        {
            saveData =
                JsonUtility.FromJson<ProductionBuildingSaveData>(json);
        }
        catch (Exception exception)
        {
            Debug.LogError($"Could not load building data: {exception.Message}");
            return;
        }

        if (saveData == null)
        {
            Debug.LogError("Building save data could not be read.");
            return;
        }

        upgradeLevel = Mathf.Clamp(
            saveData.upgradeLevel,
            0,
            Data.MaxUpgradeLevel
        );

        gridPosition = new Vector2Int(saveData.gridX, saveData.gridY);
        constructionTimer = Mathf.Max(0f, saveData.constructionTimer);
        productionTimer = Mathf.Max(0f, saveData.productionTimer);
        IsUnderConstruction = saveData.isUnderConstruction;

        if (IsUnderConstruction)
        {
            float buildTime = Mathf.Max(Data.BuildTime, 0.01f);
            ConstructionProgress =
                Mathf.Clamp01(constructionTimer / buildTime);
        }
        else
        {
            ConstructionProgress = 1f;
        }
    }
}