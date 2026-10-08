
using System.Collections.Generic;

public class PlayerResources
{
    private Dictionary<ResourceType, int> resources =
        new Dictionary<ResourceType, int>();

    public PlayerResources()
    {
        // Starting resources
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
        {
            return false;
        }

        resources[type] -= amount;

        GameEventSystem.ResourcesChanged(
            type,
            resources[type]
        );

        return true;
    }



    public void Add(ResourceType type, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        resources[type] = GetAmount(type) + amount;

        GameEventSystem.ResourcesChanged(
            type,
            resources[type]
        );
    }

}
