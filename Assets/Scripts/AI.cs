using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour {

    // The attached Player Component
    public Player playerComponent;

    // Possible AI-Modes
    // Conquering: Try to get the the most tiles
    private enum Modes { Idle, Conquering }
    // Current Mode: What Strategy is currently being played
    private Modes mode;

    // All Tiles bordering the AIs Tiles
    public List<HexTile> neighbourTiles;
    // Is any of the neighbouring tiles an enemy tile?
    public bool enemySpotted;

    // Priority List For Conquering
    public List<HexTile> priorityConquer;

	// Use this for initialization
	void Start () {
        priorityConquer = new List<HexTile>();
	}
	
	// Update is called once per frame
	void Update () {

	}

    // Starts the AIs Turn
    public void StartTurn()
    {
        FindNeighbourTiles();
        SelectMode();
        StartStrategy();

        // Skip if movement is hidden
        if (!Settings.ShowAIMovement)
        {
            GameManager.current.NextTeam();
        }
    }

    // Selects the Mode/Strategy for this round
    private void SelectMode()
    {
        mode = Modes.Idle;

        if (!enemySpotted)
        {
            mode = Modes.Conquering;
        }
    }

    // Starts the Strategy for this round
    private void StartStrategy()
    {
        switch (mode)
        {
            case Modes.Conquering:
                ConquerTiles();
                break;
            default:
                ConquerTiles();
                break;
        }
    }

    // Try To Conquer as many empty Tiles as possible
    private void ConquerTiles()
    {
        // All the feeders
        List<HexTile> feeders = new List<HexTile>();

        // Try to feed all prios first
        for(int i=0; i<priorityConquer.Count; i++)
        {
            HexTile prioTile = priorityConquer[i];
            HexTile feederTile = TryFeed(prioTile, 1);

            // Remove from PrioQueue if fed
            if(feederTile != null)
            {
                priorityConquer.RemoveAt(i);
                i--;
                neighbourTiles.Add(feederTile);
            }
        }

        // Counts all feeders
        int feederCount = 0;

        // Counts the maximum amount of fed tiles
        int maxPossible = 0;
        
        while(neighbourTiles.Count > 0)
        {
            // Select Random Tile
            int random = Random.Range(0, neighbourTiles.Count);

            HexTile neighbour = neighbourTiles[random];

            // Try To Feed the tile
            HexTile feederTile = TryFeed(neighbour, 1);

            if (feederTile != null)
            {
                feeders.Add(feederTile);
                feederCount++;
            }

            maxPossible++;

            // Remove Tile
            neighbourTiles.RemoveAt(random);
        }

        // Try to Refill all feeders
        while (feeders.Count > 0)
        {
            for (int i = 0; i < feeders.Count; i++)
            {
                // Only refeed if no more units
                if (feeders[0].units == 0 && !feeders[0].isBaseTile)
                {
                    // Try to feed
                    HexTile feederTile = TryFeed(feeders[0], 1);

                    // Found Feeder
                    if (feederTile != null)
                    {
                        feeders.Add(feederTile);
                        feederCount++;
                    }
                    else
                    {
                        if (!priorityConquer.Contains(feeders[0]))
                        {
                            priorityConquer.Add(feeders[0]);
                        }
                    }

                    maxPossible++;
                }

                // Remove original feeder because it has either been filled or it cannot be filled or it doesn't have to be filled
                feeders.RemoveAt(0);
            }
        }
    }

    // Try To Feed The Given Tile
    private HexTile TryFeed(HexTile tile, int amount)
    {
        // Feeder Tile is the one that provides the unit for this tile
        HexTile feederTile = null;
        bool foundFeeder = false;

        // Copy neighbour list
        List<HexTile> neighbours = new List<HexTile>();
        foreach(HexTile neighbour in tile.neighbourTiles)
        {
            neighbours.Add(neighbour);
        }

        foreach (HexTile neighbour in neighbours)
        {
            if (neighbour.team == playerComponent.team && !neighbour.moveLocked && UnitsToSpare(neighbour) >= amount)
            {
                if(feederTile == null)
                {
                    // Feeder found
                    feederTile = neighbour;
                    foundFeeder = true;
                    break;
                }
                if (feederTile != null && UnitsToSpare(feederTile) < UnitsToSpare(neighbour))
                {
                    // Feeder found
                    feederTile = neighbour;
                    foundFeeder = true;
                }
            }
        }

        if (foundFeeder)
        {
            // Move 1 Unit from feeder to neighbour
            feederTile.MoveUnitsToTile(tile, amount);

            // Return The Feeder Tile
            return feederTile;
        }

        return null;
    }

    // Calculates the amount of units this particular tile can/should give
    public int UnitsToSpare(HexTile tile)
    {
        // Base Tiles should keep more
        if (tile.isBaseTile)
        {
            return tile.units - playerComponent.tiles.Count;
        }
        else
        {
            // Depends on Mode
            return tile.units;
        }
    }

    // Try To Conquer as many Tiles as possible
    public IEnumerator ConquerTilesOLD()
    {
        List<HexTile> playerTiles = new List<HexTile>();
        foreach (HexTile tile in playerComponent.tiles)
        {
            playerTiles.Add(tile);
        }

        bool movementPossible = true;
        while (playerComponent.tiles[0].units > playerComponent.tiles.Count && movementPossible)
        {
            yield return null;

            movementPossible = false;
            
            foreach (HexTile ownTile in playerTiles)
            {

                foreach (HexTile neighbour in neighbourTiles)
                {

                    // Stop if Tile doesn't have units
                    if (ownTile.units == 0 || ownTile.moveLocked)
                    {
                        break;
                    }

                    movementPossible = true;

                    if (ownTile.isBaseTile && ownTile.units <= playerComponent.tiles.Count)
                    {
                        break;
                    }

                    if (neighbour.IsNeighbourTo(ownTile))
                    {
                        if (neighbour.team == GameManager.Teams.Null)
                        {
                            neighbour.units++;
                            neighbour.moveLocked = true;
                            neighbour.team = playerComponent.team;
                            GameManager.current.AddTileToPlayer(neighbour, playerComponent);

                            ownTile.units--;
                        }
                        else
                        {
                            if (neighbour.team != playerComponent.team)
                            {
                                neighbour.units--;
                                ownTile.units--;
                            }
                            else
                            {
                                neighbour.units++;
                                ownTile.units--;
                            }
                        }
                    }
                }
            }
        }
    }

    // Finds all Neighbouring Tiles
    void FindNeighbourTiles()
    {
        // Reset
        enemySpotted = false;

        // Reset List
        neighbourTiles = new List<HexTile>();

        // Loop through all ownTiles
        foreach (HexTile ownTile in playerComponent.tiles)
        {
            // Loop through all the neighbouring tiles
            foreach (HexTile neighbour in ownTile.neighbourTiles)
            {
                // Check if active & not already in List/no multiple entries
                if (neighbour.gameObject.activeSelf && !neighbourTiles.Contains(neighbour) && !priorityConquer.Contains(neighbour))
                {
                    neighbourTiles.Add(neighbour);
                }

                // Check for enemies
                if(neighbour.team != playerComponent.team && neighbour.team != GameManager.Teams.Null)
                {
                    enemySpotted = true;
                }
            }
        }

        // Remove own Tiles
        foreach (HexTile ownTile in playerComponent.tiles)
        {
            neighbourTiles.Remove(ownTile);
        }
    }
}
