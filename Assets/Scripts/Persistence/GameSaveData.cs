using System;
using System.Collections.Generic;

[Serializable]
public class GameSaveData
{
    public string resourcesJson;
    public string simulationJson;
    public string dayNightJson;
    public List<BuildingSaveData> buildings = new List<BuildingSaveData>();
}

[Serializable]
public class BuildingSaveData
{
    public BuildingType type;
    public int gridX;
    public int gridY;
    public float constructionTimer;
    public float productionTimer;
    public bool isUnderConstruction;
}