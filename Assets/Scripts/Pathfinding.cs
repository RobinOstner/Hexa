using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour {

    // Start & End Tile of Path
    public HexTile startTile, endTile;
    // Distance between Tiles
    public int distance;

    public List<HexTile> path;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        CalculatePath(startTile, endTile);

        /*
        if(path.Count > 0)
        {
            foreach(HexTile tile in path)
            {
                tile.hexDisplay.highlighted = true;
            }
        }
        */
	}

    // Calculates the shortest Path
    public static List<HexTile> CalculatePath(HexTile start, HexTile end)
    {
        if (start != null && end != null)
        {
            // Setup
            List<HexTile> search = new List<HexTile>();
            List<HexTile> searched = new List<HexTile>();
            search.Add(start);

            bool found = false;

            while (!found)
            {

                // Loop through all Tiles that have to be searched
                for (int i = search.Count-1; i >= 0; i--)
                {
                    HexTile tile = search[i];

                    // Search all neighbours
                    foreach (HexTile neighbour in tile.neighbourTiles)
                    {
                        if (!search.Contains(neighbour) && !searched.Contains(neighbour) && neighbour.gameObject.activeSelf)
                        {
                            // Add to searched
                            search.Add(neighbour);

                            // Set Discovery Record
                            neighbour.discoveredBy = tile;

                            // Check if found
                            if (neighbour == end)
                            {
                                found = true;
                                break;
                            }
                        }
                    }

                    // Check if found
                    if (found)
                    {
                        break;
                    }

                    // Remove searched Object
                    searched.Add(tile);
                    search.RemoveAt(i);

                }
            }

            /*
            foreach(HexTile tile in path)
            {
                tile.hexDisplay.highlighted = false;
            }
            */

            return BackTrace(start, end);
        }

        return new List<HexTile>();
    }

    // Backtraces the path
    private static List<HexTile> BackTrace(HexTile start, HexTile end)
    {
        List<HexTile> path = new List<HexTile>();

        HexTile current = end;

        path.Add(current);

        while(current != start)
        {
            path.Insert(0, current.discoveredBy);
            current = current.discoveredBy;
        }

        //this.path = path;

        return path;
    }
}
