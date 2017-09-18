using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour {

    // The attached Player Component
    public Player playerComponent;

    // All Tiles bordering the AIs Tiles
    public List<HexTile> neighbourTiles;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

	}

    // Starts the AIs Turn
    public void StartTurn()
    {
        FindNeighbourTiles();
        ConquerTiles();

        GameManager.current.NextTeam();
    }

    // Try To Conquer as many Tiles as possible
    public void ConquerTiles()
    {
        List<HexTile> playerTiles = new List<HexTile>();
        foreach (HexTile tile in playerComponent.tiles)
        {
            playerTiles.Add(tile);
        }
        foreach (HexTile ownTile in playerTiles)
        {

            foreach (HexTile neighbour in neighbourTiles)
            {

                // Stop if Tile doesn't have units
                if (ownTile.units == 0)
                {
                    break;
                }

                if (ownTile.isBaseTile && ownTile.units == 1)
                {
                    break;
                }

                if (neighbour.IsNeighbourTo(ownTile))
                {
                    if (neighbour.team == GameManager.Teams.Null)
                    {
                        neighbour.units++;
                        Debug.Log("Move to free tile " + neighbour + ": " + neighbour.units);
                        neighbour.team = playerComponent.team;
                        GameManager.current.AddTileToPlayer(neighbour);

                        ownTile.units--;

                        Debug.Log("Count: " + playerComponent.tiles.Count);
                    }
                    else
                    {
                        if (neighbour.team != playerComponent.team)
                        {
                            neighbour.units--;
                            ownTile.units--;
                        }
                    }
                }
            }
        }
    }

    // Finds all Neighbouring Tiles
    void FindNeighbourTiles()
    {
        neighbourTiles = new List<HexTile>();

        foreach (HexTile ownTile in playerComponent.tiles)
        {
            foreach (HexTile neighbour in ownTile.neighbourTiles)
            {
                if (neighbour.gameObject.activeSelf && !neighbourTiles.Contains(neighbour))
                {
                    neighbourTiles.Add(neighbour);
                }
            }
        }

        foreach (HexTile ownTile in playerComponent.tiles)
        {
            neighbourTiles.Remove(ownTile);
        }
    }
}
