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
    }

    // Finds all Neighbouring Tiles
    void FindNeighbourTiles()
    {
        neighbourTiles = new List<HexTile>();

        foreach (HexTile ownTile in playerComponent.tiles)
        {
            foreach (HexTile neighbour in ownTile.neighbourTiles)
            {
                neighbourTiles.Add(neighbour);
            }
        }

        foreach (HexTile ownTile in playerComponent.tiles)
        {
            neighbourTiles.Remove(ownTile);
        }
    }
}
