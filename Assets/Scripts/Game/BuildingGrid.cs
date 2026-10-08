
using System.Collections.Generic;
using UnityEngine;

public class BuildingGrid
{
    private Dictionary<Vector2Int, Building> occupiedCells =
        new Dictionary<Vector2Int, Building>();

    public bool IsOccupied(Vector2Int position, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int cell = position + new Vector2Int(x, y);

                if (occupiedCells.ContainsKey(cell))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void SetCellOccupiedState(
        Vector2Int position,
        Vector2Int size,
        Building building)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int cell = position + new Vector2Int(x, y);
                occupiedCells[cell] = building;
            }
        }
    }
}
