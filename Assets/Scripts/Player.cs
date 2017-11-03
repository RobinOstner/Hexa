using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    // Identification (Player Number)
    public int ID;

    public Text tileCountText;

    // The Players Team
    public GameManager.Teams team;

    // The AI-Component attached to the gameobject
    [HideInInspector]
    public AI aiComponent;

    // Replay Component attached to gameobject
    public Replay replays;

    // Is this player AI controlled?
    public bool isAI
    {
        get { return aiComponent.enabled; }
        set { aiComponent.enabled = value; }
    }

    // List with all Tiles of this player
    public List<HexTile> tiles;
    
    // All Active Movements
    public List<Movement> movements;

    // The last position of the camera while the player was in control
    [HideInInspector]
    public Vector3 lastCameraPos;

	// Use this for initialization
	void Start ()
    {
        name = "Player " + ID + ((isAI) ? " (AI)": "");
        tiles = new List<HexTile>();

        replays = GetComponent<Replay>();

        SetTeamColor();
	}
	
	// Update is called once per frame
	void Update () {
        ShowAllPaths();
    }

    // Highlights All Paths
    void ShowAllPaths()
    {
        if (GameManager.current.activePlayer == this)
        {
            foreach (Movement mov in movements)
            {
                mov.HighlightPath();
            }
        }
    }

    // Change Own Color according to ID
    void SetTeamColor()
    {
        switch (ID)
        {
            case 1:
                team = GameManager.Teams.Gold;
                break;
            case 2:
                team = GameManager.Teams.Blue;
                break;
            case 3:
                team = GameManager.Teams.Red;
                break;
            default:
                team = GameManager.Teams.Null;
                break;
        }
    }

    // Starts the Turn and updates the counts of the Tiles
    public void StartTurn()
    {
            tiles[0].GetComponent<HexTile>().isBaseTile = true;

            int tileCount = tiles.Count;
            tiles[0].units += CalculateUnitsGained();

            // Unlock All Tiles
            foreach (HexTile tile in tiles)
            {
                tile.SetMoveLocked(false);
            }
            
            MoveUnitsOnPaths();

            if (!isAI)
            {
                StartCoroutine(replays.ShowReplay());
            }

            // AI Movement
            if (isAI)
            {
                aiComponent.StartTurn();
            }
    }

    // Calculates the amount of units the player receives
    public int CalculateUnitsGained()
    {
        int tileCount = tiles.Count;

        int baseUnits = tiles[0].units;

        int gainedUnits = Mathf.Max(1, Mathf.Min(baseUnits, tileCount)/3);

        return gainedUnits;
    }

    private void MoveUnitsOnPaths()
    {
        List<Movement> movementsCopy = new List<Movement>();

        for(int i = 0; i < movements.Count; i++)
        {
            movementsCopy.Add(movements[i]);
        }

        // All Movements
        for (int i = 0; i < movementsCopy.Count; i++)
        {
            Movement mov = movementsCopy[i];

            // Check if Movement still valid
            if (mov.path.Count <= 1 || mov.path[0].team != team || mov.path[0].units == 0)
            {
                mov.path[0].movementsFromTile.Remove(mov);
                movements.Remove(mov);
            }
            else
            {
                mov.Move(true);

                // Check if Movement still valid
                if (mov.path.Count <= 1 || mov.path[0].team != team || mov.path[0].units == 0)
                {
                    mov.path[0].movementsFromTile.Remove(mov);
                    movements.Remove(mov);
                }
            }
        }
    }

}
