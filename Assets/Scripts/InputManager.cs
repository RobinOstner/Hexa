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
    
    //[HideInInspector]
    public HexTile selectedHexTile;

    [HideInInspector]
    public List<Movement> movements;

    //**Tapping**
    // Total Touch Time
    public float totalTouchTime;
    // Max Touch Time for Taps
    public float tapTimeThreshold;
    // Total Touch Movement
    public Vector2 totalTouchMovement;
    // Max Touch Movement for Taps
    public float tapMovementThreshold;

    public bool tapActionFound;

	// Use this for initialization
	void Start () {
        current = this;
	}
	
	// Update is called once per frame
	void Update () {
        // Only Check if Player should be able to do something
        if (GameManager.current.playerControl && !GameManager.current.activePlayer.isAI)
        {
            CheckTap();

            foreach (Movement mov in movements)
            {
                mov.HighlightPath();
            }
        }
	}

    // Checks the Input for Taps
    void CheckTap()
    {
        // Only Single Tap
        if(Input.touchCount == 1)
        {
            // Get Tap
            Touch touch = Input.touches[0];


            if (!tapActionFound)
            {
                // First Contact
                if (touch.phase != TouchPhase.Began)
                {
                    // Count Time & Distance only after first Contact
                    totalTouchTime += Time.deltaTime;
                    totalTouchMovement += touch.deltaPosition;
                }

                // Check for Thresholds
                if (touch.phase == TouchPhase.Ended && totalTouchTime <= tapTimeThreshold && totalTouchMovement.magnitude <= tapMovementThreshold)
                {
                    OnTap(touch);
                }

                if (totalTouchTime > tapTimeThreshold && totalTouchMovement.magnitude <= tapMovementThreshold)
                {
                    // Long Press
                    OnLongPress(touch);
                    tapActionFound = true;
                }
            }

            if (touch.phase == TouchPhase.Ended)
            {
                // Reset Counters
                totalTouchTime = 0;
                totalTouchMovement = Vector2.zero;
                tapActionFound = false;
            }

            /*
            // Touch has ended
            if (touch.phase == TouchPhase.Ended)
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
                }
            }
            */
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

            if(selectedHexTile == null)
            {
                if (!tappedHexTile.IsMoveLocked() && tappedHexTile.team == GameManager.current.activeTeam)
                {
                    selectedHexTile = tappedHexTile;
                    selectedHexTile.SetSelected(true);
                    selectedHexTile.attacking = true;
                    selectedHexTile.unitsAfterMovement = selectedHexTile.units;
                }
            }
            else
            {
                if(selectedHexTile == tappedHexTile)
                {
                    selectedHexTile.SetSelected(false);
                    selectedHexTile.attacking = false;
                    selectedHexTile = null;
                    GridManager.current.FinishAttackForAllTiles(movements);
                }
                else
                {
                    TapOnOtherAttack(tappedHexTile);
                }
            }

            /*
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
            */
        }
    }

    // Tap was on the already Selected HexTile
    void TapOnSelected()
    {
        // Set selected again because it was disabled in the beginning!
        selectedHexTile.SetSelected(true);

        if (selectedHexTile.IsMoveLocked())
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
                    GridManager.current.FinishAttackForAllTiles(movements);
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

        // Does the selected tile even have enough units to keep moving || && selectedHexTile.IsNeighbourTo(tappedHexTile)
        if (selectedHexTile.unitsAfterMovement > 0)
        {
            List<HexTile> path = Pathfinding.CalculatePath(selectedHexTile, tappedHexTile);

            if (path.Count > 0)
            {
                // Selected looses one Unit
                selectedHexTile.unitsAfterMovement--;

                movements.Add(new Movement(1, path, GameManager.current.activeTeam));
            }
            else
            {
                Debug.Log("Path Not Found");
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
                    StartCoroutine(CameraBehaviour.current.Shake());

                    // Long Press on selected Tile when in Attack Mode cancels all operations
                    GridManager.current.FinishAttackForAllTiles(new List<Movement>());
                    selectedHexTile.SetSelected(false);
                    selectedHexTile = null;
                }
            }
        }
    }
}
