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

        for(int i=0; i < missingTilesCount; i++)
        {
            // Receive a random Tile
            int index = Random.Range(0, gridSize * gridSize);
            HexTile tile = tiles[index / gridSize, index % gridSize].hexTile;

            tile.gameObject.SetActive(false);
        }
    }

    // Assigns a random Base Tile to each Player
    public IEnumerator AssignBaseTiles()
    {
        // Wait 2 Frames for Randomization
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

            Debug.Log("Active: " + baseTile.gameObject.activeSelf);

            // Add new BaseTile to List
            baseTiles.Add(baseTile);

            // Change Tile Team & Info
            baseTile.team = GameManager.current.activeTeam;
            baseTile.isBaseTile = true;
            baseTile.units = 1;
            GameManager.current.AddTileToPlayer(baseTile);

            // Cycle Through Teams
            GameManager.current.NextTeam();
        }

        TestBaseTilesConnection(baseTiles);
    }
    
    // Test the connection of all BaseTiles
    public void TestBaseTilesConnection(List<HexTile> baseTiles)
    {
        bool connected = false;

        // For now only 2 possible Teams/BaseTiles
        HexTile baseOne = baseTiles[0];
        HexTile baseTwo = baseTiles[1];

        // Still to be tested
        List<HexTile> tilesToTest = new List<HexTile>();
        tilesToTest.Add(baseOne);

        List<HexTile> alreadyTested = new List<HexTile>();

        // While there are still tiles that haven't been tested
        while(tilesToTest.Count > 0)
        {
            // Pick first tile
            HexTile currentTile = tilesToTest[0];

            // Only Test if active
            if (currentTile.gameObject.activeSelf)
            {
                // Test if connection was found
                if (currentTile.IsNeighbourTo(baseTwo))
                {
                    connected = true;
                }
                else
                {
                    // If not add all the neighbours
                    List<HexTile> neighbours = new List<HexTile>();

                    foreach (HexTileDisplay hexDisplay in currentTile.hexDisplay.neighbourTiles)
                    {
                        neighbours.Add(hexDisplay.hexTile);
                    }

                    // Add all the neighbourtiles that haven't already been tested
                    foreach (HexTile neighbourTile in neighbours)
                    {
                        if (!alreadyTested.Contains(neighbourTile) && !tilesToTest.Contains(neighbourTile))
                        {
                            tilesToTest.Add(neighbourTile);
                        }
                    }
                }
            }

            // Remove Current and add to tested
            tilesToTest.Remove(currentTile);
            alreadyTested.Add(currentTile);
        }

        Debug.Log("All Base tiles are connected: " + connected);

        if (!connected)
        {
            StopAllCoroutines();
            GameManager.current.InitializeGame();
            Start();
            
        }
        else
        {
            Debug.Log("One: " + baseOne.gameObject.activeSelf + " Two: " + baseTwo.gameObject.activeSelf);
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
    public void FinishAttackForAllTiles(bool acceptChange)
    {
        foreach(HexTileDisplay tileDisplay in tiles)
        {
            HexTile tile = tileDisplay.hexTile;

            if (acceptChange)
            {
                if (tile.attacking)
                {
                    if (tile.units != tile.unitsAfterMovement)
                    {
                        if (tile != InputManager.current.selectedHexTile)
                        {
                            tile.moveLocked = true;
                        }
                        tile.units = tile.unitsAfterMovement;

                        if(tile.units <= 0)
                        {
                            tile.isBaseTile = false;
                        }

                        // Negative Value means that it has been conquered
                        if (tile.units < 0)
                        {
                            tile.team = GameManager.current.activeTeam;
                            tile.units *= -1;
                            GameManager.current.AddTileToPlayer(tile);
                        }
                    }
                    tile.attacking = false;
                }
            }
            else
            {
                tile.attacking = false;
            }
        }
    }
}
