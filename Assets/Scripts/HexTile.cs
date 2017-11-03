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
    private bool moveLocked;
    public bool locked;

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
        locked = moveLocked;

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
    public bool MoveUnitsToTile(HexTile otherTile, int Amount, bool replay)
    {
        otherTile.SetMoveLocked( otherTile.team == GameManager.current.activeTeam || otherTile.team == GameManager.Teams.Null);

        otherTile.CheckFree();

        // Not possible if Amount is bigger than available units
        if(Amount > units)
        {
            return false;
        }
        // Same Team
        if (team == otherTile.team)
        {
            ChangedTile change = new ChangedTile(this, otherTile, -1, 1);
            change.AddTeamBefore(team, otherTile.team);

            otherTile.units += Amount;
            units -= Amount;

            Player player = GameManager.current.GetPlayerByTeam(otherTile.team);

            GameManager.Teams after = units == 0 ? GameManager.Teams.Null : team;
            change.AddTeamAfter(after, otherTile.team, units, otherTile.units);
            if (replay)
            {
                player.replays.AddMovementToReplay(change);
            }

            if (change.teamBBefore != change.teamBAfter && change.teamBAfter != GameManager.Teams.Null)
            {
                Debug.Log("Marked Change " + change.tileA + "->" + change.tileB + ": SAME TEAM");
                change.Mark();
            }

            CheckFree();
            otherTile.CheckFree();

            return true;
        }
        // Other Team
        if (otherTile.team != GameManager.Teams.Null)
        {
            ChangedTile change = new ChangedTile(this, otherTile, -1, -1);
            change.AddTeamBefore(team, otherTile.team);

            otherTile.units -= Amount;
            units -= Amount;

            GameManager.Teams otherBefore = otherTile.team;

            // Conquered other Tile
            if (otherTile.units < 0)
            {
                Debug.Log("Conquered Tile! Shouldn't happen! Amount: " + Amount);
                GameManager.current.AddTileToPlayer(otherTile, GameManager.current.activePlayer);

                otherTile.units *= -1;
            }
            
            Player player = GameManager.current.GetPlayerByTeam(otherTile.team);

            GameManager.Teams afterOwn = units <= 0 ? GameManager.Teams.Null : team;
            
            GameManager.Teams afterOther;

            if(otherTile.units <= 0)
            {
                afterOther = GameManager.Teams.Null;
            }
            else
            {
                afterOther = otherTile.team;
            }


            change.AddTeamAfter(afterOwn, afterOther, units, otherTile.units);
            player.replays.AddMovementToReplay(change);

            if(change.teamBBefore != change.teamBAfter && change.teamBAfter != GameManager.Teams.Null)
            {
                Debug.Log("Marked Change " + change.tileA + "->" + change.tileB + ": OTHER TEAM");
                change.Mark();
            }

            if (replay)
            {
                GameManager.current.GetPlayerByTeam(team).replays.AddMovementToReplay(change);
            }

            CheckFree();
            otherTile.CheckFree();

            return true;
        }
        // Empty HexTile
        if (otherTile.team == GameManager.Teams.Null)
        {
            ChangedTile change = new ChangedTile(this, otherTile, -1, 1);
            change.AddTeamBefore(team, otherTile.team);

            GameManager.current.AddTileToPlayer(otherTile, GameManager.current.activePlayer);

            otherTile.units = Amount;
            units -= Amount;
            

            Player player = GameManager.current.GetPlayerByTeam(team);

            GameManager.Teams after = units == 0 ? GameManager.Teams.Null : team;
            change.AddTeamAfter(after, otherTile.team, units, otherTile.units);


            if (change.teamBBefore != change.teamBAfter && change.teamBAfter != GameManager.Teams.Null && change.teamBBefore != GameManager.Teams.Null)
            {
                Debug.Log("Marked Change " + change.tileA + "->" + change.tileB + ": EMPTY");
                change.Mark();
            }

            if (replay)
            {
                player.replays.AddMovementToReplay(change);
            }

            CheckFree();
            otherTile.CheckFree();

            return true;
        }

        return false;
    }

    // Change Visuals after tapped
    public void SetSelected(bool value)
    {
        hexDisplay.SetSelected(value);
    }

    public void SetMoveLocked(bool value)
    {
        moveLocked = value;
        
        if (moveLocked)
        {
            hexDisplay.hexagonSpriteStriped.SetActive(true);
            hexDisplay.hexagonSpriteNormal.SetActive(false);
        }
        else
        {
            hexDisplay.hexagonSpriteStriped.SetActive(false);
            hexDisplay.hexagonSpriteNormal.SetActive(true);
        }
    }

    public bool IsMoveLocked() { return moveLocked; }

    // Tests itself whether or not it's free
    public void CheckFree()
    {
        if(!attacking && !CameraBehaviour.current.replayMode && units <= 0)
        {
            GameManager.current.ResetTile(this);
        }
    }

    public bool IsNeighbourTo(HexTile otherTile)
    {
        return hexDisplay.neighbourTiles.Contains(otherTile.hexDisplay);
    }

}
