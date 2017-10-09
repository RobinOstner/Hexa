using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexTile : MonoBehaviour
{
    // Pathfinding Discovery Record
    [HideInInspector]
    public HexTile discoveredBy;

    // Current Team
    public GameManager.Teams team;
    public HexTileDisplay hexDisplay;


    public List<HexTile> neighbourTiles
    {
        get {
            List<HexTile> neighbours = new List<HexTile>();
            foreach(HexTileDisplay display in hexDisplay.neighbourTiles)
            {
                neighbours.Add(display.hexTile);
            }
            return neighbours;
        }
    }


    // Is this tile the base of the player
    public bool isBaseTile;

    // Number of Units on this Tile
    public int units;
    // Number of Units when movement is done
    public int unitsAfterMovement;

    // Already moved the Units on this Tile?
    public bool moveLocked;

    public List<Movement> movementsFromTile;

    // Is the player attacking/moving with this tile?
    public bool attacking;

    // Use this for initialization
    void Start() {
        hexDisplay = GetComponent<HexTileDisplay>();
        movementsFromTile = new List<Movement>();
    }

    // Update is called once per frame
    void Update() {
        /* Height Change
        transform.localScale = new Vector3(1, 1 + units/100f, 1);
        transform.position = new Vector3(transform.position.x, units/100f/2f, transform.position.z);
        */
        if (!hexDisplay.selected)
        {
            unitsAfterMovement = 0;
        }
        if(hexDisplay.selected && !attacking)
        {
            unitsAfterMovement = 0;
        }

        hexDisplay.highlighted = false;
        CheckFree();
    }

    // Moves ALL Units from this tile to the other Tile
    public bool MoveUnitsToTile(HexTile otherTile, int Amount)
    {
        otherTile.moveLocked = otherTile.team == GameManager.current.activeTeam;

        // Not possible if Amount is bigger than available units
        if(Amount > units)
        {
            return false;
        }

        // Same Team
        if (team == otherTile.team)
        {
            otherTile.units += Amount;
            units -= Amount;

            return true;
        }
        // Other Team
        if (otherTile.team != GameManager.Teams.Null)
        {
            otherTile.units -= Amount;
            units -= Amount;

            // Conquered other Tile
            if (otherTile.units < 0)
            {
                GameManager.current.AddTileToPlayer(otherTile, GameManager.current.activePlayer);

                otherTile.units *= -1;
            }

            return true;
        }
        // Empty HexTile
        if (otherTile.team == GameManager.Teams.Null)
        {
            GameManager.current.AddTileToPlayer(otherTile, GameManager.current.activePlayer);

            otherTile.units = Amount;
            units -= Amount;
            return true;
        }

        return false;
    }

    // Change Visuals after tapped
    public void SetSelected(bool value)
    {
        hexDisplay.SetSelected(value);
    }

    // Tests itself whether or not it's free
    public void CheckFree()
    {
        if(!attacking && units <= 0)
        {
            GameManager.current.ResetTile(this);
        }
    }

    public bool IsNeighbourTo(HexTile otherTile)
    {
        return hexDisplay.neighbourTiles.Contains(otherTile.hexDisplay);
    }

}
