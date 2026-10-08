
using UnityEngine;

public class Building : MonoBehaviour
{
    [SerializeField] private BuildingData buildingData;

    public BuildingData Data => buildingData;

    public void Initialize(BuildingData data)
    {
        buildingData = data;
    }
}
