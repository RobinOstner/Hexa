using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour {

    // Singleton
    public static InputManager current;

    // LayerMask for the Tiles
    public LayerMask tilesLayer;

    /*
    // HexTile the mouse is currently hovering over
    public HexTileDisplay hoverTile;

    // Timer for Dragging, counts how long the button was pressed
    public Vector2 dragDist;
    
    public HexTileDisplay startDragTile;
    public bool dragging;
    */
    
    public HexTile selectedHexTile;

    //**Tapping**
    // Total Touch Time
    public float totalTouchTime;
    // Max Touch Time for Taps
    public float tapTimeThreshold;
    // Total Touch Movement
    public Vector2 totalTouchMovement;
    // Max Touch Movement for Taps
    public float tapMovementThreshold;

	// Use this for initialization
	void Start () {
        current = this;
	}
	
	// Update is called once per frame
	void Update () {
        CheckTap();
	}

    // Checks the Input for Taps
    void CheckTap()
    {
        // Only Single Tap
        if(Input.touchCount == 1)
        {
            // Get Tap
            Touch touch = Input.touches[0];

            // First Contact
            if(touch.phase != TouchPhase.Began)
            {
                // Count Time & Distance only after first Contact
                totalTouchTime += Time.deltaTime;
                totalTouchMovement += touch.deltaPosition;
            }
            // Touch has ended
            if(touch.phase == TouchPhase.Ended)
            {
                // Check for Thresholds
                if (totalTouchTime <= tapTimeThreshold && totalTouchMovement.magnitude <= tapMovementThreshold)
                {
                    OnTap(touch);
                }
                else
                {
                    // Distinguish between Long Press & Swipe
                    if (totalTouchMovement.magnitude <= tapMovementThreshold)
                    {
                        OnLongPress(touch);
                    }
                    else
                    {
                        Debug.Log("Swipe! No Action Specified!");
                    }
                }

                // Reset Counters
                totalTouchTime = 0;
                totalTouchMovement = Vector2.zero;
            }
        }
    }

    // Behaviour for when the screen was tapped
    void OnTap(Touch touch)
    {
        // If Screen is tapped select the tile (If there is one...) and deselect old tile!
        if (selectedHexTile != null)
        {
            if (!selectedHexTile.attacking)
            {
                selectedHexTile.SetSelected(false);
            }
        }

        // Get the taps position in ScreenSpace
        Vector2 tapPosition = touch.position;
        // Create Ray in WorldSpace
        Ray tapRay = Camera.main.ScreenPointToRay(tapPosition);
        // Hit Object
        RaycastHit tapHit;

        // Check with Raycast
        if(Physics.Raycast(tapRay, out tapHit, 100, tilesLayer))
        {
            // Hit HexTile
            HexTile tappedHexTile = tapHit.collider.GetComponentInParent<HexTile>();

            // Same HexTile as Selected? More Options should now be presented on the screen!
            if (tappedHexTile == selectedHexTile)
            {
                TapOnSelected();
            }
            // Other HexTile was tapped
            else
            {
                TapOnOther(tappedHexTile);
            }
        }
    }

    // Tap was on the already Selected HexTile
    void TapOnSelected()
    {
        // Set selected again because it was disabled in the beginning!
        selectedHexTile.SetSelected(true);

        if (selectedHexTile.moveLocked)
        {
            Debug.Log("Already moved! Cannot move twice in one go!");
        }
        else
        {
            if (selectedHexTile.team == GameManager.current.activeTeam)
            {
                // Is already in Attack Mode?
                if (!selectedHexTile.attacking)
                {
                    selectedHexTile.attacking = true;

                    selectedHexTile.unitsAfterMovement = selectedHexTile.units;
                }
                else
                {
                    GridManager.current.FinishAttackForAllTiles(true);
                }
            }
        }
    }

    // Tap was on another HexTile
    void TapOnOther(HexTile tappedHexTile)
    {
        // In Attack Mode?
        if (selectedHexTile != null && selectedHexTile.attacking)
        {
            TapOnOtherAttack(tappedHexTile);
        }
        else
        {
            // Other Tile was selected so this should now be the new selectedHexTile
            selectedHexTile = tappedHexTile;
            // Set the Tile Selected (Visual representation)
            selectedHexTile.SetSelected(true);
        }
    }

    // Tap was on another HexTile while in attack mode
    void TapOnOtherAttack(HexTile tappedHexTile)
    {
        // Selected HexTile should stay selected
        selectedHexTile.SetSelected(true);

        // Does the selected tile even have enough units to keep moving
        if (selectedHexTile.unitsAfterMovement > 0 && selectedHexTile.IsNeighbourTo(tappedHexTile))
        {
            // Selected looses one Unit
            selectedHexTile.unitsAfterMovement--;

            // Activate TappedHexTile if it's not already activated/initialized
            if (!tappedHexTile.attacking)
            {
                tappedHexTile.attacking = true;
                tappedHexTile.unitsAfterMovement = tappedHexTile.units;
            }

            // Move Unit to Tapped Tile
            if (tappedHexTile.team == selectedHexTile.team || tappedHexTile.team == GameManager.Teams.Null)
            {
                tappedHexTile.unitsAfterMovement++;
                if (tappedHexTile.team == GameManager.Teams.Null)
                {
                    tappedHexTile.team = selectedHexTile.team;
                    GameManager.current.AddTileToPlayer(tappedHexTile);
                }
            }
            else
            {
                tappedHexTile.unitsAfterMovement--;
            }
        }
    }

    // Behaviour for when the screen was pressed for a longer time
    void OnLongPress(Touch touch)
    {
        // Get the taps position in ScreenSpace
        Vector2 tapPosition = touch.position;
        // Create Ray in WorldSpace
        Ray tapRay = Camera.main.ScreenPointToRay(tapPosition);
        // Hit Object
        RaycastHit tapHit;

        // Check with Raycast
        if (Physics.Raycast(tapRay, out tapHit, 100, tilesLayer))
        {
            // Hit HexTile
            HexTile tappedHexTile = tapHit.collider.GetComponentInParent<HexTile>();

            // Long Press while in Attack Mode
            if (selectedHexTile != null && selectedHexTile.attacking)
            {
                if(tappedHexTile != selectedHexTile)
                {
                    // Long Press on other Tile moves half of Selected Tile UNITS
                    for(int i=0; i<selectedHexTile.units/2; i++)
                    {
                        TapOnOtherAttack(tappedHexTile);
                    }
                }
                else
                {
                    // Long Press on selected Tile when in Attack Mode cancels all operations
                    GridManager.current.FinishAttackForAllTiles(false);
                }
            }
        }
    }
}
