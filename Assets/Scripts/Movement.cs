using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Movement {

    public GameManager.Teams team;

    // Amount of Units in this Movement
    public int units;

    // Remaining path
    public List<HexTile> path;

    public Movement(int units, List<HexTile> newPath, GameManager.Teams team)
    {
        this.team = team;
        this.units = units;
        path = newPath;
        newPath[0].movementsFromTile.Add(this);
    }

    // Moves all the units in this Movement
    public void Move()
    {
        HexTile current = path[0];

        if (current.team == team)
        {

            current.movementsFromTile.Remove(this);

            current.MoveUnitsToTile(path[1], units);

            path.RemoveAt(0);

            path[0].movementsFromTile.Add(this);

            if(path.Count == 1)
            {
                path[0].movementsFromTile.Remove(this);
            }
        }
        else
        {
            path = new List<HexTile>();
        }
    }

    public void HighlightPath()
    {
        if (path.Count > 0)
        {
            if (path[0].hexDisplay.selected)
            {
                for (int i = 1; i < path.Count; i++)
                {
                    HexTile tile = path[i];
                    tile.hexDisplay.highlighted = true;
                    tile.unitsAfterMovement += units;
                }
            }
            else
            {
                HexTile tile = path[path.Count - 1];
                tile.hexDisplay.highlighted = true;
                tile.unitsAfterMovement += units;
            }
        }
    }
}
