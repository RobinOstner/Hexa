using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour {

    // The attached Player Component
    [HideInInspector]
    public Player playerComponent;

    // All Possible Strategies
    public enum Strategies { Null, Conquer, Defend }

    // Currently played Strategy
    public Strategies currentStrategy;

    // All Bordering Tiles
    public List<HexTile> borderTiles;
    public List<HexTile> hostileBorderTiles;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        if(GameManager.current.activePlayer == playerComponent)
        {
            foreach(HexTile tile in borderTiles)
            {
                tile.hexDisplay.highlighted = true;
            }
        }
	}

    // Starts the AIs Turn
    public void StartTurn()
    {
        FindBorder();

        SelectStrategy();

        ExecuteStrategy();

        // Skip if movement is hidden
        if (!Settings.ShowAIMovement)
        {
            GameManager.current.NextTeam();
        }
    }
    
    // Checks the Border of all Tiles
    public void FindBorder()
    {
        borderTiles = new List<HexTile>();
        hostileBorderTiles = new List<HexTile>();

        foreach(HexTile tile in playerComponent.tiles)
        {
            foreach(HexTile neighbour in tile.neighbourTiles)
            {
                if(neighbour.gameObject.activeSelf && neighbour.team != playerComponent.team)
                {
                    borderTiles.Add(neighbour);

                    if(neighbour.team != GameManager.Teams.Null)
                    {
                        hostileBorderTiles.Add(neighbour);
                    }
                }
            }
        }
    }

    // Selects a Strategy based on the current Situation
    private void SelectStrategy()
    {
        // All Metrics
        bool foundHostile = false;

        // EVALUATION
        // Check if any borderTiles are hostile
        if(hostileBorderTiles.Count > 0) { foundHostile = true; }
        
        // Actually Select a Strategy
        if (foundHostile)
        {
            currentStrategy = Strategies.Defend;
        }
        else
        {
            currentStrategy = Strategies.Conquer;
        }
    }

    private void ExecuteStrategy()
    {
        switch (currentStrategy)
        {
            case Strategies.Conquer:
                Conquer();
                break;
            case Strategies.Defend:
                Defend();
                break;
            default:
                Conquer();
                break;
        }
    }

    // Tries to Conquer as many Tiles as possible
    private void Conquer()
    {
        HexTile baseTile = playerComponent.tiles[0];

        // Remove all already Targeted Tiles
        foreach(Movement mov in playerComponent.movements)
        {
            borderTiles.Remove(mov.path[mov.path.Count - 1]);
        }


        Debug.Log(Settings.AIDifficulty);
        // Diffulty Adjustments
        int removeTileCount = (int)((borderTiles.Count / 100f) * Settings.AIDifficulty);
        for(int i=0; i<removeTileCount; i++)
        {
            borderTiles.RemoveAt(Random.Range(0, borderTiles.Count));
            Debug.Log("REMOVE");
        }


        if (borderTiles.Count > 0)
        {
            int factor = (int)((baseTile.units*Settings.AIDifficulty/100f - playerComponent.tiles.Count) / borderTiles.Count);

            while (factor > 0)
            {
                foreach (HexTile target in borderTiles)
                {
                    List<HexTile> path = Pathfinding.CalculatePath(baseTile, target);
                    if (path.Count == 0) { continue; }

                    Movement mov = new Movement(1, path, playerComponent.team);
                    playerComponent.movements.Add(mov);
                    mov.Move();
                }

                factor--;
            }

            while (baseTile.units > playerComponent.tiles.Count)
            {
                int randomID = Random.Range(0, borderTiles.Count);
                HexTile target = borderTiles[randomID];

                List<HexTile> path = Pathfinding.CalculatePath(baseTile, target);
                if (path.Count == 0) { continue; }

                Movement mov = new Movement(1, path, playerComponent.team);
                playerComponent.movements.Add(mov);
                mov.Move();
            }
        }
    }

    // Tries to send as many possible units to all hostile Tiles
    private void Defend()
    {
        HexTile baseTile = playerComponent.tiles[0];
        int factor = (baseTile.units - playerComponent.tiles.Count) / hostileBorderTiles.Count;

        while (factor > 0)
        {
            foreach (HexTile target in hostileBorderTiles)
            {
                List<HexTile> path = Pathfinding.CalculatePath(baseTile, target);
                if (path.Count == 0) { continue; }

                Movement mov = new Movement(1, path, playerComponent.team);
                playerComponent.movements.Add(mov);
                mov.Move();
            }

            factor--;
        }

    }
}
