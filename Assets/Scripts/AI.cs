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

    public int difficulty;
    public int conquerDefend = 90;

    public bool showHostileCount;

	// Use this for initialization
	void Start () {
        //difficulty = Settings.current.AIDifficulty;
	}
	
	// Update is called once per frame
	void Update () {
        if(GameManager.current.activePlayer == playerComponent)
        {
            foreach(HexTile tile in borderTiles)
            {
                //tile.hexDisplay.highlighted = true;
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

        int difficultyAdj = 100 - difficulty;

        // Remove all already Targeted Tiles
        foreach(Movement mov in playerComponent.movements)
        {
            borderTiles.Remove(mov.path[mov.path.Count - 1]);
        }
        
        // Diffulty Adjustments
        int removeTileCount = (int)((borderTiles.Count / 100f) * difficultyAdj);
        for(int i=0; i<removeTileCount; i++)
        {
            borderTiles.RemoveAt(Random.Range(0, borderTiles.Count));
        }


        if (borderTiles.Count > 0)
        {
            int factor = (int)((baseTile.units*difficultyAdj/100f - playerComponent.tiles.Count) / borderTiles.Count);

            while (factor > 0)
            {
                foreach (HexTile target in borderTiles)
                {
                    List<HexTile> path = Pathfinding.CalculatePath(baseTile, target);
                    if (path.Count == 0) { continue; }

                    Movement mov = new Movement(1, path, playerComponent.team);
                    playerComponent.movements.Add(mov);
                    mov.Move(true);

                    if (mov.path.Count <= 1 || mov.path[0].team != playerComponent.team || mov.path[0].units == 0)
                    {
                        mov.path[0].movementsFromTile.Remove(mov);
                        playerComponent.movements.Remove(mov);
                    }
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
                mov.Move(true);

                if (mov.path.Count <= 1 || mov.path[0].team != playerComponent.team || mov.path[0].units == 0)
                {
                    mov.path[0].movementsFromTile.Remove(mov);
                    playerComponent.movements.Remove(mov);
                }
            }
        }
    }

    // Tries to send as many possible units to all hostile Tiles while still considering Conquering
    private void Defend()
    {
        HexTile baseTile = playerComponent.tiles[0];

        int difficultyInv = 100 - difficulty;

        float totalBorderCount = hostileBorderTiles.Count + borderTiles.Count;
        
        // The Ideal Amount to spend Units
        int unitsToSpare = (baseTile.units - playerComponent.tiles.Count);
        unitsToSpare = (int)(unitsToSpare * Mathf.Min(1, (difficulty + 10) / 100f));

        int unitsToHostile = (int) (unitsToSpare * (hostileBorderTiles.Count / totalBorderCount));

        if (borderTiles.Count != 0)
        {
            unitsToHostile = (int)(unitsToHostile * (Mathf.Min(1, difficultyInv + 10 / 100f)));
        }

        int unitsToBorder = unitsToSpare - unitsToHostile;

        while(unitsToBorder > 0 && borderTiles.Count > 0)
        {
            int random = Random.Range(0, borderTiles.Count);
            HexTile target = borderTiles[random];

            List<HexTile> path = Pathfinding.CalculatePath(baseTile, target);
            if (path.Count == 0) { continue; }

            Movement mov = new Movement(1, path, playerComponent.team);
            playerComponent.movements.Add(mov);
            mov.Move(true);

            if (mov.path.Count <= 1 || mov.path[0].team != playerComponent.team || mov.path[0].units == 0)
            {
                mov.path[0].movementsFromTile.Remove(mov);
                playerComponent.movements.Remove(mov);
            }

            borderTiles.RemoveAt(random);
            unitsToBorder--;
        }

        while (unitsToHostile > 0)
        {
            foreach (HexTile target in hostileBorderTiles)
            {
                if (Random.Range(0, 100) <= difficulty)
                {

                    List<HexTile> path = Pathfinding.CalculatePath(baseTile, target);
                    if (path.Count == 0) { continue; }

                    Movement mov = new Movement(1, path, playerComponent.team);
                    playerComponent.movements.Add(mov);
                    mov.Move(true);

                    if (mov.path.Count <= 1 || mov.path[0].team != playerComponent.team || mov.path[0].units == 0)
                    {
                        mov.path[0].movementsFromTile.Remove(mov);
                        playerComponent.movements.Remove(mov);
                    }
                }
            }

            unitsToHostile--;
        }

    }
}
