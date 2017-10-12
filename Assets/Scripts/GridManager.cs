using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class TileColors
{
    public Color normal;
    public Color selectedGold;
    public Color selectedBlue;
    public Color selectedRed;
}

public class GridManager : MonoBehaviour {

    // Singleton
    public static GridManager current;

    // HexTile Prefab
    public GameObject HexTile;
    // All the available Colors
    public TileColors tileColors;
    // Array of all Tiles [x,y]
    private HexTileDisplay[,] tiles;
    // Grid Size
    public int gridSize;

    public int missingTilesPercent;

    public List<HexTile> path;


	// Use this for initialization
	void Start () {
        current = this;
        CreateGrid();
        StartCoroutine(RandomizeMap());
        StartCoroutine(AssignBaseTiles());
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    // Creates a new Grid
    public void CreateGrid()
    {
        // Sync with Settings
        gridSize = Settings.GridSize;
        missingTilesPercent = Settings.MissingPercent;

        // Check if Settings were NULL/Zero (Mainly so that scene starts)
        if (gridSize < 2)
        {
            Settings.ShowAIMovement = true;
        }
        gridSize = (gridSize < 2) ? 5 : gridSize;
        missingTilesPercent = (missingTilesPercent < 0) ? 30 : missingTilesPercent;

        if (tiles != null)
        {
            // Deleting the old Grid
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                for (int x = 0; x < tiles.GetLength(0); x++)
                {
                    // Destroying the Gameobject
                    DestroyImmediate(tiles[x, y].gameObject);
                }
            }
        }

        // Create a new Array with new Dimensions
        tiles = new HexTileDisplay[gridSize, gridSize];

        // Creating the new Grid
        for(int y=0; y<gridSize; y++)
        {
            for(int x=0; x<gridSize; x++)
            {
                // Calculating the Position
                float hexHeight = 3 * 0.5f / Mathf.Sqrt(3);
                Vector3 yOffset = Vector3.forward * hexHeight;
                Vector3 offset = Vector3.right * (y % 2) / 2 ;
                Vector3 position = transform.position + Vector3.right * x + (yOffset) * y + offset;
                // Creating a new Instance
                GameObject newInstance = Instantiate(HexTile, position, Quaternion.identity, transform);
                // Renaming
                newInstance.name = "HexTile " + x + "/" + y;
                // Reference to HexTile Script
                HexTileDisplay newHexTile = newInstance.GetComponent<HexTileDisplay>();
                // Putting it into the Array
                tiles[x, y] = newHexTile;
            }
        }

        // Adding Neighbours
        for(int y=0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                AddNeighboursToTile(tiles[x,y], x, y);
            }
        }
    }

    // Adds all the neighbour Tiles to a new Tile
    public void AddNeighboursToTile(HexTileDisplay tile, int x, int y)
    {
        if (y % 2 == 0)
        {
            if(x-1 >= 0)
                tile.neighbourTiles.Add(tiles[x - 1, y]);
            if(x+1 < gridSize)
                tile.neighbourTiles.Add(tiles[x+1,y]);
            if(x-1 >= 0 && y+1 < gridSize)
                tile.neighbourTiles.Add(tiles[x-1, y+1]);
            if(x-1 >= 0 && y-1 >= 0)
                tile.neighbourTiles.Add(tiles[x-1, y-1]);
            if(y+1 < gridSize)
                tile.neighbourTiles.Add(tiles[x, y+1]);
            if(y-1 >= 0)
                tile.neighbourTiles.Add(tiles[x, y-1]);
        }
        else
        {
            if(x-1 >= 0)
                tile.neighbourTiles.Add(tiles[x-1,y]);
            if(x+1 < gridSize)
                tile.neighbourTiles.Add(tiles[x+1,y]);
            if(x+1 < gridSize && y+1 < gridSize)
                tile.neighbourTiles.Add(tiles[x+1,y+1]);
            if(x+1 < gridSize && y-1 >= 0)
                tile.neighbourTiles.Add(tiles[x+1,y-1]);
            if(y+1 < gridSize)
                tile.neighbourTiles.Add(tiles[x,y+1]);
            if(y-1 >= 0)
                tile.neighbourTiles.Add(tiles[x,y-1]);
        }
    }

    // Returns Actual Size of the current Grid
    public int GetActualGridSize()
    {
        if (tiles != null)
        {
            return tiles.GetLength(0);
        }
        else
        {
            return 0;
        }
    }

    // Change the GridSize
    public void ChangeGridSize(float value) { gridSize = (int) value; CreateGrid(); }

    // Deletes/Deactivates random Tiles of the Map
    public IEnumerator RandomizeMap()
    {
        yield return null;

        int missingTilesCount = (int)(gridSize * gridSize * (missingTilesPercent / 100f));

        missingTilesCount = Mathf.Min(missingTilesCount, gridSize * gridSize - 2);

        for(int i=0; i < missingTilesCount; i++)
        {
            // Receive a random Tile
            int index = Random.Range(0, gridSize * gridSize);
            HexTile tile = tiles[index / gridSize, index % gridSize].hexTile;

            if (tile.gameObject.activeSelf)
            {
                tile.gameObject.SetActive(false);
            }
            else
            {
                i--;
            }
        }
    }

    // Assigns a random Base Tile to each Player
    public IEnumerator AssignBaseTiles()
    {
        // Wait 3 Frames for Randomization
        yield return null;
        yield return null;
        yield return null;

        // All BaseTiles
        List<HexTile> baseTiles = new List<HexTile>();

        // Go Through all players
        foreach (Player player in GameManager.current.players)
        {
            bool foundActive = false;
            bool foundFree = false;
            HexTile baseTile = null;

            while (!foundActive || !foundFree)
            {
                // Receive a random Tile
                int index = Random.Range(0, gridSize * gridSize);
                baseTile = tiles[index / gridSize, index % gridSize].hexTile;
                foundActive = baseTile.gameObject.activeSelf;
                foundFree = !baseTiles.Contains(baseTile);
            }

            player.lastCameraPos = new Vector3(baseTile.gameObject.transform.position.x + 0.5f, 4, baseTile.transform.position.z + 0.5f);

            // Add new BaseTile to List
            baseTiles.Add(baseTile);

            // Change Tile Team & Info
            baseTile.isBaseTile = true;
            baseTile.units = 1;
            GameManager.current.AddTileToPlayer(baseTile, GameManager.current.activePlayer);

            // Cycle Through Teams
            GameManager.current.NextTeam();
        }

        CheckBaseTilesConnection(baseTiles);
    }
    
    // Test the connection of all BaseTiles
    public void CheckBaseTilesConnection(List<HexTile> baseTiles)
    {
        // For now only 2 possible Teams/BaseTiles
        HexTile baseOne = baseTiles[0];
        HexTile baseTwo = baseTiles[1];

        path = Pathfinding.CalculatePath(baseOne, baseTwo);

        if (path.Count-2 < gridSize/2)
        {
            StopAllCoroutines();
            GameManager.current.InitializeGame();
            Start();
            
        }
    }

    // Returns the HexTile in the Middle of the Grid
    public HexTileDisplay GetMidTile()
    {
        return tiles[gridSize / 2, gridSize / 2];
    }

    // Destroy all HexTileDisplayTexts
    public void DisableHTDT()
    {
        foreach(HexTileDisplay htd in tiles)
        {
            htd.DisableText();
        }
    }

    // Accepts the change in Units for all tiles
    public void FinishAttackForAllTiles(List<Movement> movements)
    {
        foreach(HexTileDisplay tileDisplay in tiles)
        {
            HexTile tile = tileDisplay.hexTile;

            tile.attacking = false;
            tile.hexDisplay.highlighted = false;
        }

        foreach(Movement mov in movements)
        {
            GameManager.current.activePlayer.movements.Add(mov);
            mov.Move();
            if (mov.path.Count <= 1 || mov.path[0].team != GameManager.current.activeTeam)
            {
                GameManager.current.activePlayer.movements.Remove(mov);
            }
        }

        InputManager.current.movements = new List<Movement>();
    }

    // Resets the Move Lock value for all tiles
    public void FinishAllTiles()
    {
        foreach(HexTileDisplay tile in tiles)
        {
            tile.hexTile.moveLocked = false;
            tile.SetBlank();
        }
    }
}
