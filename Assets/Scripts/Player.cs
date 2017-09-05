using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    // List with all Tiles of this player
    public List<HexTile> tiles;

    // Identification (Player Number)
    public int ID;

    public GameManager.Teams team;

	// Use this for initialization
	void Start ()
    {
        name = "Player " + ID;
        tiles = new List<HexTile>();

        SetTeamColor();
	}
	
	// Update is called once per frame
	void Update () {
		
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

    // Finishes the Turn and updates the counts of the Tiles
    public void FinishTurn()
    {
        if (tiles.Count > 0)
        {
            tiles[0].GetComponent<HexTile>().isBaseTile = true;

            int tileCount = tiles.Count;
            if (tileCount >= 0)
            {
                int baseUnits = tiles[0].units;

                int gainedUnits = Mathf.Max(tileCount / 3, 1) + Mathf.Min(baseUnits, tileCount);

                tiles[0].units += gainedUnits;
            }

            foreach (HexTile tile in tiles)
            {
                tile.moveLocked = false;
            }
        }
        else
        {
            if (GameManager.current.rounds > 2)
            {
                Debug.Log("Defeat!");
                StartCoroutine(GameManager.current.Defeat(this));
            }
        }
    }
}
