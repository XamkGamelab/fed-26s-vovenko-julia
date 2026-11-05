using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerResources : IJsonSaveable
{
    [Serializable]
    private class ResourceSaveData
    {
        public int wood;
        public int stone;
    }

    private Dictionary<ResourceType, int> resources =
        new Dictionary<ResourceType, int>();

    public PlayerResources()
    {
        resources[ResourceType.Wood] = 20;
        resources[ResourceType.Stone] = 0;
    }

    public int GetAmount(ResourceType type)
    {
        return resources.TryGetValue(type, out int amount)
            ? amount
            : 0;
    }

    public bool HasEnough(ResourceType type, int amount)
    {
        return amount >= 0 && GetAmount(type) >= amount;
    }

    public bool TrySpend(ResourceType type, int amount)
    {
        if (!HasEnough(type, amount))
            return false;

        resources[type] -= amount;
        GameEventSystem.ResourcesChanged(type, resources[type]);
        return true;
    }

    public void Add(ResourceType type, int amount)
    {
        if (amount <= 0)
            return;

        resources[type] = GetAmount(type) + amount;
        GameEventSystem.ResourcesChanged(type, resources[type]);
    }

    public string CaptureState()
    {
        ResourceSaveData data = new ResourceSaveData
        {
            wood = GetAmount(ResourceType.Wood),
            stone = GetAmount(ResourceType.Stone)
        };

        return JsonUtility.ToJson(data);
    }

    public void RestoreState(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning("Resource save data is empty.");
            return;
        }

        ResourceSaveData data;

        try
        {
            data = JsonUtility.FromJson<ResourceSaveData>(json);
        }
        catch (Exception exception)
        {
            Debug.LogError($"Could not load resource data: {exception.Message}");
            return;
        }

        if (data == null)
        {
            Debug.LogError("Resource save data could not be read.");
            return;
        }

        resources[ResourceType.Wood] = Mathf.Max(0, data.wood);
        resources[ResourceType.Stone] = Mathf.Max(0, data.stone);

        GameEventSystem.ResourcesChanged(
            ResourceType.Wood,
            resources[ResourceType.Wood]);

        GameEventSystem.ResourcesChanged(
            ResourceType.Stone,
            resources[ResourceType.Stone]);
    }
}