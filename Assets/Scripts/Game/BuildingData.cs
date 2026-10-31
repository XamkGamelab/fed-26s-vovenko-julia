using UnityEngine;

public enum BuildingType
{
    Woodcutter,
    Quarry
}

public enum ResourceType
{
    Wood,
    Stone
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

    [Header("Production")]
    public ResourceType ProducedResource;
    public int ProductionAmount = 1;
    public float ProductionInterval = 2f;

    [Header("Ambient Audio")]
    public bool UseAmbientAudio;
    public AmbientAudioProfile AmbientProfile;

    [Header("Prefab")]
    public GameObject Prefab;
}