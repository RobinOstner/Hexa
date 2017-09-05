using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexTile : MonoBehaviour {

    // Current Team
    public GameManager.Teams team;

    public HexTileDisplay hexDisplay;

    // Is this tile the base of the player
    public bool isBaseTile;

    // Number of Units on this Tile
    public int units;
    // Number of Units when movement is done
    public int unitsAfterMovement;

    // Already moved the Units on this Tile?
    public bool moveLocked;

    // Is the player attacking/moving with this tile?
    public bool attacking;

    // Use this for initialization
    void Start() {
        hexDisplay = GetComponent<HexTileDisplay>();
    }

    // Update is called once per frame
    void Update() {
        CheckFree();
    }

    // Moves ALL Units from this tile to the other Tile
    public void MoveUnitsToTile(HexTile otherTile)
    {
        moveLocked = true;

        // Same Team
        if (team == otherTile.team)
        {
            otherTile.moveLocked = true;
            otherTile.units += units;
            units = 0;
        }
        // Other Team
        if (otherTile.team != GameManager.Teams.Null)
        {
            int otherUnitsSave = otherTile.units;
            otherTile.units -= Mathf.Min(units, otherTile.units);
            units -= Mathf.Min(otherUnitsSave, units);

            if (otherTile.units == 0 && units > 0)
            {
                otherTile.team = team;
                GameManager.current.AddTileToPlayer(otherTile);

                otherTile.units = units;
                units = 0;
            }
        }
        // Empty HexTile
        if (otherTile.team == GameManager.Teams.Null)
        {
            otherTile.moveLocked = true;

            otherTile.team = team;
            GameManager.current.AddTileToPlayer(otherTile);

            otherTile.units = units;
            units = 0;
        }
    }

    // Change Visuals after tapped
    public void SetSelected(bool value)
    {
        hexDisplay.SetSelected(value);
    }

    // Tests itself whether or not it's free
    public void CheckFree()
    {
        if(!attacking && units == 0)
        {
            GameManager.current.ResetTile(this);
        }
    }

    public bool IsNeighbourTo(HexTile otherTile)
    {
        return hexDisplay.neighbourTiles.Contains(otherTile.hexDisplay);
    }
}
