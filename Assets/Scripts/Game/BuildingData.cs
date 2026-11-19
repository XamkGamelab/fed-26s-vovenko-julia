using UnityEngine;

public enum BuildingType
{
    Woodcutter,
    Quarry,
    Smelter
}

public enum ResourceType
{
    Wood,
    Stone,
    Metal
}

[CreateAssetMenu(
    fileName = "NewBuildingData",
    menuName = "Game/Building Data"
)]
public class BuildingData : ScriptableObject
{
    [Header("Building")]
    public BuildingType Type;
    public Vector2Int Size = Vector2Int.one;
    public float BuildTime = 1f;

    [Header("Construction Cost")]
    public ResourceType CostType;
    public int Cost = 10;
    public ResourceType AdditionalCostType;
    [Min(0)] public int AdditionalCost;

    [Header("Production")]
    public ResourceType ProducedResource;
    public int ProductionAmount = 1;
    public float ProductionInterval = 2f;

    [Header("Production Inputs")]
    public ResourceType InputResource;
    [Min(0)] public int InputAmount;

    public ResourceType SecondInputResource;
    [Min(0)] public int SecondInputAmount;

    [Header("Ambient Audio")]
    public bool UseAmbientAudio;
    public AmbientAudioProfile AmbientProfile;

    [Header("Prefab")]
    public GameObject Prefab;
}